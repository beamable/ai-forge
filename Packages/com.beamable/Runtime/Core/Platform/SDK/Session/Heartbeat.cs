using Beamable.Api.Connectivity;
using Beamable.Common.Api.Presence;
using Beamable.Common.Dependencies;
using Beamable.Coroutines;
using System;
using System.Collections;
using UnityEngine;

namespace Beamable.Api.Sessions
{
	public interface IHeartbeatService
	{
		/// <summary>Indicates if <see cref="IHeartbeatService"/> started sending hearbeats.</summary>
		bool IsRunning { get; }

		/// <summary>
		/// Start sending heartbeats with the <see cref="SessionService.SendHeartbeat"/> method.
		/// </summary>
		void Start();

		/// <summary>
		/// Reset the interval between heartbeats to the default value 30 seconds.
		/// </summary>
		void ResetLegacyInterval();

		/// <summary>
		/// Change how often a heartbeat is sent to the session.
		/// <see cref="ResetInterval"/> will revert the interval back to the default setting.
		/// </summary>
		/// <param name="seconds">The number of seconds between heartbeats.</param>
		void UpdateLegacyInterval(int seconds);
	}

	public class Heartbeat : IHeartbeatService
	{
		public bool IsRunning { get; private set; }

		private const int LegacyHeartbeatInterval = 30;
		private const int HeartbeatInterval = 5;

		private readonly ISessionService _sessionService;
		private readonly CoroutineService _coroutineService;
		private readonly IConnectivityService _connectivityService;
		private readonly IDependencyProviderScope _provider;
		private readonly IPresenceApi _presenceApi;

		private IEnumerator _legacyHeartbeatRoutine;
		private IEnumerator _heartbeatRoutine;

		public Heartbeat(
			ISessionService sessionService,
			CoroutineService coroutineService,
			IConnectivityService connectivityService,
			IDependencyProviderScope provider,
			IPresenceApi presenceApi)
		{
			_sessionService = sessionService;
			_coroutineService = coroutineService;
			_connectivityService = connectivityService;
			_provider = provider;
			_presenceApi = presenceApi;
			IsRunning = false;
			_legacyHeartbeatRoutine = SendLegacyHeartbeat(LegacyHeartbeatInterval);
			_heartbeatRoutine = SendHeartbeat(HeartbeatInterval);
		}

		public void Start()
		{
			if (IsRunning) return; // don't allow the heartbeat to start multiple times...
			IsRunning = true;
			_coroutineService.StartCoroutine(_legacyHeartbeatRoutine);
			_coroutineService.StartCoroutine(_heartbeatRoutine);
		}

		public void ResetLegacyInterval()
		{
			UpdateLegacyInterval(LegacyHeartbeatInterval);
		}

		public void UpdateLegacyInterval(int seconds)
		{
			_coroutineService.StopCoroutine(_legacyHeartbeatRoutine);
			_legacyHeartbeatRoutine = SendLegacyHeartbeat(seconds);
			_coroutineService.StartCoroutine(_legacyHeartbeatRoutine);
		}

		/// <summary>
		/// Coroutine: send heartbeat requests to Platform.
		/// </summary>
		private IEnumerator SendLegacyHeartbeat(int intervalSeconds)
		{
			var wait = new WaitForSeconds(intervalSeconds);
			while (_provider.IsActive)
			{
				if (_connectivityService.HasConnectivity)
				{
					_sessionService.SendHeartbeat();
				}
				yield return wait;
			}
		}

		private IEnumerator SendHeartbeat(int intervalSeconds)
		{
			var wait = new WaitForSeconds(intervalSeconds);

			while (_provider.IsActive)
			{
				try
				{
					_presenceApi.SendHeartbeat().Error(_ =>
					{
						// let it go!
					});
				}
				catch (Exception)
				{
					// Let it Gooo!
					// because we want the heartbeat to go forever...
				}

				yield return wait;
			}
		}

	}
}
