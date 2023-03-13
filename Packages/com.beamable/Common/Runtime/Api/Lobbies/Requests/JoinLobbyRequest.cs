using System;
using System.Collections.Generic;

namespace Beamable.Experimental.Api.Lobbies
{
	/// <summary>
	/// Request payload for joining a <see cref="Lobby"/> by id.
	/// </summary>
	[Serializable]
	public class JoinLobbyRequest
	{
		/// <summary>
		/// List of <see cref="Tag"/> to associate to the joined player in the <see cref="Lobby"/>.
		/// </summary>
		public List<Tag> tags;

		public JoinLobbyRequest(List<Tag> tags)
		{
			this.tags = tags;
		}
	}
}
