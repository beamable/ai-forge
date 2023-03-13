using Beamable.Common;
using Beamable.Common.Api.Auth;
using Beamable.Stats;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Beamable.Common.Constants.MenuItems.Assets;

namespace Beamable.AccountManagement
{
#if BEAMABLE_DEVELOPER
	[CreateAssetMenu(
	   fileName = "Account Management Configuration",
	   menuName = Paths.MENU_ITEM_PATH_ASSETS_BEAMABLE_CONFIGURATIONS + "/" +
	   "Account Management Configuration",
	   order = Orders.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1)]
#endif
	public class AccountManagementConfiguration : ModuleConfigurationObject, IAuthSettings
	{
		public struct UserThirdPartyAssociation
		{
			public AuthThirdParty ThirdParty;
			public bool HasAssociation;
			public bool ThirdPartyEnabled;

			public bool ShouldShowIcon => ThirdPartyEnabled && HasAssociation;
			public bool ShouldShowButton => ThirdPartyEnabled && !HasAssociation;
		}

		public static AccountManagementConfiguration Instance => Get<AccountManagementConfiguration>();

		public bool Facebook, Apple, Google, GooglePlayGames;

		[Tooltip("Web App Google Client ID https://console.cloud.google.com/apis/credentials (note: Android needs web ID for auth)")]
		public string GoogleClientID;

		[Tooltip("iOS Google Client ID https://console.cloud.google.com/apis/credentials")]
		public string GoogleIosClientID;

		[Tooltip("Enable Google Sign-In on iOS (causes build post processor to add -ObjC to OTHER_LDFLAGS in Xcode)")]
		public bool EnableGoogleSignInOnApple;

		[Tooltip("The stat to use to show account names")]
		public StatObject DisplayNameStat;

		[Tooltip("The label to use next to the sub text")]
		public string SubtextLabel = "Progress";

		[Tooltip("The stat to use to show account sub text")]
		public StatObject SubtextStat;

		[Tooltip("The stat to use to hold an avatar addressable sprite asset")]
		public StatObject AvatarStat;

		[Tooltip("When a player requests a forgot-password email, control what type of reset code they are given.")]
		public CodeType PasswordResetCodeType = CodeType.PIN;

		[Tooltip("Allows you to override specific account management functionality")]
		[SerializeField]
		private AccountManagementAdapter _overrides;

		[Tooltip("The max character limit of a player's alias")]
		public int AliasCharacterLimit = 18;

		[Tooltip("Controls the presence of the promotional banner on the main menu of account management.")]
		public bool ShowPromotionalSlider = false;


#if UNITY_EDITOR
      public event Action OnValidated;
      private void OnValidate()
      {
         OnValidated?.Invoke();
      }
#endif

		public AccountManagementAdapter Overrides
		{
			get
			{
				if (_overrides == null)
				{
					if (_overrides == null)
					{
						var gob = new GameObject();
						_overrides = gob.AddComponent<AccountManagementAdapter>();
					}
				}
				return _overrides;
			}
		}


		/// <summary>
		/// Tell whether a given auth third party is available and enabled in the config.
		/// </summary>
		/// <param name="thirdParty">The third party to check</param>
		/// <returns>True if the third party is available</returns>
		public bool AuthEnabled(AuthThirdParty thirdParty)
		{
			switch (thirdParty)
			{
				case AuthThirdParty.Facebook:
					return Facebook;
#if UNITY_IOS
            case AuthThirdParty.Apple:
               // We currently only support Apple sign in on iOS.
               return Apple;
            case AuthThirdParty.Google:
               // Google Sign-In on iOS requires extra flags.
               return Google && EnableGoogleSignInOnApple;
#else
				case AuthThirdParty.Google:
					// On non-iOS platforms, just honor the Google checkbox.
					return Google;

#if UNITY_EDITOR
				case AuthThirdParty.Apple:
					return Apple;	
#endif
#endif // UNITY_IOS

				default:
					return false;
			}
		}

		public Promise<List<UserThirdPartyAssociation>> GetAllEnabledThirdPartiesForUser(User user)
		{
			// for each user, we need to run a promise out
			var promises = new List<Promise<UserThirdPartyAssociation>>();

			var thirdParties = (AuthThirdParty[])Enum.GetValues(typeof(AuthThirdParty));
			foreach (var thirdParty in thirdParties)
			{
				if (!AuthEnabled(thirdParty))
				{
					promises.Add(Promise<UserThirdPartyAssociation>.Successful(new UserThirdPartyAssociation
					{
						HasAssociation = false,
						ThirdParty = thirdParty,
						ThirdPartyEnabled = false
					}));
				}
				else
				{
					// TODO, somehow we should be able to cache this fact, so we don't keep on pinging apis.
					promises.Add(Overrides.DoesUserHaveThirdParty(user, thirdParty).Map(hasThirdParty =>
					new UserThirdPartyAssociation
					{
						HasAssociation = hasThirdParty,
						ThirdParty = thirdParty,
						ThirdPartyEnabled = true
					}));
				}
			}

			return Promise.Sequence(promises);
		}

		CodeType IAuthSettings.PasswordResetCodeType => PasswordResetCodeType;
	}
}
