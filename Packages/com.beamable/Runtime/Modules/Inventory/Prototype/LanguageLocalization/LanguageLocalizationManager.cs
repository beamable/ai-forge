using System.Collections.Generic;

namespace Beamable.Modules.Inventory.Prototypes
{
	public static class LanguageLocalizationManager
	{
		private static readonly Dictionary<string, string> _plTranslations = new Dictionary<string, string>()
		{
			{"items.sword.name", "Miecz"},
			{"items.sword.description", "Potężny miecz który zmiażdzy potwory"},
			{"items.bow.name", "Łuk"},
			{"items.bow.description", "Niezniczalny łuk z niesamowitymi statystykami"},
			{"headers.inventory", "EKWIPUNEK"},
			{"buttons.close", "ZAMKNIJ"},
			{"others.loading_data", "POBIERAM DANE"}
		};

		private static readonly Dictionary<string, string> _enTranslations = new Dictionary<string, string>()
		{
			{"items.sword.name", "Sword"},
			{"items.sword.description", "Mighty sword to crush monsters"},
			{"items.bow.name", "Bow"},
			{"items.bow.description", "Indestructible bow with awesome stats"},
			{"headers.inventory", "INVENTORY"},
			{"buttons.close", "CLOSE"},
			{"others.loading_data", "DOWNLOADING DATA"}
		};

		private static readonly Dictionary<string, Dictionary<string, string>> _languages =
			new Dictionary<string, Dictionary<string, string>>
			{
				{"PL", _plTranslations},
				{"EN", _enTranslations}
			};

		private static string _selectedLanguage = "EN";

		public static string GetTranslation(string key)
		{
			return _languages[_selectedLanguage].TryGetValue(key, out string value) ? value : key;
		}
	}
}
