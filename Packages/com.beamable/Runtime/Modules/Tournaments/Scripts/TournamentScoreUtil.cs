using UnityEngine;

namespace Beamable.Tournaments
{
	public static class TournamentScoreUtil
	{
		private const float THOUSAND = 1000;
		private const float MILLION = 1000000;
		private const float BILLION = 1000000000;
		private const float TRILLION = 1000000000000;
		private const float QUADRILLION = 1000000000000000;

		private static readonly char[] Units = new[] { 'K', 'M', 'B', 'T', 'Q' };

		public static string GetCommaString(long number)
		{
			return $"{number:n0}";
		}

		public static string GetShortScore(ulong number)
		{
			if (number < THOUSAND)
			{
				return $"{number}"; // up to 999 can render as-is.
			}

			var unit = THOUSAND;
			var unitIndex = 0;

			if (number >= QUADRILLION)
			{
				unit = QUADRILLION;
				unitIndex = 4;
			}
			else if (number >= TRILLION)
			{
				unit = TRILLION;
				unitIndex = 3;
			}
			else if (number >= BILLION)
			{
				unit = BILLION;
				unitIndex = 2;
			}
			else if (number >= MILLION)
			{
				unit = MILLION;
				unitIndex = 1;
			}

			var count = number / unit;
			var factor = 100f;
			if (count >= 10)
			{
				factor = 1f;
			}
			else if (count >= 1)
			{
				factor = 10f;
			}
			count = Mathf.RoundToInt(count * factor) / factor;
			if (count >= 1000)
			{
				if (unitIndex + 1 < Units.Length)
				{
					return $"1{Units[unitIndex + 1]}";
				}
				else
				{
					return "max";
				}
			}

			return $"{count:.##}{Units[unitIndex]}";
		}
	}
}
