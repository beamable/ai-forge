using Beamable.Common.Content;
using Beamable.Editor.UI.Components;
using Beamable.Editor.UI.Validation;
using System;
using System.Collections.Generic;

namespace Beamable.Editor.Models.Schedules
{
	public class EventDatesScheduleModel : ScheduleWindowModel
	{
		private readonly LabeledTextField _descriptionComponent;
		private readonly LabeledHourPickerVisualElement _startTimeComponent;
		private readonly LabeledCalendarVisualElement _calendarComponent;
		private readonly LabeledCheckboxVisualElement _neverExpiresComponent;
		private readonly LabeledDatePickerVisualElement _activeToDateComponent;
		private readonly LabeledHourPickerVisualElement _activeToHourComponent;

		public override WindowMode Mode => WindowMode.Dates;

		private Action<bool, string> _refreshConfirmButtonCallback;

		public EventDatesScheduleModel(LabeledTextField descriptionComponent,
			LabeledHourPickerVisualElement startTimeComponent,
			LabeledCalendarVisualElement calendarComponent, LabeledCheckboxVisualElement neverExpiresComponent,
			LabeledDatePickerVisualElement activeToDateComponent, LabeledHourPickerVisualElement activeToHourComponent,
			Action<bool, string> refreshConfirmButtonCallback)
		{
			_descriptionComponent = descriptionComponent;
			_startTimeComponent = startTimeComponent;
			_calendarComponent = calendarComponent;
			_neverExpiresComponent = neverExpiresComponent;
			_activeToDateComponent = activeToDateComponent;
			_activeToHourComponent = activeToHourComponent;

			_refreshConfirmButtonCallback = refreshConfirmButtonCallback;

			Validator = new ComponentsValidator(refreshConfirmButtonCallback);
			Validator.RegisterRule(new AtLeastOneDaySelectedRule(_calendarComponent.Label), _calendarComponent);

			_calendarComponent.Calendar.OnValueChanged -= AdditionalValidation;
			_calendarComponent.Calendar.OnValueChanged += AdditionalValidation;
		}

		public override Schedule GetSchedule()
		{
			Schedule newSchedule = new Schedule();

			ScheduleParser.PrepareGeneralData(newSchedule, _descriptionComponent.Value, _neverExpiresComponent.Value,
				$"{_activeToDateComponent.SelectedDate}{_activeToHourComponent.SelectedHour}");
			ScheduleParser.PrepareDateModeData(newSchedule, _startTimeComponent.Hour, _startTimeComponent.Minute,
				_startTimeComponent.Second,
				_calendarComponent.SelectedDays);

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
