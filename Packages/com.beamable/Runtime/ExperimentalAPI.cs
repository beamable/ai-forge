using Beamable.Experimental.Api.Calendars;
using Beamable.Experimental.Api.Chat;
using Beamable.Experimental.Api.Matchmaking;
using Beamable.Experimental.Api.Sim;
using Beamable.Experimental.Api.Social;
using System;

namespace Beamable.Experimental
{
	/// <summary>
	/// This type defines the %Client main entry point for the %experimental features.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public interface IExperimentalAPI
	{
		/// <summary>
		/// Entry point for the <a target="_blank" href="https://docs.beamable.com/docs/chat-feature">Chat</a> feature.
		/// </summary>
		ChatService ChatService { get; }

		/// <summary>
		/// Entry point for the <a target="_blank" href="https://docs.beamable.com/docs/multiplayer-feature">Multiplayer</a> feature.
		/// </summary>
		GameRelayService GameRelayService { get; }

		/// <summary>
		/// Entry point for the <a target="_blank" href="https://docs.beamable.com/docs/multiplayer-feature">Multiplayer</a> feature.
		/// </summary>
		MatchmakingService MatchmakingService { get; }

		/// <summary>
		/// Entry point for the <a target="_blank" href="https://docs.beamable.com/docs/friends-feature">Friends</a> feature.
		/// </summary>
		SocialService SocialService { get; }

		/// <summary>
		/// This feature is no longer supported.
		/// </summary>
		[Obsolete("This feature is no longer supported.")]
		CalendarsService CalendarService { get; }
	}
}
