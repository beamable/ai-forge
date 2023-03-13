#if BEAMABLE_GPGS && UNITY_ANDROID
using Beamable.Common;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;

namespace Beamable.Platform.SDK.Auth
{
	public class SignInWithGPG
	{
		public Action<bool> OnLoginResult;
		public Action<bool, string> OnRequestServerSideAccessResult;

		public bool ForceRefreshToken { get; set; } = true;

		public SignInWithGPG()
		{
			PlayGamesPlatform.Activate();
		}

		public void Login()
		{
			PlayGamesPlatform.Instance.Authenticate(HandleAuthenticate);
		}

		public static Promise<string> RequestServerSideToken()
		{
			var promise = new Promise<string>();
			PlayGamesPlatform.Instance.RequestServerSideAccess(ForceRefreshToken, result =>
			{
				if(string.IsNullOrEmpty(result))
					promise.CompleteError(new Exception("Cannot get server side token"));
				else
					promise.CompleteSuccess(result);
			});
			return promise;
		}

		private void HandleAuthenticate(SignInStatus status)
		{
			if(status == SignInStatus.Success)
			{
				RequestServerSideToken().Then(HandleRequestServerSideAccess);
			}
			OnLoginResult?.Invoke(status == SignInStatus.Success);
		}

		private void HandleRequestServerSideAccess(string key)
		{
			OnRequestServerSideAccessResult?.Invoke(!string.IsNullOrEmpty(key), key);
		}
	}
}
#endif
