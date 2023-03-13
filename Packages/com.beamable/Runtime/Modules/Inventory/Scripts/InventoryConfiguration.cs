using Beamable.Common.Inventory;
using System.Collections.Generic;
using UnityEngine;
using static Beamable.Common.Constants.MenuItems.Assets;

namespace Beamable.Inventory.Scripts
{

	[System.Serializable]
	public struct InventoryGroup
	{
		public ItemRef ItemRef;
		public string DisplayName;
	}

#if BEAMABLE_DEVELOPER
	[CreateAssetMenu(
	   fileName = "Inventory Configuration",
	   menuName = Paths.MENU_ITEM_PATH_ASSETS_BEAMABLE_CONFIGURATIONS + "/" +
	   "Inventory Configuration",
	   order = Orders.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1)]
#endif
	public class InventoryConfiguration : ModuleConfigurationObject
	{
		public static InventoryConfiguration Instance => Get<InventoryConfiguration>();

		public List<InventoryGroup> Groups;

		public InventoryObjectUI DefaultObjectPrefab;
	}
}
