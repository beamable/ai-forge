using Beamable.AccountManagement;
using Beamable.Stats;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Tournaments
{
	public enum TournamentLockState
	{
		TOP, LOW, UNLOCKED
	}

	public class TournamentEntryPlayerBehaviour : MonoBehaviour
	{
		public float TargetHeight = 70;
		public float LockLowPadding = 10;
		public float LockTopPadding = 10;
		public float LockLeftPadding = 5;
		public float LockRightPadding = 5;
		public RectTransform TargetTransform;
		public ScrollRect ScrollRect;
		public TournamentLockState State;
		public TournamentEntryBehavior EntryBehavior;
		public List<GameObject> ActiveOnlyWhenTopLocked, ActiveOnlyWhenLowLocked;
		public StatBehaviour AliasStatBehaviour;
		public StatBehaviour AvatarStatBehaviour;
		private GameObject _emptySlotObject;
		private TournamentLockState _lastState;
		private bool _isAbove, _isBelow;
		private void Start()
		{
			ScrollRect.onValueChanged.AddListener(HandleScrollEvent);

			AliasStatBehaviour.Stat = AccountManagementConfiguration.Instance.DisplayNameStat;
			AvatarStatBehaviour.Stat = AccountManagementConfiguration.Instance.AvatarStat;

			AliasStatBehaviour.OnStatReceived.AddListener(nextAlias =>
			{
				EntryBehavior.AliasReference.Value = nextAlias;
			});
			AvatarStatBehaviour.OnStatReceived.AddListener(nextAvatar =>
			{
				EntryBehavior.AvatarBehaviour.Refresh(nextAvatar);
			});
		}

		public void ProvideTrackingElement(GameObject emptySlotObject)
		{
			_emptySlotObject = emptySlotObject;
		}

		void Update()
		{
			if (_lastState != State)
			{
				SetState(State);
			}
		}

		void SetState(TournamentLockState nextState)
		{
			_lastState = nextState;
			State = nextState;
			var content = ScrollRect.content;

			switch (State)
			{
				case TournamentLockState.LOW:

					TargetTransform.SetParent(ScrollRect.viewport);

					TargetTransform.pivot = new Vector2(0, 0);
					TargetTransform.anchorMin = new Vector2(0, 0);
					TargetTransform.anchorMax = new Vector2(1, 0);
					TargetTransform.anchoredPosition = new Vector2(content.anchoredPosition.x + LockLeftPadding, LockLowPadding);
					TargetTransform.sizeDelta = new Vector2(content.sizeDelta.x - (LockRightPadding + LockLeftPadding), TargetHeight);
					ActivateLowObjects();
					break;
				case TournamentLockState.TOP:
					TargetTransform.SetParent(ScrollRect.viewport);

					TargetTransform.pivot = new Vector2(0, 1);
					TargetTransform.anchorMin = new Vector2(0, 1);
					TargetTransform.anchorMax = new Vector2(1, 1);
					TargetTransform.anchoredPosition = new Vector2(content.anchoredPosition.x + LockLeftPadding, -LockTopPadding);
					TargetTransform.sizeDelta = new Vector2(content.sizeDelta.x - (LockRightPadding + LockLeftPadding), TargetHeight);
					ActivateTopObjects();
					break;
				case TournamentLockState.UNLOCKED:
					var slot = _emptySlotObject.GetComponent<RectTransform>();
					TargetTransform.SetParent(slot);
					TargetTransform.pivot = new Vector2(.5f, .5f);
					TargetTransform.anchorMin = new Vector2(0, 0);
					TargetTransform.anchorMax = new Vector2(1, 1);
					TargetTransform.anchoredPosition = new Vector2(0, 0);
					TargetTransform.sizeDelta = new Vector2(0, 0);
					DeactivateTopLowObjects();
					break;
			}
		}

		void ActivateLowObjects()
		{
			ActiveOnlyWhenLowLocked.ForEach(g => g?.SetActive(true));
			ActiveOnlyWhenTopLocked.ForEach(g => g?.SetActive(false));
		}
		void ActivateTopObjects()
		{
			ActiveOnlyWhenLowLocked.ForEach(g => g?.SetActive(false));
			ActiveOnlyWhenTopLocked.ForEach(g => g?.SetActive(true));
		}
		void DeactivateTopLowObjects()
		{
			ActiveOnlyWhenLowLocked.ForEach(g => g?.SetActive(false));
			ActiveOnlyWhenTopLocked.ForEach(g => g?.SetActive(false));
		}

		private void HandleScrollEvent(Vector2 pos)
		{
			if (_emptySlotObject == null) return;

			var slot = _emptySlotObject.GetComponent<RectTransform>();

			var slotLow = -slot.offsetMin.y;
			var slotTop = -slot.offsetMax.y;

			var scrollTop = ScrollRect.content.offsetMax.y;
			var scrollLow = scrollTop + ScrollRect.viewport.rect.height;


			_isAbove = scrollTop > slotTop - LockTopPadding;
			_isBelow = scrollLow < slotLow + LockLowPadding;

			if (_isAbove)
			{
				// XXX: For UX reasons, we always want to lock the component to the bottom.
				//      If in the future, we want to lock to the top, change this line.
				SetState(TournamentLockState.LOW);
			}
			else if (_isBelow)
			{
				SetState(TournamentLockState.LOW);

			}
			else
			{
				SetState(TournamentLockState.UNLOCKED);
			}
		}
	}
}
