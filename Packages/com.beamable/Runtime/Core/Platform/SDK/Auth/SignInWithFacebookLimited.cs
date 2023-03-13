#if UNITY_IOS && BEAMABLE_FACEBOOK
using System.Collections.Generic;
using System;
using UnityEngine;
using Facebook.Unity;

namespace Beamable.Platform.SDK.Auth
{

	public class SignInWithFacebookLimited
	{
		public List<string> BaseScopes = new List<string>() {"public_profile", "email"};
		private List<string> UserScopes;
		private Action<string> OnFBLimitedLoginSuccess;


		public SignInWithFacebookLimited(Action<string> authenticationSuccess = null, List<string> scopes = null)
		{
			OnFBLimitedLoginSuccess += authenticationSuccess;
			UserScopes = scopes;
		}

		public static Profile CurrentPlayerProfile()
        {
			return FB.Mobile.CurrentProfile();
        }

		public void Login()
		{

			if (UserScopes == null)
			{
				UserScopes = BaseScopes;
			}

		Activate();
		}

		public void Activate()
		{
			if (!FB.IsInitialized)
			{
		        FB.Init(InitCallback, OnHideUnity);
		    }
		    else
		    {
		        FB.ActivateApp();
				GetFacebookAuthentication();
			}

		}

		private void InitCallback()
		{
		    if (FB.IsInitialized)
		    {
		        FB.ActivateApp();
				GetFacebookAuthentication();

			}
		    else
		    {
		        Debug.LogError("Failed to Initialize the Facebook SDK");
		    }

		}

		private void GetFacebookAuthentication()
        {
			FB.Mobile.LoginWithTrackingPreference(LoginTracking.LIMITED, UserScopes, null, AuthCallback);
		}

		private void AuthCallback (IResult result) {

		    if (FB.IsLoggedIn)
		    {
				var token = FB.Mobile.CurrentAuthenticationToken();
				if (OnFBLimitedLoginSuccess != null)
                {
					OnFBLimitedLoginSuccess?.Invoke(token.TokenString);
				}
		    }
		    else
		    {
		        Debug.Log("User cancelled login");
		    }
		}

		private void OnHideUnity (bool isGameShown)
		{
			if (!isGameShown)
			{
				Time.timeScale = 0;
			}
			else
			{
				Time.timeScale = 1;
			}
		}

		public static void Logout(Action action = null)
        {
			FB.LogOut();
			if (action != null)
            {
				action?.Invoke();
			}
        }

	}
}
#endif
