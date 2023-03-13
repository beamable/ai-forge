using Beamable.Common.Inventory;
using Beamable.Inventory.Scripts;
using Beamable.UI.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Inventory
{
	public class ItemImageSetter : MonoBehaviour
	{
		public Image Image;
		public ItemRef Item;

		private void Start()
		{
			Refresh();
		}

		public void SetItem(ItemRef item)
		{
			Item = item;
			Refresh();
		}

		public void SetItem(InventoryEventArgs args)
		{
			Item = args.Group.ItemRef;
			Refresh();
		}

		public void Refresh()
		{
			if (Image == null || Item == null) return;
			Item.Resolve().Then(async content =>
			{
				if (content.icon == null || !content.icon.RuntimeKeyIsValid()) return;
				Image.sprite = await content.icon.LoadSprite();
			}).Error(Debug.LogError);
		}
	}
}
