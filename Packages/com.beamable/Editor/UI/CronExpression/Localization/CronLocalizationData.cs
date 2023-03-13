using UnityEngine;

namespace Beamable.CronExpression
{
	public class CronLocalizationData : ScriptableObject
	{
		public string Localization
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_localization))
					_localization = localization.ConvertCronLocaleToLocale();
				return _localization;
			}
		}

		private string _localization = string.Empty;

		[SerializeField] private CronLocale localization = CronLocale.en_US;
		[Space]

		public string EveryMinute = "every minute";
		public string EveryHour = "every hour";
		public string AnErrorOccuredWhenGeneratingTheExpressionD = "An error occured when generating the expression description. Check the cron expression syntax.";
		public string AtSpace = "At ";
		public string EveryMinuteBetweenX0AndX1 = "Every minute between {0} and {1}";
		public string At = "At";
		public string SpaceAnd = " and";
		public string EverySecond = "every second";
		public string EveryX0Seconds = "every {0} seconds";
		public string SecondsX0ThroughX1PastTheMinute = "seconds {0} through {1} past the minute";
		public string AtX0SecondsPastTheMinute = "at {0} seconds past the minute";
		public string EveryX0Minutes = "every {0} minutes";
		public string MinutesX0ThroughX1PastTheHour = "minutes {0} through {1} past the hour";
		public string AtX0MinutesPastTheHour = "at {0} minutes past the hour";
		public string EveryX0Hours = "every {0} hours";
		public string BetweenX0AndX1 = "between {0} and {1}";
		public string AtX0 = "at {0}";
		public string ComaEveryDay = ", every day";
		public string ComaEveryX0DaysOfTheWeek = ", every {0} days of the week";
		public string ComaX0ThroughX1 = ", {0} through {1}";
		public string First = "first";
		public string Second = "second";
		public string Third = "third";
		public string Fourth = "fourth";
		public string Fifth = "fifth";
		public string ComaOnThe = ", on the ";
		public string SpaceX0OfTheMonth = " {0} of the month";
		public string ComaOnTheLastX0OfTheMonth = ", on the last {0} of the month";
		public string ComaOnlyOnX0 = ", only on {0}";
		public string ComaEveryX0Months = ", every {0} months";
		public string ComaOnlyInX0 = ", only in {0}";
		public string ComaOnTheLastDayOfTheMonth = ", on the last day of the month";
		public string ComaOnTheLastWeekdayOfTheMonth = ", on the last weekday of the month";
		public string FirstWeekday = "first weekday";
		public string WeekdayNearestDayX0 = "weekday nearest day {0}";
		public string ComaOnTheX0OfTheMonth = ", on the {0} of the month";
		public string ComaEveryX0Days = ", every {0} days";
		public string ComaBetweenDayX0AndX1OfTheMonth = ", between day {0} and {1} of the month";
		public string ComaOnDayX0OfTheMonth = ", on day {0} of the month";
		public string SpaceAndSpace = " and ";
		public string ComaEveryMinute = ", every minute";
		public string ComaEveryHour = ", every hour";
		public string ComaEveryX0Years = ", every {0} years";
		public string CommaStartingX0 = ", starting {0}";
		public string AMPeriod = "AM";
		public string PMPeriod = "PM";
		public string CommaDaysBeforeTheLastDayOfTheMonth = ", {0} days before the last day of the month";
		public string ComaOnlyInYearX0 = ", only in {0}";

		public string GetString(string resourceName)
		{
			return (string)typeof(CronLocalizationData).GetField(resourceName)?.GetValue(this);
		}
	}
}
