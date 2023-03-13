using Beamable.Common.Api.Auth;
using UnityEngine;
#if UNITY_IOS
using Beamable.Api;
using Beamable.Platform.SDK.Auth;
#endif

namespace Beamable.AccountManagement
{
	public class AppleSignInBehavior : MonoBehaviour
	{
#if UNITY_IOS

      private SignInWithApple signInWithApple;

      public void Start()
      {
         signInWithApple = new SignInWithApple();
      }

      public void StartAppleLogin(ThirdPartyLoginPromise promise)
      {
         if (promise.ThirdParty != AuthThirdParty.Apple)
         {
            return;
         }

         // TODO: Need to do something graceful in the event that the device in question doesn't have iOS13.
         // Also need to ensure that the button doesn't show up if the user isn't on iOS13+

         signInWithApple.Login(callbackArgs =>
         {
            if (!string.IsNullOrEmpty(callbackArgs.error))
            {
               promise.CompleteError(new ErrorCode(1, error: "UnableToSignInToApple", message: callbackArgs.error));
            }
            else
            {
               promise.CompleteSuccess(new ThirdPartyLoginResponse
               {
                  AuthToken = callbackArgs.userInfo.idToken
               });
            }
         });
      }
#else
		public void StartAppleLogin(ThirdPartyLoginPromise promise)
		{
			if (promise.ThirdParty != AuthThirdParty.Apple)
			{
				return;
			}

			if (Application.isEditor) // we aren't on apple, so don't do _anything_ except unity editor
			{
				Debug.LogError("Apple Sign-In is not functional in Editor. Please build to device.");

				ThirdPartyLoginResponse response = ThirdPartyLoginResponse.CANCELLED;
				promise.CompleteSuccess(response);
			}
		}
#endif
	}
}
