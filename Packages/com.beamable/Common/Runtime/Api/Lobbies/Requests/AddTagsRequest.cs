using System;
using System.Collections.Generic;

namespace Beamable.Experimental.Api.Lobbies
{
	/// <summary>
	/// Request payload to add tags to a player in a <see cref="Lobby"/>.
	/// </summary>
	[Serializable]
	public class AddTagsRequest
	{
		/// <summary>
		/// The player's id.
		/// </summary>
		public string playerId;

		/// <summary>
		/// List of tags to add to the player.
		/// </summary>
		public List<Tag> tags;

		/// <summary>
		/// When true, these tags will replace tags with the same name with the ones provided.
		/// </summary>
		public bool replace;

		public AddTagsRequest(string playerId, List<Tag> tags, bool replace)
		{
			this.playerId = playerId;
			this.tags = tags;
			this.replace = replace;
		}
	}
}
