using Beamable.UI.Scripts;
using System;
using UnityEngine;

namespace Beamable.Inventory.Scripts
{
	public class InventoryMainMenu : MenuBase
	{
		public RectTransform GroupContainer;
		public InventoryGroupUI GroupUIPrefab;
		public InventoryMenuBehaviour RootMenu;
		public InventoryMenuConfiguration Data => RootMenu.InventoryConfig;

		// Start is called before the first frame update
		void Start()
		{


		}

		public override void OnOpened()
		{
			base.OnOpened();
			RefreshGroups();

		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (GroupContainer == null || Data.ItemPreviewPrefab == null) return;
			if (Data.ItemPreviewPrefab.transform.IsChildOf(GroupContainer))
			{
				string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath("5e01421c531e24cdc90b41b04cd49ef5");
				Debug.LogError($"You should not use child of a {nameof(InventoryMainMenu)} " +
				               $"{nameof(GroupContainer)} as a {nameof(Data.ItemPreviewPrefab)}. " +
				               $"Try using `InventoryObjectUI.prefab` from Beamable package at path: {assetPath}");
				
			}
		}
#endif


		// Update is called once per frame
		void Update()
		{

		}

		void RefreshGroups()
		{
			for (var i = GroupContainer.childCount - 1; i >= 0; i--)
			{
				Destroy(GroupContainer.GetChild(i).gameObject);
			}

			foreach (var group in Data.Groups)
			{
				var gob = Instantiate(GroupUIPrefab, GroupContainer);
				gob.InventoryObjectUIPrefab = Data.ItemPreviewPrefab;
				gob.Setup(group);
			}
		}
	}
}
