using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Api.Sessions
{
	public static class SessionServiceHelper
	{
		/// <summary>
		/// Returns the current ISO639-1 language code
		/// </summary>
		/// <returns>A ISO639-1 language code</returns>
		public static string GetISO639CountryCodeFromSystemLanguage() =>
			GetISO639CountryCodeFromSystemLanguage(Application.systemLanguage);

		private static Dictionary<string, SystemLanguage> _codeToLang = new Dictionary<string, SystemLanguage>();

		static SessionServiceHelper()
		{
			foreach (var rawValue in Enum.GetValues(typeof(SystemLanguage)))
			{
				var enumValue = (SystemLanguage)rawValue;
				var code = GetISO639CountryCodeFromSystemLanguage(enumValue);
				_codeToLang[code] = enumValue;
			}
		}

		/// <summary>
		/// Convert an ISO639-1 code to the SystemLanguage.
		/// If the given code is invalid, the default system language is returned.
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		public static SystemLanguage GetSystemLanguageFromIso639CountryCode(string code)
		{
			if (string.IsNullOrEmpty(code))
			{
				return Application.systemLanguage;
			}
			if (!_codeToLang.TryGetValue(code, out var enumValue))
			{
				enumValue = Application.systemLanguage;
			}

			return enumValue;
		}

		/// <summary>
		/// Return the ISO639-1 language code from a given <see cref="SystemLanguage"/>
		/// Use the <see cref="GetISO639CountryCodeFromSystemLanguage()"/> to get the language code for the current application
		/// </summary>
		/// <param name="lang">The <see cref="SystemLanguage"/></param>
		/// <returns>a ISO639-1 language code</returns>
		public static string GetISO639CountryCodeFromSystemLanguage(SystemLanguage lang)
		{
			string res = "en";
			switch (lang)
			{
				case SystemLanguage.Afrikaans:
					res = "af";
					break;
				case SystemLanguage.Arabic:
					res = "ar";
					break;
				case SystemLanguage.Basque:
					res = "eu";
					break;
				case SystemLanguage.Belarusian:
					res = "by";
					break;
				case SystemLanguage.Bulgarian:
					res = "bg";
					break;
				case SystemLanguage.Catalan:
					res = "ca";
					break;
				case SystemLanguage.Chinese:
				case SystemLanguage.ChineseSimplified:
				case SystemLanguage.ChineseTraditional:
					res = "zh";
					break;
				case SystemLanguage.Czech:
					res = "cs";
					break;
				case SystemLanguage.Danish:
					res = "da";
					break;
				case SystemLanguage.Dutch:
					res = "nl";
					break;
				case SystemLanguage.English:
					res = "en";
					break;
				case SystemLanguage.Estonian:
					res = "et";
					break;
				case SystemLanguage.Faroese:
					res = "fo";
					break;
				case SystemLanguage.Finnish:
					res = "fi";
					break;
				case SystemLanguage.French:
					res = "fr";
					break;
				case SystemLanguage.German:
					res = "de";
					break;
				case SystemLanguage.Greek:
					res = "el";
					break;
				case SystemLanguage.Hebrew:
					res = "iw";
					break;
				case SystemLanguage.Hungarian:
					res = "hu";
					break;
				case SystemLanguage.Icelandic:
					res = "is";
					break;
				case SystemLanguage.Indonesian:
					res = "in";
					break;
				case SystemLanguage.Italian:
					res = "it";
					break;
				case SystemLanguage.Japanese:
					res = "ja";
					break;
				case SystemLanguage.Korean:
					res = "ko";
					break;
				case SystemLanguage.Latvian:
					res = "lv";
					break;
				case SystemLanguage.Lithuanian:
					res = "lt";
					break;
				case SystemLanguage.Norwegian:
					res = "no";
					break;
				case SystemLanguage.Polish:
					res = "pl";
					break;
				case SystemLanguage.Portuguese:
					res = "pt";
					break;
				case SystemLanguage.Romanian:
					res = "ro";
					break;
				case SystemLanguage.Russian:
					res = "ru";
					break;
				case SystemLanguage.SerboCroatian:
					res = "sh";
					break;
				case SystemLanguage.Slovak:
					res = "sk";
					break;
				case SystemLanguage.Slovenian:
					res = "sl";
					break;
				case SystemLanguage.Spanish:
					res = "es";
					break;
				case SystemLanguage.Swedish:
					res = "sv";
					break;
				case SystemLanguage.Thai:
					res = "th";
					break;
				case SystemLanguage.Turkish:
					res = "tr";
					break;
				case SystemLanguage.Ukrainian:
					res = "uk";
					break;
				case SystemLanguage.Unknown:
					res = "en";
					break;
				case SystemLanguage.Vietnamese:
					res = "vi";
					break;
				default:
					res = "en";
					break;
			}

			return res;
		}
	}
}
