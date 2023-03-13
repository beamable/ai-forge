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
	public class LabeledHourPickerVisualElement : ValidableVisualElement<string>
	{
		public new class UxmlFactory : UxmlFactory<LabeledHourPickerVisualElement, UxmlTraits>
		{
		}

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			readonly UxmlStringAttributeDescription _label = new UxmlStringAttributeDescription
			{ name = "label", defaultValue = "Label" };

			readonly UxmlBoolAttributeDescription _hour = new UxmlBoolAttributeDescription
			{ name = "hour", defaultValue = true };

			readonly UxmlBoolAttributeDescription _minute = new UxmlBoolAttributeDescription
			{ name = "minute", defaultValue = true };

			readonly UxmlBoolAttributeDescription _second = new UxmlBoolAttributeDescription
			{ name = "second", defaultValue = true };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				if (ve is LabeledHourPickerVisualElement component)
				{
					component.Label = _label.GetValueFromBag(bag, cc);
					component.ActiveHour = _hour.GetValueFromBag(bag, cc);
					component.ActiveMinute = _minute.GetValueFromBag(bag, cc);
					component.ActiveSecond = _second.GetValueFromBag(bag, cc);
				}
			}
		}

		public Action OnValueChanged;

		private Label _label;
		private HourPickerVisualElement _hourPicker;

		private bool ActiveHour { get; set; }
		private bool ActiveMinute { get; set; }
		private bool ActiveSecond { get; set; }
		public string Label { get; private set; }
		public string SelectedHour => _hourPicker.GetFullHour();
		public string Hour => _hourPicker.Hour;
		public string Minute => _hourPicker.Minute;
		public string Second => _hourPicker.Second;

		public LabeledHourPickerVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(LabeledHourPickerVisualElement)}/{nameof(LabeledHourPickerVisualElement)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			_label = Root.Q<Label>("label");
			_label.text = Label;

			_hourPicker = Root.Q<HourPickerVisualElement>("hourPicker");
			_hourPicker.Setup(OnHourChanged, ActiveHour, ActiveMinute, ActiveSecond);
			_hourPicker.Refresh();
		}

		public void Set(DateTime date) => _hourPicker.Set(date);

		public void SetGroupEnabled(bool b)
		{
			SetEnabled(b);
			_hourPicker.SetGroupEnabled(b);
		}

		private void OnHourChanged()
		{
			OnValueChanged?.Invoke();
		}
	}
}
