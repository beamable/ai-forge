using Beamable.Common.Dependencies;
using Beamable.ConsoleCommands;
using UnityEngine;
using UnityEngine.Scripting;

namespace Beamable.Api.CloudData
{
	[BeamableConsoleCommandProvider]
	public class CloudDataConsoleCommands
	{
		private readonly IDependencyProvider _provider;
		private BeamableConsole Console => _provider.GetService<BeamableConsole>();
		private CloudDataService CloudDataService => _provider.GetService<CloudDataService>();

		[Preserve]
		public CloudDataConsoleCommands(IDependencyProvider provider)
		{
			_provider = provider;
		}


		[BeamableConsoleCommand("CLOUD-MANIFEST", "Fetch the game cloud manifest", "CLOUD-MANIFEST")]
		protected string GetManifest(params string[] args)
		{
			CloudDataService.GetGameManifest().Then(response =>
			{
				string json = JsonUtility.ToJson(response);
				Debug.Log($"Game Cloud Manifest: {json}");
			});

			return "Fetching Cloud Game Manifest...";
		}

		[BeamableConsoleCommand("CLOUD-PLAYER", "Fetch the player cloud manifest", "CLOUD-PLAYER")]
		protected string GetPlayerManifest(params string[] args)
		{
			CloudDataService.GetPlayerManifest().Then(response =>
			{
				string json = JsonUtility.ToJson(response);
				Debug.Log($"Player Cloud Manifest for current player: {json}");
			});

			return $"Fetching Cloud Player Manifest for current player...";
		}
	}

	[System.Serializable]
	public class TestCloudData
	{
		public string name;
	}
}
