using System;
using UnityEngine;

namespace Beamable.Content.Utility
{
	public static class DateUtility
	{

		public const string ISO_FORMAT = "yyyy-MM-ddTHH:mm:ssZ";

		public static bool TryToCreateDateTime(int year, int month, int day, int hour, int minute, int second, out DateTime dateTime)
		{
			try
			{
				dateTime = new DateTime(year, month, day, hour, minute, second);
				return true;
			}
			catch (ArgumentOutOfRangeException)
			{
				dateTime = DateTime.UtcNow;
				return false;
			}
		}

		public static bool TryReplaceYear(this DateTime dateTime, int year, out DateTime result, bool clampDays = false)
		{
			try
			{
				var day = clampDays ? Mathf.Min(dateTime.Day, DateTime.DaysInMonth(year, dateTime.Month)) : dateTime.Day;
				result = new DateTime(year, dateTime.Month, day, dateTime.Hour,
					dateTime.Minute, dateTime.Second, dateTime.Millisecond);
				return true;
			}
			catch (ArgumentOutOfRangeException)
			{
				result = dateTime.Date;
				return false;
			}
		}

		public static bool TryReplaceMonth(this DateTime dateTime, int month, out DateTime result, bool clampDays = false)
		{
			try
			{
				var day = clampDays ? Mathf.Min(dateTime.Day, DateTime.DaysInMonth(dateTime.Year, month)) : dateTime.Day;
				result = new DateTime(dateTime.Year, month, day, dateTime.Hour,
					dateTime.Minute, dateTime.Second, dateTime.Millisecond);
				return true;
			}
			catch (ArgumentOutOfRangeException)
			{
				result = dateTime.Date;
				return false;
			}
		}

		public static bool TryReplaceDay(this DateTime dateTime, int day, out DateTime result)
		{
			try
			{
				result = new DateTime(dateTime.Year, dateTime.Month, day, dateTime.Hour,
					dateTime.Minute, dateTime.Second, dateTime.Millisecond);
				return true;
			}
			catch (ArgumentOutOfRangeException)
			{
				result = dateTime.Date;
				return false;
			}
		}

		public static bool TryReplaceHour(this DateTime dateTime, int hour, out DateTime result)
		{
			try
			{
				result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hour,
					dateTime.Minute, dateTime.Second, dateTime.Millisecond);
				return true;
			}
			catch (ArgumentOutOfRangeException)
			{
				result = dateTime.Date;
				return false;
			}
		}

		public static bool TryReplaceMinute(this DateTime dateTime, int minute, out DateTime result)
		{
			try
			{
				result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour,
					minute, dateTime.Second, dateTime.Millisecond);
				return true;
			}
			catch (ArgumentOutOfRangeException)
			{
				result = dateTime.Date;
				return false;
			}
		}

		public static bool TryReplaceSecond(this DateTime dateTime, int second, out DateTime result)
		{
			try
			{
				result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour,
					dateTime.Minute, second, dateTime.Millisecond);
				return true;
			}
			catch (ArgumentOutOfRangeException)
			{
				result = dateTime.Date;
				return false;
			}
		}
	}
}
