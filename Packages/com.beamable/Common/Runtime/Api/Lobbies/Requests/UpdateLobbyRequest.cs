using System;

namespace Beamable.Experimental.Api.Lobbies
{
	/// <summary>
	/// Request payload to update a <see cref="Lobby"/>
	/// </summary>
	[Serializable]
	public class UpdateLobbyRequest
	{
		/// <summary>
		/// Optional name of the <see cref="Lobby"/>.
		/// </summary>
		public string name;

		/// <summary>
		/// Optional description of the <see cref="Lobby"/>.
		/// </summary>
		public string description;

		/// <summary>
		/// Optional stringified version of the <see cref="LobbyRestriction"/>.
		/// </summary>
		public string restriction;

		/// <summary>
		/// Optional stringified version of <see cref="Beamable.Common.Content.SimGameType"/>. This is necessary to
		/// allow for back-filling a <see cref="Lobby"/> from our matchmaking service.
		/// </summary>
		public string matchType;

		/// <summary>
		/// Optional number of max players allowed for this lobby. The team configuration in the `matchType` WILL
		/// override this property.
		/// </summary>
		public int? maxPlayers;

		/// <summary>
		/// Optional host of the <see cref="Lobby"/>
		/// on the server.
		/// </summary>
		public string newHost;

		public UpdateLobbyRequest(string name,
								  string description,
								  string restriction,
								  string newHost,
								  string matchType,
								  int? maxPlayers)
		{
			this.name = name;
			this.description = description;
			this.restriction = restriction;
			this.matchType = matchType;
			this.maxPlayers = maxPlayers;
			this.newHost = newHost;
		}
	}
}
