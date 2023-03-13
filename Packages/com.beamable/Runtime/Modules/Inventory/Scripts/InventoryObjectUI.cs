using Beamable.Common.Api.Inventory;
using Beamable.UI.Scripts;
using UnityEngine;

namespace Beamable.Inventory.Scripts
{
	[System.Serializable]
	public class InventoryItemEvent : UnityEngine.Events.UnityEvent<InventoryEventArgs> { }

	[System.Serializable]
	public class InventoryEventArgs
	{
		public InventoryGroup Group;
		public ItemView Item;
	}

	public class InventoryObjectUI : MonoBehaviour
	{
		public InventoryItemEvent OnReceived;
		protected ItemView Item;
		protected InventoryGroup Group;
		private InventoryEventArgs _args = new InventoryEventArgs();
		public virtual void Setup(InventoryGroup group, ItemView item)
		{
			// maybe do something???
			Group = group;
			Item = item;
			_args.Group = group;
			_args.Item = item;
			OnReceived?.Invoke(_args);
		}

		public void SetName(TextReference text)
		{
			text.Value = $"{Group.DisplayName} {Item.id}"; // items.hat ... 2
		}
	}
}
