using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Auth;
using Beamable.Coroutines;
using Beamable.Platform.SDK.Auth;
using Beamable.Signals;
using Beamable.UI.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Beamable.AccountManagement
{
	[System.Serializable]
	public class ToggleEvent : DeSignal<bool> { }

	[System.Serializable]
	public class EmailEvent : DeSignal<string> { }

	[System.Serializable]
	public class ErrorEvent : DeSignal<string>
	{
		public const string BAD_CREDENTIALS = "bad_credentials";
		public const string SERVER_ERROR = "server_error";
		public const string PASSWORD_STRENGTH = "password_strength";
	}

	[System.Serializable]
	public class LoadingEvent : DeSignal<LoadingArg> { }

	[System.Serializable]
	public class LoadingArg
	{
		public readonly Promise<Unit> Promise = new Promise<Unit>();
		public readonly string Message = "Loading...";
		public readonly bool Critical;

		public LoadingArg(string message, bool critical = false)
		{
			Message = message;
			Critical = critical;
		}

		public LoadingArg() { }

		public void Complete()
		{
			Promise.CompleteSuccess(new Unit());
		}
	}

	public static class LoadingArgPromiseExtensions
	{
		public static LoadingArg ToLoadingArg<T>(this Promise<T> self,
												 string message = "loading",
												 bool critical = false)
		{
			var arg = new LoadingArg(message, critical);
			self.Then(x => arg.Complete());
			self.Error(x => arg.Complete());
			return arg;
		}
	}

	[System.Serializable]
	public class UserEvent : DeSignal<User> { }

	[System.Serializable]
	public class UsersEvent : DeSignal<DeviceUserArg> { }

	[System.Serializable]
	public class ThirdPartyLoginPromise : Promise<ThirdPartyLoginResponse>
	{
		public AuthThirdParty ThirdParty { get; }

		public ThirdPartyLoginPromise(AuthThirdParty thirdParty)
		{
			ThirdParty = thirdParty;
		}
	}

	public class ThirdPartyLoginResponse
	{
		public string AuthToken;
		public readonly bool Cancelled;
		public readonly bool AuthTokenOneUseOnly;

		public ThirdPartyLoginResponse() { }

		public ThirdPartyLoginResponse(string authToken, bool cancelled = false, bool oneUseOnly = false)
		{
			AuthToken = authToken;
			Cancelled = cancelled;
			AuthTokenOneUseOnly = oneUseOnly;
		}

		public static ThirdPartyLoginResponse CANCELLED = new ThirdPartyLoginResponse(null, true);
	}

	[System.Serializable]
	public class ThirdPartyLoginPromiseEvent : DeSignal<ThirdPartyLoginPromise> { }

	[System.Serializable]
	public class DeviceUserArg
	{
		public User ActiveUser;
		public List<UserBundle> OtherUsers;
	}

	[HelpURL(Constants.URLs.Documentations.URL_DOC_ACCOUNT_HUD)]
	public class AccountManagementSignals : DeSignalTower
	{
#pragma warning disable CS0649
		[SerializeField] private AccountForgotPassword _accountForgotPassword;
#pragma warning restore CS0649

		[Header("Flow Events")]
		public ToggleEvent OnToggleAccountManagement;

		public LoadingEvent Loading;
		public ErrorEvent OnError;

		[Space(10f)]
		[Header("Registration Events")]
		public EmailEvent EmailIsAvailable;

		public EmailEvent EmailIsRegistered;
		public EmailEvent EmailIsInvalid;

		[Space(10f)]
		[Header("Forgot Password Events")]
		public EmailEvent ForgotPasswordEmailSent;

		[Space(10f)]
		[Header("User Change Events")]
		public UserEvent UserAnonymous;

		public UserEvent UserAvailable;
		public UserEvent UserLoggedIn;
		public UserEvent UserSwitchAvailable;
		public UserEvent UserLoggingOut;
		public UsersEvent DeviceUsersAvailable;
		public ThirdPartyLoginPromiseEvent ThirdPartyLoginAttempted;

		private string _currentEmail;

		private static TokenResponse _pendingToken;
		private static User _pendingUser;
		private static bool _toggleState;

		public static bool ToggleState => _toggleState;

		protected override void OnAfterDisable()
		{
			API.Instance.Then(de =>
			{
				de.OnUserLoggingOut -= OnUserLoggingOut;
				de.OnUserChanged -= TriggerUserLoggedIn;
			});
		}

		protected override void OnAfterEnable()
		{
			API.Instance.Then(de =>
			{
				de.OnUserLoggingOut += OnUserLoggingOut;
				de.OnUserChanged += TriggerUserLoggedIn;
			});
		}

		private void OnUserLoggingOut(User user)
		{
			this.BroadcastSignal(user, UserLoggingOut);
		}

		public void ToggleAccountManagement()
		{
			_toggleState = !_toggleState;
			var count = 0;
			ForAll<AccountManagementSignals>(signals =>
			{
				count += signals?.OnToggleAccountManagement?.GetPersistentEventCount() ?? 0;
			});
			if (count == 0)
			{
				Debug.LogWarning("There is no account management flow in the scene, so this toggle button does nothing. Please ensure there is an Account Management Flow in the scene.");
				return;
			}
			Broadcast(_toggleState, s => s.OnToggleAccountManagement);
		}

		public void ToggleAccountManagement(bool desiredState)
		{
			if (desiredState == ToggleState) return;

			_toggleState = desiredState;
			Broadcast(_toggleState, s => s.OnToggleAccountManagement);
		}

		public void CheckSignedInUser()
		{
			WithLoading("Fetching Account...", API.Instance.FlatMap(de =>
			{
				var activeUser = de.User;
				if (activeUser.HasAnyCredentials())
				{
					DeferBroadcast(activeUser, s => s.UserAvailable);
				}
				else
				{
					DeferBroadcast(activeUser, s => s.UserAnonymous);
				}

				return de.GetDeviceUsers().Error(ex =>
				{
					Debug.LogError("Unable to load device users");
					Debug.LogError(ex);
				}).Map(userBundles =>
				{
					var otherUserBundles = userBundles
										   .Where(userBundle => userBundle.User.id != activeUser.id)
										   .ToList();
					otherUserBundles.Sort((a, b) => a.User.id.CompareTo(b.User.id));

					var deviceUserArg = new DeviceUserArg
					{
						ActiveUser = activeUser,
						OtherUsers = otherUserBundles
					};
					Broadcast(deviceUserArg, s => s.DeviceUsersAvailable);
					return deviceUserArg;
				});
			})).Error(HandleError);
		}

		public void UpdateLoginEmail(TextReference textReference)
		{
			UpdateLoginEmail(textReference.Value);
		}

		public void UpdateLoginEmail(string currentEmail)
		{
			if (string.IsNullOrEmpty(currentEmail))
			{
				return; // don't do anything for an empty email, it was probably erroneous
			}

			_currentEmail = currentEmail;

			if (!IsValidEmail(currentEmail))
			{
				// invoke for all instances...
				DeferBroadcast(currentEmail, s => s.EmailIsInvalid);
				return;
			}

			WithLoading("Checking Account...", API.Instance.Then(api => api.IsEmailRegistered(_currentEmail).Then(registered =>
			{
				if (registered)
				{
					DeferBroadcast(_currentEmail, s => s.EmailIsRegistered);
				}
				else
				{
					DeferBroadcast(_currentEmail, s => s.EmailIsAvailable);
				}
			})));
		}

		public void Login(LoginArguments reference)
		{
			Login(reference.Email.Value, reference.Password.Value);
		}

		public void Login(TokenReference reference)
		{
			_pendingToken = reference.Bundle.Token;
			_pendingUser = reference.Bundle.User;

			OfferSwitch(_pendingUser);
		}

		public void Login(string email, string password)
		{
			/*
			 * When a login attempt is made with email/password, 4 things could be true.
			 *
			 * | IS_EMAIL_REGISTERED | USER_ALREADY_EXISTS_ON_DEVICE | USER_HAS_EMAIL_CREDS | RESULT
			 * | ------------------- | ----------------------------- | -------------------- | ------>
			 * | YES                 | YES                           | -                    | use the existing token to sign in.
			 * | YES                 | NO                            | -                    | offer an account switch
			 * | NO                  | -                             | NO                   | attach the credentials to the current account
			 * | NO                  | -                             | YES                  | attach the credentials to a new account <---
			 *
			 */

			_pendingUser = null;
			_pendingToken = null;

			if (!IsValidEmail(email))
			{
				Broadcast(email, s => s.EmailIsInvalid);
				return;
			}

			if (!GuardPasswordStrength(password))
			{
				return;
			}

			WithLoading("Logging In...", API.Instance.FlatMap(de =>
			{
				return de.GetDeviceUsers().FlatMap(deviceUsers =>
				{
					return de.IsEmailRegistered(email).FlatMap(registered =>
					{
						var currentUserHasEmail = de.User.HasDBCredentials();
						var storedUser =
							deviceUsers.FirstOrDefault(b => b.User.email != null && b.User.email.Equals(email));

						var shouldSwitchUser = registered;
						var shouldCreateNewUser = !registered && currentUserHasEmail;
						var shouldAttachToCurrentUser = !registered && !currentUserHasEmail;

						if (shouldSwitchUser)
						{
							return GetAccountWithCredentials(de, email, password)
								.Then(OfferSwitch);
						}

						if (shouldCreateNewUser)
						{
							return LoginToNewUser(de)
								.FlatMap(_ => AttachEmailToCurrentUser(de, email, password));
						}

						if (shouldAttachToCurrentUser)
						{
							return AttachEmailToCurrentUser(de, email, password);
						}

						throw new Exception(
							$"unrecognized login state. registered=[{registered}] currentUserHasEmail=[{currentUserHasEmail}] storedUser=[{storedUser}]");
					});
				});
			})).Error(HandleError);
		}

		public void LoginThirdParty(ThirdPartyLoginArgument argument)
		{
			/*
			 * When a 3rd party login attempt is made, 3 things could be true.
			 *
			 * | IS_THIRD_PARTY_REGISTERED | USER_HAS_3RD_PARTY_CREDS | RESULT
			 * | ------------------------- | -------------------------| --------------------
			 * | NO                        | -                        | offer account switch with the 3rd party token
			 * | YES                       | YES                      | attach the credentials to a new account
			 * | YES                       | NO                       | attach the credentials to the current account
			 *
			 */
			var promise = new ThirdPartyLoginPromise(argument.ThirdParty);

			WithLoading("Logging In...",
						promise.FlatMap(response => StartThirdPartyLogin(response, argument.ThirdParty)))
				.Error(HandleError);
			DeferBroadcast(promise, s => s.ThirdPartyLoginAttempted);
		}

		public void BecomeAnonymous()
		{
			API.Instance.Then(de =>
			{
				WithCriticalLoading("New Account...", de.AuthService.CreateUser().Then(newToken =>
				{
					de.ApplyToken(newToken).Then(_ => { CheckSignedInUser(); });
				}));
			}).Error(HandleError);
		}

		public void ForgetUser(TokenReference reference)
		{
			API.Instance.Then(de =>
			{
				if (reference.Bundle.User.id == de.User.id)
				{
					throw new Exception("Cannot forget current user");
				}

				de.RemoveDeviceUser(reference.Bundle.Token);
			}).Error(HandleError);
		}

		public void StartForgotPassword(ForgotPasswordArguments reference)
		{
			var email = reference.Email.Value;
			API.Instance
			   .Then(de =>
			   {
				   WithLoading("Sending Email...", de.AuthService.IssuePasswordUpdate(email))
				   .Then(_ => DeferBroadcast(email, s => s.ForgotPasswordEmailSent));
			   })
			   .Error(ex =>
			   {
				   _accountForgotPassword.ChangePasswordRequestSent(false);
				   HandleError(ex);
			   });
		}

		public void ConfirmForgotPassword(ForgotPasswordArguments reference)
		{
			ConfirmForgotPassword(reference.Email.Value, reference.Code.Value, reference.Password.Value);
		}

		public void ConfirmForgotPassword(string email, string code, string password)
		{
			API.Instance.Then(de =>
			{
				WithLoading("Confirming Code...", de.AuthService.ConfirmPasswordUpdate(code, password)).Then(_ =>
			 {
				 Login(email, password);
			 }).Error(ex =>
				{
					_accountForgotPassword.ChangePasswordRequestSent(false);
					HandleError(ex);
				});
			});
		}

		public void AcceptAccountSwitch()
		{
			if (_pendingToken == null || _pendingUser == null)
			{
				throw new Exception(
					"There was no account switch available. This can only be run after a login as been attempted, that links to another account");
			}

			API.Instance.Then(de =>
			{
				WithCriticalLoading("Switching Account...", de.ApplyToken(_pendingToken))
					.Error(HandleError);
			});
		}

		private Promise<User> StartThirdPartyLogin(ThirdPartyLoginResponse thirdPartyResponse,
												   AuthThirdParty thirdParty)
		{
			return API.Instance.FlatMap(api => ThirdPartyLogin(api, thirdPartyResponse, thirdParty));
		}

		Promise<User> ThirdPartyLogin(IBeamableAPI beamableAPI,
									  ThirdPartyLoginResponse thirdPartyResponse,
									  AuthThirdParty thirdParty)
		{
			if (thirdPartyResponse.Cancelled)
			{
				return Promise<User>.Successful(beamableAPI.User);
			}

			var token = thirdPartyResponse.AuthToken;
			return beamableAPI.AuthService.IsThirdPartyAvailable(thirdParty, token)
							  .FlatMap(available =>
										   HandleThirdPartyToken(beamableAPI, available, thirdPartyResponse,
																 thirdParty));
		}

		Promise<User> HandleThirdPartyToken(IBeamableAPI beamableAPI,
											bool available,
											ThirdPartyLoginResponse thirdPartyResponse,
											AuthThirdParty thirdParty)
		{
			var userHasCredentials = beamableAPI.User.HasThirdPartyAssociation(thirdParty);

			var shouldSwitchUsers = !available;
			var shouldCreateUser = available && userHasCredentials;
			var shouldAttachToCurrentUser = available && !userHasCredentials;

			Promise<User> ConnectTokenWithAccount(string result)
			{
				var token = result;
				if (shouldSwitchUsers)
				{
					return GetAccountWithCredentials(beamableAPI, thirdParty, token)
						.Then(OfferSwitch);
				}

				if (shouldCreateUser)
				{
					return LoginToNewUser(beamableAPI)
						.FlatMap(_ => AttachThirdPartyToCurrentUser(beamableAPI, thirdParty, token));
				}

				if (shouldAttachToCurrentUser)
				{
					return AttachThirdPartyToCurrentUser(beamableAPI, thirdParty, token);
				}

				throw new Exception(
					$"unrecognized third party state. thirdparty=[{thirdParty}] available=[{available}] userHasCredentials=[{userHasCredentials}]");
			}

			if (thirdPartyResponse.AuthTokenOneUseOnly)
			{
				return GetNewAuthToken(thirdParty)
					.FlatMap(ConnectTokenWithAccount);
			}

			return ConnectTokenWithAccount(thirdPartyResponse.AuthToken);
		}

		Promise<string> GetNewAuthToken(AuthThirdParty thirdParty)
		{
			switch (thirdParty)
			{
#if BEAMABLE_GPGS && UNITY_ANDROID
				case AuthThirdParty.GoogleGamesServices:
					return SignInWithGPG.RequestServerSideToken();
#endif
				default:
					throw new Exception($"{thirdParty} does not need new auth token each time");
			}
		}

		private void HandleError(Exception err)
		{
			switch (err)
			{
				case PlatformRequesterException ex when ex.Status == 401 || ex.Status == 403:
					DeferBroadcast(
						AccountManagementConfiguration.Instance.Overrides.GetErrorMessage(ErrorEvent.BAD_CREDENTIALS),
						s => s.OnError);
					break;
				case PlatformRequesterException ex when string.IsNullOrEmpty(ex.Error.message):
					Debug.LogError("Account Flow Error with empty message ");
					DeferBroadcast(
						AccountManagementConfiguration.Instance.Overrides.GetErrorMessage(ErrorEvent.SERVER_ERROR),
						s => s.OnError);
					break;
				case Exception ex:
					Debug.LogError("Account Flow Error: " + ex.Message);
					DeferBroadcast(ex.Message, s => s.OnError);
					break;
				default:
					DeferBroadcast(
						AccountManagementConfiguration.Instance.Overrides.GetErrorMessage(ErrorEvent.SERVER_ERROR),
						s => s.OnError);
					break;
			}
		}

		private bool GuardPasswordStrength(string password)
		{
			if (!AccountManagementConfiguration.Instance.Overrides.IsPasswordStrong(password))
			{
				DeferBroadcast(
					AccountManagementConfiguration.Instance.Overrides.GetErrorMessage(ErrorEvent.PASSWORD_STRENGTH),
					s => s.OnError);
				return false;
			}

			return true;
		}

		private void OfferSwitch(User user)
		{
			API.Instance.Then(api =>
			{
				if (api.User.id == user.id)
				{
					api.UpdateUserData(user);
				}
				else
				{
					Broadcast(user, s => s.UserSwitchAvailable);
				}
			});
		}

		private Promise<User> GetAccountWithCredentials(IBeamableAPI de, string email, string password)
		{
			return de.AuthService.Login(email, password)
					 .RecoverWith(ex =>
					 {
						 if (ex is PlatformRequesterException platEx && string.Equals("auth", platEx.Error.service) &&
							 string.Equals("UnableToMergeError", platEx.Error.error))
						 {
							 return de.AuthService.Login(email, password, false);
						 }
						 return Promise<TokenResponse>.Failed(ex);
					 })
					 .FlatMap(token => SetPendingUser(de, token));
		}

		private Promise<User> GetAccountWithCredentials(IBeamableAPI de, AuthThirdParty thirdParty, string accessToken)
		{
			return de.AuthService.LoginThirdParty(thirdParty, accessToken)
					 .RecoverWith(ex =>
					 {
						 if (ex is PlatformRequesterException platEx && string.Equals("auth", platEx.Error.service) &&
							 string.Equals("UnableToMergeError", platEx.Error.error))
						 {
							 return de.AuthService.LoginThirdParty(thirdParty, accessToken, false);
						 }
						 return Promise<TokenResponse>.Failed(ex);
					 })
					 .FlatMap(token => SetPendingUser(de, token));
		}

		private Promise<User> SetPendingUser(IBeamableAPI de, TokenResponse token)
		{
			_pendingToken = token;
			return de.AuthService.GetUser(token).Then(user =>
			{
				_pendingUser = user;
			});
		}

		private Promise<Unit> LoginToNewUser(IBeamableAPI de)
		{
			return WithCriticalLoading("New Account...", de.LoginToNewUser());
		}

		private Promise<User> AttachEmailToCurrentUser(IBeamableAPI de, string email, string password)
		{
			return WithCriticalLoading("Loading...", de.AttachEmailToCurrentUser(email, password));
		}

		private Promise<User> AttachThirdPartyToCurrentUser(IBeamableAPI de,
															AuthThirdParty thirdParty,
															string accessToken)
		{
			return WithCriticalLoading("Loading...", de.AttachThirdPartyToCurrentUser(thirdParty, accessToken));
		}

		private Promise<User> GetExistingAccount(IBeamableAPI de, UserBundle bundle)
		{
			return de.AuthService.LoginRefreshToken(bundle.Token.refresh_token).Map(token =>
			{
				_pendingToken = token;
				_pendingUser = bundle.User;
				return _pendingUser;
			});
		}

		private void TriggerUserLoggedIn(User user)
		{
			this.BroadcastSignal(user, UserLoggedIn);
		}

		public Promise<T> WithLoading<T>(string message, Promise<T> promise)
		{
			return WithLoading(message, false, promise);
		}

		public Promise<T> WithCriticalLoading<T>(string message, Promise<T> promise)
		{
			return WithLoading(message, true, promise);
		}

		private Promise<T> WithLoading<T>(string message, bool critical, Promise<T> promise)
		{
			var arg = promise.ToLoadingArg(message, critical);
			Broadcast(arg, s => s.Loading);

			return promise;
		}

		public void DeferBroadcast<TArg>(TArg arg, Func<AccountManagementSignals, DeSignal<TArg>> getter)
		{
			if (this != null && gameObject != null && gameObject.activeInHierarchy)
			{
				StartCoroutine(Defer(() => Broadcast(arg, getter)));
			}
		}

		private void Broadcast<TArg>(TArg arg, Func<AccountManagementSignals, DeSignal<TArg>> getter)
		{
			this.BroadcastSignal(arg, getter);
		}

		private IEnumerator Defer(Action action)
		{
			yield return Yielders.EndOfFrame;
			if (isActiveAndEnabled)
			{
				action();
			}
		}

		private static bool IsValidEmail(string email)
		{
			try
			{
				var addr = new System.Net.Mail.MailAddress(email);
				return addr.Address == email;
			}
			catch
			{
				return false;
			}
		}

		public static void SetPending(User user, TokenResponse token)
		{
			_pendingUser = user;
			_pendingToken = token;
		}
	}
}
