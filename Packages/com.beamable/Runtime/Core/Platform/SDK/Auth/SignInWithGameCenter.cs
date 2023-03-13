using Beamable.Common.Api.Auth;
using System;
using System.Text;
using UnityEngine;

namespace Beamable.Platform.SDK.Auth
{
	public class SignInWithGameCenter : MonoBehaviour
	{

		[Obsolete("Use OnGameCenterTokenSuccessWithThirdParty")]
		public Action<string> OnGameCenterTokenSuccess;

		public Action<string, AuthThirdParty> OnGameCenterTokenSuccessWithThirdParty;

		[Obsolete("Parameter expects Action<string, AuthThirdParty>")]
		public void GetGameCenterLoginRequest(Action<string> tokenSuccess)
		{
#if UNITY_IOS && !UNITY_EDITOR
            OnGameCenterTokenSuccess += tokenSuccess;
            Social.localUser.Authenticate(GameCenterAuthenticated);
#endif
		}

		public void GetGameCenterLoginRequest(Action<string, AuthThirdParty> tokenSuccess)
		{
#if UNITY_IOS && !UNITY_EDITOR
            OnGameCenterTokenSuccessWithThirdParty += tokenSuccess;
            Social.localUser.Authenticate(GameCenterAuthenticated);
#endif
		}

		private AuthThirdParty determineGameCenterAuth(string playerId)
		{
			if (playerId.StartsWith("G:"))
			{
				return AuthThirdParty.GameCenter;
			}
			else
			{
				return AuthThirdParty.GameCenterLimited;
			}
		}

		private void GameCenterAuthenticated(bool authenticated)
		{
			if (authenticated)
			{
				GCIdentityPlugin.GCIdentity.GenerateIdentity(this.name);
			}
		}

		public void OnIdentitySuccess(string identity)
		{
			var parsedIdentity = GCIdentityPlugin.GCIdentity.ParseIdentity(identity);
			var publicKeyUrl = parsedIdentity[0];
			var signature = parsedIdentity[1];
			var salt = parsedIdentity[2];
			var timestamp = long.Parse(parsedIdentity[3]);
			var playerId = parsedIdentity[4];
			var thirdParty = determineGameCenterAuth(playerId);
			var encodedEntity = new GameCenterVerificationRequest(
			   publicKeyUrl,
			   signature,
			   salt,
			   timestamp,
			   playerId,
			   Application.identifier).Encoded();

#pragma warning disable 618
			OnGameCenterTokenSuccess?.Invoke(encodedEntity);
#pragma warning restore 618
			OnGameCenterTokenSuccessWithThirdParty?.Invoke(encodedEntity, thirdParty);
		}
		public void OnIdentityError(string error)
		{
			Debug.Log("Identity error: " + error);
		}
		[System.Serializable]
		public class GameCenterVerificationRequest
		{
			public string publicKeyUrl;
			public string signature;
			public string salt;
			public long timestamp;
			public string playerID;
			public string bundleID;
			public GameCenterVerificationRequest(string publicKeyUrl, string signature, string salt, long timestamp, string playerID, string bundleID)
			{
				this.publicKeyUrl = publicKeyUrl;
				this.signature = signature;
				this.salt = salt;
				this.timestamp = timestamp;
				this.playerID = playerID;
				this.bundleID = bundleID;
			}
			public string Encoded()
			{
				var payloadJSON = JsonUtility.ToJson(this);
				var payloadBytes = Encoding.UTF8.GetBytes(payloadJSON);
				var encodedPayload = System.Convert.ToBase64String(payloadBytes);
				return encodedPayload;
			}
		}

	}
}
