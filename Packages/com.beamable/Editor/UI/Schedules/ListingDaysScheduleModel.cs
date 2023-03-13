using Beamable.Common.Content;
using Beamable.Editor.UI.Components;
using Beamable.Editor.UI.Validation;
using System;

namespace Beamable.Editor.Models.Schedules
{
	public class ListingDaysScheduleModel : ScheduleWindowModel
	{
		private readonly LabeledTextField _descriptionComponent;
		private readonly LabeledDaysPickerVisualElement _daysPickerComponent;
		private readonly LabeledCheckboxVisualElement _neverExpiresComponent;
		private readonly LabeledDatePickerVisualElement _activeToDateComponent;
		private readonly LabeledHourPickerVisualElement _activeToHourComponent;
		private readonly LabeledCheckboxVisualElement _allDayComponent;
		private readonly LabeledHourPickerVisualElement _periodFromHourComponent;
		private readonly LabeledHourPickerVisualElement _periodToHourComponent;

		public ListingDaysScheduleModel(LabeledTextField descriptionComponent,
			LabeledDaysPickerVisualElement daysPickerComponent, LabeledCheckboxVisualElement neverExpiresComponent,
			LabeledDatePickerVisualElement activeToDateComponent, LabeledHourPickerVisualElement activeToHourComponent,
			LabeledCheckboxVisualElement allDayComponent, LabeledHourPickerVisualElement periodFromHourComponent,
			LabeledHourPickerVisualElement periodToHourComponent, Action<bool, string> refreshConfirmButton)
		{
			_descriptionComponent = descriptionComponent;
			_daysPickerComponent = daysPickerComponent;
			_neverExpiresComponent = neverExpiresComponent;
			_activeToDateComponent = activeToDateComponent;
			_activeToHourComponent = activeToHourComponent;
			_allDayComponent = allDayComponent;
			_periodFromHourComponent = periodFromHourComponent;
			_periodToHourComponent = periodToHourComponent;

			Validator = new ComponentsValidator(refreshConfirmButton);
			Validator.RegisterRule(new AtLeastOneDaySelectedRule(_daysPickerComponent.Label),
				_daysPickerComponent);
			Validator.RegisterRule(new NotAllDaysSelectedRule(_daysPickerComponent.Label),
				_daysPickerComponent);
		}

		public override WindowMode Mode => WindowMode.Days;

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
				ScheduleParser.PrepareListingDaysModeData(newSchedule, fromHour, toHour, fromMinute,
					toMinute, _daysPickerComponent.DaysPicker.GetSelectedDays());
			}
			else
			{
				ScheduleParser.PrepareDaysModeData(newSchedule, "*", "*",
					"*", _daysPickerComponent.DaysPicker.GetSelectedDays());
			}

			return newSchedule;
		}
	}
}
