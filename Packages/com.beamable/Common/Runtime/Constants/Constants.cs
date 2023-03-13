namespace Beamable.Common
{
	public static partial class Constants
	{
		public const string REALM_PREFERENCE = "BeamableSelectedRealms";
		public const string SESSION_STATE_INSTALL_DEPS = "BEAM_INSTALL_DEPS";
		public const string BEAMABLE_MAIN_WEBSITE = "beamable.com";
		public const string BEAMABLE_DOCS_WEBSITE = "docs.beamable.com";
		public const string BEAMABLE_ASSET_GROUP = "Beamable Assets";

		public const int SYSTEM_DEPENDENCY_ORDER = -1000;

		public static class Commons
		{
			public const string OBSOLETE_WILL_BE_REMOVED = "This is no longer supported, and will be removed in the future.";
			public const string OBSOLETE_BUSS_INTRODUCED = "Not used after introducing BUSS system";
			public const string OPEN = "Open";

			public const string OFFLINE = "offline";
		}

		public static class Environment
		{
			public const string BUILD__SDK__VERSION__STRING = "BUILD__SDK__VERSION__STRING";
			public const string UNITY__VSP__UID = "UNITY__VSP__UID";
		}

		public static class EditorPrefKeys
		{
			public const string ALLOWED_SAMPLES_REGISTER_FUNCTIONS = "ALLOWED_SAMPLES_REGISTER_FUNCTIONS";
		}

		public static class Directories
		{
			public const string BEAMABLE_ASSETS = "Assets/Beamable";

			public const string BEAMABLE_PACKAGE = "Packages/com.beamable";
			public const string BEAMABLE_PACKAGE_EDITOR = BEAMABLE_PACKAGE + "/Editor";
			public const string BEAMABLE_PACKAGE_EDITOR_UI = BEAMABLE_PACKAGE_EDITOR + "/UI";

			public const string BEAMABLE_SERVER_PACKAGE = "Packages/com.beamable.server";
			public const string BEAMABLE_SERVER_PACKAGE_EDITOR = BEAMABLE_SERVER_PACKAGE + "/Editor";
			public const string BEAMABLE_SERVER_PACKAGE_EDITOR_UI = BEAMABLE_SERVER_PACKAGE_EDITOR + "/UI";

			public const string COMMON_COMPONENTS_PATH = BEAMABLE_PACKAGE_EDITOR_UI + "/Common/Components";

			public const string ASSET_DIR = BEAMABLE_ASSETS + "/DefaultAssets";
			public const string DATA_DIR = BEAMABLE_ASSETS + "/Editor/content";
			public const string DEFAULT_DATA_DIR = BEAMABLE_PACKAGE_EDITOR + "/Modules/Content/DefaultContent";
			public const string DEFAULT_ASSET_DIR = BEAMABLE_PACKAGE_EDITOR + "/Modules/Content/DefaultAssets~";
		}

		public static class Files
		{
			public const string COMMON_USS_FILE = Directories.BEAMABLE_PACKAGE_EDITOR_UI + "/Common/Common.uss";
		}
		public static class URLs
		{
			public const string URL_BEAMABLE_MAIN_WEBSITE = "https://www.beamable.com";
			public const string URL_BEAMABLE_DOCS_WEBSITE = "https://docs.beamable.com/docs";
			public const string URL_BEAMABLE_BLOG_RELEASES_UNITY_SDK = "https://www.beamable.com/blog/beamable-release-unity-sdk";
			public const string URL_BEAMABLE_LEGAL_WEBSITE = "https://app.termly.io/document/terms-of-use-for-website/c44e18e4-675f-4eeb-8fa4-a9a5267ec2c5";

			public static class Documentations
			{
				public const string URL_DOC_ACCOUNT_HUD = URL_BEAMABLE_DOCS_WEBSITE + "/account-hud-prefab";
				public const string URL_DOC_ADMIN_FLOW = URL_BEAMABLE_DOCS_WEBSITE + "/admin-feature-overview";
				public const string URL_DOC_ANNOUNCEMENTS_FLOW = URL_BEAMABLE_DOCS_WEBSITE + "/announcements-feature-overview";
				public const string URL_DOC_CURRENCY_HUD = URL_BEAMABLE_DOCS_WEBSITE + "/virtual-currency-prefab";
				public const string URL_DOC_LEADERBOARD_FLOW = URL_BEAMABLE_DOCS_WEBSITE + "/leaderboards-feature-overview";
				public const string URL_DOC_LOGIN_FLOW = URL_BEAMABLE_DOCS_WEBSITE + "/account-management-flow-prefab";
				public const string URL_DOC_INVENTORY_FLOW = URL_BEAMABLE_DOCS_WEBSITE + "/inventory-feature-overview";
				public const string URL_DOC_STORE_FLOW = URL_BEAMABLE_DOCS_WEBSITE + "/stores-feature-overview";
				public const string URL_DOC_MICROSERVICES = URL_BEAMABLE_DOCS_WEBSITE + "/microservices-feature";

				public const string URL_DOC_WINDOW_CONTENT_MANAGER = URL_BEAMABLE_DOCS_WEBSITE + "/content-manager-overview";
				public const string URL_DOC_WINDOW_CONTENT_NAMESPACES = URL_BEAMABLE_DOCS_WEBSITE + "/content-manager#namespaces";
				public const string URL_DOC_WINDOW_CONFIG_MANAGER = URL_BEAMABLE_DOCS_WEBSITE + "/configuration-manager-overview";
				public const string URL_DOC_WINDOW_TOOLBOX = URL_BEAMABLE_DOCS_WEBSITE + "/unity-toolbox";

				public const string URL_DOC_BUSS_THEME_MANAGER = URL_BEAMABLE_DOCS_WEBSITE + "/theme-manager-overview";
			}
		}
		public static class MenuItems
		{
			public static class Windows
			{
				public static class Names
				{
					public const string BEAMABLE = "Beamable";
					public const string CONTENT_MANAGER = "Content Manager";
					public const string CONFIG_MANAGER = "Configuration Manager";
					public const string THEME_MANAGER = "Theme Manager";
					public const string MICROSERVICES_MANAGER = "Microservices Manager";
					public const string PORTAL = "Portal";
					public const string TOOLBOX = "Toolbox";
					public const string ENVIRONMENT = "Beamable Environment";
					public const string SAMPLE_UTILITY = "Sample Utilities";
					public const string BEAMABLE_ASSISTANT = BEAMABLE + " Assistant";
					public const string BUSS = BEAMABLE + " Styles";
					public const string BUSS_SHEET_EDITOR = "Sheet Inspector";
					public const string BUSS_WIZARD = "Theme Wizard";
					public const string LOGIN = "Beamable Login";
					public const string SDF_GENERATOR = "SDF Generator";
				}
				public static class Paths
				{
					private const string MENU_ITEM_PATH_WINDOW = "Window";

					public const string MENU_ITEM_PATH_WINDOW_BEAMABLE = MENU_ITEM_PATH_WINDOW + "/Beamable";
					public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_SAMPLES = MENU_ITEM_PATH_WINDOW_BEAMABLE + "/Samples";
					public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_HELP = MENU_ITEM_PATH_WINDOW_BEAMABLE + "/Help";
					public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_HELP_DIAGNOSTIC_DATA = MENU_ITEM_PATH_WINDOW_BEAMABLE_HELP + "/Generate Diagnostic Info";
					public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES = MENU_ITEM_PATH_WINDOW_BEAMABLE + "/Utilities";
					public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_BEAMABLE_DEVELOPER = MENU_ITEM_PATH_WINDOW_BEAMABLE + "/Beamable Developer";
					public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_MICROSERVICES = MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES + "/Microservices";
					public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_POOLING = MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES + "/Pooling";
					public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_ENV = MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES + "/Change Environment";
					public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_SAMPLE = MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES + "/Sample Utility";

					//Menu Items: Window (#ifdef BEAMABLE_DEVELOPER)
					public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_BEAMABLE_DEVELOPER_SAMPLES = MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_BEAMABLE_DEVELOPER + "/Samples";
					public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_UNITY = MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_BEAMABLE_DEVELOPER + "/Unity";
					public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_THEME_MANAGER = MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_BEAMABLE_DEVELOPER + "/Theme Manager";

					public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_BUSS = "/New BUSS";
					public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_SDF_GENERATOR =
						MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_BEAMABLE_DEVELOPER + MENU_ITEM_PATH_WINDOW_BEAMABLE_BUSS + "/Open SDF Generator";

				}
				public static class Orders
				{
					public const int MENU_ITEM_PATH_WINDOW_PRIORITY_1 = 0;
					public const int MENU_ITEM_PATH_WINDOW_PRIORITY_2 = 20;
					public const int MENU_ITEM_PATH_WINDOW_PRIORITY_3 = 40;
					public const int MENU_ITEM_PATH_WINDOW_PRIORITY_4 = 60;
				}
			}
			public static class Assets
			{
				public static class Paths
				{
					public const string MENU_ITEM_PATH_ASSETS_BEAMABLE = "Beamable";
					public const string MENU_ITEM_PATH_ASSETS_BEAMABLE_CONFIGURATIONS = MENU_ITEM_PATH_ASSETS_BEAMABLE + "/Configurations";
					public const string MENU_ITEM_PATH_ASSETS_BEAMABLE_SAMPLES = MENU_ITEM_PATH_ASSETS_BEAMABLE + "/Samples";
				}
				public static class Orders
				{
					public const int MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1 = 0;
					public const int MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_2 = 50;
					public const int MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_3 = 100;
					public const int MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_LAST = int.MaxValue;
				}
			}

			public static class Icons
			{
				public const char ARROW_DOWN_UTF = '\u25BC';
				public const char ARROW_UP_UTF = '\u25B2';
			}
		}
	}
}
