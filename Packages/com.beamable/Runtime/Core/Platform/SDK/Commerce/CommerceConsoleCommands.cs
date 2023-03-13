using Beamable.Common.Dependencies;
using Beamable.ConsoleCommands;
using UnityEngine;
using UnityEngine.Scripting;

namespace Beamable.Api.Commerce
{
	[BeamableConsoleCommandProvider]
	public class CommerceConsoleCommands
	{
		private readonly IDependencyProvider _provider;
		private BeamableConsole Console => _provider.GetService<BeamableConsole>();
		private CommerceService Commerce => _provider.GetService<CommerceService>();


		[Preserve]
		public CommerceConsoleCommands(IDependencyProvider provider)
		{
			_provider = provider;
		}

		[BeamableConsoleCommand("STORE-VIEW", "Outputs the player view of a specific store to the console (in json)", "STORE-VIEW stores.default")]
		protected string PreviewCurrency(params string[] args)
		{
			string store;
			if (args.Length > 0)
			{
				store = args[0];
			}
			else
			{
				return "Please specify the content id of the store.";
			}

			Commerce.GetCurrent(store).Then(response =>
			{
				string json = JsonUtility.ToJson(response);
				Debug.Log($"Player Store View: {json}");
			});

			return "Fetching player store view...";
		}
	}

}
