using System;
using UnityEngine;
using static Beamable.Common.Constants.MenuItems.Assets;

namespace Beamable.Samples.SampleProjectBase
{
	/// <summary>
	/// Custom-formatted readme file with markdown-like display. 
	/// 
	/// Inspired by Unity's "Learn" Sample Projects
	/// 
	/// </summary>
#if BEAMABLE_DEVELOPER
	[CreateAssetMenu(
		fileName = "ReadMe",
		menuName = Paths.MENU_ITEM_PATH_ASSETS_BEAMABLE_SAMPLES + "/" +
		"ReadMe",
		order = Orders.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1)]
#endif
	public class Readme : ScriptableObject
	{
		public Texture2D icon;
		public string title;
		public Section[] sections;
		public bool loadedLayout;

		[Serializable]
		public class Section
		{
			public string heading, text, linkText, url;
		}
	}
}
