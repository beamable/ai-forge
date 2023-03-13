using Beamable.Common.Content;
using Beamable.Editor.Models.Schedules;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants.Features.Schedules;

namespace Beamable.Editor.UI.Components
{
	public class EventScheduleWindow : BeamableVisualElement, IScheduleWindow<EventContent>
	{
		public event Action OnCancelled;
		public event Action<Schedule> OnScheduleUpdated;

		private LabeledTextField _eventNameComponent;
		private LabeledTextField _descriptionComponent;
		private LabeledHourPickerVisualElement _startTimeComponent;
		private LabeledDatePickerVisualElement _activeToDateComponent;
		private LabeledHourPickerVisualElement _activeToHourComponent;
		private LabeledDropdownVisualElement _dropdownComponent;
		private LabeledCheckboxVisualElement _neverExpiresComponent;
		private LabeledDaysPickerVisualElement _daysPickerComponent;
		private LabeledCalendarVisualElement _calendarComponent;

		private VisualElement _daysGroup;
		private VisualElement _datesGroup;
		private PrimaryButtonVisualElement _confirmButton;
		private GenericButtonVisualElement _cancelButton;

		private readonly List<ScheduleWindowModel> _models = new List<ScheduleWindowModel>();
		private ScheduleWindowModel _currentModel;

		#region Tests related properties and methods

		public LabeledHourPickerVisualElement StartTimeComponent => _startTimeComponent;
		public LabeledDatePickerVisualElement ActiveToDateComponent => _activeToDateComponent;
		public LabeledHourPickerVisualElement ActiveToHourComponent => _activeToHourComponent;
		public LabeledCheckboxVisualElement NeverExpiresComponent => _neverExpiresComponent;
		public LabeledDropdownVisualElement ModeComponent => _dropdownComponent;
		public LabeledDaysPickerVisualElement DaysComponent => _daysPickerComponent;
		public LabeledCalendarVisualElement CalendarComponent => _calendarComponent;

		public void InvokeTestConfirm() => ConfirmClicked();

		#endregion

		public EventScheduleWindow() : base(
			$"{SCHEDULES_PATH}/{nameof(EventScheduleWindow)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			_eventNameComponent = Root.Q<LabeledTextField>("eventName");
			_eventNameComponent.Refresh();

			_descriptionComponent = Root.Q<LabeledTextField>("description");
			_descriptionComponent.Refresh();

			_startTimeComponent = Root.Q<LabeledHourPickerVisualElement>("startTime");
			_startTimeComponent.Refresh();

			_neverExpiresComponent = Root.Q<LabeledCheckboxVisualElement>("expiresNever");
			_neverExpiresComponent.OnValueChanged += OnExpirationChanged;
			_neverExpiresComponent.Refresh();
			_neverExpiresComponent.DisableIcon();

			_activeToDateComponent = Root.Q<LabeledDatePickerVisualElement>("activeToDate");
			_activeToDateComponent.Refresh();

			_activeToHourComponent = Root.Q<LabeledHourPickerVisualElement>("activeToHour");
			_activeToHourComponent.Refresh();

			// Days mode
			_daysPickerComponent = Root.Q<LabeledDaysPickerVisualElement>("daysPicker");
			_daysPickerComponent.Refresh();

			// Date mode
			_calendarComponent = Root.Q<LabeledCalendarVisualElement>("calendar");
			_calendarComponent.Refresh();

			// Buttons
			_confirmButton = Root.Q<PrimaryButtonVisualElement>("confirmBtn");
			_confirmButton.Button.clickable.clicked += ConfirmClicked;
			_confirmButton.Disable();

			_cancelButton = Root.Q<GenericButtonVisualElement>("cancelBtn");
			_cancelButton.OnClick += CancelClicked;

			// Groups
			_daysGroup = Root.Q<VisualElement>("daysGroup");
			_datesGroup = Root.Q<VisualElement>("datesGroup");

			SetupModels();

			_dropdownComponent = Root.Q<LabeledDropdownVisualElement>("dropdown");
			_dropdownComponent.Setup(PrepareOptions(), OnModeChanged);
			_dropdownComponent.Refresh();

			RefreshGroups();
			OnExpirationChanged(_neverExpiresComponent.Value);

			EditorApplication.delayCall += () => { _currentModel.ForceValidationCheck(); };
		}

		private void SetupModels()
		{
			EventDailyScheduleModel dailyModel = new EventDailyScheduleModel(_descriptionComponent, _startTimeComponent,
				_neverExpiresComponent, _activeToDateComponent, _activeToHourComponent, RefreshConfirmButton);

			EventDaysScheduleModel daysModel = new EventDaysScheduleModel(_descriptionComponent, _startTimeComponent,
				_daysPickerComponent, _neverExpiresComponent, _activeToDateComponent, _activeToHourComponent,
				RefreshConfirmButton);

			EventDatesScheduleModel datesModel = new EventDatesScheduleModel(_descriptionComponent, _startTimeComponent,
				_calendarComponent, _neverExpiresComponent, _activeToDateComponent, _activeToHourComponent,
				RefreshConfirmButton);

			_models.Clear();
			_models.Add(dailyModel);
			_models.Add(daysModel);
			_models.Add(datesModel);
		}

		private void RefreshConfirmButton(bool value, string message)
		{
			if (value)
			{
				_confirmButton.Enable();
				_confirmButton.tooltip = string.Empty;
			}
			else
			{
				_confirmButton.Disable();
				_confirmButton.tooltip = message;
			}
		}

		public void Set(Schedule schedule, EventContent content)
		{
			_descriptionComponent.Value = schedule.description;

			_eventNameComponent.SetEnabled(false);
			_eventNameComponent.Value = content.name;

			var neverExpires = !schedule.activeTo.HasValue;
			if (!neverExpires && schedule.TryGetActiveTo(out var activeToDate))
			{
				_activeToDateComponent.Set(activeToDate);
				_activeToHourComponent.Set(activeToDate);
			}

			_neverExpiresComponent.Value = neverExpires;
			var date = content.startDate.ParseEventStartDate(out var _);
			_startTimeComponent.Set(date);

			var explicitDates = schedule.definitions.Any(definition => definition.dayOfMonth.Any(day => day != "*"));
			var hasDaysOfWeek = schedule.definitions.Any(definition => definition.dayOfWeek.Any(day => day != "*"));
			if (explicitDates)
			{
				_dropdownComponent.Set(2);
				_calendarComponent.Calendar.SetInitialValues(schedule.definitions);
			}
			else if (hasDaysOfWeek)
			{
				_dropdownComponent.Set(1);
				_daysPickerComponent.SetSelectedDays(schedule.definitions[0].dayOfWeek);
			}
		}

		public void ApplyDataTransforms(EventContent data)
		{
			data.name = _eventNameComponent.Value;
		}

		protected override void OnDestroy()
		{
			if (_neverExpiresComponent == null) return;
			_neverExpiresComponent.OnValueChanged -= OnExpirationChanged;
		}

		private void OnExpirationChanged(bool value)
		{
			_activeToDateComponent.SetEnabled(!value);
			_activeToHourComponent.SetEnabled(!value);
		}

		private void ConfirmClicked()
		{
			OnScheduleUpdated?.Invoke(_currentModel.GetSchedule());
		}

		private void CancelClicked()
		{
			OnCancelled?.Invoke();
		}

		private void RefreshGroups()
		{
			RefreshSingleGroup(_daysGroup, ScheduleWindowModel.WindowMode.Days);
			RefreshSingleGroup(_datesGroup, ScheduleWindowModel.WindowMode.Dates);
		}

		private void RefreshSingleGroup(VisualElement ve, ScheduleWindowModel.WindowMode mode)
		{
			if (ve == null) return;
			ve.visible = _currentModel.Mode == mode;
			ve.EnableInClassList("--positionHidden", !ve.visible);
		}

		private void OnModeChanged(int option)
		{
			_currentModel = _models[option];

			if (_currentModel?.Mode == ScheduleWindowModel.WindowMode.Dates)
				_calendarComponent.Calendar.SetDefaultValues();

			_currentModel.ForceValidationCheck();
			RefreshGroups();
		}

		private List<string> PrepareOptions()
		{
			return _models.Select(model => model.Mode.ToString()).ToList();
		}
	}
}
