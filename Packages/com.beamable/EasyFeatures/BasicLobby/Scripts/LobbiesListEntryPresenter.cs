using Beamable.UI.Buss;
using Beamable.UI.Scripts;
using EasyFeatures.Components;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.EasyFeatures.BasicLobby
{
	public class LobbiesListEntryPresenter : MonoBehaviour
	{
		public struct ViewData
		{
			public string Id;
			public string Name;
			public string Description;
			public int CurrentPlayers;
			public int MaxPlayers;
		}

		public class PoolData : PoolableScrollView.IItem
		{
			public ViewData ViewData { get; set; }
			public float Height { get; set; }
			public int Index { get; set; }
		}

		[Header("Components")]
		public TextMeshProUGUI Name;
		public TextMeshProUGUI Users;
		public Button Button;

		public BussElement FrameBussElement;

		private Action<LobbiesListEntryPresenter> _onLobbySelected;

		public void Setup(ViewData viewData, Action<LobbiesListEntryPresenter> onLobbySelected)
		{
			Name.text = viewData.Name;
			Users.text = $"{viewData.CurrentPlayers}/{viewData.MaxPlayers}";
			_onLobbySelected = onLobbySelected;

			Button.onClick.ReplaceOrAddListener(OnClick);
			FrameBussElement.UpdateClasses(new List<string> { "panel", "lobby" });

			SetSelected(false);
		}

		private void OnClick()
		{
			_onLobbySelected.Invoke(this);
		}

		public void SetSelected(bool value)
		{
			FrameBussElement.SetSelected(value);
		}
	}
}
