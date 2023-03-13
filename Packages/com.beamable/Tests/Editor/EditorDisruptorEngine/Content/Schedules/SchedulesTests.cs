using Beamable.Common.Content;
using Beamable.Editor.UI.Components;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Beamable.Editor.Tests.Content
{
	public class SchedulesTests
	{
		private readonly string _dateStartString = "2021-01-01T01:00:00.0000000Z";

		private DateTime _dateStart;

		[SetUp]
		public void SetUp()
		{
			Assert.IsTrue(DateTime.TryParse(_dateStartString, out _dateStart), "Date strings are not parsing correctly");
		}

		[Test]
		public void Event_Schedule_Daily_Mode_Test()
		{
			String warningHeader = "Daily event schedule:";
			bool received = false;

			void ScheduleReceived(Schedule schedule)
			{
				received = true;
				Assert.IsTrue(schedule.definitions.Count == 1, $"{warningHeader} should have only one definition");
			}

			EventScheduleWindow window = new EventScheduleWindow();
			window.Refresh();
			window.ModeComponent.Set(0);
			window.StartTimeComponent.Set(_dateStart);
			window.NeverExpiresComponent.Value = true;
			window.ActiveToDateComponent.Set(_dateStart + TimeSpan.FromDays(2));
			window.ActiveToHourComponent.Set(_dateStart + TimeSpan.FromDays(2));
			window.OnScheduleUpdated += ScheduleReceived;
			window.InvokeTestConfirm();
			window.OnScheduleUpdated -= ScheduleReceived;
			Assert.IsTrue(received, "Schedule not received. Test failed");
		}

		[Test]
		public void Event_Schedule_Days_Mode_Test()
		{
			String warningHeader = "Days event schedule:";
			bool received = false;

			void ScheduleReceived(Schedule schedule)
			{
				received = true;
				List<string> days = schedule.definitions[0].dayOfWeek;

				Assert.IsTrue(schedule.definitions.Count == 1, $"{warningHeader} should have only one definition");
				Assert.IsTrue(days.Count > 0, $"{warningHeader} minimum one day should be selected");

				foreach (string day in days)
				{
					bool isDayParsed = int.TryParse(day, out int parsedDayValue);
					Assert.IsTrue(isDayParsed, $"{warningHeader} problem with parsing {day} in days list");
					Assert.IsTrue(parsedDayValue <= 7, $"{warningHeader} parsed day value should be less or equal 7");
				}

				if (schedule.definitions.Count > 0)
				{
					TestHour(warningHeader, schedule.definitions[0]);
				}
			}

			EventScheduleWindow window = new EventScheduleWindow();
			window.Refresh();
			window.ModeComponent.Set(1);
			window.StartTimeComponent.Set(_dateStart);
			window.DaysComponent.SetSelectedDays(new List<string> { "1", "3", "5" });
			window.NeverExpiresComponent.Value = true;
			window.ActiveToDateComponent.Set(_dateStart + TimeSpan.FromDays(2));
			window.ActiveToHourComponent.Set(_dateStart + TimeSpan.FromDays(2));
			window.OnScheduleUpdated += ScheduleReceived;
			window.InvokeTestConfirm();
			window.OnScheduleUpdated -= ScheduleReceived;
			Assert.IsTrue(received, "Schedule not received. Test failed");
		}

		[Test]
		public void Event_Schedule_Dates_Mode_Test()
		{
			String warningHeader = "Dates event schedule:";
			bool received = false;

			void ScheduleReceived(Schedule schedule)
			{
				received = true;

				Assert.IsTrue(schedule.definitions.Count > 0, $"{warningHeader} should have at least on definition");

				List<string> days = schedule.definitions[0].dayOfMonth;
				Assert.IsTrue(days.Count > 0, $"{warningHeader} minimum one day should be selected");

				foreach (string day in days)
				{
					bool isDayParsed = int.TryParse(day, out int parsedDayValue);
					Assert.IsTrue(isDayParsed, $"{warningHeader} problem with parsing {day} in days list");
					Assert.IsTrue(parsedDayValue <= 7, $"{warningHeader} parsed day value should be less or equal 7");
				}

				if (schedule.definitions.Count > 0)
				{
					TestHour(warningHeader, schedule.definitions[0]);
				}
			}

			EventScheduleWindow window = new EventScheduleWindow();
			window.Refresh();
			window.ModeComponent.Set(2);
			window.StartTimeComponent.Set(_dateStart);
			window.CalendarComponent.Calendar.SetInitialValues(new List<string>
			{
				"5-10-2021",
				"10-11-2021",
				"12-12-2022"
			});
			window.NeverExpiresComponent.Value = false;
			window.ActiveToDateComponent.Set(_dateStart + TimeSpan.FromDays(2));
			window.ActiveToHourComponent.Set(_dateStart + TimeSpan.FromDays(2));
			window.OnScheduleUpdated += ScheduleReceived;
			window.InvokeTestConfirm();
			window.OnScheduleUpdated -= ScheduleReceived;
			Assert.IsTrue(received, "Schedule not received. Test failed");
		}

		[Test]
		public void Listing_Schedule_Daily_Mode_Test()
		{
			String warningHeader = "Daily listing schedule:";
			bool received = false;

			void ScheduleReceived(Schedule schedule)
			{
				received = true;
				Assert.IsTrue(schedule.definitions.Count > 0 && schedule.definitions.Count <= 3,
							  $"{warningHeader} definitions amount should be greater than 0 and less or equal to 3");

				foreach (ScheduleDefinition scheduleDefinition in schedule.definitions)
				{
					string minuteString = scheduleDefinition.minute[0];
					string hoursString = scheduleDefinition.hour[0];
					bool minutesMatchPattern = Regex.IsMatch(minuteString, "\\b([0-9]|[1-5][0-9])\\b") ||
											   Regex.IsMatch(minuteString, "/*");
					bool hoursMatchPattern =
						Regex.IsMatch(hoursString, "\\b([0-9]|1[0-9]|2[0-3])-([0-9]|1[0-9]|2[0-3])\\b") ||
						Regex.IsMatch(hoursString, "\\b([0-9]|1[0-9]|2[0-3])\\b");
					Assert.IsTrue(minutesMatchPattern, $"{warningHeader} minutes doesn't match pattern");
					Assert.IsTrue(hoursMatchPattern, $"{warningHeader} hours doesn't match pattern");
				}
			}

			ListingScheduleWindow window = new ListingScheduleWindow();
			window.Refresh();
			window.ModeComponent.Set(0);
			window.AllDayComponent.Value = false;
			window.PeriodFromHourComponent.Set(_dateStart);
			window.PeriodToHourComponent.Set(_dateStart + TimeSpan.FromHours(2));
			window.NeverExpiresComponent.Value = true;
			window.ActiveToDateComponent.Set(_dateStart + TimeSpan.FromDays(2));
			window.ActiveToHourComponent.Set(_dateStart + TimeSpan.FromDays(2));
			window.OnScheduleUpdated += ScheduleReceived;
			window.InvokeTestConfirm();
			window.OnScheduleUpdated -= ScheduleReceived;
			Assert.IsTrue(received, "Schedule not received. Test failed");
		}

		[Test]
		public void Listing_Schedule_Days_Mode_Test()
		{
			String warningHeader = "Days listing schedule:";
			bool received = false;

			void ScheduleReceived(Schedule schedule)
			{
				received = true;

				Assert.IsTrue(schedule.definitions.Count > 0 && schedule.definitions.Count <= 3,
					$"{warningHeader} definitions amount should be greater than 0 and less or equal to 3");

				List<string> days = schedule.definitions[0].dayOfWeek;
				Assert.IsTrue(days.Count > 0 && days.Count < 7,
					$"{warningHeader} minimum one and maximum 6 days should be selected");

				foreach (string day in days)
				{
					bool isDayParsed = int.TryParse(day, out int parsedDayValue);
					Assert.IsTrue(isDayParsed, $"{warningHeader} problem with parsing {day} in days list");
					Assert.IsTrue(parsedDayValue <= 7, $"{warningHeader} parsed day value should be less or equal 7");
				}

				TestPeriod(schedule, warningHeader);
			}

			ListingScheduleWindow window = new ListingScheduleWindow();
			window.Refresh();
			window.ModeComponent.Set(1);
			window.DaysComponent.SetSelectedDays(new List<string> { "1", "3", "5" });
			window.AllDayComponent.Value = false;
			window.PeriodFromHourComponent.Set(_dateStart);
			window.PeriodToHourComponent.Set(_dateStart + TimeSpan.FromHours(2));
			window.NeverExpiresComponent.Value = false;
			window.ActiveToDateComponent.Set(_dateStart + TimeSpan.FromDays(2));
			window.ActiveToHourComponent.Set(_dateStart + TimeSpan.FromDays(2));
			window.OnScheduleUpdated += ScheduleReceived;
			window.InvokeTestConfirm();
			window.OnScheduleUpdated -= ScheduleReceived;
			Assert.IsTrue(received, "Schedule not received. Test failed");
		}

		[Test]
		public void Listing_Schedule_Dates_Mode_Test()
		{
			String warningHeader = "Dates event schedule:";
			bool received = false;

			void ScheduleReceived(Schedule schedule)
			{
				received = true;

				Assert.IsTrue(schedule.definitions.Count > 0,
					$"{warningHeader} definitions amount should be greater than 0");

				List<string> days = schedule.definitions[0].dayOfMonth;
				Assert.IsTrue(days.Count > 0, $"{warningHeader} minimum one day should be selected");

				foreach (string day in days)
				{
					bool isDayParsed = int.TryParse(day, out int parsedDayValue);
					Assert.IsTrue(isDayParsed, $"{warningHeader} problem with parsing {day} in days list");
					Assert.IsTrue(parsedDayValue <= 7, $"{warningHeader} parsed day value should be less or equal 7");
				}

				TestPeriod(schedule, warningHeader);
			}

			ListingScheduleWindow window = new ListingScheduleWindow();
			window.Refresh();
			window.ModeComponent.Set(2);
			window.CalendarComponent.Calendar.SetInitialValues(new List<string>
			{
				"05-10-2021",
				"10-11-2021",
				"12-12-2022"
			});
			window.AllDayComponent.Value = false;
			window.PeriodFromHourComponent.Set(_dateStart);
			window.PeriodToHourComponent.Set(_dateStart + TimeSpan.FromHours(2));
			window.NeverExpiresComponent.Value = false;
			window.ActiveToDateComponent.Set(_dateStart + TimeSpan.FromDays(2));
			window.ActiveToHourComponent.Set(_dateStart + TimeSpan.FromDays(2));
			window.OnScheduleUpdated += ScheduleReceived;
			window.InvokeTestConfirm();
			window.OnScheduleUpdated -= ScheduleReceived;
			Assert.IsTrue(received, "Schedule not received. Test failed");
		}

		private void TestHour(string warningHeader, ScheduleDefinition definition)
		{
			bool isHourParsed = int.TryParse(definition.hour[0], out int parsedHour);
			bool isMinuteParsed = int.TryParse(definition.minute[0], out int parsedMinute);
			bool isSecondParsed = int.TryParse(definition.second[0], out int parsedSecond);

			if (definition.hour[0] != "*")
				Assert.IsTrue(isHourParsed, $"{warningHeader} problem with parsing hour");
			if (definition.minute[0] != "*")
				Assert.IsTrue(isMinuteParsed, $"{warningHeader} problem with parsing minute");
			if (definition.second[0] != "*")
				Assert.IsTrue(isSecondParsed, $"{warningHeader} problem with parsing second");

			Assert.IsTrue(definition.hour[0] == "*" || parsedHour >= 0 && parsedHour < 24,
						  $"{warningHeader} hour should be greater or equal 0 and less than 24 or marked as *");
			Assert.IsTrue(definition.minute[0] == "*" || parsedMinute >= 0 && parsedMinute < 60,
						  $"{warningHeader} minute should be greater or equal 0 and less than 60 or marked as *");
			Assert.IsTrue(definition.second[0] == "*" || parsedSecond >= 0 && parsedSecond < 60,
						  $"{warningHeader} second should be greater or equal 0 and less than 60 or marked as *");
		}

		private static void TestPeriod(Schedule schedule, string warningHeader)
		{
			if (schedule.definitions.Count > 0)
			{
				if (schedule.IsPeriod)
				{
					var startHour = schedule.definitions[0].hour[0].Contains("*")
						? 0
						: Convert.ToInt32(schedule.definitions[0].hour[0]);

					var endHour = schedule.definitions[schedule.definitions.Count - 1].hour[schedule.definitions[schedule.definitions.Count - 1].hour.Count - 1].Contains("*")
						? 23
						: Convert.ToInt32(schedule.definitions[schedule.definitions.Count - 1].hour[schedule.definitions[schedule.definitions.Count - 1].hour.Count - 1]);

					var startMinute = schedule.definitions[0].minute[0].Contains("*")
						? 0
						: Convert.ToInt32(schedule.definitions[0].minute[0]);

					var endMinute = schedule.definitions[schedule.definitions.Count - 1].minute[schedule.definitions[schedule.definitions.Count - 1].minute.Count - 1].Contains("*")
						? 59
						: Convert.ToInt32(schedule.definitions[schedule.definitions.Count - 1].minute[schedule.definitions[schedule.definitions.Count - 1].minute.Count - 1]);

					endMinute++;
					if (endMinute == 60)
					{
						endMinute = 0;
						endHour += 1;
					}

					bool valid = endHour > startHour || (endHour == startHour && endMinute > startMinute);
					Assert.IsTrue(valid, $"{warningHeader} active period to should be later than active period from");
				}
			}
		}
	}
}
