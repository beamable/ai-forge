using Beamable.AccountManagement;
using Beamable.Coroutines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Beamable.Avatars
{
	[System.Serializable]
	public class AccountAvatarEvent : UnityEvent<AccountAvatar> { }

	public class AvatarPickerBehaviour : MonoBehaviour
	{
		public RectTransform ContentContainer;
		public ScrollRect AvatarScroller;

		public GameObject AvatarSelectionIndicator;
		public AccountAvatarBehaviour AvatarPreviewPrefab;
		public AccountAvatarEvent OnSelected;

		public AccountAvatar Selected
		{
			get => _selected;
			set
			{
				Select(value?.Name);
			}
		}

		private AccountAvatar _selected;

		private List<AccountAvatarBehaviour> _avatarPreviews;

		// Start is called before the first frame update
		void Start()
		{
			Refresh();
		}

		public void Select(string avatarName)
		{
			if (_avatarPreviews == null)
			{
				Refresh();
			}

			var found = _avatarPreviews.Find(a => string.Equals(avatarName, a.AvatarName));
			PositionSelection(found);
			_selected = found?.Avatar;
		}

		public void Refresh()
		{
			// remove all avatars, and recreate them.
			PositionSelection(null);

			for (var i = 0; i < ContentContainer.childCount; i++)
			{
				Destroy(ContentContainer.GetChild(i).gameObject);
			}

			var avatars = AccountManagementConfiguration.Instance.Overrides.GetAvailableAvatars();
			_avatarPreviews = new List<AccountAvatarBehaviour>();
			foreach (var avatar in avatars)
			{
				var avatarPreview = Instantiate(AvatarPreviewPrefab, ContentContainer);
				avatarPreview.Button.onClick.AddListener(() => SetPreviewAvatar(avatarPreview, avatar));
				_avatarPreviews.Add(avatarPreview);
				avatarPreview.Set(avatar);
			}
		}

		public void SetPreviewAvatar(AccountAvatarBehaviour instance, AccountAvatar avatar)
		{
			Selected = avatar;
			OnSelected?.Invoke(Selected);
		}

		public void PositionSelection(AccountAvatarBehaviour selected)
		{
			if (selected == null)
			{
				AvatarSelectionIndicator.transform.SetParent(transform, false);
				AvatarSelectionIndicator?.SetActive(false);
				return;
			}
			AvatarSelectionIndicator.SetActive(true);
			AvatarSelectionIndicator.transform.SetParent(selected.transform, false);
			AvatarSelectionIndicator.transform.localPosition = Vector3.zero;

			Canvas.ForceUpdateCanvases();
			var currentScrollPosition = AvatarScroller.horizontalScrollbar.value * (AvatarScroller.content.rect.width - AvatarScroller.viewport.rect.width);
			var desiredScrollPosition = selected.transform.localPosition.x - .5f * AvatarScroller.viewport.rect.width;
			AvatarScroller.velocity = new Vector2(2 * (currentScrollPosition - desiredScrollPosition), 0);
		}
	}
}
