using Beamable.Avatars;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.AccountManagement
{
	public class AccountAvatarBehaviour : MonoBehaviour
	{
		public Image Renderer;
		public Button Button;

		public AccountAvatar Avatar => _avatar ?? AccountManagementConfiguration.Instance.Overrides.GetDefaultAvatar();
		public string AvatarName => _lastAvatarName;

		private AccountAvatar _avatar;
		private bool _hasRefreshPending;
		private string _lastAvatarName;

		private void Update()
		{
			if (_hasRefreshPending)
			{
				Refresh(_lastAvatarName);
			}
		}

		public void Set(AccountAvatar avatar)
		{
			_avatar = avatar;
			_lastAvatarName = avatar.Name;
			Renderer.sprite = avatar.Sprite;
			Renderer.enabled = true;
		}

		public void Refresh(string avatarName)
		{

			_lastAvatarName = avatarName;

			var avatar = AccountManagementConfiguration.Instance.Overrides.GetAvailableAvatars().FirstOrDefault(a => a.Name.Equals(avatarName));
			if (avatar == null)
			{
				avatar = AccountManagementConfiguration.Instance.Overrides.GetDefaultAvatar();
			}

			Set(avatar);
			_hasRefreshPending = false;
		}
	}
}
