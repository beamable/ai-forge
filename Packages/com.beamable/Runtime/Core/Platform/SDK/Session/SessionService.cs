using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Auth;
using Beamable.Common.Dependencies;
using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Beamable.Api.Sessions
{
	public interface ISessionService
	{
		/// <summary>
		/// Begin a new session for the current player.
		/// This happens automatically during the initialization of a player's BeamContext.
		/// </summary>
		/// <param name="user">
		/// The <see cref="User"/> that is starting a session.
		/// </param>
		/// <param name="advertisingId">
		/// An advertising ID that will be included in the session stats.
		/// When the session is started, by default, the AdvertisingIdentifier.GetIdentifier is given.
		/// </param>
		/// <param name="locale">
		/// The language code string that the session should use.
		/// By default, the Application.systemLanguage will be used to infer the locale, but you can override this.
		/// </param>
		/// <returns>A <see cref="Promise"/> representing the network call.</returns>
		Promise<EmptyResponse> StartSession(User user, string advertisingId, string locale = null);

		/// <summary>
		/// Get the current <see cref="Session"/> of a player by their gamertag.
		/// </summary>
		/// <param name="gamerTag">The gamertag of the player to find the <see cref="Session"/> for.</param>
		/// <returns>A <see cref="Promise{T}"/> containing the player's <see cref="Session"/></returns>
		Promise<Session> GetHeartbeat(long gamerTag);

		/// <summary>
		/// Send a heartbeat for the current user's session. Sending a heartbeat prolongs the session that was started with
		/// <see cref="StartSession"/>. This method is called automatically after the <see cref="IHeartbeatService.Start"/>
		/// method has been called, which happens automatically when the BeamContext starts.
		/// </summary>
		/// <returns></returns>
		Promise<EmptyResponse> SendHeartbeat();

		/// <summary>
		/// The number of seconds after the game startup that the session was begun.
		/// </summary>
		float SessionStartedAt { get; }

		/// <summary>
		/// The number of seconds ago that the most recent session was started.
		/// </summary>
		float TimeSinceLastSessionStart { get; }
	}

	/// <summary>
	/// This type defines the %Client main entry point for the %Session feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/accounts-feature">Accounts</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class SessionService : ISessionService
	{
		private static long TTL_MS = 60 * 1000;

		private UnityUserDataCache<Session> cache;
		private IBeamableRequester _requester;

		private readonly SessionParameterProvider _parameterProvider;
		private readonly SessionDeviceOptions _deviceOptions;

		public float SessionStartedAt { get; private set; }
		public float TimeSinceLastSessionStart => Time.realtimeSinceStartup - SessionStartedAt;

		public SessionService(IBeamableRequester requester,
							  IDependencyProvider provider,
							  SessionParameterProvider parameterProvider,
							  SessionDeviceOptions deviceOptions)
		{
			_requester = requester;
			// _parameterProvider = ServiceManager.ResolveIfAvailable<SessionParameterProvider>();
			// _deviceOptions = ServiceManager.ResolveIfAvailable<SessionDeviceOptions>();
			_parameterProvider = parameterProvider;
			_deviceOptions = deviceOptions;
			cache = new UnityUserDataCache<Session>("Session", TTL_MS, resolve, provider);
		}

		private Promise<Dictionary<long, Session>> resolve(List<long> gamerTags)
		{
			string queryString = "";
			for (int i = 0; i < gamerTags.Count; i++)
			{
				if (i > 0)
				{
					queryString += "&";
				}

				queryString += String.Format("gts={0}", gamerTags[i]);
			}

			return _requester.Request<MultiOnlineStatusesResponse>(
				Method.GET,
				String.Format("/presence/bulk?{0}", queryString)
			).Map(rsp =>
			{
				Dictionary<long, Session> result = new Dictionary<long, Session>();
				var dict = rsp.ToDictionary();
				for (int i = 0; i < gamerTags.Count; i++)
				{
					if (!dict.ContainsKey(gamerTags[i]))
					{
						dict[gamerTags[i]] = 0;
					}

					result.Add(gamerTags[i], new Session(dict[gamerTags[i]]));
				}

				return result;
			});
		}

		private ArrayDict GenerateDeviceParams(SessionStartRequestArgs args)
		{
			ArrayDict deviceParams = new ArrayDict(); // by default, don't send anything...
			if (_deviceOptions != null)
			{
				foreach (var option in _deviceOptions.All)
				{
					if (option == null || !option.IsEnabled) continue;
					deviceParams.Add(option.Key, option.Get(args));
				}
			}

			return deviceParams;
		}

		private ArrayDict GenerateSessionLanguageContextParams(SessionLanguageContext sessionLanguageContext)
		{
			return new ArrayDict { { "code", sessionLanguageContext.code }, { "ctx", sessionLanguageContext.ctx } };
		}

		private Promise<ArrayDict> GenerateCustomParams(ArrayDict deviceParams, User user)
		{
			return (_parameterProvider != null)
				? _parameterProvider.GetCustomParameters(deviceParams, user)
				: Promise<ArrayDict>.Successful(null);
		}

		private Promise<string> GenerateCustomLocale()
		{
			return (_parameterProvider != null)
				? _parameterProvider.GetCustomLocale()
				: Promise<string>.Successful(SessionServiceHelper.GetISO639CountryCodeFromSystemLanguage());
		}

		/// <summary>
		/// Starts a new Beamable user session. A session will record user analytics and track the user's play times.
		/// This method is automatically called by the Beamable SDK anytime the user changes and when Beamable SDK is initialized.
		/// </summary>
		/// <param name="advertisingId"></param>
		/// <param name="locale"></param>
		/// <returns></returns>
		public async Promise<EmptyResponse> StartSession(User user, string advertisingId, string locale = null)
		{
			SessionStartedAt = Time.realtimeSinceStartup;
			locale = locale ?? await GenerateCustomLocale();

			var args = new SessionStartRequestArgs { advertisingId = advertisingId, locale = locale };
			var deviceParams = GenerateDeviceParams(args);
			var promise = GenerateCustomParams(deviceParams, user);

			var languageContext = new SessionLanguageContext { code = locale, ctx = LanguageContext.ISO639.ToString() };
			var serializedLanguageContext = GenerateSessionLanguageContextParams(languageContext);

			return await promise.FlatMap(customParams =>
			{
				var req = new ArrayDict
				{
					{"platform", Application.platform.ToString()},
					{"device", SystemInfo.deviceModel.ToString()},
					{"locale", locale},
					{"language", serializedLanguageContext},
				};

				if (customParams != null && customParams.Count > 0)
				{
					req["customParams"] = customParams;
				}

				var json = Json.Serialize(req, new StringBuilder());

				return _requester.Request<EmptyResponse>(
					Method.POST,
					"/basic/session",
					json
				);
			});
		}

		/// <summary>
		/// Notifies the Beamable platform that the session is still active.
		/// This method is automatically called at a standard interval by the Beamable SDK itself.
		/// </summary>
		/// <returns></returns>
		public Promise<EmptyResponse> SendHeartbeat()
		{
			return _requester.Request<EmptyResponse>(
				Method.POST,
				"/basic/session/heartbeat"
			);
		}

		public Promise<Session> GetHeartbeat(long gamerTag)
		{
			return cache.Get(gamerTag);
		}
	}

	[Serializable]
	public class SessionLanguageContext
	{
		public string code;
		public string ctx;
	}

	[Serializable]
	public enum LanguageContext
	{
		UNITY,
		ISO6391,
		ISO639
	}

	public class SessionStartRequestArgs
	{
		public string advertisingId;
		public string locale;
	}

	[Serializable]
	public class MultiOnlineStatusesResponse
	{
		public List<SessionHeartbeat> statuses;

		public Dictionary<long, long> ToDictionary()
		{
			Dictionary<long, long> result = new Dictionary<long, long>();
			for (int i = 0; i < statuses.Count; i++)
			{
				var next = statuses[i];
				result[next.gt] = next.heartbeat;
			}

			return result;
		}
	}

	[Serializable]
	public class SessionHeartbeat
	{
		public long gt;
		public long heartbeat;
	}

	public class Session
	{
		private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		private static long CurrentTimeSeconds()
		{
			return (long)(DateTime.UtcNow - Jan1st1970).TotalSeconds;
		}

		public long Heartbeat;
		public long LastSeenMinutes;

		public Session(long heartbeat)
		{
			Heartbeat = heartbeat;
			LastSeenMinutes = (CurrentTimeSeconds() - heartbeat) / 60;
		}
	}
}
