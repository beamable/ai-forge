using Beamable.Editor.UI.Validation;
using System;
using System.Collections.Generic;
using System.Text;
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
	public class DatePickerVisualElement : ValidableVisualElement<string>
	{
		public new class UxmlFactory : UxmlFactory<DatePickerVisualElement, UxmlTraits>
		{
		}

		private Action _onDateChanged;

		public LabeledNumberPicker YearPicker { get; private set; }
		public LabeledNumberPicker MonthPicker { get; private set; }
		public LabeledNumberPicker DayPicker { get; private set; }

		public DatePickerVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(DatePickerVisualElement)}/{nameof(DatePickerVisualElement)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			YearPicker = Root.Q<LabeledNumberPicker>("yearPicker");
			YearPicker.Setup(OnDateChanged, GenerateYears(out int startYear, out int endYear));
			YearPicker.SetupMinMax(startYear, endYear);
			YearPicker.Refresh();

			MonthPicker = Root.Q<LabeledNumberPicker>("monthPicker");
			MonthPicker.Setup(OnDateChanged, GenerateMonths());
			MonthPicker.Refresh();

			DayPicker = Root.Q<LabeledNumberPicker>("dayPicker");
			DayPicker.Setup(OnDateChanged, GenerateDays());
			DayPicker.Refresh();
		}

		public void Setup(Action onDateChanged)
		{
			_onDateChanged = onDateChanged;
		}

		public void Set(DateTime date)
		{
			YearPicker.Set(date.Year.ToString());
			MonthPicker.Set(date.Month.ToString());
			DayPicker.Set((date.Day).ToString());
		}

		public string GetIsoDate()
		{
			return $"{GetSimpleDate()}T";
		}

		private void OnDateChanged()
		{
			InvokeValidationCheck(GetSimpleDate());
			if (YearPicker != null && MonthPicker != null)
			{
				int daysInMonth = DateTime.DaysInMonth(int.Parse(YearPicker.Value), int.Parse(MonthPicker.Value));
				DayPicker?.Setup(OnDateChanged, GenerateDays(daysInMonth));
			}
			else
			{
				DayPicker?.Setup(OnDateChanged, GenerateDays());
			}

			_onDateChanged?.Invoke();
		}

		private string GetSimpleDate()
		{
			if (YearPicker == null || MonthPicker == null || DayPicker == null)
			{
				return string.Empty;
			}

			int daysInMonth = DateTime.DaysInMonth(int.Parse(YearPicker.Value), int.Parse(MonthPicker.Value));
			int day = int.Parse(DayPicker.Value);
			day = Mathf.Clamp(day, 1, daysInMonth);

			StringBuilder builder = new StringBuilder();
			builder.Append(
				$"{int.Parse(YearPicker.Value):0000}-{int.Parse(MonthPicker.Value):00}-{day:00}");
			return builder.ToString();
		}

		private List<string> GenerateYears(out int startYear, out int endYear)
		{
			int yearsAdvance = 20;
			startYear = 0;
			endYear = 0;

			List<string> options = new List<string>();

			DateTime now = DateTime.Now;

			for (int i = 0; i < yearsAdvance; i++)
			{
				int currentYear = now.Year + i;
				if (i == 0)
				{
					startYear = currentYear;
				}
				else if (i == yearsAdvance - 1)
				{
					endYear = currentYear;
				}

				string option = currentYear.ToString("0000");
				options.Add(option);
			}

			return options;
		}

		private List<string> GenerateMonths()
		{
			List<string> options = new List<string>();

			for (int i = 0; i < 12; i++)
			{
				string option = (i + 1).ToString("00");
				options.Add(option);
			}

			return options;
		}

		private List<string> GenerateDays(int daysInMonth = 31)
		{
			List<string> options = new List<string>();

			for (int i = 0; i < daysInMonth; i++)
			{
				string option = (i + 1).ToString("00");
				options.Add(option);
			}

			return options;
		}
	}
}
