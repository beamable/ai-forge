using Beamable.Common.Shop;
using System.Collections.Generic;
using UnityEngine;
using static Beamable.Common.Constants.MenuItems.Assets;

namespace Beamable.Shop
{
#if BEAMABLE_DEVELOPER
	[CreateAssetMenu(
	   fileName = "Shop Configuration",
	   menuName = Paths.MENU_ITEM_PATH_ASSETS_BEAMABLE_CONFIGURATIONS + "/" +
	   "Shop Configuration",
	   order = Orders.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1)]
#endif
	public class ShopConfiguration : ModuleConfigurationObject
	{
		public static ShopConfiguration Instance => Get<ShopConfiguration>();

		public List<StoreRef> Stores = new List<StoreRef>();
		public ListingRenderer ListingRenderer;
		public ObtainRenderer ObtainRenderer;
	}
}
