using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Beamable.Tournaments
{
	[Serializable]
	public struct TabData
	{
		public Sprite Sprite;
		public Color Color;
		public Color TextColor;
		public Material Material;
	}

	[Serializable]
	public struct TabCollection
	{
		public Image Image;
		public Button Button;
		public TextMeshProUGUI Text;
		public TabData Active, Inactive;
	}

	[Serializable]
	public class TabChangeEventArgs
	{
		public TabCollection Tab;
		public int index;
	}

	[Serializable]
	public class TabChangeEvent : UnityEvent<TabChangeEventArgs> { }

	public class TabBehaviour : MonoBehaviour
	{
		public List<TabCollection> Tabs;
		public TabChangeEvent OnActive;
		public List<TabBehaviour> SyncActiveTab;

		[SerializeField]
		private int _activeTabIndex;
		// Start is called before the first frame update
		void Start()
		{
			for (var i = 0; i < Tabs.Count; i++)
			{
				var index = i;
				Tabs[i].Button.onClick.AddListener(() => SetActiveTab(index));
			}
		}

		private void OnEnable()
		{
			SetActiveTab(_activeTabIndex);
		}


		public void SetActiveTab(int index)
		{
			foreach (var other in SyncActiveTab)
			{
				other._activeTabIndex = index;
			}
			_activeTabIndex = index;
			if (!isActiveAndEnabled)
			{
				return;
			}

			Refresh();
			var arg = new TabChangeEventArgs
			{
				Tab = Tabs[index],
				index = index
			};
			OnActive?.Invoke(arg);
		}

		public void Refresh()
		{
			for (var i = 0; i < Tabs.Count; i++)
			{
				var tab = Tabs[i];
				var data = _activeTabIndex == i ? tab.Active : tab.Inactive;

				tab.Image.sprite = data.Sprite;
				tab.Image.color = data.Color;
				tab.Image.material = data.Material;
				tab.Text.color = data.TextColor;
			}
		}
	}
}
