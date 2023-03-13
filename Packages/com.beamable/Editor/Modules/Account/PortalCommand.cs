using Beamable.Common;
using Beamable.ConsoleCommands;
using System.Threading.Tasks;
using UnityEngine;

namespace Beamable.Editor.Modules.Account
{
	[BeamableConsoleCommandProvider]
	public class PortalCommand
	{
		private readonly BeamContext _ctx;

		public PortalCommand(BeamContext ctx)
		{
			_ctx = ctx;
		}

		[BeamableConsoleCommand("portal", "Opens portal for the current user", "portal")]
		private string OpenPortal(string[] args)
		{
			_ctx.OnReady.Then(_ =>
			{
				var DBID = _ctx.PlayerId;
				Debug.Log($"Current user: {DBID}");
				GetPortalUrl(DBID).Then(Application.OpenURL);
			});
			return "Opening portal..";
		}
		private Promise<string> GetPortalUrl(long DBID)
		{
			var api = BeamEditorContext.Default;
			return Promise<string>.Successful($"{BeamableEnvironment.PortalUrl}/{api.CurrentCustomer.Alias}/games/{api.ProductionRealm.Pid}/realms/{api.CurrentRealm.Pid}/players/{DBID}?refresh_token={api.Requester.Token.RefreshToken}");
		}
	}
}
