namespace Beamable.Theme
{
	public class ThemeConfiguration : ModuleConfigurationObject
	{
		public static ThemeConfiguration Instance => Get<ThemeConfiguration>();
		public ThemeObject Style;
	}
}
