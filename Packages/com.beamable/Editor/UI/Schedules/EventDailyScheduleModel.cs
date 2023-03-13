using Beamable.Common.Content;
using Beamable.Editor.UI.Components;
using Beamable.Editor.UI.Validation;
using System;

namespace Beamable.Editor.Models.Schedules
{
	public class EventDailyScheduleModel : ScheduleWindowModel
	{
		public override WindowMode Mode => WindowMode.Daily;

		private readonly LabeledTextField _descriptionComponent;
		private readonly LabeledHourPickerVisualElement _startTimeComponent;
		private readonly LabeledCheckboxVisualElement _neverExpiresComponent;
		private readonly LabeledDatePickerVisualElement _activeToDateComponent;
		private readonly LabeledHourPickerVisualElement _activeToHourComponent;

		public EventDailyScheduleModel(LabeledTextField descriptionComponent, LabeledHourPickerVisualElement startTimeComponent,
			LabeledCheckboxVisualElement neverExpiresComponent, LabeledDatePickerVisualElement activeToDateComponent,
			LabeledHourPickerVisualElement activeToHourComponent, Action<bool, string> refreshConfirmButtonCallback)
		{
			_descriptionComponent = descriptionComponent;
			_startTimeComponent = startTimeComponent;
			_neverExpiresComponent = neverExpiresComponent;
			_activeToDateComponent = activeToDateComponent;
			_activeToHourComponent = activeToHourComponent;

			Validator = new ComponentsValidator(refreshConfirmButtonCallback);
		}

		public override Schedule GetSchedule()
		{
			Schedule newSchedule = new Schedule();

			ScheduleParser.PrepareGeneralData(newSchedule, _descriptionComponent.Value, _neverExpiresComponent.Value,
				$"{_activeToDateComponent.SelectedDate}{_activeToHourComponent.SelectedHour}");
			ScheduleParser.PrepareDailyModeData(newSchedule, _startTimeComponent.Hour, _startTimeComponent.Minute, _startTimeComponent.Second);

			return newSchedule;
		}
	}
}
