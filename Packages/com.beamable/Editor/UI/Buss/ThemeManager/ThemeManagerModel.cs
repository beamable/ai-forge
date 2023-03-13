using Beamable.UI.Buss;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Beamable.Common.Constants.Features.Buss.ThemeManager;
using Object = UnityEngine.Object;

namespace Beamable.Editor.UI.Buss
{
	public class ThemeManagerModel : ThemeModel
	{
		public override BussElement SelectedElement { get; set; }

		public string SelectedElementId =>
			SelectedElement != null ? BussNameUtility.AsIdSelector(SelectedElement.Id) : String.Empty;

		public BussStyleSheet SelectedElementStyleSheet => SelectedElement != null ? SelectedElement.StyleSheet : null;

		protected override List<BussStyleSheet> SceneStyleSheets { get; } = new List<BussStyleSheet>();

		private List<BussStyleSheet> GlobalStyleSheets
		{
			get
			{
				var configuration = BussConfiguration.OptionalInstance.Value;

				List<BussStyleSheet> list = new List<BussStyleSheet>();
				if (configuration != null)
				{
					list.AddRange(configuration.FactoryStyleSheets);
					list.AddRange(configuration.DeveloperStyleSheets);
				}

				return list;
			}
		}

		private List<BussStyleSheet> AllStyleSheets
		{
			get
			{
				List<BussStyleSheet> list = new List<BussStyleSheet>();
				list.AddRange(GlobalStyleSheets);
				list.AddRange(SceneStyleSheets);
				return list;
			}
		}

		public override Dictionary<BussStyleRule, BussStyleSheet> FilteredRules =>
			Filter.GetFiltered(AllStyleSheets, SelectedElement);

		public override List<BussStyleSheet> WritableStyleSheets
		{
			get
			{
#if BEAMABLE_DEVELOPER
				return AllStyleSheets ?? new List<BussStyleSheet>();
#else
				return AllStyleSheets?.Where(s => !s.IsReadOnly).ToList() ?? new List<BussStyleSheet>();
#endif
			}
		}

		public ThemeManagerModel()
		{
			EditorApplication.hierarchyChanged += OnHierarchyChanged;
			Selection.selectionChanged += OnSelectionChanged;

			Filter = new BussCardFilter();

			OnHierarchyChanged();
		}

		public void AddInlineProperty()
		{
			if (SelectedElement == null)
			{
				return;
			}

			var keys = new HashSet<string>();
			foreach (BussPropertyProvider propertyProvider in SelectedElement.InlineStyle.Properties)
			{
				keys.Add(propertyProvider.Key);
			}

			IOrderedEnumerable<string> sorted = BussStyle.Keys.OrderBy(k => k);
			var context = new GenericMenu();

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
					var label = new GUIContent(types.Count() > 1 ? key + "/" + type.Name : key);
					context.AddItem(new GUIContent(label), false, () =>
					{
						if (SelectedElement.InlineStyle.TryAddProperty(
								key, (IBussProperty)Activator.CreateInstance(type)))
						{
							// TODO: TD000004. We shouldn't need to call this from model. This should happen "under the hood". Subject for deeper refactor of buss core system.
							EditorUtility.SetDirty(SelectedElement);
							SelectedElement.RecalculateStyle();
							ForceRefresh();
						}
					});
				}
			}

			context.ShowAsContext();
		}

		public void AddInlineVariable()
		{
			if (SelectedElement == null)
			{
				return;
			}

			NewVariableWindow window = NewVariableWindow.ShowWindow();
			if (window != null)
			{
				window.Init((key, property) =>
				{
					if (SelectedElement.InlineStyle.TryAddProperty(key, property))
					{
						// TODO: TD000004. We shouldn't need to call this from model. This should happen "under the hood". Subject for deeper refactor of buss core system.
						EditorUtility.SetDirty(SelectedElement);
						SelectedElement.RecalculateStyle();
						ForceRefresh();
					}
				});
			}
		}

		public void Clear()
		{
			EditorApplication.hierarchyChanged -= OnHierarchyChanged;
			Selection.selectionChanged -= OnSelectionChanged;

			foreach (var styleSheet in SceneStyleSheets)
			{
				styleSheet.Change -= OnStyleSheetChanged;
			}

			SceneStyleSheets.Clear();

			foreach (var element in FoundElements)
			{
				element.Key.Change -= OnStyleSheetChanged;
			}

			FoundElements.Clear();
		}

		public void OnCopyButtonClicked()
		{
			List<BussStyleSheet> readonlyStyles = AllStyleSheets.Where(styleSheet => styleSheet.IsReadOnly).ToList();
			OpenCopyMenu(readonlyStyles);
		}

		public void OnIdChanged(string value)
		{
			if (SelectedElement == null)
			{
				return;
			}

			Undo.RecordObject(SelectedElement, "Change Id");
			SelectedElement.Id = BussNameUtility.CleanString(value);

			EditorUtility.SetDirty(SelectedElement);
			ForceRefresh();
		}

		public void OnSearch(string value)
		{
			Filter.CurrentFilter = value;
			ForceRefresh();
		}

		public void OnStyleSheetSelected(Object styleSheet)
		{
			if (SelectedElement != null)
			{
				BussStyleSheet newStyleSheet = (BussStyleSheet)styleSheet;
				SelectedElement.StyleSheet = newStyleSheet;
			}

			BussConfiguration.OptionalInstance.Value.ForceRefresh();
			ForceRefresh();
		}

		public void SetInlinePropertyValueType(string propertyKey, BussPropertyValueType valueType)
		{
			if (SelectedElement == null) return;
			var propertyProvider = SelectedElement.InlineStyle.Properties.Find(property => property.Key == propertyKey);
			if (propertyProvider == null) return;

			propertyProvider.GetProperty().ValueType = valueType;
			// TODO: TD000004. We shouldn't need to call this from model. This should happen "under the hood". Subject for deeper refactor of buss core system.
			EditorUtility.SetDirty(SelectedElement);
			SelectedElement.RecalculateStyle();
			ForceRefresh();
		}

		public void RemoveInlineProperty(string value)
		{
			if (SelectedElement == null)
			{
				return;
			}

			var propertyProvider = SelectedElement.InlineStyle.Properties.Find(property => property.Key == value);

			if (propertyProvider != null)
			{
				SelectedElement.InlineStyle.Properties.Remove(propertyProvider);

				// TODO: TD000004. We shouldn't need to call this from model. This should happen "under the hood". Subject for deeper refactor of buss core system.
				EditorUtility.SetDirty(SelectedElement);
				SelectedElement.RecalculateStyle();
				ForceRefresh();
			}
		}

		private void BussElementClicked(BussElement element)
		{
			SelectedElement = element;
			ForceRefresh();
		}

		private void OnHierarchyChanged()
		{
			FoundElements.Clear();

			foreach (Object foundObject in Object.FindObjectsOfType(typeof(GameObject)))
			{
				GameObject gameObject = (GameObject)foundObject;
				if (gameObject.transform.parent == null)
				{
					Traverse(gameObject, 0);
				}
			}

			ForceRefresh();
		}

		private void OnObjectRegistered(BussElement registeredObject)
		{
			registeredObject.Change += OnStyleSheetChanged;

			BussStyleSheet styleSheet = registeredObject.StyleSheet;

			if (styleSheet == null) return;

			if (!SceneStyleSheets.Contains(styleSheet))
			{
				SceneStyleSheets.Add(styleSheet);
				styleSheet.Change += OnStyleSheetChanged;
			}
		}

		private void OnSelectionChanged()
		{
			if (Selection.activeGameObject != null)
			{
				BussElement bussElement = Selection.activeGameObject.GetComponent<BussElement>();
				BussElementClicked(bussElement);
			}
			else
			{
				BussElementClicked(null);
			}
		}

		private void OnStyleSheetChanged()
		{

		}

		private void OpenCopyMenu(IEnumerable<BussStyleSheet> bussStyleSheets)
		{
			GenericMenu context = new GenericMenu();
			context.AddItem(new GUIContent(DUPLICATE_STYLESHEET_OPTIONS_HEADER), false, () => { });
			context.AddSeparator(string.Empty);
			foreach (BussStyleSheet styleSheet in bussStyleSheets)
			{
				context.AddItem(new GUIContent(styleSheet.name), false, () =>
				{
					NewStyleSheetWindow window = NewStyleSheetWindow.ShowWindow();
					if (window != null)
					{
						window.Init(styleSheet.Styles);
					}
				});
			}

			context.ShowAsContext();
		}

		private void Traverse(GameObject gameObject, int currentLevel)
		{
			if (!gameObject) return;

			BussElement foundComponent = gameObject.GetComponent<BussElement>();

			if (foundComponent != null)
			{
				FoundElements.Add(foundComponent, currentLevel);
				OnObjectRegistered(foundComponent);

				foreach (Transform child in gameObject.transform)
				{
					Traverse(child.gameObject, currentLevel + 1);
				}
			}
			else
			{
				foreach (Transform child in gameObject.transform)
				{
					Traverse(child.gameObject, currentLevel);
				}
			}
		}
	}
}
