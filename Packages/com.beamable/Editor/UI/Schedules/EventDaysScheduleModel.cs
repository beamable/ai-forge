using Beamable.Common.Content;
using Beamable.Editor.UI.Components;
using Beamable.Editor.UI.Validation;
using System;

namespace Beamable.Editor.Models.Schedules
{
	public class EventDaysScheduleModel : ScheduleWindowModel
	{
		public override WindowMode Mode => WindowMode.Days;

		private readonly LabeledTextField _descriptionComponent;
		private readonly LabeledHourPickerVisualElement _startTimeComponent;
		private readonly LabeledDaysPickerVisualElement _daysComponent;
		private readonly LabeledCheckboxVisualElement _neverExpiresComponent;
		private readonly LabeledDatePickerVisualElement _activeToDateComponent;
		private readonly LabeledHourPickerVisualElement _activeToHourComponent;

		public EventDaysScheduleModel(LabeledTextField descriptionComponent,
			LabeledHourPickerVisualElement startTimeComponent,
			LabeledDaysPickerVisualElement daysComponent, LabeledCheckboxVisualElement neverExpiresComponent,
			LabeledDatePickerVisualElement activeToDateComponent, LabeledHourPickerVisualElement activeToHourComponent,
			Action<bool, string> refreshConfirmButtonCallback)
		{
			_descriptionComponent = descriptionComponent;
			_startTimeComponent = startTimeComponent;
			_daysComponent = daysComponent;
			_neverExpiresComponent = neverExpiresComponent;
			_activeToDateComponent = activeToDateComponent;
			_activeToHourComponent = activeToHourComponent;

			Validator = new ComponentsValidator(refreshConfirmButtonCallback);
			Validator.RegisterRule(new AtLeastOneDaySelectedRule(_daysComponent.Label), _daysComponent);
		}

		public override Schedule GetSchedule()
		{
			Schedule newSchedule = new Schedule();

			ScheduleParser.PrepareGeneralData(newSchedule, _descriptionComponent.Value, _neverExpiresComponent.Value,
				$"{_activeToDateComponent.SelectedDate}{_activeToHourComponent.SelectedHour}");

			ScheduleParser.PrepareDaysModeData(newSchedule, _startTimeComponent.Hour, _startTimeComponent.Minute,
				_startTimeComponent.Second, _daysComponent.DaysPicker.GetSelectedDays());

			return newSchedule;
		}
	}
}
