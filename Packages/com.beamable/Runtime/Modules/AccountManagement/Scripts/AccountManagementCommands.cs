using Beamable.Common.Api.Auth;
using Beamable.ConsoleCommands;
using Beamable.Signals;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting;

namespace Beamable.AccountManagement
{
	[BeamableConsoleCommandProvider]
	public class AccountManagementCommands
	{
		private readonly BeamContext _ctx;

		[Preserve]
		public AccountManagementCommands(BeamContext ctx)
		{
			_ctx = ctx;
		}

		[BeamableConsoleCommand("account_toggle", "emit an account management toggle event", "account_toggle")]
		public string ToggleAccount(string[] args)
		{
			DeSignalTower.ForAll<AccountManagementSignals>(
				s => s.OnToggleAccountManagement?.Invoke(!AccountManagementSignals.ToggleState));
			return "okay";
		}

		[BeamableConsoleCommand("account_list", "list user data", "account_list")]
		public string ListCredentials(string[] args)
		{
			_ctx.OnReady.Map(_ => _ctx.Api).Then(de =>
			{
				de.GetDeviceUsers().Then(all =>
				{
					all.ToList().ForEach(bundle =>
					{
						User user = bundle.User;
						string userType = user.id == de.User.id ? "CURRENT" : "DEVICE";
						Debug.Log(
							$"{userType} : EMAIL: [{user.email}] ID: [{user.id}] 3RD PARTIES: [{string.Join(",", user.thirdPartyAppAssociations)}]" +
							$" DEVICE IDS: [{string.Join(",", user.deviceIds)}]");
					});
				});
			});
			return string.Empty;
		}
	}
}
