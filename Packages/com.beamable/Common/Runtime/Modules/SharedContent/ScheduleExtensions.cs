using Beamable.Content.Utility;
using System;
using System.Globalization;

namespace Beamable.Common.Content
{
	public static class ScheduleExtensions
	{
		public static bool TryGetActiveTo(this Schedule schedule, out DateTime activeToDate)
		{
			activeToDate = DateTime.Now;
			if (!schedule.activeTo.HasValue)
			{
				return false;
			}

			if (!DateTime.TryParseExact(schedule.activeTo.Value, DateUtility.ISO_FORMAT, CultureInfo.InvariantCulture,
				DateTimeStyles.None, out activeToDate)) return false;
			activeToDate = activeToDate.ToUniversalTime();

			return true;
		}

		public static DateTime ParseEventStartDate(this string content, out bool isSuccess)
		{
			isSuccess = DateTime.TryParseExact(content, DateUtility.ISO_FORMAT, CultureInfo.InvariantCulture,
										DateTimeStyles.None, out var result);
			return isSuccess
				? result.ToUniversalTime()
				: DateTime.UtcNow;
		}
	}
}
