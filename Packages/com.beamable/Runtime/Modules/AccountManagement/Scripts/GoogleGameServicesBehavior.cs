using Beamable.Common.Api.Auth;
#if BEAMABLE_GPGS && UNITY_ANDROID
using Beamable.Platform.SDK.Auth;
#endif
using System;
using UnityEngine;

namespace Beamable.AccountManagement
{
	public class GoogleGameServicesBehavior : MonoBehaviour
	{
#if BEAMABLE_GPGS && UNITY_ANDROID
		private SignInWithGPG _gpg;
#endif
		private ThirdPartyLoginPromise _promise;
#pragma warning disable CS0414
		[SerializeField] private bool forceRefreshToken = true;
#pragma warning restore CS0414

		public void StartLogin(ThirdPartyLoginPromise promise)
		{
			if (promise.ThirdParty != AuthThirdParty.GoogleGamesServices)
			{
				return;
			}
			_promise = promise;

			if (Application.isEditor)
			{
				Debug.LogError("Google Games Services are not functional in Editor. Please build to device.");
				return;
			}
#if BEAMABLE_GPGS && UNITY_ANDROID
			_gpg = new SignInWithGPG();
			_gpg.ForceRefreshToken = forceRefreshToken;
			_gpg.OnLoginResult += HandleLoginResult;
			_gpg.OnRequestServerSideAccessResult += HandleRequestServerSideAccessResult;
			
			_gpg.Login();
#else
			Debug.LogError("Google Games Services are not enabled.");
#endif
		}

#if BEAMABLE_GPGS && UNITY_ANDROID
		private void HandleLoginResult(bool success)
		{
			if(!success)
			{
				_promise.CompleteSuccess(ThirdPartyLoginResponse.CANCELLED);
			}
		}

		private void HandleRequestServerSideAccessResult(bool success, string token)
		{
			if(!success)
			{
				_promise.CompleteError(new Exception("Cannot get server token from GPGS, please check if your configuration is correct."));
			}
			else
			{
				_promise.CompleteSuccess(new ThirdPartyLoginResponse(token, false, true));
			}
		}
#endif
	}
}
