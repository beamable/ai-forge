using System;

namespace Beamable.Experimental.Api.Matchmaking
{
	/// <summary>
	/// This type defines the %TicketReservationResponse for the %MatchmakingService.
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
	public class TicketReservationResponse
	{
		public Ticket[] tickets;
	}
}
