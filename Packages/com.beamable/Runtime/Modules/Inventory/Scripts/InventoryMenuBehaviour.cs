using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Api.Inventory;
using Beamable.UI.Scripts;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Beamable.Common.Constants.URLs;

namespace Beamable.Inventory.Scripts
{
	[HelpURL(Documentations.URL_DOC_INVENTORY_FLOW)]
	public class InventoryMenuBehaviour : MonoBehaviour
	{
		public MenuManagementBehaviour MenuManager;
		public InventoryMenuConfiguration InventoryConfig;
		private Promise<PlatformSubscription<InventoryView>> _inventorySubscription;

		private Promise<Unit> _inventoryViewPromise = new Promise<Unit>();
		private InventoryView _inventoryView;

		public void HandleToggle(bool shouldShow)
		{
			if (!shouldShow && MenuManager.IsOpen)
			{
				MenuManager.CloseAll();
			}
			else if (shouldShow && !MenuManager.IsOpen)
			{
				var menu = MenuManager.Show<InventoryMainMenu>();
			}
		}

		void Start()
		{
		}

		private void OnDestroy()
		{
			//_inventorySubscription.Then(s => s.Unsubscribe());
		}

		void HandleInventoryEvent(InventoryView inventory)
		{
			_inventoryView = inventory;
			_inventoryViewPromise.CompleteSuccess(PromiseBase.Unit);
		}
	}

	[Serializable]
	public class InventoryMenuConfiguration
	{
		public List<InventoryGroup> Groups;
		public InventoryObjectUI ItemPreviewPrefab;
	}
}
