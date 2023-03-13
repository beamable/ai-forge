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
	public class LabeledDatePickerVisualElement : BeamableVisualElement
	{
		public new class UxmlFactory : UxmlFactory<LabeledDatePickerVisualElement, UxmlTraits>
		{
		}

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			readonly UxmlStringAttributeDescription _label = new UxmlStringAttributeDescription
			{ name = "label", defaultValue = "Label" };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				if (ve is LabeledDatePickerVisualElement component)
				{
					component.Label = _label.GetValueFromBag(bag, cc);
				}
			}
		}

		public event Action OnValueChanged;
		private Label _label;

		public DatePickerVisualElement DatePicker { get; private set; }
		public string Label { get; private set; }
		public string SelectedDate => DatePicker.GetIsoDate();

		public LabeledDatePickerVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(LabeledDatePickerVisualElement)}/{nameof(LabeledDatePickerVisualElement)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			_label = Root.Q<Label>("label");
			_label.text = Label;

			DatePicker = Root.Q<DatePickerVisualElement>("datePicker");
			DatePicker.Setup(OnDateChanged);
			DatePicker.Refresh();
		}

		public void Set(DateTime date) => DatePicker.Set(date);

		private void OnDateChanged()
		{
			OnValueChanged?.Invoke();
		}
	}
}
