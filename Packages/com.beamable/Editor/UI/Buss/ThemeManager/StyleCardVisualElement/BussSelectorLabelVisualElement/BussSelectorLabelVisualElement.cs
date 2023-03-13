using Beamable.Editor.Common;
using Beamable.Editor.UI.Common;
using Beamable.UI.Buss;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants.Features.Buss.ThemeManager;

namespace Beamable.Editor.UI.Components
{
	public class BussSelectorLabelVisualElement : BeamableBasicVisualElement
	{
		private TextField _editableLabel;
		private TextElement _styleSheetLabel;
		private BussStyleRule _styleRule;
		private BussStyleSheet _styleSheet;

		private Func<List<GenericMenuCommand>> _refreshCommands;
		private Action<BussStyleRule, BussStyleSheet> _onSelectorChanged;

		public BussSelectorLabelVisualElement() : base(
			$"{BUSS_THEME_MANAGER_PATH}/{nameof(StyleCardVisualElement)}/{nameof(BussSelectorLabelVisualElement)}/{nameof(BussSelectorLabelVisualElement)}.uss")
		{ }

		public void Setup(BussStyleRule styleRule,
						  BussStyleSheet styleSheet,
						  Action<BussStyleRule, BussStyleSheet> onSelectorChanged,
						  Func<List<GenericMenuCommand>> refreshCommands)
		{
			_onSelectorChanged = onSelectorChanged;
			base.Init();

			_styleRule = styleRule;
			_styleSheet = styleSheet;
			_refreshCommands = refreshCommands;

			Refresh();
		}

		private new void Refresh()
		{
			Root.Clear();

			var labelRow = new VisualElement();
			labelRow.name = "labelRow";
			Root.Add(labelRow);

			var inheritedLabel = new Label("(Inherited)");
			inheritedLabel.name = "inheritedLabel";
			labelRow.Add(inheritedLabel);

			var notAppliedLabel = new Label("(Selector does not match current selection)");
			notAppliedLabel.name = "notApplied";
			labelRow.Add(notAppliedLabel);

			var mainRow = new VisualElement();
			Root.Add(mainRow);

#if BEAMABLE_DEVELOPER
			_editableLabel = new TextField();
			_editableLabel.AddToClassList("interactable");
			_editableLabel.value = _styleRule.SelectorString;
			_editableLabel.RegisterValueChangedCallback(StyleIdChanged);
			_editableLabel.RegisterCallback<KeyDownEvent>(KeyboardPressed);
			mainRow.Add(_editableLabel);
#else
			if (_styleSheet.IsReadOnly)
			{
				TextElement styleId = new TextElement();
				styleId.text = _styleRule.SelectorString;
				mainRow.Add(styleId);
			}
			else
			{
				_editableLabel = new TextField();
				if (!_styleSheet.IsReadOnly)
				{
					_editableLabel.AddToClassList("interactable");
				}

				_editableLabel.value = _styleRule.SelectorString;
				_editableLabel.RegisterValueChangedCallback(StyleIdChanged);
				_editableLabel.RegisterCallback<KeyDownEvent>(KeyboardPressed);
				mainRow.Add(_editableLabel);
			}
#endif

			TextElement separator01 = new TextElement { name = "separator", text = "|" };
			mainRow.Add(separator01);

			_styleSheetLabel = new TextElement();
			_styleSheetLabel.AddToClassList("interactable");

			_styleSheetLabel.name = "styleSheetName";
			_styleSheetLabel.text = $"{_styleSheet.name}";
			_styleSheetLabel.RegisterCallback<MouseDownEvent>(OnStyleSheetClicked);
			mainRow.Add(_styleSheetLabel);

			if (_styleSheet.IsReadOnly)
			{
				TextElement separator02 = new TextElement { name = "separator", text = "|" };
				mainRow.Add(separator02);

				TextElement readonlyLabel = new TextElement { text = "readonly" };
				mainRow.Add(readonlyLabel);
			}
		}

		private void KeyboardPressed(KeyDownEvent evt)
		{
			if (evt.keyCode == KeyCode.Return)
			{
				Focus();
			}
		}

		private void OnStyleSheetClicked(MouseDownEvent evt)
		{
			GenericMenu context = new GenericMenu();

			List<GenericMenuCommand> commands = _refreshCommands.Invoke();

			foreach (GenericMenuCommand command in commands)
			{
				GUIContent label = new GUIContent(command.Name);
				context.AddItem(new GUIContent(label), false, () => { command.Invoke(); });
			}

			context.ShowAsContext();
		}

		protected override void OnDestroy()
		{
			_editableLabel.UnregisterValueChangedCallback(StyleIdChanged);
			_styleSheetLabel.UnregisterCallback<MouseDownEvent>(OnStyleSheetClicked);
		}

		private void StyleIdChanged(ChangeEvent<string> evt)
		{
			var newValue = evt.newValue;
			EditorDebouncer.Debounce("buss-set-selector", () =>
			{
				Undo.RecordObject(_styleSheet, $"Change selector");
				_styleRule.SelectorString = newValue;
				_styleSheet.TriggerChange();
				_onSelectorChanged?.Invoke(_styleRule, _styleSheet);
			});

		}
	}
}
