using System;

namespace Beamable.Experimental.Api.Lobbies
{
	/// <summary>
	/// Request payload for querying <see cref="Lobby"/> by game type id.
	/// </summary>
	[Serializable]
	public class LobbyQueryRequest
	{
		/// <summary>
		/// Amount of lobbies skipped in response
		/// </summary>
		public int skip;

		/// <summary>
		/// Amount of lobbies returned in response
		/// </summary>
		public int limit;

		/// <summary>
		/// Game type id we are querying for
		/// </summary>
		public string matchType;

		public LobbyQueryRequest(int skip, int limit, string matchType)
		{
			this.skip = skip;
			this.limit = limit;
			this.matchType = matchType;
		}
	}
}
