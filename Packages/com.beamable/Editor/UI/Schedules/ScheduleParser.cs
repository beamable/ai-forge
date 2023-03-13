using Beamable.Common.Content;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Editor.Models.Schedules
{
	public class ScheduleParser
	{
		public void PrepareGeneralData(Schedule newSchedule, string description, bool expires,
			string activeTo)
		{
			newSchedule.description = description;
			newSchedule.activeTo.HasValue = !expires;
			newSchedule.activeTo.Value = activeTo;
		}

		public void PrepareDailyModeData(Schedule newSchedule, string hour, string minute, string second)
		{
			ScheduleDefinition definition =
				new ScheduleDefinition(second, minute, hour, new List<string> { "*" }, "*", "*", new List<string> { "*" });
			newSchedule.AddDefinition(definition);
		}

		public void PrepareDaysModeData(Schedule newSchedule, string hour, string minute, string second,
			List<string> selectedDays)
		{
			var definition = new ScheduleDefinition(
				new List<string> { second },
				new List<string> { minute },
				new List<string> { hour },
				new List<string> { "*" },
				new List<string> { "*" },
				new List<string> { "*" },
				selectedDays);
			newSchedule.AddDefinition(definition);
		}

		public void PrepareDateModeData(Schedule newSchedule, string hour, string minute, string second,
			List<string> selectedDays)
		{
			var scheduleDateModeModels = ParseDates(selectedDays);
			foreach (var model in scheduleDateModeModels)
			{
				var definition = new ScheduleDefinition(
					new List<string> { second },
					new List<string> { minute },
					new List<string> { hour },
					model.Days,
					model.Months,
					model.Years,
					new List<string> { "*" });
				newSchedule.AddDefinition(definition);
			}
		}

		public void PrepareListingDailyModeData(Schedule newSchedule, int fromHour, int toHour, int fromMinute,
			int toMinute)
		{
			List<ScheduleDefinition> definitions = GetPeriodsSchedulesDefinitions(fromHour, toHour, fromMinute,
				toMinute,
				new List<string> { "*" });
			newSchedule.AddDefinitions(definitions);
		}

		public void PrepareListingDaysModeData(Schedule newSchedule, int fromHour, int toHour, int fromMinute,
			int toMinute, List<string> selectedDays)
		{
			List<ScheduleDefinition> definitions = GetPeriodsSchedulesDefinitions(fromHour, toHour, fromMinute,
				toMinute,
				selectedDays);
			newSchedule.AddDefinitions(definitions);
		}

		public void PrepareListingDatesModeData(Schedule newSchedule, int fromHour, int toHour, int fromMinute,
			int toMinute, List<string> selectedDates)
		{
			var scheduleDateModeModels = ParseDates(selectedDates);
			var periodsSchedulesDefinitions = GetPeriodsSchedulesDefinitions(fromHour, toHour, fromMinute, toMinute, new List<string> { "*" });

			foreach (var model in scheduleDateModeModels)
			{
				foreach (var scheduleDefinition in periodsSchedulesDefinitions)
				{
					var definition = new ScheduleDefinition(
						new List<string> { "*" },
						scheduleDefinition.minute,
						scheduleDefinition.hour,
						model.Days,
						model.Months,
						model.Years,
						new List<string> { "*" });
					newSchedule.AddDefinition(definition);
				}
			}
		}

		protected List<ScheduleDefinition> GetPeriodsSchedulesDefinitions(int fromHour, int toHour, int fromMinute,
			int toMinute, List<string> selectedDays)
		{
			List<string> ConvertIntoRangeList(int from, int to)
			{
				var tempList = new List<string>();
				for (int i = from; i <= to; i++)
					tempList.Add($"{i}");
				return tempList;
			}

			var definitions = new List<ScheduleDefinition>();
			var allRange = new List<string> { "*" };

			if (fromHour == 0 && toHour == 0 && fromMinute == 0 && toMinute == 0)
			{
				var definition = new ScheduleDefinition(
					allRange,
					allRange,
					allRange,
					allRange,
					allRange,
					allRange,
					selectedDays);
				definitions.Add(definition);
				return definitions;
			}

			if (toHour == 0 && toMinute == 0)
				toHour = 24;

			var hoursDelta = toHour - fromHour;
			if (hoursDelta == 0)
			{
				if (toHour != fromHour)
				{
					var definition = new ScheduleDefinition(
						allRange,
						ConvertIntoRangeList(fromMinute, toMinute - 1),
						new List<string> { $"{fromHour}" },
						allRange,
						allRange,
						allRange,
						selectedDays);
					definitions.Add(definition);
				}
				else
				{
					var definition = new ScheduleDefinition(
						allRange,
						ConvertIntoRangeList(fromMinute, toMinute - 1),
						new List<string> { $"{fromHour}" },
						allRange,
						allRange,
						allRange,
						selectedDays);
					definitions.Add(definition);
				}
			}
			else if (hoursDelta == 1)
			{
				var startDefinition = new ScheduleDefinition(
					allRange,
					fromMinute == 0 ? allRange : ConvertIntoRangeList(fromMinute, 59),
					new List<string> { $"{fromHour}" },
					allRange,
					allRange,
					allRange,
					selectedDays);
				definitions.Add(startDefinition);

				if (toMinute != 0)
				{
					var endDefinition = new ScheduleDefinition(
						allRange,
						ConvertIntoRangeList(0, toMinute - 1),
						new List<string> { $"{toHour}" },
						allRange,
						allRange,
						allRange,
						selectedDays);
					definitions.Add(endDefinition);
				}
			}
			else if (hoursDelta == 2 && toMinute == 0)
			{
				var startDefinition = new ScheduleDefinition(
					allRange,
					fromMinute == 0 ? allRange : ConvertIntoRangeList(fromMinute, 59),
					ConvertIntoRangeList(fromHour, toHour - 1),
					allRange,
					allRange,
					allRange,
					selectedDays);
				definitions.Add(startDefinition);
			}
			else
			{
				if (fromMinute == 0 && toMinute == 0)
				{
					var definition = new ScheduleDefinition(
						allRange,
						allRange,
						ConvertIntoRangeList(fromHour, toHour - 1),
						allRange,
						allRange,
						allRange,
						selectedDays);
					definitions.Add(definition);
				}
				else
				{
					if (fromMinute != 0 && toMinute != 0)
					{
						var startDefinition = new ScheduleDefinition(
							allRange,
							ConvertIntoRangeList(fromMinute, 59),
							new List<string> { $"{fromHour}" },
							allRange,
							allRange,
							allRange,
							selectedDays);
						definitions.Add(startDefinition);

						var middleDefinition = new ScheduleDefinition(
							allRange,
							allRange,
							hoursDelta == 2 ? new List<string> { $"{fromHour + 1}" } : ConvertIntoRangeList(fromHour + 1, toHour - 1),
							allRange,
							allRange,
							allRange,
							selectedDays);
						definitions.Add(middleDefinition);

						var endDefinition = new ScheduleDefinition(
							allRange,
							ConvertIntoRangeList(0, toMinute - 1),
							new List<string> { $"{toHour}" },
							allRange,
							allRange,
							allRange,
							selectedDays);
						definitions.Add(endDefinition);
						return definitions;
					}

					if (toMinute == 0)
					{
						var startDefinition = new ScheduleDefinition(
							allRange,
							fromMinute == 0 ? allRange : ConvertIntoRangeList(fromMinute, 59),
							new List<string> { $"{fromHour}" },
							allRange,
							allRange,
							allRange,
							selectedDays);
						definitions.Add(startDefinition);

						var endDefinition = new ScheduleDefinition(
							allRange,
							allRange,
							hoursDelta == 2 ? new List<string> { $"{fromHour + 1}" } : ConvertIntoRangeList(fromHour + 1, toHour - 1),
							allRange,
							allRange,
							allRange,
							selectedDays);
						definitions.Add(endDefinition);
					}
					else
					{
						var startDefinition = new ScheduleDefinition(
							allRange,
							allRange,
							ConvertIntoRangeList(fromHour, toHour - 1),
							allRange,
							allRange,
							allRange,
							selectedDays);
						definitions.Add(startDefinition);

						var endDefinition = new ScheduleDefinition(
							allRange,
							ConvertIntoRangeList(0, toMinute - 1),
							new List<string> { $"{toHour}" },
							allRange,
							allRange,
							allRange,
							selectedDays);
						definitions.Add(endDefinition);
					}
				}
			}
			return definitions;
		}

		private List<ScheduleDateModeModel> ParseDates(List<string> dates)
		{
			var monthYearKeyDates = PrepareMonthYearKeys();
			SortDays();
			var groupsWithMergedMonths = CreateGroupsBasedOnDaysAndYearAndMergeMonths();
			var groupsWithMergedYears = CreateGroupsBasedOnDaysAndMonthsAndMergeYears();
			var scheduleDateModeModels = CreateDateModels();

			return scheduleDateModeModels;

			Dictionary<string, string> PrepareMonthYearKeys()
			{
				var dict = new Dictionary<string, string>();
				foreach (string date in dates)
				{
					string[] dateElements = date.Split('-');
					string day = dateElements[0];
					string month = dateElements[1];
					string year = dateElements[2];
					string monthAndYear = $"{month}-{year}";

					if (dict.ContainsKey(monthAndYear))
						dict[monthAndYear] += $",{day}";
					else
						dict.Add(monthAndYear, $"{day}");
				}
				return dict;
			}
			void SortDays()
			{
				foreach (var monthYearKeyDate in monthYearKeyDates.ToList())
				{
					monthYearKeyDates[monthYearKeyDate.Key] = String.Join(",", monthYearKeyDate.Value.Split(',').OrderBy(q => q).ToArray());
				}
			}
			Dictionary<string, Dictionary<string, string>> CreateGroupsBasedOnDaysAndYearAndMergeMonths()
			{
				var dict = new Dictionary<string, Dictionary<string, string>>();
				foreach (var kvp in monthYearKeyDates)
				{
					var splittedKey = kvp.Key.Split('-');
					var month = splittedKey[0];
					var year = splittedKey[1];

					if (dict.ContainsKey(kvp.Value))
					{
						if (dict[kvp.Value].ContainsKey(year))
						{
							dict[kvp.Value][year] += $",{month}";
						}
						else
						{
							dict[kvp.Value].Add(year, month);
						}
					}
					else
					{
						dict.Add(kvp.Value, new Dictionary<string, string> { { year, month } });
					}
				}

				foreach (var kvp in dict)
					foreach (var kvp2 in kvp.Value.ToList())
						dict[kvp.Key][kvp2.Key] = String.Join(",", kvp2.Value.Split(',').OrderBy(q => q).ToArray());

				return dict;
			}
			Dictionary<string, List<string>> CreateGroupsBasedOnDaysAndMonthsAndMergeYears()
			{
				var dict = new Dictionary<string, List<string>>();
				foreach (var kvp in groupsWithMergedMonths)
				{
					foreach (var kvp2 in kvp.Value)
					{
						var newKey = $"{kvp.Key}-{kvp2.Value}";
						if (dict.ContainsKey(newKey))
						{
							dict[newKey].Add(kvp2.Key);
						}
						else
						{
							dict.Add(newKey, new List<string> { kvp2.Key });
						}
					}

				}
				return dict;
			}
			List<ScheduleDateModeModel> CreateDateModels()
			{
				var models = new List<ScheduleDateModeModel>();
				foreach (var kvp in groupsWithMergedYears)
				{
					var splittedKey = kvp.Key.Split('-');

					var days = splittedKey[0].Split(',');
					var months = splittedKey[1].Split(',');
					var years = kvp.Value;
					models.Add(new ScheduleDateModeModel(days, months, years));
				}
				return models;
			}
		}
	}

	public class ScheduleDateModeModel
	{
		public List<string> Days { get; }
		public List<string> Months { get; }
		public List<string> Years { get; }

		public ScheduleDateModeModel(IEnumerable<string> days, IEnumerable<string> months, IEnumerable<string> years)
		{
			Days = days.OrderBy(x => x).ToList();
			Months = months.OrderBy(x => x).ToList();
			Years = years.OrderBy(x => x).ToList();
		}
	}
}
