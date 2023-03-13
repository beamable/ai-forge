using Beamable.Common.Api.Auth;
using Beamable.UI.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.AccountManagement
{
	public class AccountSwitch : MenuBase
	{
		public Button newGameButton, emailButton, facebookButton, appleButton, googleButton;

		// TODO refactor entire menu thing to allow for instances instead of prefabs.
		public AccountManagementBehaviour AccountBehaviour;

		public override void OnOpened()
		{
			facebookButton.gameObject.SetActive(AccountBehaviour.Configuration.AuthEnabled(AuthThirdParty.Facebook));
			appleButton.gameObject.SetActive(
			   (Application.platform == RuntimePlatform.IPhonePlayer ||
				Application.platform == RuntimePlatform.tvOS) &&
			   AccountBehaviour.Configuration.AuthEnabled(AuthThirdParty.Apple)
			);
			googleButton.gameObject.SetActive(AccountBehaviour.Configuration.AuthEnabled(AuthThirdParty.Google));
			SetAllButtonInteractive(true);
		}

		public void OnEmailPressed()
		{
			Manager.Show<AccountEmailLogin>();
		}

		public void SetAllButtonInteractive(bool interactive)
		{
			if (newGameButton != null)
			{
				newGameButton.interactable = interactive;
			}

			if (emailButton != null)
			{
				emailButton.interactable = interactive;
			}

			if (facebookButton != null)
			{
				facebookButton.interactable = interactive;
			}

			if (appleButton != null)
			{
				appleButton.interactable = interactive;
			}
		}
	}
}
