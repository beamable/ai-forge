using Beamable.UI.Scripts;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.EasyFeatures.BasicLobby
{
	public class LobbySlotPresenter : MonoBehaviour
	{
		public class ViewData : PoolableScrollView.IItem
		{
			public float FoldedHeight => 150.0f;
			public float UnfoldedHeight => 300.0f;

			public string PlayerId { get; set; }
			public bool IsReady { get; set; }
			public float Height { get; set; }
			public int Index { get; set; }
			public bool IsUnfolded { get; set; }
		}

		[Header("Components")]
		public GameObject Filled;
		public GameObject Empty;
		public TextMeshProUGUI Name;
		public GameObject ReadyTag;
		public GameObject NotReadyTag;
		public GameObject ButtonsGroup;
		public Button AdminButton;
		public Button KickButton;
		public Button PassLeadershipButton;

		public void SetupEmpty()
		{
			Empty.SetActive(true);
			Filled.SetActive(false);
			ButtonsGroup.SetActive(false);
		}

		public void SetupFilled(string playerName,
								bool isReady,
								bool isAdmin,
								bool isUnfolded,
								Action onAdminButtonClicked,
								Action onKickButtonClicked,
								Action onPassLeadershipButtonClicked)
		{
			Name.text = playerName;

			KickButton.onClick.ReplaceOrAddListener(onKickButtonClicked.Invoke);
			PassLeadershipButton.onClick.ReplaceOrAddListener(onPassLeadershipButtonClicked.Invoke);
			AdminButton.onClick.ReplaceOrAddListener(onAdminButtonClicked.Invoke);

			Empty.SetActive(false);
			Filled.SetActive(true);
			ButtonsGroup.SetActive(isUnfolded);
			ReadyTag.gameObject.SetActive(isReady);
			NotReadyTag.gameObject.SetActive(!isReady);
			AdminButton.interactable = isAdmin;
		}
	}
}
