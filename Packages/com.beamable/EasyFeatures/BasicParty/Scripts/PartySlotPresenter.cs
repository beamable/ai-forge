using Beamable.UI.Scripts;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.EasyFeatures.BasicParty
{
	public class PartySlotPresenter : MonoBehaviour
	{
		public RectTransform RectTransform;
		public Image AvatarImage;
		public TextMeshProUGUI PlayerNameText;
		public TextMeshProUGUI AcceptButtonText;
		public Button AcceptButton;
		public Button AskToLeaveButton;
		public Button PromoteButton;
		public Button AddMemberButton;
		public GameObject ExpandableButtons;
		public GameObject OccupiedSlotGroup;
		public GameObject LeaderBadge;

		protected PlayersListPresenter ListPresenter;
		protected PoolData Item;
		protected bool IsExpandable;

		public struct ViewData
		{
			public string PlayerId;
			public string Name;
			public Sprite Avatar;
		}

		public class PoolData : PoolableScrollView.IItem
		{
			public ViewData ViewData { get; set; }
			public int Index { get; set; }
			public float Height { get; set; }
		}

		public void Setup(PoolData item,
						  PlayersListPresenter listPresenter,
						  bool isExpandable,
						  Action<string> onAcceptButton,
						  Action<string> onAskToLeaveButton,
						  Action<string> onPromoteButton,
						  Action onAddMemberButton)
		{
			bool isSlotOccupied = !string.IsNullOrWhiteSpace(item.ViewData.PlayerId);
			OccupiedSlotGroup.SetActive(isSlotOccupied);
			AddMemberButton.gameObject.SetActive(!isSlotOccupied);

			bool isLeader = BeamContext.Default.Party.IsInParty &&
							item.ViewData.PlayerId == BeamContext.Default.Party.Leader;
			LeaderBadge.SetActive(isLeader);
			AvatarImage.sprite = item.ViewData.Avatar;
			PlayerNameText.text = item.ViewData.Name;
			AcceptButton.gameObject.SetActive(onAcceptButton != null);
			AcceptButtonText.text = BeamContext.Default.Party.IsInParty ? "Invite" : "Accept";
			AcceptButton.onClick.ReplaceOrAddListener(() => onAcceptButton?.Invoke(item.ViewData.PlayerId));
			AskToLeaveButton.onClick.ReplaceOrAddListener(() => onAskToLeaveButton?.Invoke(item.ViewData.PlayerId));
			PromoteButton.onClick.ReplaceOrAddListener(() => onPromoteButton?.Invoke(item.ViewData.PlayerId));
			AddMemberButton.onClick.ReplaceOrAddListener(() => onAddMemberButton?.Invoke());

			ListPresenter = listPresenter;
			Item = item;
			IsExpandable = isExpandable;
		}

		public void ToggleExpand()
		{
			if (!IsExpandable || BeamContext.Default.Party.Leader == Item.ViewData.PlayerId)
			{
				return;
			}

			ExpandableButtons.SetActive(!ExpandableButtons.activeSelf);
			StartCoroutine(UpdateItemHeight());
		}

		protected IEnumerator UpdateItemHeight()
		{
			yield return new WaitForEndOfFrame();
			Item.Height = RectTransform.sizeDelta.y;
			ListPresenter.UpdateContent();
		}
	}
}
