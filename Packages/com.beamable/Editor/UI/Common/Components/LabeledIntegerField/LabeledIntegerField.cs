using Beamable.Editor.UI.Validation;
using System;
using System.Collections.Generic;
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
	public class LabeledIntegerField : ValidableVisualElement<int>
	{
		public new class UxmlFactory : UxmlFactory<LabeledIntegerField, UxmlTraits>
		{
		}

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			readonly UxmlStringAttributeDescription _label = new UxmlStringAttributeDescription
			{ name = "label", defaultValue = "Label" };

			readonly UxmlIntAttributeDescription _value = new UxmlIntAttributeDescription
			{ name = "value" };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				if (ve is LabeledIntegerField component)
				{
					component.Label = _label.GetValueFromBag(bag, cc);
					component.Value = _value.GetValueFromBag(bag, cc);
				}
			}
		}


		private Action _onValueChanged;
		private Label _labelComponent;
		private IntegerField _integerFieldComponent;
		private int _value;
		private int _minValue;
		private int _maxValue;
		private VisualElement _mainElement;

		public int Value
		{
			get => _value;
			set
			{
				int tempValue = Mathf.Clamp(value, _minValue, _maxValue);
				_value = tempValue;
				_integerFieldComponent?.SetValueWithoutNotify(_value);
				_onValueChanged?.Invoke();
			}
		}

		private string Label { get; set; }

		public LabeledIntegerField() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(LabeledIntegerField)}/{nameof(LabeledIntegerField)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			_mainElement = Root.Q<VisualElement>("mainVisualElement");

			_labelComponent = new Label(Label);
			_labelComponent.name = "label";
			_mainElement.Add(_labelComponent);

			_integerFieldComponent = new IntegerField();
			_integerFieldComponent.name = "integerField";
			_integerFieldComponent.value = Value;
			_integerFieldComponent.RegisterValueChangedCallback(ValueChanged);
			_mainElement.Add(_integerFieldComponent);
		}

		public void Setup(string label, int value, Action onValueChanged, int minValue, int maxValue)
		{
			_minValue = minValue;
			_maxValue = maxValue;
			Label = label;
			Value = value;
			_onValueChanged = onValueChanged;

			Refresh();
		}

		protected override void OnDestroy()
		{
			_integerFieldComponent.UnregisterValueChangedCallback(ValueChanged);
		}

		private void ValueChanged(ChangeEvent<int> evt)
		{
			Value = evt.newValue;
			InvokeValidationCheck(Value);
		}
	}
}
