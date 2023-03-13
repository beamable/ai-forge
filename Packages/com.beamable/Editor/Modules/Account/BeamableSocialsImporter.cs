using Beamable.AccountManagement;

namespace Beamable.Editor.Modules.Account
{
	public class BeamableSocialsImporter : UnityEditor.AssetModificationProcessor
	{
		public const string BEAMABLE_FACEBOOK = "BEAMABLE_FACEBOOK";
		public const string BEAMABLE_GPGS = "BEAMABLE_GPGS";
		private const string ACCOUNT_CONFIG_PATH = "Assets/Beamable/Resources/AccountManagementConfiguration.asset";

		static void EnableFacebook() => PlayerSettingsHelper.EnableFlag(BEAMABLE_FACEBOOK);
		static void DisableFacebook() => PlayerSettingsHelper.DisableFlag(BEAMABLE_FACEBOOK);
		static void EnableGooglePlayGames() => PlayerSettingsHelper.EnableFlag(BEAMABLE_GPGS);
		static void DisableGooglePlayGames() => PlayerSettingsHelper.DisableFlag(BEAMABLE_GPGS);

		static BeamableSocialsImporter()
		{
			try
			{
				if (AccountManagementConfiguration.Instance == null)
				{
					return;
				}

				AccountManagementConfiguration.Instance.OnValidated += SetFlag;
			}
			catch
			{
				// we actively want to let this exception slide.
			}
		}
		private static string[] OnWillSaveAssets(string[] paths)
		{
			var found = false;
			foreach (var path in paths)
			{
				if (!ACCOUNT_CONFIG_PATH.Equals(path)) continue;
				found = true;
			}

			if (!found) return paths;
			SetFlag();

			return paths;
		}

		public static void SetFlag()
		{
			var config = AccountManagementConfiguration.Instance;
			if (config.Facebook)
			{
				EnableFacebook();
			}
			else
			{
				DisableFacebook();
			}

			if (config.GooglePlayGames)
			{
				EnableGooglePlayGames();
			}
			else
			{
				DisableGooglePlayGames();
			}
		}

	}
}
