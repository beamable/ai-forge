using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Content;
using Beamable.Serialization.SmallerJSON;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Beamable.Experimental.Api.Matchmaking
{
	/// <summary>
	/// This type defines the %Client main entry point for the %Multiplayer feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/multiplayer-feature">Multiplayer</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class MatchmakingService
	{
		private readonly IBeamableRequester _requester;
		private readonly IPlatformService _platform;

		public MatchmakingService(IPlatformService platform, IBeamableRequester requester)
		{
			_requester = requester;
			_platform = platform;
		}

		/// <summary>
		/// Initialize the matchmaking process.
		/// </summary>
		/// <param name="gameTypeRef"></param>
		/// <param name="updateHandler"></param>
		/// <param name="readyHandler"></param>
		/// <param name="timeoutHandler"></param>
		/// <returns>A `MatchmakingHandle` which will be updated via push notifications.</returns>
		public Promise<MatchmakingHandle> StartMatchmaking(
		  ContentRef<SimGameType> gameTypeRef,
		  Action<MatchmakingHandle> updateHandler = null,
		  Action<MatchmakingHandle> readyHandler = null,
		  Action<MatchmakingHandle> timeoutHandler = null
		)
		{
			return gameTypeRef.Resolve().FlatMap(gameType =>
			{
				TimeSpan? maxWait = null;
				if (gameType.maxWaitDurationSecs.HasValue)
				{
					maxWait = TimeSpan.FromSeconds(gameType.maxWaitDurationSecs.Value);
				}

				return StartMatchmaking(
			gameType.Id,
			updateHandler,
			readyHandler,
			timeoutHandler,
			maxWait
		  );
			});
		}

		/// <summary>
		/// Initialize the matchmaking process.
		/// </summary>
		/// <param name="gameType"></param>
		/// <param name="updateHandler"></param>
		/// <param name="readyHandler"></param>
		/// <param name="timeoutHandler"></param>
		/// <param name="maxWait"></param>
		/// <returns>A `MatchmakingHandle` which will be updated via push notifications.</returns>
		public Promise<MatchmakingHandle> StartMatchmaking(
		  string gameType,
		  Action<MatchmakingHandle> updateHandler = null,
		  Action<MatchmakingHandle> readyHandler = null,
		  Action<MatchmakingHandle> timeoutHandler = null,
		  TimeSpan? maxWait = null
		)
		{
			if (_platform.Heartbeat.IsRunning)
			{
				return MakeMatchmakingRequest(gameType).Map(tickets => new MatchmakingHandle(
																this,
																_platform,
																tickets.tickets,
																maxWait,
																updateHandler,
																readyHandler,
																timeoutHandler
															));
			}

			const string info =
#if UNITY_EDITOR
				"<b>IHeartbeatService</b> is not running, " +
				"<b>MatchmakingService</b> will not work correctly" +
				"This could be caused by disabling <b>SendHeartbeat</b> option in <b>Beamable Core Configuration</b>.";
#else
				"IHeartbeatService is not running, MatchmakingService will not work correctly.";
#endif
			return Promise<MatchmakingHandle>.Failed(new Exception(info));

		}

		/// <summary>
		/// Find this player a match for the given game type
		/// </summary>
		/// <param name="gameTypes">The string gameTypes </param>
		/// <returns></returns>
		private Promise<TicketReservationResponse> MakeMatchmakingRequest(params string[] gameTypes)
		{
			return _requester.Request<TicketReservationResponse>(
			  Method.POST,
			  "/matchmaking/tickets",
			  new TicketReservationRequest(gameTypes)
			);
		}

		/// <summary>
		/// Fetch a match given its Id.
		/// </summary>
		/// <param name="matchId">The id of the match to fetch.</param>
		public Promise<Match> GetMatch(string matchId)
		{
			return _requester.Request<Match>(
			  Method.GET,
			  $"/matchmaking/matches/{matchId}"
			);
		}

		/// <summary>
		/// Cancels matchmaking for the player
		/// </summary>
		/// <param name="ticketId">The id of the ticket to cancel.</param>
		public Promise<Unit> CancelMatchmaking(string ticketId)
		{
			return _requester.Request<EmptyResponse>(
			  Method.DELETE,
			  $"/matchmaking/tickets/{ticketId}"
			).Map(_ => PromiseBase.Unit);
		}
	}

	/// <summary>
	/// This type defines the %MatchmakingHandle for the %MatchmakingService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Experimental.Api.Matchmaking.MatchmakingService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class MatchmakingHandle : IDisposable
	{
		public Ticket[] Tickets { get; }
		public MatchmakingStatus Status { get; }
		public MatchmakingState State { get; private set; }

		public Match Match { get; private set; }
		public bool MatchmakingIsComplete => State.IsTerminal();

		public event Action<MatchmakingHandle> OnUpdate;
		public event Action<MatchmakingHandle> OnMatchReady;
		public event Action<MatchmakingHandle> OnMatchTimeout;

		private readonly float _createdTime;
		private readonly TimeSpan? _maxWait;

		private readonly IPlatformService _platform;
		private static string MessageType(string gameType) => $"matchmaking.update.{gameType}";
		private static string TimeoutMessageType(string gameType) => $"matchmaking.timeout.{gameType}";

		private readonly MatchmakingService _service;

		public MatchmakingHandle(
		  MatchmakingService service,
		  IPlatformService platform,
		  Ticket[] tickets,
		  TimeSpan? maxWait = null,
		  Action<MatchmakingHandle> onUpdate = null,
		  Action<MatchmakingHandle> onMatchReady = null,
		  Action<MatchmakingHandle> onMatchTimeout = null
		)
		{
			OnUpdate = onUpdate;
			OnMatchReady = onMatchReady;
			OnMatchTimeout = onMatchTimeout;

			Tickets = tickets;
			State = MatchmakingState.Searching;

			Status = new MatchmakingStatus();
			foreach (var ticket in Tickets)
			{
				ProcessUpdate(ticket);
			}

			_platform = platform;
			_maxWait = maxWait;

			_createdTime = Time.realtimeSinceStartup;

			_service = service;

			_platform.Heartbeat.UpdateLegacyInterval(2);
			StartTimeoutTask();
			SubscribeToUpdates();
		}

		public async void Dispose()
		{
			await Cancel();
		}

		/// <summary>
		/// Promise which will complete when the matchmaking client reaches a "resolution".
		/// </summary>
		/// <returns>A promise containing the matchmaking handle itself.</returns>
		public Promise<MatchmakingHandle> WhenCompleted()
		{
			var promise = new Promise<MatchmakingHandle>();
			WaitForComplete(promise);
			return promise;
		}

		private async void WaitForComplete(Promise<MatchmakingHandle> promise)
		{
			var endTime = _createdTime + _maxWait?.TotalSeconds ?? double.MaxValue;
			while (Time.realtimeSinceStartup < endTime)
			{
				if (MatchmakingIsComplete)
				{
					promise.CompleteSuccess(this);
				}

				await Task.Delay(TimeSpan.FromSeconds(1));
			}
		}

		/// <summary>
		/// Cancels matchmaking for this player.
		/// </summary>
		/// <returns>The MatchmakingHandle</returns>
		public Promise<MatchmakingHandle> Cancel()
		{
			State = MatchmakingState.Cancelled;
			foreach (var ticket in Tickets)
			{
				_service.CancelMatchmaking(ticket.ticketId);
			}
			EndMatchmaking();
			return Promise<MatchmakingHandle>.Successful(this);
		}

		private async void StartTimeoutTask()
		{
			if (!_maxWait.HasValue)
			{
				return;
			}

			var endTime = _createdTime + _maxWait.Value.TotalSeconds;
			await Task.Delay(TimeSpan.FromSeconds(endTime - Time.realtimeSinceStartup));
			if (MatchmakingIsComplete)
			{
				return;
			}

			// Ensure that we cancel matchmaking if the client is giving up before the server.
			foreach (var ticket in Tickets)
			{
				await _service.CancelMatchmaking(ticket.ticketId);
			}

			ProcessTimeout();
		}

		private void SubscribeToUpdates()
		{
			foreach (var ticket in Tickets)
			{
				_platform.Notification.Subscribe(MessageType(ticket.matchType), OnRawUpdate);
				_platform.Notification.Subscribe(TimeoutMessageType(ticket.matchType), OnRawTimeout);
			}
		}

		private void EndMatchmaking()
		{
			foreach (var ticket in Tickets)
			{
				_platform.Notification.Unsubscribe(MessageType(ticket.matchType), OnRawUpdate);
				_platform.Notification.Unsubscribe(TimeoutMessageType(ticket.matchType), OnRawTimeout);
			}
			_platform.Heartbeat.ResetLegacyInterval();
		}

		private void OnRawUpdate(object msg)
		{
			// XXX: Ugh. This is an annoying shape to get messages in.
			var serialized = Json.Serialize(msg, new StringBuilder());
			var deserialized = JsonUtility.FromJson<Ticket>(serialized);
			ProcessUpdate(deserialized);
		}

		private async void ProcessUpdate(Ticket ticket)
		{
			Status.Apply(ticket);


			try
			{
				OnUpdate?.Invoke(this);
			}
			catch (Exception e)
			{
				BeamableLogger.LogException(e);
			}

			// Once the game has been marked as "Ready" we will no longer receive messages from the server.
			// However, let's ensure that we invoke OnUpdate regardless in case someone doesn't want to use the
			// OnMatchReady event.
			if (ticket.Status != MatchmakingState.Ready)
			{
				return;
			}

			State = MatchmakingState.Ready;
			// Once we're ready, we should be able to ask the matchmaking service for match information.
			Match = await _service.GetMatch(ticket.matchId);
			Status.Apply(Match);

			try
			{
				OnMatchReady?.Invoke(this);
			}
			catch (Exception e)
			{
				BeamableLogger.LogException(e);
			}
			finally
			{
				EndMatchmaking();
			}
		}

		private void OnRawTimeout(object msg)
		{
			ProcessTimeout();
		}

		private void ProcessTimeout()
		{
			State = MatchmakingState.Timeout;
			try
			{
				OnMatchTimeout?.Invoke(this);
			}
			catch (Exception e)
			{
				BeamableLogger.LogException(e);
			}
			finally
			{
				// Once we get a timeout, we know that we no longer will receive any updates.
				EndMatchmaking();
			}
		}
	}

	/// <summary>
	/// This type defines the %MatchmakingStatus for the %MatchmakingService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Experimental.Api.Matchmaking.MatchmakingService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class MatchmakingStatus
	{
		public string GameId { get; private set; }
		public int SecondsRemaining { get; private set; }
		public string[] Players { get; private set; }
		public bool MinPlayersReached { get; private set; }
		public bool GameStarted { get; private set; }

		public void Apply(Ticket ticket)
		{
			GameId = ticket.matchId;
			SecondsRemaining = ticket.SecondsRemaining;
		}

		public void Apply(Match match)
		{
			GameStarted = match.IsRunning;
			// If we have a match created, we've already reached min players.
			MinPlayersReached = true;
			Players = match.teams.SelectMany(team => team.players).ToArray();
		}
	}

	/// <summary>
	/// This type defines the %MatchmakingState for the %MatchmakingService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Experimental.Api.Matchmaking.MatchmakingService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public enum MatchmakingState
	{
		Searching,
		Ready,
		Timeout,
		Cancelled
	}

	/// <summary>
	/// This type defines the %MatchmakingStateExtensions for the %MatchmakingService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Experimental.Api.Matchmaking.MatchmakingService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public static class MatchmakingStateExtensions
	{
		public static bool IsTerminal(this MatchmakingState state)
		{
			return state != MatchmakingState.Searching;
		}
	}
}
