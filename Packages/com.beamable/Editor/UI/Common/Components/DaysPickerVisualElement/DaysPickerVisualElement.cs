using System;
using System.Collections.Generic;
using System.Linq;
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
	public class DaysPickerVisualElement : BeamableVisualElement
	{
		public new class UxmlFactory : UxmlFactory<DaysPickerVisualElement, UxmlTraits>
		{
		}

		public Action<List<string>> OnValueChanged;

		private readonly List<DayToggleVisualElement> _daysToggles = new List<DayToggleVisualElement>();

		private DayToggleVisualElement _mondayToggle;
		private DayToggleVisualElement _tuesdayToggle;
		private DayToggleVisualElement _wednesdayToggle;
		private DayToggleVisualElement _thursdayToggle;
		private DayToggleVisualElement _fridayToggle;
		private DayToggleVisualElement _saturdayToggle;
		private DayToggleVisualElement _sundayToggle;

		public DaysPickerVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(DaysPickerVisualElement)}/{nameof(DaysPickerVisualElement)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			_mondayToggle = Root.Q<DayToggleVisualElement>("mon");
			_mondayToggle.Setup("M", "1");
			_mondayToggle.OnValueChanged = OnChange;
			_daysToggles.Add(_mondayToggle);
			_mondayToggle.Refresh();

			_tuesdayToggle = Root.Q<DayToggleVisualElement>("tue");
			_tuesdayToggle.Setup("T", "2");
			_tuesdayToggle.OnValueChanged = OnChange;
			_daysToggles.Add(_tuesdayToggle);
			_tuesdayToggle.Refresh();

			_wednesdayToggle = Root.Q<DayToggleVisualElement>("wed");
			_wednesdayToggle.Setup("W", "3");
			_wednesdayToggle.OnValueChanged = OnChange;
			_daysToggles.Add(_wednesdayToggle);
			_wednesdayToggle.Refresh();

			_thursdayToggle = Root.Q<DayToggleVisualElement>("thu");
			_thursdayToggle.Setup("T", "4");
			_thursdayToggle.OnValueChanged = OnChange;
			_daysToggles.Add(_thursdayToggle);
			_thursdayToggle.Refresh();

			_fridayToggle = Root.Q<DayToggleVisualElement>("fri");
			_fridayToggle.Setup("F", "5");
			_fridayToggle.OnValueChanged = OnChange;
			_daysToggles.Add(_fridayToggle);
			_fridayToggle.Refresh();

			_saturdayToggle = Root.Q<DayToggleVisualElement>("sat");
			_saturdayToggle.Setup("S", "6");
			_saturdayToggle.OnValueChanged = OnChange;
			_daysToggles.Add(_saturdayToggle);
			_saturdayToggle.Refresh();

			_sundayToggle = Root.Q<DayToggleVisualElement>("sun");
			_sundayToggle.Setup("S", "7");
			_sundayToggle.OnValueChanged = OnChange;
			_daysToggles.Add(_sundayToggle);
			_sundayToggle.Refresh();
		}

		private void OnChange()
		{
			OnValueChanged?.Invoke(GetSelectedDays());
		}

		public List<string> GetSelectedDays()
		{
			List<string> selectedOptions = new List<string>();

			foreach (DayToggleVisualElement toggle in _daysToggles)
			{
				if (toggle.Selected)
				{
					selectedOptions.Add(toggle.Value);
				}
			}

			return selectedOptions;
		}

		public void SetSelectedDays(IEnumerable<string> dayCodes)
		{
			foreach (var dayCode in dayCodes)
			{
				var dayToggle = _daysToggles.FirstOrDefault(toggle => toggle.Value == dayCode);
				if (dayToggle != null)
				{
					dayToggle.Set(true);
				}
				else
				{
					Debug.LogWarning("No Day Toggle found for " + dayCode);
				}
			}

			// Forcing to ensure validation check
			OnValueChanged?.Invoke(GetSelectedDays());
		}
	}
}
