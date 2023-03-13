namespace Beamable.CronExpression
{
	/// <summary>
	///     Options for parsing and describing a Cron Expression
	/// </summary>
	public class Options
	{
		public Options()
		{
			ThrowExceptionOnParseError = true;
			Verbose = false;
			DayOfWeekStartIndexZero = true;
		}

		public bool ThrowExceptionOnParseError { get; set; }
		public bool Verbose { get; set; }
		public bool DayOfWeekStartIndexZero { get; set; }
		public bool? Use24HourTimeFormat { get; set; }
		public CronLocale? Locale { get; set; }
	}
}
