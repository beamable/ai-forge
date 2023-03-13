using Beamable.Editor.UI.Validation;
using System;
using System.Collections.Generic;
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
	public class LabeledTextField : ValidableVisualElement<string>
	{
		public new class UxmlFactory : UxmlFactory<LabeledTextField, UxmlTraits> { }

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			readonly UxmlStringAttributeDescription _label = new UxmlStringAttributeDescription
			{
				name = "label",
				defaultValue = "Label"
			};

			readonly UxmlStringAttributeDescription _value = new UxmlStringAttributeDescription { name = "value" };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				if (ve is LabeledTextField component)
				{
					component.Label = _label.GetValueFromBag(bag, cc);
					component.Value = _value.GetValueFromBag(bag, cc);
				}
			}
		}

		public TextField TextFieldComponent { get; private set; }

		private Label _labelComponent;
		private Action<string> _onValueChanged;
		private string _value;

		public string Value
		{
			get => _value;
			set
			{
				_value = value;
				TextFieldComponent?.SetValueWithoutNotify(_value);
				_onValueChanged?.Invoke(_value);
			}
		}

		private string Label { get; set; }
		public bool IsDelayed { get; set; }
		public bool IsMultiline { get; set; }

		public LabeledTextField() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(LabeledTextField)}/{nameof(LabeledTextField)}")
		{ }

		public void OverrideLabelWidth(float width)
		{
			_labelComponent?.style.SetWidth(width);
		}

		public override void Refresh()
		{
			base.Refresh();

			_labelComponent = Root.Q<Label>("label");
			_labelComponent.text = Label;

			TextFieldComponent = Root.Q<TextField>("textField");
			TextFieldComponent.value = Value;
			TextFieldComponent.isDelayed = IsDelayed;
			TextFieldComponent.multiline = IsMultiline;
			TextFieldComponent.RegisterValueChangedCallback(ValueChanged);
		}

		public void Setup(string label,
						  string value,
						  Action<string> onValueChanged,
						  bool isDelayed = false,
						  bool isMultiline = false)
		{
			Label = label;
			Value = value;
			IsDelayed = isDelayed;
			IsMultiline = isMultiline;
			_onValueChanged = onValueChanged;
		}

		public void SetWithoutNotify(string value)
		{
			_value = value;
			TextFieldComponent?.SetValueWithoutNotify(value);
		}

		protected override void OnDestroy()
		{
			TextFieldComponent.UnregisterValueChangedCallback(ValueChanged);
		}

		private void ValueChanged(ChangeEvent<string> evt)
		{
			Value = evt.newValue;
			InvokeValidationCheck(Value);
		}
	}
}
