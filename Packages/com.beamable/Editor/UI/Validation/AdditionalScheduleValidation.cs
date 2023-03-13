using Beamable.Editor.UI.Validation;
using System;
using System.Collections.Generic;

namespace Beamable.Editor.Models.Schedules
{
	public static class AdditionalScheduleValidation
	{
		/// <summary>
		/// The validation will fail if the scheduled dates contain past dates (before DateTime.UtcNow)
		/// </summary>
		public static void ValidatePastDates(List<string> selectedDays, Action<bool, string> refreshConfirmButton, out bool isValid)
		{
			int todayTimeStamp = int.Parse(DateTime.UtcNow.ToString("yyyyMMdd"));

			foreach (var selectedDay in selectedDays)
			{
				var splitted = selectedDay.Split('-');
				var year = splitted[2];
				var month = splitted[1].Length == 1 ? $"0{splitted[1]}" : splitted[1];
				var day = splitted[0].Length == 1 ? $"0{splitted[0]}" : splitted[0];
				var timeStamp = int.Parse($"{year}{month}{day}");

				if (timeStamp < todayTimeStamp)
				{
					refreshConfirmButton?.Invoke(false, "Date cannot be past");
					isValid = false;
					return;
				}
			}

			isValid = true;
			refreshConfirmButton?.Invoke(true, string.Empty);
		}
	}
}
