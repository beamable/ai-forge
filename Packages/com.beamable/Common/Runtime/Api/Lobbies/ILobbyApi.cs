using Beamable.Common;
using Beamable.Common.Content;
using System.Collections.Generic;

namespace Beamable.Experimental.Api.Lobbies
{
	public interface ILobbyApi
	{
		/// <summary>
		/// Find lobbies for the player to join.
		/// </summary>
		/// <returns>A <see cref="Promise{LobbyQueryResponse}"/> representing a list of public lobbies.</returns>
		Promise<LobbyQueryResponse> FindLobbies();

		/// <summary>
		/// Find lobbies of specified game type.
		/// </summary>
		/// <param name="skip">Amount of lobbies skipped in response</param>
		/// <param name="limit">Amount of lobbies returned in response</param>
		/// <param name="matchType">Game type id</param>
		/// <returns>A <see cref="Promise{LobbyQueryResponse}"/> representing a list of public lobbies.</returns>
		Promise<LobbyQueryResponse> FindLobbiesOfType(string matchType, int limit, int skip);

		/// <summary>
		/// Create a new <see cref="Lobby"/> with the current player as the host.
		/// </summary>
		/// <param name="name">Name of the lobby</param>
		/// <param name="restriction">The privacy value for the created lobby.</param>
		/// <param name="gameTypeId">If this lobby should be subject to matchmaking, a gametype id should be provided</param>
		/// <param name="description">Short optional description of what the lobby is for.</param>
		/// <param name="playerTags">Arbitrary list of tags to include on the creating player.</param>
		/// <param name="passcodeLength">Configurable value for how long the generated passcode should be.</param>
		/// <param name="maxPlayers">Configurable value for the maximum number of players this lobby can have.</param>
		/// <param name="statsToInclude">Stat keys to include with Lobby requests.</param>
		/// <returns>A <see cref="Promise{Lobby}"/> representing the created lobby.</returns>
		Promise<Lobby> CreateLobby(string name,
								   LobbyRestriction restriction,
								   string gameTypeId = null,
								   string description = null,
								   List<Tag> playerTags = null,
								   int? maxPlayers = null,
								   int? passcodeLength = null,
								   List<string> statsToInclude = null);

		/// <summary>
		/// Create a new <see cref="Lobby"/> with the current player as the host.
		/// </summary>
		/// <param name="name">Name of the lobby</param>
		/// <param name="restriction">The privacy value for the created lobby.</param>
		/// <param name="gameTypeRef">If this lobby should be subject to matchmaking, a gametype ref should be provided</param>
		/// <param name="description">Short optional description of what the lobby is for.</param>
		/// <param name="playerTags">Arbitrary list of tags to include on the creating player.</param>
		/// <param name="passcodeLength">Configurable value for how long the generated passcode should be.</param>
		/// <param name="maxPlayers">Configurable value for the maximum number of players this lobby can have.</param>
		/// <param name="statsToInclude">Stat keys to include with Lobby requests.</param>
		/// <returns>A <see cref="Promise{Lobby}"/> representing the created lobby.</returns>
		Promise<Lobby> CreateLobby(string name,
								   LobbyRestriction restriction,
								   SimGameTypeRef gameTypeRef = null,
								   string description = null,
								   List<Tag> playerTags = null,
								   int? maxPlayers = null,
								   int? passcodeLength = null,
								   List<string> statsToInclude = null);

		/// <summary>
		/// Join a <see cref="Lobby"/> given its id.
		/// </summary>
		/// <param name="lobbyId">The id of the <see cref="Lobby"/> to join.</param>
		/// <param name="playerTags">List of <see cref="Tag"/> to associate with the joining player.</param>
		/// <returns>A <see cref="Promise{Lobby}"/> representing the modified lobby.</returns>
		Promise<Lobby> JoinLobby(string lobbyId, List<Tag> playerTags = null);

		/// <summary>
		/// Join a <see cref="Lobby"/> given its passcode.
		/// </summary>
		/// <param name="passcode">The passcode of the <see cref="Lobby"/> to join.</param>
		/// <param name="playerTags">List of <see cref="Tag"/> to associate with the joining player.</param>
		/// <returns>A <see cref="Promise{Lobby}"/> representing the modified lobby.</returns>
		Promise<Lobby> JoinLobbyByPasscode(string passcode, List<Tag> playerTags = null);

		/// <summary>
		/// Fetch the current status of a <see cref="Lobby"/>.
		/// </summary>
		/// <param name="lobbyId">The id of the <see cref="Lobby"/>.</param>
		/// <returns>A <see cref="Promise{Lobby}"/> representing the modified lobby.</returns>
		Promise<Lobby> GetLobby(string lobbyId);

		/// <summary>
		/// Notify the given lobby that the player intends to leave.
		/// </summary>
		/// <param name="lobbyId">The id of the <see cref="Lobby"/> to leave.</param>
		Promise LeaveLobby(string lobbyId);

		/// <summary>
		/// Add a list of <see cref="Tag"/> to the given player in the given lobby.
		/// </summary>
		/// <param name="lobbyId">The id of the <see cref="Lobby"/>.</param>
		/// <param name="tags">List of <see cref="Tag"/> to associate with the player.</param>
		/// <param name="playerId">The id of the player.</param>
		/// <returns>A <see cref="Promise{Lobby}"/> representing the modified lobby.</returns>
		Promise<Lobby> AddPlayerTags(string lobbyId, List<Tag> tags, string playerId = null, bool replace = false);

		/// <summary>
		/// Remove a list of tags from the given player in the given <see cref="Lobby"/>.
		/// </summary>
		/// <param name="lobbyId">The id of the <see cref="Lobby"/>.</param>
		/// <param name="tags">List of <see cref="Tag"/> to remove from the player.</param>
		/// <param name="playerId">The id of the player.</param>
		/// <returns>A <see cref="Promise{Lobby}"/> representing the modified lobby.</returns>
		Promise<Lobby> RemovePlayerTags(string lobbyId, List<string> tags, string playerId = null);

		/// <summary>
		/// Send a request to the given <see cref="Lobby"/> to remove the player with the given playerId. If the
		/// requesting player doesn't have the capability to boot players, this will throw an exception.
		/// </summary>
		/// <param name="lobbyId">The id of the <see cref="Lobby"/>.</param>
		/// <param name="playerId">The id of the player to remove.</param>
		Promise<Lobby> KickPlayer(string lobbyId, string playerId);

		/// <summary>
		/// Send a request to the given <see cref="Lobby"/> to update its data. If the
		/// requesting player doesn't have the capability to boot players, this will throw an exception.
		/// </summary>
		/// <param name="lobbyId">The id of the <see cref="Lobby"/>.</param>
		/// <param name="name">New lobby name</param>
		/// <param name="description">New lobby description</param>
		/// <param name="restriction">New restriction</param>
		/// <param name="gameType">New game type</param>
		/// <param name="maxPlayers">New max players value</param>
		/// <param name="newHost">New lobby host</param>
		Promise<Lobby> UpdateLobby(string lobbyId,
								   LobbyRestriction restriction,
								   string newHost,
								   string name = null,
								   string description = null,
								   string gameType = null,
								   int? maxPlayers = null);
	}
}
