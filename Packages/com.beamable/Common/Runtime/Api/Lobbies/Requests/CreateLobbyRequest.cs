using System;
using System.Collections.Generic;

namespace Beamable.Experimental.Api.Lobbies
{
	/// <summary>
	/// Request payload to create a new <see cref="Lobby"/>
	/// </summary>
	[Serializable]
	public class CreateLobbyRequest
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
		/// Stringified version of the <see cref="LobbyRestriction"/>.
		/// </summary>
		public string restriction;

		/// <summary>
		/// Optional stringified version of <see cref="Beamable.Common.Content.SimGameType"/>. This is necessary to
		/// allow for back-filling a <see cref="Lobby"/> from our matchmaking service.
		/// </summary>
		public string matchType;

		/// <summary>
		/// List of <see cref="Tag"/> to associate with the requesting player upon creation.
		/// </summary>
		public List<Tag> playerTags;

		/// <summary>
		/// Optional number of max players allowed for this lobby. The team configuration in the `matchType` WILL
		/// override this property.
		/// </summary>
		public int? maxPlayers;

		/// <summary>
		/// Configurable value to specify how long a generated passcode should be. Defaults to 6 AlphaNumeric characters
		/// on the server.
		/// </summary>
		public int? passcodeLength;

		public CreateLobbyRequest(
		  string name,
		  string description,
		  string restriction,
		  string matchType,
		  List<Tag> playerTags,
		  int? maxPlayers,
		  int? passcodeLength)
		{
			this.name = name;
			this.description = description;
			this.restriction = restriction;
			this.matchType = matchType;
			this.playerTags = playerTags;
			this.maxPlayers = maxPlayers;
			this.passcodeLength = passcodeLength;
		}
	}
}
