using UnityEditor;
using static Beamable.Common.Constants.MenuItems.Windows;

namespace Beamable.Samples.SampleProjectBase
{
	/// <summary>
	/// Ping a custom-formatted readme file and force-show in inspector. Parse the
	/// custom format to markdown-like display.
	///
	/// Inspired by Unity's "Learn" Sample Projects
	///
	/// NOTE: Want to create a NEW SAMPLE PROJECT that has its own Readme? Include a COPY of this class
	/// in a new namespace that fits the sample project.
	///
	/// </summary>
	[CustomEditor(typeof(Readme))]
	[InitializeOnLoad]
	public class AutoOpenReadme : BeamableReadmeEditor
	{
		private const string SessionStateKeyWasAlreadyShown = "Beamable.Samples.SampleProjectBase.AutoOpenReadme.wasAlreadyShown";
		private const string FindAssetsFilter = "Readme t:Readme";
		private static readonly string[] FindAssetsFolders = new string[] { "Packages" };

		static AutoOpenReadme()
		{
#if !BEAMABLE_DEVELOPER
			EditorApplication.delayCall += SelectReadmeAutomatically;
#endif
		}

		private static void SelectReadmeAutomatically()
		{
			if (EditorPrefs.GetBool(SessionStateKeyWasAlreadyShown, false)) return;
			SelectSpecificReadmeMenuItem();
			EditorPrefs.SetBool(SessionStateKeyWasAlreadyShown, true);
		}

		[MenuItem(
		   Paths.MENU_ITEM_PATH_WINDOW_BEAMABLE_HELP + "/Readme",
		   priority = Orders.MENU_ITEM_PATH_WINDOW_PRIORITY_4)]
		private static Readme SelectSpecificReadmeMenuItem()
		{
			return BeamableReadmeEditor.SelectReadme(FindAssetsFilter, FindAssetsFolders);
		}
	}
}
