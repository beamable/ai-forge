using Beamable.Common;
using Beamable.Editor.Common;
using Beamable.Editor.UI.Buss;
using Beamable.UI.Buss;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Beamable.Common.Constants.Features.Buss.ThemeManager;

namespace Beamable.Editor.UI.Components
{
	public enum RuleAppliedStatus
	{
		Exact,
		Inherited,
		NotApplied
	}

	public class StyleCardModel
	{
		public event Action Change;
		public event Action SelectorChanged;
		private readonly ThemeModel.PropertyDisplayFilter _currentDisplayFilter;
		private readonly Action _globalRefresh;

		public bool IsSelected { get; }
		public BussStyleSheet StyleSheet { get; }
		public BussStyleRule StyleRule { get; }
		private PropertySourceDatabase PropertiesDatabase { get; }
		private IEnumerable<BussStyleSheet> WritableStyleSheets { get; }
		public bool IsWritable => StyleSheet.IsWritable;
		public bool IsFolded => StyleRule.Folded;
		public bool ShowAll => StyleRule.ShowAll;

		private RuleAppliedStatus _previousAppliedStatus = RuleAppliedStatus.Exact;
		public RuleAppliedStatus RuleAppliedStatus
		{
			get
			{
				if (StyleRule?.Selector == null || SelectedElement == null)
				{
					return _previousAppliedStatus;
				}
				if (StyleRule.Selector.IsElementIncludedInSelector(SelectedElement, out var exact))
				{
					return _previousAppliedStatus = exact
						? RuleAppliedStatus.Exact
						: RuleAppliedStatus.Inherited;
				}

				return _previousAppliedStatus = RuleAppliedStatus.NotApplied;
			}
		}

		private BussElement SelectedElement { get; }

		public StyleCardModel(BussStyleSheet styleSheet,
							  BussStyleRule styleRule,
							  BussElement selectedElement,
							  bool isSelected,
							  PropertySourceDatabase propertiesDatabase,
							  IEnumerable<BussStyleSheet> writableStyleSheets,
							  Action globalRefresh,
							  ThemeModel.PropertyDisplayFilter currentDisplayFilter)
		{
			StyleSheet = styleSheet;
			StyleRule = styleRule;
			SelectedElement = selectedElement;
			IsSelected = isSelected;
			PropertiesDatabase = propertiesDatabase;
			WritableStyleSheets = writableStyleSheets;

			_globalRefresh = globalRefresh;
			_currentDisplayFilter = currentDisplayFilter;
		}

		public void AddRuleButtonClicked(MouseDownEvent evt)
		{
			HashSet<string> keys = new HashSet<string>();
			foreach (BussPropertyProvider propertyProvider in StyleRule.Properties)
			{
				keys.Add(propertyProvider.Key);
			}

			IOrderedEnumerable<string> sorted = BussStyle.Keys.OrderBy(k => k);
			GenericMenu context = new GenericMenu();

			foreach (string key in sorted)
			{
				if (keys.Contains(key)) continue;
				Type baseType = BussStyle.GetBaseType(key);
				SerializableValueImplementationHelper.ImplementationData data =
					SerializableValueImplementationHelper.Get(baseType);
				IEnumerable<Type> types = data.subTypes.Where(t => t != null && t.IsClass && !t.IsAbstract &&
																   t != typeof(FractionFloatBussProperty)).ToList();

				foreach (Type type in types)
				{
					GUIContent label = new GUIContent(types.Count() > 1
														  ? ThemeManagerHelper.FormatKey(key) + "/" +
															ThemeManagerHelper.FormatKey(type.Name)
														  : ThemeManagerHelper.FormatKey(key));
					context.AddItem(new GUIContent(label), false, () =>
					{
						Undo.RecordObject(StyleSheet, $"Add {label}");
						StyleRule.Properties.Add(
							BussPropertyProvider.Create(key, (IBussProperty)Activator.CreateInstance(type)));
#if UNITY_EDITOR
						EditorUtility.SetDirty(StyleSheet);
#endif
						AssetDatabase.SaveAssets();
						StyleSheet.TriggerChange();
						Change?.Invoke();
						_globalRefresh?.Invoke();
					});
				}
			}

			context.ShowAsContext();
		}

		public void AddVariableButtonClicked(MouseDownEvent evt)
		{
			NewVariableWindow window = NewVariableWindow.ShowWindow();

			if (window != null)
			{
				window.Init((key, property) =>
				{
					Undo.RecordObject(StyleSheet, $"Add {key}");
					if (!StyleRule.TryAddProperty(key, property))
					{
						return;
					}
#if UNITY_EDITOR
					EditorUtility.SetDirty(StyleSheet);
#endif

					AssetDatabase.SaveAssets();
					StyleSheet.TriggerChange();
					Change?.Invoke();
				});
			}
		}

		public void ClearAllButtonClicked(MouseDownEvent evt)
		{
			BeamablePopupWindow.CloseConfirmationWindow();

			ConfirmationPopupVisualElement confirmationPopup = new ConfirmationPopupVisualElement(
				CLEAR_ALL_PROPERTIES_MESSAGE,
				() =>
				{
					StyleSheet.RemoveAllProperties(StyleRule);
					_globalRefresh?.Invoke();
				},
				BeamablePopupWindow.CloseConfirmationWindow
			);

			BeamablePopupWindow.ShowConfirmationUtility(CLEAR_ALL_PROPERTIES_HEADER, confirmationPopup, null);
		}

		public void FoldButtonClicked(MouseDownEvent evt)
		{
#if UNITY_EDITOR
			EditorUtility.SetDirty(StyleSheet);
#endif

			StyleRule.SetFolded(!StyleRule.Folded);
			AssetDatabase.SaveAssets();
			_globalRefresh?.Invoke();
		}

		public List<StylePropertyModel> GetProperties(bool sort = true)
		{
			var models = new List<StylePropertyModel>();

			foreach (string key in BussStyle.Keys)
			{
				var propertyProvider = StyleRule.Properties.Find(provider => provider.Key == key) ??
									   BussPropertyProvider.Create(key, BussStyle.GetDefaultValue(key).CopyProperty());

				var model = new StylePropertyModel(StyleSheet, StyleRule, propertyProvider,
												   PropertiesDatabase.GetTracker(SelectedElement), SelectedElement,
												   null, RemovePropertyClicked, _globalRefresh, SetValueTypeClicked);

				if (!(_currentDisplayFilter == ThemeModel.PropertyDisplayFilter.IgnoreOverridden && model.IsOverriden))
				{
					models.Add(model);
				}
			}

			var sortedModels = models.ToList();
			sortedModels.Sort((a, b) =>
			{
				var overridenComparison = a.IsOverriden.CompareTo(b.IsOverriden);
				if (overridenComparison != 0) return overridenComparison;

				return String.Compare(a.PropertyProvider.Key, b.PropertyProvider.Key, StringComparison.Ordinal);
			});
			models = sortedModels;
			models.AddRange(GetVariables());
			return models;
		}

		public void OptionsButtonClicked(MouseDownEvent evt)
		{
			GenericMenu context = new GenericMenu();

			foreach (GenericMenuCommand command in PrepareCommands())
			{
				GUIContent label = new GUIContent(command.Name);
				context.AddItem(new GUIContent(label), false, () => { command.Invoke(); });
			}

			context.ShowAsContext();
		}

		public void OnSelectorChanged(BussStyleRule rule, BussStyleSheet sheet)
		{
			if (SelectedElement != null)
			{
				SelectedElement.RecalculateStyle();
			}
			SelectorChanged?.Invoke();
		}

		public List<GenericMenuCommand> PrepareCommands()
		{
			List<GenericMenuCommand> commands = new List<GenericMenuCommand>();

			if (StyleSheet.IsWritable)
			{
				commands.Add(new GenericMenuCommand(Constants.Features.Buss.MenuItems.DUPLICATE, () =>
				{
					BussStyleSheetUtility.CopySingleStyle(StyleSheet, StyleRule);
					_globalRefresh.Invoke();
				}));
			}

			List<BussStyleSheet> writableStyleSheets = new List<BussStyleSheet>(WritableStyleSheets);
			writableStyleSheets.Remove(StyleSheet);

			if (writableStyleSheets.Count > 0)
			{
				foreach (BussStyleSheet targetStyleSheet in writableStyleSheets)
				{
					commands.Add(new GenericMenuCommand(
									 $"{Constants.Features.Buss.MenuItems.COPY_TO}/{targetStyleSheet.name}",
									 () =>
									 {
										 BussStyleSheetUtility.CopySingleStyle(targetStyleSheet, StyleRule);
										 _globalRefresh.Invoke();
									 }));
				}
			}
			else
			{
				commands.Add(new GenericMenuCommand($"{Constants.Features.Buss.MenuItems.COPY_INTO_NEW_STYLE_SHEET}",
													() =>
													{
														NewStyleSheetWindow window = NewStyleSheetWindow.ShowWindow();
														if (window != null)
														{
															window.Init(new List<BussStyleRule> { StyleRule });
														}
													}));
			}

			if (IsWritable)
			{
				commands.Add(new GenericMenuCommand(Constants.Features.Buss.MenuItems.REMOVE, RemoveStyleClicked));
			}

			return commands;
		}

		public void ShowAllButtonClicked(MouseDownEvent evt)
		{
#if UNITY_EDITOR
			EditorUtility.SetDirty(StyleSheet);
#endif

			Undo.RecordObject(StyleSheet, StyleRule.ShowAll ? "Hide All" : "Show All");
			StyleRule.SetShowAll(!StyleRule.ShowAll);
			AssetDatabase.SaveAssets();
			_globalRefresh?.Invoke();
		}

		private List<StylePropertyModel> GetVariables()
		{
			var variables = new List<StylePropertyModel>();

			foreach (var propertyProvider in StyleRule.Properties)
			{
				if (!propertyProvider.IsVariable)
				{
					continue;
				}

				var model = new StylePropertyModel(StyleSheet, StyleRule, propertyProvider,
												   PropertiesDatabase.GetTracker(SelectedElement), SelectedElement, null,
												   RemovePropertyClicked, _globalRefresh, SetValueTypeClicked);
				variables.Add(model);
			}

			return variables;
		}

		private void SetValueTypeClicked(string propertyKey, BussPropertyValueType valueType)
		{
			var propertyModel = GetProperties(false).Find(property => property.PropertyProvider.Key == propertyKey);
			if (propertyModel == null)
			{
				Debug.LogWarning($"StyleCardModel:{nameof(SetValueTypeClicked)}: can't find property with {propertyKey} key");
				return;
			}

			propertyModel.PropertyProvider.GetProperty().ValueType = valueType;
#if UNITY_EDITOR
			EditorUtility.SetDirty(propertyModel.StyleSheet);
#endif
			AssetDatabase.SaveAssets();

			Change?.Invoke();
			_globalRefresh?.Invoke();
		}

		private void RemovePropertyClicked(string propertyKey)
		{
			Undo.RecordObject(StyleSheet, "Remove property");

			var propertyModel = GetProperties(false).Find(property => property.PropertyProvider.Key == propertyKey);

			if (propertyModel == null)
			{
				Debug.LogWarning($"StyleCardModel:{nameof(RemovePropertyClicked)}: can't find property with {propertyKey} key");
				return;
			}

			if (propertyModel.InlineStyleOwner != null)
			{
				propertyModel.InlineStyleOwner.InlineStyle.Properties.Remove(propertyModel.PropertyProvider);
			}
			else
			{
				IBussProperty bussProperty = propertyModel.PropertyProvider.GetProperty();
				propertyModel.StyleSheet.RemoveStyleProperty(bussProperty, propertyModel.StyleRule);

#if UNITY_EDITOR
				EditorUtility.SetDirty(propertyModel.StyleSheet);
#endif
				AssetDatabase.SaveAssets();
			}

			Change?.Invoke();
			_globalRefresh?.Invoke();
		}

		private void RemoveStyleClicked()
		{
			BeamablePopupWindow.CloseConfirmationWindow();

			ConfirmationPopupVisualElement confirmationPopup = new ConfirmationPopupVisualElement(
				DELETE_STYLE_MESSAGE,
				() =>
				{
					BussStyleSheetUtility.RemoveSingleStyle(StyleSheet, StyleRule);
					_globalRefresh?.Invoke();
				},
				BeamablePopupWindow.CloseConfirmationWindow
			);

			BeamablePopupWindow.ShowConfirmationUtility(DELETE_STYLE_HEADER, confirmationPopup, null);
		}
	}
}
