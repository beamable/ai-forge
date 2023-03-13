namespace Beamable.Common
{
	public static partial class Constants
	{
		public static partial class Features
		{
			public static partial class Toolbox
			{
				public const string BASE_PATH = Directories.BEAMABLE_PACKAGE_EDITOR_UI + "/Toolbox";
				public const string COMPONENTS_PATH = BASE_PATH + "/Components";

				public static class EditorPrefsKeys
				{
					public const string IS_PACKAGE_UPDATE_IGNORED = "IsPackageUpdateIgnored";
					public const string IS_PACKAGE_WHATSNEW_ANNOUNCEMENT_IGNORED = "IsPackageWhatsNewAnnouncementIgnored";
					public const string NEWEST_VERSION_NUMBER = "NewestVersionNumber";
					public const string NEWEST_SERVER_VERSION_NUMBER = "NewestServerVersionNumber";
					public const string VSP_IGNORED_PACKAGE_VERSION = "VspIgnoredPackageVersion";
				}
			}
		}
	}
}
