using Beamable.Avatars;
using Beamable.Common;
using Beamable.Common.Api.Auth;
using Beamable.Stats;
using Beamable.UI.Scripts;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.AccountManagement
{
	public class AccountManagementAdapter : MonoBehaviour
	{
		public virtual Promise<bool> DoesUserHaveThirdParty(User user, AuthThirdParty thirdParty)
		{
			return Promise<bool>.Successful(user.HasThirdPartyAssociation(thirdParty));
		}

		public virtual void HandleUserChange(MenuManagementBehaviour menuManager, User user)
		{
			var aliasStat = AccountManagementConfiguration.Instance.DisplayNameStat;

			user.GetStat(aliasStat).Then(alias =>
			{
				if (!menuManager) return;

				// only show the player data menu if there is no alias, and if there the menu wasn't just opened
				var isAliasUndefined = string.IsNullOrEmpty(alias);
				if (isAliasUndefined && !menuManager.IsFirst)
				{
					menuManager.Show<AccountPlayerDataMenu>();
				}
				else
				{
					menuManager.GoBackToPage<AccountMainMenu>();
				}
			});
		}

		public virtual void HandleAnonymousUserDataUpdated(MenuManagementBehaviour menuManager, User user)
		{
			menuManager.GoBackToPage<AccountMainMenu>();
		}

		public virtual bool IsPasswordStrong(string password)
		{
			return !string.IsNullOrEmpty(password) && password.Length > 3;
		}

		public virtual string GetErrorMessage(string errorCode)
		{
			switch (errorCode)
			{
				case ErrorEvent.SERVER_ERROR:
					return "Uh oh, something broke on our end. Please try again in a few minutes, or contact support.";
				case ErrorEvent.BAD_CREDENTIALS:
					return "Oops, that isn't the correct password";
				case ErrorEvent.PASSWORD_STRENGTH:
					return "Passwords must be at least 4 characters long";
				default:
					return "Something went wrong, sorry.";
			}
		}

		public virtual List<AccountAvatar> GetAvailableAvatars()
		{
			return AvatarConfiguration.Instance.Avatars;
		}

		public virtual AccountAvatar GetDefaultAvatar()
		{
			return AvatarConfiguration.Instance.Default;
		}

	}
}
