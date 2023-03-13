using UnityEditor;
using UnityEngine;
using static Beamable.Common.Constants;

namespace Beamable.Editor
{
	public static class HelpMenuItems
	{
		[MenuItem(
		   MenuItems.Windows.Paths.MENU_ITEM_PATH_WINDOW_BEAMABLE_HELP + "/" +
		   Commons.OPEN + " " +
		   BEAMABLE_MAIN_WEBSITE,
		   priority = MenuItems.Windows.Orders.MENU_ITEM_PATH_WINDOW_PRIORITY_3)]
		private static void OpenBeamableMainWebsite()
		{
			Application.OpenURL(URLs.URL_BEAMABLE_MAIN_WEBSITE);
		}

		[MenuItem(
		   MenuItems.Windows.Paths.MENU_ITEM_PATH_WINDOW_BEAMABLE_HELP + "/" +
		   Commons.OPEN + " " +
		   BEAMABLE_DOCS_WEBSITE,
		   priority = MenuItems.Windows.Orders.MENU_ITEM_PATH_WINDOW_PRIORITY_3)]
		private static void OpenBeamableDocsWebsite()
		{
			Application.OpenURL(URLs.URL_BEAMABLE_DOCS_WEBSITE);
		}
	}
}
