using System;

namespace Beamable.Experimental.Api.Matchmaking
{
	// TODO: One day this should be replaced by code generated from the protobuf IDL

	/// <summary>
	/// This type defines the %TicketReservationRequest for the %MatchmakingService.
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
	public class TicketReservationRequest
	{
		public string[] matchTypes;

		public int? maxWaitDurationSecs;

		public string team;

		// XXX: This is needed to allow for the server to support both new and old clients. Eventually, this
		// property will be "true" by default and can be removed.
		public bool watchOnlineStatus;

		public TicketReservationRequest(
			string[] matchTypes,
			string team = null,
			int? maxWaitDurationSecs = null
		)
		{
			this.matchTypes = matchTypes;
			this.maxWaitDurationSecs = maxWaitDurationSecs;
			this.team = team;
			watchOnlineStatus = true;
		}
	}
}
