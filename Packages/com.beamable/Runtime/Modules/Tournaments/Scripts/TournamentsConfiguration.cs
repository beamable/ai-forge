using System;
using System.Collections.Generic;
using UnityEngine;
using static Beamable.Common.Constants.MenuItems.Assets;

namespace Beamable.Tournaments
{
#if BEAMABLE_DEVELOPER
	[CreateAssetMenu(
		fileName = "Tournament Configuration",
		menuName = Paths.MENU_ITEM_PATH_ASSETS_BEAMABLE_CONFIGURATIONS + "/" +
				  "Tournament Configuration",
		order = Orders.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1)]
#endif
	public class TournamentsConfiguration : ModuleConfigurationObject
	{
		public static TournamentsConfiguration Instance => Get<TournamentsConfiguration>();
		public List<TournamentInfoPageSection> Info;

	}

	[Serializable]
	public class TournamentInfoPageSection
	{
		public string Title;
		[TextArea(4, 12)]
		public string Body;
		public string DetailTitle;
		public TournamentInfoDetailBehaviour DetailPrefab;
	}
}
