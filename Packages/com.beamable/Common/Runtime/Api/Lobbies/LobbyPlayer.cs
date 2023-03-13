using System;
using System.Collections.Generic;

namespace Beamable.Experimental.Api.Lobbies
{
	/// <summary>
	/// Represents a player as seen from the <see cref="Lobby"/> structure.
	/// </summary>
	[Serializable]
	public class LobbyPlayer
	{
		/// <summary>
		/// Id of the player.
		/// </summary>
		public string playerId;

		/// <summary>
		/// List of optional tags associated with the player. This can be used to generate teams, other
		/// arbitrary groupings of players per the creator's needs.
		/// </summary>
		public List<Tag> tags;

		/// <summary>
		/// Populated by the stats requested upon lobby creation.
		/// </summary>
		public Dictionary<string, string> stats;

		/// <summary>
		/// DateTime that this player joined the lobby.
		/// </summary>
		public DateTime joined;
	}

	/// <summary>
	/// An arbitrary name/value pair associated with a <see cref="LobbyPlayer"/>.
	/// </summary>
	[Serializable]
	public class Tag
	{
		/// <summary>
		/// Name of this tag.
		/// </summary>
		public string name;

		/// <summary>
		/// Value of this tag.
		/// </summary>
		public string value;

		public Tag(string name, string value)
		{
			this.name = name;
			this.value = value;
		}
	}
}
