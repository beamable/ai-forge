using Beamable.Common.Api.Auth;
using Beamable.Platform.SDK.Auth;
using UnityEngine;

namespace Beamable.AccountManagement
{
	public class GoogleSignInBehavior : MonoBehaviour
	{
		private GoogleSignIn _google;
		private ThirdPartyLoginPromise _promise;

		/// <summary>
		/// Begin the Google Sign-In process.
		/// </summary>
		/// <param name="promise">Promise to be completed when sign-in succeeds or fails</param>
		public void StartGoogleLogin(ThirdPartyLoginPromise promise)
		{
			if (promise.ThirdParty != AuthThirdParty.Google)
			{
				return;
			}

			var webClientId = AccountManagementConfiguration.Instance.GoogleClientID;
			var iosClientId = AccountManagementConfiguration.Instance.GoogleIosClientID;
			_google = new GoogleSignIn(gameObject, "GoogleAuthResponse", webClientId, iosClientId);
			_promise = promise;

			if (Application.isEditor)
			{
				Debug.LogError("Google Sign-In is not functional in Editor. Please build to device.");
				GoogleAuthResponse("CANCELED");
				return;
			}
			_google.Login();
		}

		/// <summary>
		/// Callback to be invoked via UnitySendMessage when the plugin either
		/// receives a valid ID token or indicates an error.
		/// </summary>
		/// <param name="message">Response message from the Google Sign-In plugin</param>
		private void GoogleAuthResponse(string message)
		{
			if (_promise == null)
			{
				return;
			}

			GoogleSignIn.HandleResponse(
			   message,
			   token =>
			   {
				   ThirdPartyLoginResponse response;
				   if (token == null)
				   {
					   response = ThirdPartyLoginResponse.CANCELLED;
				   }
				   else
				   {
					   response = new ThirdPartyLoginResponse(token);
				   }
				   _promise.CompleteSuccess(response);
			   },
			   exc =>
			   {
				   _promise.CompleteError(exc);
			   });
		}
	}
}
