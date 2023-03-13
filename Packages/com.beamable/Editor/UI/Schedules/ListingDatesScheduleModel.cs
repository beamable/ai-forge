using Beamable.Common.Content;
using Beamable.Editor.UI.Components;
using Beamable.Editor.UI.Validation;
using System;
using System.Collections.Generic;

namespace Beamable.Editor.Models.Schedules
{
	public class ListingDatesScheduleModel : ScheduleWindowModel
	{
		private readonly LabeledTextField _descriptionComponent;
		private readonly LabeledCalendarVisualElement _calendarComponent;
		private readonly LabeledCheckboxVisualElement _neverExpiresComponent;
		private readonly LabeledDatePickerVisualElement _activeToDateComponent;
		private readonly LabeledHourPickerVisualElement _activeToHourComponent;
		private readonly LabeledCheckboxVisualElement _allDayComponent;
		private readonly LabeledHourPickerVisualElement _periodFromHourComponent;
		private readonly LabeledHourPickerVisualElement _periodToHourComponent;

		private Action<bool, string> _refreshConfirmButtonCallback;

		public ListingDatesScheduleModel(LabeledTextField descriptionComponent,
			LabeledCalendarVisualElement calendarComponent, LabeledCheckboxVisualElement neverExpiresComponent,
			LabeledDatePickerVisualElement activeToDateComponent, LabeledHourPickerVisualElement activeToHourComponent,
			LabeledCheckboxVisualElement allDayComponent, LabeledHourPickerVisualElement periodFromHourComponent,
			LabeledHourPickerVisualElement periodToHourComponent, Action<bool, string> refreshConfirmButton)
		{
			_descriptionComponent = descriptionComponent;
			_calendarComponent = calendarComponent;
			_neverExpiresComponent = neverExpiresComponent;
			_activeToDateComponent = activeToDateComponent;
			_activeToHourComponent = activeToHourComponent;
			_allDayComponent = allDayComponent;
			_periodFromHourComponent = periodFromHourComponent;
			_periodToHourComponent = periodToHourComponent;

			_refreshConfirmButtonCallback = refreshConfirmButton;

			Validator = new ComponentsValidator(refreshConfirmButton);
			Validator.RegisterRule(new AtLeastOneDaySelectedRule(_calendarComponent.Label), _calendarComponent);

			_calendarComponent.Calendar.OnValueChanged -= AdditionalValidation;
			_calendarComponent.Calendar.OnValueChanged += AdditionalValidation;
		}

		public override WindowMode Mode => WindowMode.Dates;

		public override Schedule GetSchedule()
		{
			Schedule newSchedule = new Schedule();

			ScheduleParser.PrepareGeneralData(newSchedule, _descriptionComponent.Value, _neverExpiresComponent.Value,
				$"{_activeToDateComponent.SelectedDate}{_activeToHourComponent.SelectedHour}");

			int fromHour = 0;
			int toHour = 0;
			int fromMinute = 0;
			int toMinute = 0;

			if (!_allDayComponent.Value)
			{
				fromHour = int.Parse(_periodFromHourComponent.Hour);
				toHour = int.Parse(_periodToHourComponent.Hour);
				fromMinute = int.Parse(_periodFromHourComponent.Minute);
				toMinute = int.Parse(_periodToHourComponent.Minute);
			}

			if (!_allDayComponent.Value)
			{
				ScheduleParser.PrepareListingDatesModeData(newSchedule, fromHour, toHour, fromMinute, toMinute,
					_calendarComponent.SelectedDays);
			}
			else
			{
				ScheduleParser.PrepareDateModeData(newSchedule, "*", "*", "*",
					_calendarComponent.SelectedDays);
			}

			return newSchedule;
		}

		private void AdditionalValidation(List<string> selectedDays)
		{
			AdditionalScheduleValidation.ValidatePastDates(selectedDays, _refreshConfirmButtonCallback, out var isValid);
			if (!isValid)
				return;
			Validator.ForceValidationCheck();
		}
	}
}
