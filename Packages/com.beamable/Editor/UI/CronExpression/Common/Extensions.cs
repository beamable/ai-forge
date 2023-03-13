using System.Collections.Generic;

namespace Beamable.CronExpression
{
	public static class Extensions
	{
		private static readonly Dictionary<CronLocale, string> _cronLocaleToLocale = new Dictionary<CronLocale, string>
		{
			{ CronLocale.en_US, "en-US" },
			{ CronLocale.pl_PL, "pl-PL" }
		};

		public static string ConvertCronLocaleToLocale(this CronLocale? cronLocale)
		{
			return !cronLocale.HasValue ? string.Empty : ConvertCronLocaleToLocale(cronLocale.Value);
		}
		public static string ConvertCronLocaleToLocale(this CronLocale cronLocale)
		{
			return _cronLocaleToLocale.ContainsKey(cronLocale) ? _cronLocaleToLocale[cronLocale] : string.Empty;
		}
	}
}
