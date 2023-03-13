namespace Beamable.Common
{
	public static partial class Constants
	{
		public static partial class Features
		{
			public static partial class Environment
			{
				public const string BASE_PATH = Directories.BEAMABLE_PACKAGE_EDITOR_UI + "/Environment";
				public const string COMPONENTS_PATH = BASE_PATH + "/Components";

				public const string OVERRIDE_PATH = "Assets/Beamable/Resources/beamable-env-overrides.json";

				public const string StagingPath = "Packages/com.beamable/Runtime/Environment/Resources/env-staging.json";
				public const string DevPath = "Packages/com.beamable/Runtime/Environment/Resources/env-dev.json";
				public const string ProdPath = "Packages/com.beamable/Runtime/Environment/Resources/env-prod.json";

			}
		}
	}
}
