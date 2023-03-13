using Beamable.UI.Scripts;
using UnityEngine;

namespace Beamable.Inventory.Scripts
{
	public class InventoryGroupUI : MonoBehaviour
	{
		public InventoryBehaviour InventoryBehaviour;

		public TextReference Title;

		public RectTransform InventoryObjectsContainer;
		public InventoryObjectUI InventoryObjectUIPrefab;

		// Start is called before the first frame update
		void Start()
		{
			if (InventoryObjectUIPrefab == null)
			{
				InventoryObjectUIPrefab = InventoryConfiguration.Instance.DefaultObjectPrefab;
			}
		}

		// Update is called once per frame
		void Update()
		{

		}

		public void Setup(InventoryGroup group)
		{
			InventoryBehaviour.Group = group;
			Title.Value = group.DisplayName;
			ClearInventory();
		}

		public void ClearInventory()
		{
			for (var i = 0; i < InventoryObjectsContainer.childCount; i++)
			{
				Destroy(InventoryObjectsContainer.GetChild(i).gameObject);
			}
		}

		public void ReceiveInventory(InventoryUpdateArg view)
		{
			ClearInventory();

			foreach (var item in view.Inventory)
			{
				var instance = Instantiate(InventoryObjectUIPrefab, InventoryObjectsContainer);
				instance.Setup(view.Group, item);
			}
		}
	}
}
