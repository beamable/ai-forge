using System.Collections.Generic;
using UnityEngine;
using static Beamable.Common.Constants.MenuItems.Assets;

namespace Beamable.Avatars
{
	[System.Serializable]
	public class AccountAvatar
	{
		public string Name;
		public Sprite Sprite;
	}

#if BEAMABLE_DEVELOPER
	[CreateAssetMenu(
	   fileName = "Avatar Configuration",
	   menuName = Paths.MENU_ITEM_PATH_ASSETS_BEAMABLE_CONFIGURATIONS + "/" +
	   "Avatar Configuration",
	   order = Orders.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1)]
#endif
	public class AvatarConfiguration : ModuleConfigurationObject
	{
		public static AvatarConfiguration Instance => Get<AvatarConfiguration>();

		public AccountAvatar Default;
		public List<AccountAvatar> Avatars;
	}
}
