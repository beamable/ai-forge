using Beamable.Api;
using Beamable.Common.Api.Inventory;
using Beamable.Common.Inventory;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Beamable.Inventory.Scripts
{
	[System.Serializable]
	public class InventoryUpdateEvent : UnityEvent<InventoryUpdateArg>
	{

	}

	public class InventoryBehaviour : MonoBehaviour
	{
		public InventoryGroup Group;
		public InventoryUpdateEvent OnInventoryReceived;

		private PlatformSubscription<InventoryView> _subscription;
		private ItemContent _content;

		private void Start()
		{
			Beamable.API.Instance.Then(de =>
			{
				if (this == null) return; // unity lifecycle check.

				Group.ItemRef.Resolve().Then(content =>
				{
					_content = content;
					_subscription = de.InventoryService.Subscribe(content.Id, HandleInventory);
				});
			});
		}

		private void OnDestroy()
		{
			_subscription?.Unsubscribe();
		}

		void HandleInventory(InventoryView inventory)
		{
			List<ItemView> itemGroup;
			if (!inventory.items.TryGetValue(_content.Id, out itemGroup))
			{
				itemGroup = new List<ItemView>();
			}

			var arg = new InventoryUpdateArg
			{
				Group = Group,
				Inventory = itemGroup
			};
			OnInventoryReceived?.Invoke(arg);
		}
	}
}
