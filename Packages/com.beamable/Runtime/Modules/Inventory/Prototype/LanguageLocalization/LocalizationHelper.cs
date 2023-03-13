namespace Beamable.Modules.Inventory.LanguageLocalization
{
	public class LocalizationHelper
	{
		public static string GetItemName(string key)
		{
			return $"{key}.name";
		}

		public static string GetItemDescription(string key)
		{
			return $"{key}.description";
		}
	}
}
