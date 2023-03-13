using System.Collections.Generic;
using UnityEngine;

namespace Beamable.CronExpression
{
	public class CronLocalizationDatabase : ScriptableObject
	{
		public CronLocale DefaultLocalization => defaultLocalization;
		public List<CronLocalizationData> SupportedLocalizations => supportedLocalizations;

		[SerializeField] private CronLocale defaultLocalization = CronLocale.en_US;
		[SerializeField] private List<CronLocalizationData> supportedLocalizations = null;
	}

	public enum CronLocale
	{
		en_US,
		pl_PL
	}
}
