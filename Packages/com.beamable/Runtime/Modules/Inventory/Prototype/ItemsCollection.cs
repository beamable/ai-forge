using Beamable.Api;
using Beamable.Api.Inventory;
using Beamable.Common.Api.Inventory;
using Beamable.Common.Inventory;
using Beamable.Modules.Generics;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Modules.Inventory
{
	public class ItemGroupData
	{
		public ItemContent Content;
		public List<ItemView> Items;
	}

	public class ItemsCollection : DataCollection<ItemGroupData>
	{
		private PlatformSubscription<InventoryView> _subscription;

		public ItemsCollection(Action onCollectionUpdated) : base(onCollectionUpdated)
		{
		}

		protected sealed override async void Subscribe()
		{
			var beamable = await Beamable.API.Instance;
			if (beamable != null)
			{
				InventorySubscription inventorySubscription = beamable.InventoryService.Subscribable;
				_subscription = inventorySubscription.Subscribe(HandleSubscription);
			}
			else
			{
				Debug.LogWarning("Problem with an API access...");
			}
		}

		private void HandleSubscription(InventoryView inventory)
		{
			foreach (KeyValuePair<string, List<ItemView>> pair in inventory.items)
			{
				Update(pair);
			}

			CollectionUpdated?.Invoke();
		}

		private async void Update(KeyValuePair<string, List<ItemView>> pair)
		{
			ItemGroupData itemGroupData = Find(group => @group.Content.Id == pair.Key);

			if (itemGroupData == null)
			{
				ItemContent itemContent = await new ItemRef(pair.Key).Resolve();
				RegisterItemGroup(itemContent, out itemGroupData);
			}

			itemGroupData.Items = pair.Value;
		}

		private void RegisterItemGroup(ItemContent content, out ItemGroupData itemGroupData)
		{
			itemGroupData = new ItemGroupData
			{
				Content = content,
				Items = new List<ItemView>()
			};

			Add(itemGroupData);
		}

		public sealed override void Unsubscribe()
		{
			_subscription?.Unsubscribe();
		}
	}
}
