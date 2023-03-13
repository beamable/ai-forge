using Beamable.Editor.UI.Validation;
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
	public class LabeledCalendarVisualElement : ValidableVisualElement<int>
	{
		public new class UxmlFactory : UxmlFactory<LabeledCalendarVisualElement, UxmlTraits>
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
				if (ve is LabeledCalendarVisualElement component)
				{
					component.Label = _label.GetValueFromBag(bag, cc);
				}
			}
		}

		private Label _label;
		private CalendarVisualElement _calendar;

		public string Label { get; private set; }
		public List<string> SelectedDays => _calendar.SelectedDays;
		public CalendarVisualElement Calendar => _calendar;

		public LabeledCalendarVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(LabeledCalendarVisualElement)}/{nameof(LabeledCalendarVisualElement)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			_label = Root.Q<Label>("label");
			_label.text = Label;

			_calendar = Root.Q<CalendarVisualElement>("calendar");
			_calendar.OnValueChanged = OnChanged;
			_calendar.Refresh();
		}

		private void OnChanged(List<string> options)
		{
			InvokeValidationCheck(options.Count);
		}
	}
}
