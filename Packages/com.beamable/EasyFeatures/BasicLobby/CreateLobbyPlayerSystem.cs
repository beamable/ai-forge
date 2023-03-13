using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Experimental.Api.Lobbies;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.EasyFeatures.BasicLobby
{
	public class CreateLobbyPlayerSystem : CreateLobbyView.IDependencies
	{
		public BeamContext BeamContext { get; }

		public List<SimGameType> GameTypes { get; set; }
		public int SelectedGameTypeIndex { get; set; }
		public Dictionary<string, LobbyRestriction> AccessOptions { get; } = new Dictionary<string, LobbyRestriction>();
		public int SelectedAccessOption { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }

		public CreateLobbyPlayerSystem(BeamContext beamContext)
		{
			BeamContext = beamContext;
		}

		public virtual void Setup(List<SimGameType> gameTypes)
		{
			GameTypes = gameTypes;

			ResetData();

			AccessOptions.Clear();
			AccessOptions.Add("Public", LobbyRestriction.Open);
			AccessOptions.Add("Private", LobbyRestriction.Closed);
		}

		public virtual bool ValidateConfirmButton()
		{
			return Name.Length > 5;
		}

		public virtual void ResetData()
		{
			SelectedGameTypeIndex = 0;
			SelectedAccessOption = 0;
			Name = string.Empty;
			Description = string.Empty;
		}

		public async Promise CreateLobby()
		{
			await BeamContext.Lobby.Create(Name,
										   AccessOptions.ElementAt(SelectedAccessOption).Value,
										   GameTypes[SelectedGameTypeIndex].Id,
										   Description,
										   maxPlayers: GameTypes[SelectedGameTypeIndex].CalculateMaxPlayers());
		}
	}
}
