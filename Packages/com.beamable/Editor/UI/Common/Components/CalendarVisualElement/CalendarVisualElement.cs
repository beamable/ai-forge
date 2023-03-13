using Beamable.Common.Content;
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
	public class CalendarVisualElement : BeamableVisualElement
	{
		public new class UxmlFactory : UxmlFactory<CalendarVisualElement, UxmlTraits> { }

		public CalendarVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(CalendarVisualElement)}/{nameof(CalendarVisualElement)}")
		{ }

		public Action<List<string>> OnValueChanged;

		private PreviousNextOptionSelectorVisualElement _yearSelector;
		private PreviousNextOptionSelectorVisualElement _monthSelector;

		private VisualElement _mainVisualElement;
		private readonly List<VisualElement> _dayRows = new List<VisualElement>();
		private readonly List<DayToggleVisualElement> _currentDayToggles = new List<DayToggleVisualElement>();
		private List<string> _selectedDays = new List<string>();

		public List<string> SelectedDays => _selectedDays;

		public override void Refresh()
		{
			base.Refresh();

			_mainVisualElement = Root.Q<VisualElement>("mainVisualElement");

			_yearSelector = Root.Q<PreviousNextOptionSelectorVisualElement>("yearSelector");
			_yearSelector.Setup(GenerateYears(), 0, OnDateChanged);
			_yearSelector.Refresh();

			_monthSelector = Root.Q<PreviousNextOptionSelectorVisualElement>("monthSelector");
			_monthSelector.Setup(GenerateMonths(), 0, OnDateChanged);
			_monthSelector.Refresh();
		}

		private void OnDateChanged()
		{
			if (_yearSelector != null && _monthSelector != null)
			{
				RenderCalendar(_yearSelector.CurrentOption.Key, _monthSelector.CurrentOption.Key);
			}
		}

		private void RenderCalendar(int year, int month)
		{
			foreach (DayToggleVisualElement toggle in _currentDayToggles)
			{
				toggle.RemoveFromHierarchy();
			}

			_currentDayToggles.Clear();

			foreach (VisualElement dayRow in _dayRows)
			{
				dayRow.RemoveFromHierarchy();
			}

			_dayRows.Clear();

			int daysInMonth = DateTime.DaysInMonth(year, month);
			int firstDay = DetermineFirstDayOffset(year, month);
			int necessaryRows = (daysInMonth + firstDay) / 7;

			if ((daysInMonth + firstDay) % 7 > 0)
			{
				necessaryRows++;
			}

			for (int i = 0; i < necessaryRows; i++)
			{
				VisualElement currentRow = new VisualElement();
				currentRow.AddToClassList("row");
				_mainVisualElement.Add(currentRow);
				_dayRows.Add(currentRow);

				for (int j = 0; j < 7; j++)
				{
					DayToggleVisualElement toggle = new DayToggleVisualElement();
					string path =
						$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(DayToggleVisualElement)}/{nameof(DayToggleVisualElement)}.uss";
					toggle.AddStyleSheet(path);
					toggle.AddToClassList("--margin5px");
					currentRow.Add(toggle);
					_currentDayToggles.Add(toggle);
				}
			}

			int specificDayCounter = 0;

			for (int i = 0; i < _currentDayToggles.Count; i++)
			{
				DayToggleVisualElement toggle = _currentDayToggles[i];

				if (i < firstDay)
				{
					toggle.SetInactive();
				}
				else
				{
					specificDayCounter++;

					if (specificDayCounter > daysInMonth)
					{
						toggle.SetInactive();
					}
					else
					{
						string option = FormatDate(year, month, specificDayCounter);
						toggle.Setup($"{specificDayCounter}", option);

						// Buttons with dates earlier than today should be inactive
						DateTime today = DateTime.Today;
						DateTime optionDate = new DateTime(year, month, specificDayCounter);

						if (DateTime.Compare(optionDate, today) < 0)
						{
							toggle.SetInactive();
						}
						else
						{
							toggle.Set(_selectedDays.Contains(option));
							toggle.OnValueChanged = () => DayToggleClicked(toggle.Selected, toggle.Value);
						}
					}
				}

				toggle.Refresh();
			}
		}

		private void DayToggleClicked(bool toggleSelected, string toggleValue)
		{
			if (toggleSelected)
			{
				if (!_selectedDays.Contains(toggleValue))
				{
					_selectedDays.Add(toggleValue);
				}
			}
			else
			{
				if (_selectedDays.Contains(toggleValue))
				{
					_selectedDays.Remove(toggleValue);
				}
			}

			OnValueChanged?.Invoke(_selectedDays);
		}

		private string FormatDate(int year, int month, int day)
		{
			return $"{day}-{month}-{year}";
		}

		private int DetermineFirstDayOffset(int year, int month)
		{
			DateTime firstDay = new DateTime(year, month, 1);
			return (int)firstDay.DayOfWeek;
		}

		private Dictionary<int, string> GenerateYears()
		{
			int yearsAdvance = 3;

			Dictionary<int, string> options = new Dictionary<int, string>();

			DateTime now = DateTime.Now;

			for (int i = 0; i < yearsAdvance; i++)
			{
				int year = now.Year + i;
				options.Add(year, year.ToString());
			}

			return options;
		}

		private Dictionary<int, string> GenerateMonths()
		{
			Dictionary<int, string> options = new Dictionary<int, string>
			{
				{1, "January"},
				{2, "February"},
				{3, "March"},
				{4, "April"},
				{5, "May"},
				{6, "June"},
				{7, "July"},
				{8, "August"},
				{9, "September"},
				{10, "October"},
				{11, "November"},
				{12, "December"},
			};

			return options;
		}

		public void SetDefaultValues()
		{
			if (_selectedDays.Count == 0)
			{
				_monthSelector.SetCurrentOption(DateTime.Now.Month - 1);
				_selectedDays.Add($"{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}");

				OnDateChanged();
				OnValueChanged?.Invoke(_selectedDays);
			}
		}

		public void SetInitialValues(List<ScheduleDefinition> definitions)
		{
			_selectedDays.Clear();

			if (definitions.Count == 0)
			{
				OnDateChanged();
				return;
			}
			else if (int.TryParse(definitions[0]?.month[0], out int month))
			{
				_monthSelector.SetCurrentOption(month - 1);
			}

			foreach (ScheduleDefinition scheduleDefinition in definitions)
			{
				foreach (string year in scheduleDefinition.year)
					foreach (string month in scheduleDefinition.month)
						foreach (string day in scheduleDefinition.dayOfMonth)
							if (!_selectedDays.Contains($"{day}-{month}-{year}"))
								_selectedDays.Add($"{day}-{month}-{year}");
			}

			OnDateChanged();

			// Forcing to ensure validation check
			OnValueChanged?.Invoke(_selectedDays);
		}

		public void SetInitialValues(List<string> dates)
		{
			if (dates.Count == 0)
			{
				OnDateChanged();
				return;
			}

			_selectedDays = new List<string>(dates);
			OnDateChanged();
			OnValueChanged?.Invoke(_selectedDays);
		}
	}
}
