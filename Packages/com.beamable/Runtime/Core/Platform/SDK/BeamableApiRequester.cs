using Beamable.Api;
using Beamable.Api.Caches;
using Beamable.Api.Connectivity;
using Beamable.Common;
using Beamable.Common.Api;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Core.Platform.SDK
{
	public interface IBeamableApiRequester : IRequester
	{
		AccessToken Token { get; set; }
		Promise RefreshToken();
	}

	// Since the new access tokens are short lived, no need to store them. We can use the same refresh tokens
	// in AccessTokenStorage and store the JWT in memory until it expires and we need to fetch a new one.
	public class BeamableApiRequester : PlatformRequester, IDisposable, IBeamableApiRequester
	{
		private const string ACCEPT_HEADER = "application/vnd.beamable.v1+json";

		public BeamableApiRequester(string host,
									PackageVersion beamableVersion,
									AccessTokenStorage accessTokenStorage,
									IConnectivityService connectivityService,
									OfflineCache offlineCache) : base(host, beamableVersion, accessTokenStorage,
																	  connectivityService, offlineCache)
		{

		}

		protected override string GetAcceptHeader() => ACCEPT_HEADER;

		protected override void AddCidPidHeaders(UnityWebRequest request)
		{
			// no-op
		}

		protected override void AddShardHeader(UnityWebRequest request)
		{
			// no-op
		}

		protected override string GenerateAuthorizationHeader()
		{
			if (Token == null || string.IsNullOrEmpty(Token?.Token))
			{
				return null;
			}

			return $"Bearer {Token.Token}";
		}

		protected override async Promise<T> HandleError<T>(Exception error, string contentType, byte[] body, SDKRequesterOptions<T> opts)
		{
			if (error is PlatformRequesterException e && e.Status == 401)
			{
				try
				{
					await RefreshToken();
				}
				catch (Exception err)
				{
					Debug.LogError($"Failed to refresh account for {Token.RefreshToken} for uri=[{opts.uri}] method=[{opts.method}] includeAuth=[{opts.includeAuthHeader}]");
					Debug.LogException(err);
				}

				return await MakeRequest(contentType, body, opts);
			}

			throw error;
		}

		public async Promise RefreshToken()
		{
			var authBody = new BeamableApiTokenRequest
			{
				refreshToken = Token.RefreshToken,
				customerId = Token.Cid,
				realmId = Token.Pid
			};
			var rsp = await Request<BeamableApiTokenResponse>(Method.POST, "/auth/refresh-token", authBody,
															  false);
			Token = new AccessToken(accessTokenStorage, Token.Cid, Token.Pid, rsp.accessToken,
									rsp.refreshToken, long.MaxValue - 1);
		}
	}

	[Serializable]
	public class BeamableApiTokenRequest
	{
		public string refreshToken;
		public string customerId;
		public string realmId;
	}

	[Serializable]
	public class BeamableApiTokenResponse
	{
		public string accessToken;
		public string refreshToken;
	}
}
