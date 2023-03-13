using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Experimental.Api.Lobbies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.EasyFeatures.BasicLobby
{
	public class JoinLobbyPlayerSystem : JoinLobbyView.IDependencies
	{
		public BeamContext BeamContext { get; }
		public List<SimGameType> GameTypes { get; set; } = new List<SimGameType>();
		public bool HasInitialData { get; set; }
		public bool IsLoading { get; set; }
		public int SelectedGameTypeIndex { get; set; }
		public int? SelectedLobbyIndex { get; set; }
		public string NameFilter { get; set; }
		public string Passcode { get; set; }
		public int CurrentPlayersFilter { get; set; }
		public int MaxPlayersFilter { get; set; }
		public List<LobbiesListEntryPresenter.ViewData> LobbiesData { get; private set; }
		public int MinPasscodeLength => 5;

		public readonly Dictionary<string, List<string>> PerGameTypeLobbiesIds = new Dictionary<string, List<string>>();

		public readonly Dictionary<string, List<string>> PerGameTypeLobbiesNames =
			new Dictionary<string, List<string>>();

		public readonly Dictionary<string, List<string>> PerGameTypeLobbiesDescriptions =
			new Dictionary<string, List<string>>();

		public readonly Dictionary<string, List<List<LobbyPlayer>>> PerGameTypeLobbiesCurrentPlayers =
			new Dictionary<string, List<List<LobbyPlayer>>>();

		public readonly Dictionary<string, List<int>>
			PerGameTypeLobbiesMaxPlayers = new Dictionary<string, List<int>>();

		public virtual SimGameType SelectedGameType => GameTypes[SelectedGameTypeIndex];
		public virtual string SelectedGameTypeId => SelectedGameType.Id;
		public virtual IReadOnlyList<string> Ids => PerGameTypeLobbiesIds[SelectedGameTypeId];
		public virtual IReadOnlyList<string> Names => PerGameTypeLobbiesNames[SelectedGameTypeId];
		public virtual IReadOnlyList<string> Descriptions => PerGameTypeLobbiesDescriptions[SelectedGameTypeId];

		public virtual IReadOnlyList<List<LobbyPlayer>> CurrentPlayers =>
			PerGameTypeLobbiesCurrentPlayers[SelectedGameTypeId];

		public virtual IReadOnlyList<int> MaxPlayers => PerGameTypeLobbiesMaxPlayers[SelectedGameTypeId];

		public JoinLobbyPlayerSystem(BeamContext beamContext)
		{
			BeamContext = beamContext;
		}

		public virtual void Setup(List<SimGameType> gameTypes)
		{
			GameTypes = gameTypes;

			SelectedGameTypeIndex = 0;
			SelectedLobbyIndex = null;
			NameFilter = string.Empty;
			Passcode = string.Empty;
		}

		public virtual void RegisterLobbyData(SimGameType gameType, List<Lobby> data) =>
			RegisterLobbyData(gameType.Id, data);

		public virtual void RegisterLobbyData(SimGameTypeRef gameTypeRef, List<Lobby> data) =>
			RegisterLobbyData(gameTypeRef.Id, data);

		public virtual void RegisterLobbyData(string gameTypeId, List<Lobby> data)
		{
			PerGameTypeLobbiesIds.TryGetValue(gameTypeId, out var ids);
			PerGameTypeLobbiesNames.TryGetValue(gameTypeId, out var names);
			PerGameTypeLobbiesDescriptions.TryGetValue(gameTypeId, out var descriptions);
			PerGameTypeLobbiesCurrentPlayers.TryGetValue(gameTypeId, out var currentPlayers);
			PerGameTypeLobbiesMaxPlayers.TryGetValue(gameTypeId, out var maxPlayers);

			BuildLobbiesClientData(data, ref ids, ref names, ref descriptions, ref currentPlayers, ref maxPlayers);

			if (PerGameTypeLobbiesIds.ContainsKey(gameTypeId))
			{
				PerGameTypeLobbiesIds[gameTypeId] = ids;
			}
			else
			{
				PerGameTypeLobbiesIds.Add(gameTypeId, ids);
			}

			if (PerGameTypeLobbiesNames.ContainsKey(gameTypeId))
			{
				PerGameTypeLobbiesNames[gameTypeId] = names;
			}
			else
			{
				PerGameTypeLobbiesNames.Add(gameTypeId, names);
			}

			if (PerGameTypeLobbiesDescriptions.ContainsKey(gameTypeId))
			{
				PerGameTypeLobbiesDescriptions[gameTypeId] = descriptions;
			}
			else
			{
				PerGameTypeLobbiesDescriptions.Add(gameTypeId, descriptions);
			}

			if (PerGameTypeLobbiesCurrentPlayers.ContainsKey(gameTypeId))
			{
				PerGameTypeLobbiesCurrentPlayers[gameTypeId] = currentPlayers;
			}
			else
			{
				PerGameTypeLobbiesCurrentPlayers.Add(gameTypeId, currentPlayers);
			}

			if (PerGameTypeLobbiesMaxPlayers.ContainsKey(gameTypeId))
			{
				PerGameTypeLobbiesMaxPlayers[gameTypeId] = maxPlayers;
			}
			else
			{
				PerGameTypeLobbiesMaxPlayers.Add(gameTypeId, maxPlayers);
			}

			LobbiesData = BuildViewData();
		}

		/// <summary>
		/// The actual data transformation function that converts lobbies entries into data that is relevant for our <see cref="JoinLobbyView.IDependencies"/>. 
		/// </summary>
		public virtual void BuildLobbiesClientData(List<Lobby> entries,
												   ref List<string> ids,
												   ref List<string> names,
												   ref List<string> descriptions,
												   ref List<List<LobbyPlayer>> currentPlayers,
												   ref List<int> maxPlayers)
		{
			void GuaranteeInitList<T>(ref List<T> toInit)
			{
				if (toInit != null) toInit.Clear();
				else toInit = new List<T>();
			}

			GuaranteeInitList(ref ids);
			ids.AddRange(entries.Select(lobby => lobby.lobbyId));

			GuaranteeInitList(ref names);

			IEnumerable<string> enumerable = entries.Select(lobby => lobby.name);

			names.AddRange(entries.Select(lobby => lobby.name));

			GuaranteeInitList(ref descriptions);
			descriptions.AddRange(entries.Select(lobby => lobby.description));

			GuaranteeInitList(ref currentPlayers);
			currentPlayers.AddRange(entries.Select(lobby => lobby.players));

			GuaranteeInitList(ref maxPlayers);
			maxPlayers.AddRange(entries.Select(lobby => lobby.maxPlayers));
		}

		public virtual void ClearLobbyData(SimGameType gameType) => ClearLobbyData(gameType.Id);

		public virtual void ClearLobbyData(SimGameTypeRef gameTypeRef) => ClearLobbyData(gameTypeRef.Id);

		public virtual void ClearLobbyData(string gameTypeId)
		{
			PerGameTypeLobbiesNames.Remove(gameTypeId);
			PerGameTypeLobbiesCurrentPlayers.Remove(gameTypeId);
			PerGameTypeLobbiesMaxPlayers.Remove(gameTypeId);
		}

		public virtual void OnLobbySelected(int? lobbyIndex)
		{
			SelectedLobbyIndex = lobbyIndex;
			Passcode = string.Empty;
		}

		public virtual bool CanJoinLobby()
		{
			if (Passcode != String.Empty)
			{
				return Passcode.Length >= MinPasscodeLength;
			}

			if (SelectedLobbyIndex == null)
			{
				return false;
			}

			return LobbiesData[SelectedLobbyIndex.Value].CurrentPlayers <
				   LobbiesData[SelectedLobbyIndex.Value].MaxPlayers;
		}

		public async Promise JoinLobby()
		{
			if (Passcode != string.Empty)
			{
				if (Passcode.Length >= MinPasscodeLength)
				{
					await BeamContext.Lobby.JoinByPasscode(Passcode);
				}
			}
			else
			{
				if (SelectedLobbyIndex != null)
				{
					await BeamContext.Lobby.Join(LobbiesData[SelectedLobbyIndex.Value].Id);
				}
			}
		}

		public async Promise GetLobbies()
		{
			LobbyQueryResponse response = await BeamContext.Lobby.FindLobbiesOfType(SelectedGameType.Id);
			RegisterLobbyData(GameTypes[SelectedGameTypeIndex], response.results);
		}

		public virtual void ApplyPasscode(string passcode)
		{
			Passcode = passcode;
			SelectedLobbyIndex = null;
		}

		public virtual void ApplyFilter(string name)
		{
			ApplyFilter(name, CurrentPlayers.Count, MaxPlayers.Count);
		}

		public virtual void ApplyFilter(string name, int currentPlayers, int maxPlayers)
		{
			NameFilter = name;
			CurrentPlayersFilter = currentPlayers;
			MaxPlayersFilter = maxPlayers;
		}

		public virtual List<LobbiesListEntryPresenter.ViewData> BuildViewData()
		{
			int entriesCount = Names.Count;

			List<LobbiesListEntryPresenter.ViewData> data = new List<LobbiesListEntryPresenter.ViewData>();

			for (int i = 0; i < entriesCount; i++)
			{
				data.Add(new LobbiesListEntryPresenter.ViewData
				{
					Id = Ids[i],
					Name = Names[i],
					Description = Descriptions[i],
					CurrentPlayers = CurrentPlayers[i].Count,
					MaxPlayers = MaxPlayers[i]
				});
			}

			return NameFilter == string.Empty
				? data
				: data.Where(entry => entry.Name.Contains(NameFilter)).ToList();
		}
	}
}
