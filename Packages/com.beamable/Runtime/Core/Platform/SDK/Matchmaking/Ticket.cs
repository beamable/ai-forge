using System;

namespace Beamable.Experimental.Api.Matchmaking
{
	// TODO: One day this should be replaced by code generated from the protobuf IDL description

	/// <summary>
	/// This type defines the %Ticket for the %MatchmakingService.
	/// 
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	/// 
	/// #### Related Links
	/// - See Beamable.Experimental.Api.Matchmaking.MatchmakingService script reference
	/// 
	/// ![img beamable-logo]
	/// 
	/// </summary>
	[Serializable]
	public class Ticket
	{
		public string ticketId;
		public string status;
		public string created;
		public string expires;
		public string[] players;
		public string matchType;

		/// <summary>
		/// This type defines the Matchmaking "Room" where players interact 
		/// </summary>
		public string matchId;

		public MatchmakingState Status => (MatchmakingState)Enum.Parse(typeof(MatchmakingState), status);
		public int SecondsRemaining => expires != null ? Convert.ToInt32((DateTime.Parse(expires) - DateTime.Now).TotalSeconds) : -1;
	}
}
