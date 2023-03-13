using Beamable.Common.Api.Auth;
using Beamable.UI.Scripts;

namespace Beamable.AccountManagement
{
	public class AccountThirdPartyWaitingMenu : MenuBase
	{

		public TextReference ThirdPartyMessage;

		public void GoBackToMainPage()
		{
			Manager.GoBackToPage<AccountMainMenu>();
		}

		public void SetFor(AuthThirdParty argThirdParty)
		{
			ThirdPartyMessage.Value = $"Signing into {argThirdParty}";
		}

	}
}
