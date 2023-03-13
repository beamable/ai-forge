using Beamable.Common.Player;
using System;
using System.Collections.Generic;

namespace Beamable.Experimental.Api.Lobbies
{
	/// <summary>
	/// A Lobby is a grouping of online players for use in multiplayer gaming. State about the lobby will be shared
	/// amongst the players in it.
	/// </summary>
	[Serializable]
	public class Lobby : DefaultObservable
	{
		/// <summary>
		/// The id of the lobby. Use this id when making requests for a particular lobby via <see cref="ILobbyApi"/>
		/// </summary>
		public string lobbyId;

		/// <summary>
		/// The name of the <see cref="Lobby"/>. This value is optional.
		/// </summary>
		public string name;

		/// <summary>
		/// Optional description associated with the lobby. This is useful when trying to distinguish lobbies when
		/// querying for open lobbies.
		/// </summary>
		public string description;

		/// <summary>
		/// String version of the `Restriction` property.
		/// </summary>
		public string restriction;

		/// <summary>
		/// If a player creates a lobby directly, rather than through matchmaking, this property will be filled in by the
		/// playerId who made the initial create call.
		/// </summary>
		public string host;

		/// <summary>
		/// List of <see cref="LobbyPlayer"/> who are currently active in the lobby.
		/// </summary>
		public List<LobbyPlayer> players;

		/// <summary>
		/// Unique AlphaNumeric string which can be shared to allow for players to join the lobby.
		/// </summary>
		public string passcode;

		/// <summary>
		/// Configured max number of players this lobby can hold. This is set on creation or via the <see cref="Beamable.Common.Content.SimGameTypeRef"/>.
		/// </summary>
		public int maxPlayers;

		/// <summary>
		/// Either "Open" or "Closed" representing who can query and join the <see cref="Lobby"/>.
		/// </summary>
		public LobbyRestriction Restriction => (LobbyRestriction)Enum.Parse(typeof(LobbyRestriction), restriction);

		/// <summary>
		/// Update the state of the current lobby with the data from another lobby instance.
		/// This will trigger the observable callbacks.
		/// </summary>
		/// <param name="updatedState">The latest copy of the lobby</param>
		public void Set(Lobby updatedState)
		{
			lobbyId = updatedState?.lobbyId;
			description = updatedState?.description;
			name = updatedState?.name;
			restriction = updatedState?.restriction;
			host = updatedState?.host;
			players = updatedState?.players;
			passcode = updatedState?.passcode;
			maxPlayers = updatedState?.maxPlayers ?? 0;
			TriggerUpdate();
		}
	}
}
