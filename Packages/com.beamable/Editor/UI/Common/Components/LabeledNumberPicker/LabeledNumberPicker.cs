using Beamable.Editor.UI.Buss;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants;

namespace Beamable.Editor.UI.Components
{
	public class LabeledNumberPicker : BeamableVisualElement
	{

		public new class UxmlFactory : UxmlFactory<LabeledNumberPicker, UxmlTraits>
		{
		}

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			readonly UxmlStringAttributeDescription _label = new UxmlStringAttributeDescription
			{ name = "label", defaultValue = "Label" };

			private readonly UxmlIntAttributeDescription _minValue = new UxmlIntAttributeDescription
			{ name = "min", defaultValue = Int32.MinValue };

			private readonly UxmlIntAttributeDescription _maxValue = new UxmlIntAttributeDescription
			{ name = "max", defaultValue = Int32.MaxValue };
			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				if (ve is LabeledNumberPicker component)
				{
					component.Label = _label.GetValueFromBag(bag, cc);
					component.MinValue = _minValue.GetValueFromBag(bag, cc);
					component.MaxValue = _maxValue.GetValueFromBag(bag, cc);
				}
			}
		}

		private LabeledIntegerField _labeledIntegerFieldComponent;
		private ContextualMenuManipulator _contextualMenuManipulator;
		private Button _button;
		private List<string> _options;
		private int _maxVisibleOptions;
		private int _startPos;
		private Action _onValueChanged;
		private Vector2 _cachedPos;

		public string Value => _labeledIntegerFieldComponent.Value.ToString();
		private int MinValue { get; set; }
		private int MaxValue { get; set; }
		private string Label { get; set; }

		public LabeledNumberPicker() : base($"{Directories.COMMON_COMPONENTS_PATH}/{nameof(LabeledNumberPicker)}/{nameof(LabeledNumberPicker)}")
		{
			_options = new List<string>();
		}

		public override void Refresh()
		{
			base.Refresh();

			_labeledIntegerFieldComponent = Root.Q<LabeledIntegerField>("labelAndValue");
			_labeledIntegerFieldComponent.Setup(Label, Int32.Parse(Value), _onValueChanged, MinValue, MaxValue);

			_button = Root.Q<Button>("button");

			ConfigureOptions();
		}

		public void Setup(Action onValueChanged, List<string> options, bool active = true, int maxVisibleOptions = 10)
		{
			_onValueChanged = onValueChanged;
			SetEnabled(active);
			_options = options;
			_maxVisibleOptions = maxVisibleOptions;
			_startPos = 0;
		}

		public void SetupMinMax(int min, int max)
		{
			MinValue = min;
			MaxValue = max;
		}

		private void ConfigureOptions()
		{
			_button.clickable.clicked += ShowContext;

			if (_options != null && _options.Count > 0)
			{
				SetOption(_options[_startPos]);
			}
		}

		private void ShowContext()
		{
			var menu = new GenericMenu();

			if (_maxVisibleOptions > 0)
			{
				int pageStartPos = _startPos;
				int pageEndPos = Mathf.Clamp(pageStartPos + _maxVisibleOptions, 0, _options.Count);

				if (pageStartPos > 0)
				{
					menu.AddItem(new GUIContent(MenuItems.Icons.ARROW_UP_UTF.ToString()), false, () =>
					{
						_startPos = Mathf.Clamp(_startPos - 1, 0, _options.Count);
						menu = null;
						EditorApplication.delayCall += () => ShowContext();
					});
				}

				for (int i = pageStartPos; i < pageEndPos; i++)
				{
					int cachedIndexForCallback = i;
					menu.AddItem(new GUIContent(_options[cachedIndexForCallback]), false, () =>
					{
						SetOption(_options[cachedIndexForCallback]);
					});
				}

				if (pageStartPos < _options.Count - _maxVisibleOptions)
				{
					menu.AddItem(new GUIContent(MenuItems.Icons.ARROW_DOWN_UTF.ToString()), false, () =>
					{
						_startPos = Mathf.Clamp(_startPos + 1, 0, _options.Count);
						menu = null;
						EditorApplication.delayCall += () => ShowContext();
					});
				}

				Vector2 pp = GUIUtility.GUIToScreenPoint(Vector2.zero);

				if (pp != Vector2.zero)
					_cachedPos = pp;

				// dropdown has a bug with position when we want to draw generic menu from generic menu for refresh, that workaround works ok

				menu.DropDown(
					new Rect(
						GUIUtility.ScreenToGUIPoint(_cachedPos + new Vector2(_button.worldBound.xMin, _button.worldBound.yMin)),
						Vector2.zero));
			}
			else
			{
				foreach (var t in _options)
				{
					menu.AddItem(new GUIContent(t), false, () =>
					{
						SetOption(t);
					});
				}

				menu.DropDown(_button.worldBound);
			}
		}

		private void SetOption(string value)
		{
			_labeledIntegerFieldComponent.Value = Int32.Parse(value);
			_labeledIntegerFieldComponent.Refresh();
		}

		public void Set(string option) => SetOption(option);
	}
}
