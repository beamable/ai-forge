using Beamable.Common.Content;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Beamable.Common.Api.Auth
{
	public class AuthApi : IAuthApi
	{
		protected const string TOKEN_URL = "/basic/auth/token";
		public const string ACCOUNT_URL = "/basic/accounts";

		private IBeamableRequester _requester;
		private readonly IAuthSettings _settings;

		public IBeamableRequester Requester => _requester;

		public AuthApi(IBeamableRequester requester, IAuthSettings settings = null)
		{
			_requester = requester;
			_settings = settings ?? new DefaultAuthSettings();
		}

		public Promise<User> GetUser()
		{
			return _requester.Request<User>(Method.GET, $"{ACCOUNT_URL}/me", useCache: true);
		}

		public Promise<User> SetLanguage(string languageCodeISO6391)
		{
			return _requester.Request<User>(Method.PUT, $"{ACCOUNT_URL}/me?language={languageCodeISO6391}");
		}

		public virtual async Promise<User> GetUser(TokenResponse token)
		{
			var tokenizedRequester = _requester.WithAccessToken(token);
			var user = await tokenizedRequester.Request<User>(Method.GET, $"{ACCOUNT_URL}/me", useCache: true);
			return user;
		}

		public Promise<bool> IsEmailAvailable(string email)
		{
			var encodedEmail = _requester.EscapeURL(email);
			return _requester
				   .Request<AvailabilityResponse>(Method.GET, $"{ACCOUNT_URL}/available?email={encodedEmail}", null,
												  false)
				   .Map(resp => resp.available);
		}

		public Promise<bool> IsThirdPartyAvailable(AuthThirdParty thirdParty, string token)
		{
			return _requester
				   .Request<AvailabilityResponse>(
					   Method.GET,
					   $"{ACCOUNT_URL}/available/third-party?thirdParty={thirdParty.GetString()}&token={token}", null,
					   false)
				   .Map(resp => resp.available);
		}

		public Promise<TokenResponse> CreateUser()
		{
			var req = new CreateUserRequest { grant_type = "guest" };
			return _requester.Request<TokenResponse>(Method.POST, TOKEN_URL, req, false);
			//return _requester.RequestForm<TokenResponse>(TOKEN_URL, form, false);
		}

		[Serializable]
		private class CreateUserRequest
		{
			public string grant_type;
		}

		public Promise<TokenResponse> LoginRefreshToken(string refreshToken)
		{
			var req = new LoginRefreshTokenRequest { grant_type = "refresh_token", refresh_token = refreshToken };
			return _requester.Request<TokenResponse>(Method.POST, TOKEN_URL, req, includeAuthHeader: false);
		}

		[Serializable]
		private class LoginRefreshTokenRequest
		{
			public string grant_type;
			public string refresh_token;
		}

		public Promise<TokenResponse> Login(string username,
											string password,
											bool mergeGamerTagToAccount = true,
											bool customerScoped = false)
		{
			var body = new LoginRequest
			{
				username = username,
				grant_type = "password",
				password = password,
				customerScoped = customerScoped
			};

			return _requester.Request<TokenResponse>(Method.POST, TOKEN_URL, body,
													 includeAuthHeader: mergeGamerTagToAccount);
		}

		[Serializable]
		private class LoginRequest
		{
			public string grant_type;
			public string username;
			public string password;
			public bool customerScoped;
		}

		public Promise<TokenResponse> LoginThirdParty(AuthThirdParty thirdParty,
													  string thirdPartyToken,
													  bool includeAuthHeader = true)
		{
			var req = new LoginThirdPartyRequest
			{
				grant_type = "third_party",
				third_party = thirdParty.GetString(),
				token = thirdPartyToken
			};
			return _requester.Request<TokenResponse>(Method.POST, TOKEN_URL, req, includeAuthHeader);
		}

		[Serializable]
		private class LoginThirdPartyRequest
		{
			public string grant_type;
			public string third_party;
			public string token;
		}

		public Promise<User> RegisterDBCredentials(string email, string password)
		{
			var req = new RegisterDBCredentialsRequest { email = email, password = password };
			return _requester.Request<User>(Method.POST, $"{ACCOUNT_URL}/register", req);
		}

		[Serializable]
		private class RegisterDBCredentialsRequest
		{
			public string email, password;
		}

		public Promise<User> RemoveThirdPartyAssociation(AuthThirdParty thirdParty, string token)
		{
			return _requester.Request<User>(Method.DELETE,
											$"{ACCOUNT_URL}/me/third-party?thirdParty={thirdParty.GetString()}&token={token}",
											null, true);
		}

		public Promise<User> RegisterThirdPartyCredentials(AuthThirdParty thirdParty, string accessToken)
		{
			var req = new RegisterThirdPartyCredentialsRequest
			{
				thirdParty = thirdParty.GetString(),
				token = accessToken
			};
			return _requester.Request<User>(Method.PUT, $"{ACCOUNT_URL}/me", req);
		}

		[Serializable]
		private class RegisterThirdPartyCredentialsRequest
		{
			public string thirdParty;
			public string token;
		}

		public Promise<EmptyResponse> IssueEmailUpdate(string newEmail)
		{
			var req = new IssueEmailUpdateRequest { newEmail = newEmail };
			return _requester.Request<EmptyResponse>(Method.POST, $"{ACCOUNT_URL}/email-update/init", req);
		}

		[Serializable]
		private class IssueEmailUpdateRequest
		{
			public string newEmail;
		}

		public Promise<EmptyResponse> ConfirmEmailUpdate(string code, string password)
		{
			var req = new ConfirmEmailUpdateRequest { code = code, password = password };
			return _requester.Request<EmptyResponse>(Method.POST, $"{ACCOUNT_URL}/email-update/confirm", req);
		}

		[Serializable]
		private class ConfirmEmailUpdateRequest
		{
			public string code, password;
		}

		public Promise<EmptyResponse> IssuePasswordUpdate(string email)
		{
			var req = new IssuePasswordUpdateRequest
			{
				email = email,
				codeType = _settings.PasswordResetCodeType.Serialize()
			};
			return _requester.Request<EmptyResponse>(Method.POST, $"{ACCOUNT_URL}/password-update/init", req);
		}

		[Serializable]
		private class IssuePasswordUpdateRequest
		{
			public string email;
			public string codeType;
		}

		public Promise<EmptyResponse> ConfirmPasswordUpdate(string code, string newPassword)
		{
			var req = new ConfirmPasswordUpdateRequest { code = code, newPassword = newPassword };
			return _requester.Request<EmptyResponse>(Method.POST, $"{ACCOUNT_URL}/password-update/confirm", req);
		}

		[Serializable]
		private class ConfirmPasswordUpdateRequest
		{
			public string code, newPassword;
		}

		public Promise<CustomerRegistrationResponse> RegisterCustomer(string email,
																	  string password,
																	  string projectName,
																	  string customerName,
																	  string alias)
		{
			var request = new CustomerRegistrationRequest(email, password, projectName, customerName, alias);
			return _requester.Request<CustomerRegistrationResponse>(Method.POST, "/basic/realms/customer", request,
																	false);
		}

		public Promise<CurrentProjectResponse> GetCurrentProject()
		{
			return _requester.Request<CurrentProjectResponse>(Method.GET, "/basic/realms/project", null,
															  useCache: true);
		}

		public Promise<ExternalLoginResponse> LoginExternalIdentity(
			string externalToken,
			string providerService,
			string providerNamespace,
			ChallengeSolution challengeSolution = null,
			bool mergeGamerTagToAccount = true)
		{
			ExternalAuthenticationRequest body;

			if (challengeSolution == null)
			{
				body = new ExternalAuthenticationRequest
				{
					grant_type = "external",
					external_token = externalToken,
					provider_service = providerService,
					provider_namespace = providerNamespace,
				};
			}
			else
			{
				body = new ChallengedExternalAuthenticationRequest
				{
					grant_type = "external",
					external_token = externalToken,
					provider_service = providerService,
					provider_namespace = providerNamespace,
					challenge_solution = challengeSolution

				};
			}
			return Requester.Request<ExternalLoginResponse>(
				Method.POST, TOKEN_URL, body, includeAuthHeader: mergeGamerTagToAccount, parser: json =>
				{
					var res = new ExternalLoginResponse();

					var authResult = JsonUtility.FromJson<ExternalAuthenticationResponse>(json);

					if (authResult?.challenge?.Length > 0)
					{
						// the response object is requesting a further challenge to be made.
						res.challenge.Set(authResult);
					}
					else
					{
						var tokenResult = JsonUtility.FromJson<TokenResponse>(json);
						res.tokenResponse.Set(tokenResult);
					}

					return res;
				});
		}

		public Promise<AttachExternalIdentityResponse> AttachIdentity(string externalToken,
																	  string providerService,
																	  string providerNamespace = "",
																	  ChallengeSolution challengeSolution = null)
		{
			AttachExternalIdentityRequest body;

			if (challengeSolution == null)
			{
				body = new AttachExternalIdentityRequest
				{
					provider_service = providerService,
					provider_namespace = providerNamespace,
					external_token = externalToken
				};
			}
			else
			{
				body = new ChallengedAttachExternalIdentityRequest
				{
					provider_service = providerService,
					provider_namespace = providerNamespace,
					external_token = externalToken,
					challenge_solution = challengeSolution,
				};
			}

			return Requester.Request<AttachExternalIdentityResponse>(
				Method.POST, $"{ACCOUNT_URL}/external_identity", body);
		}

		public Promise<DetachExternalIdentityResponse> DetachIdentity(string providerService,
																	  string userId,
																	  string providerNamespace = "")
		{
			DetachExternalIdentityRequest body =
				new DetachExternalIdentityRequest
				{
					provider_service = providerService,
					provider_namespace = providerNamespace,
					user_id = userId,
				};

			return Requester.Request<DetachExternalIdentityResponse>(Method.DELETE, $"{ACCOUNT_URL}/external_identity",
																	 body);
		}

		public Promise<ExternalAuthenticationResponse> AuthorizeExternalIdentity(string externalToken,
			string providerService,
			string providerNamespace = "",
			ChallengeSolution challengeSolution = null)
		{
			ExternalAuthenticationRequest body;

			if (challengeSolution == null)
			{
				body = new ExternalAuthenticationRequest
				{
					grant_type = "external",
					external_token = externalToken,
					provider_service = providerService,
					provider_namespace = providerNamespace,
				};
			}
			else
			{
				body = new ChallengedExternalAuthenticationRequest
				{
					grant_type = "external",
					external_token = externalToken,
					provider_service = providerService,
					challenge_solution = challengeSolution
				};
			}

			return Requester.Request<ExternalAuthenticationResponse>(Method.POST, TOKEN_URL, body);
		}

		public ChallengeToken ParseChallengeToken(string token)
		{
			string[] tokenParts = token.Split('.');

			if (long.TryParse(tokenParts[1], out long validUntil))
			{
				return new ChallengeToken
				{
					challenge = tokenParts[0],
					validUntil = validUntil,
					signature = tokenParts[2]
				};
			}

			throw new Exception("Problem with challenge token parsing");
		}
	}

	/// <summary>
	/// This type defines the %UserBundle which combines %User and %TokenResponse.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/accounts-feature">Accounts</a> feature documentation
	/// - See Beamable.Api.Auth.AuthService script reference
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class UserBundle
	{
		/// <summary>
		/// The <see cref="User"/>
		/// </summary>
		public User User;

		/// <summary>
		/// The stored <see cref="TokenResponse"/> that the <see cref="User"/> last used to sign in.
		/// </summary>
		public TokenResponse Token;

		public override bool Equals(object obj)
		{
			return Equals(obj as UserBundle);
		}

		public override int GetHashCode()
		{
			return User.id.GetHashCode();
		}

		public bool Equals(UserBundle other)
		{
			if (other == null) return false;

			return other.User.id == User.id;
		}
	}

	/// <summary>
	/// This type defines the %Client main entry point for the %User feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/accounts-feature">Accounts</a> feature documentation
	/// - See Beamable.Api.Auth.AuthService script reference
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class User
	{

		/// <summary>
		/// The unique id of the player, sometimes called a "dbid".
		/// </summary>
		public long id;

		/// <summary>
		/// If the player has associated an email with their account, the email will appear here. Null otherwise.
		/// The email can be associated with the <see cref="IAuthApi.RegisterDBCredentials"/> method
		/// </summary>
		public string email;

		/// <summary>
		/// If the player has chosen a language for their account, the language code will appear here. EN by default.
		/// </summary>
		public string language;

		/// <summary>
		/// Scopes are permissions that the player has over the Beamable ecosystem.
		/// Most players will have no scopes.
		/// Players with the role of "tester" will have some "read" based scopes,
		/// Players with the role of "developer" will have most all scopes except those relating to team management, and
		/// Players with the role of "admin" will have single scope with the value of "*", which indicates ALL scopes.
		/// </summary>
		public List<string> scopes;

		/// <summary>
		/// If the player has associated any third party accounts with their account, those will appear here.
		/// The values of the strings will be taken from the <see cref="AuthThirdPartyMethods.GetString"/> method.
		/// Third parties can be associated with the <see cref="IAuthApi.RegisterThirdPartyCredentials"/> method.
		/// </summary>
		public List<string> thirdPartyAppAssociations;

		/// <summary>
		/// If the player has associated any device Ids with their account, those will appear here.
		/// </summary>
		public List<string> deviceIds;

		/// <summary>
		/// If the player has associated any external identities with their account, they will appear here.
		/// </summary>
		public List<ExternalIdentity> external;

		/// <summary>
		/// Check if the player has registered an email address with their account.
		/// </summary>
		/// <returns>true if the email address has been provided, false otherwise.</returns>
		public bool HasDBCredentials()
		{
			return !string.IsNullOrEmpty(email);
		}

		/// <summary>
		/// Check if a specific <see cref="AuthThirdParty"/> exists in the player's <see cref="thirdPartyAppAssociations"/> list.
		/// </summary>
		/// <param name="thirdParty">The <see cref="AuthThirdParty"/> to check the player for</param>
		/// <returns>true if the third party has been associated with the player account, false otherwise.</returns>
		public bool HasThirdPartyAssociation(AuthThirdParty thirdParty)
		{
			return thirdPartyAppAssociations != null && thirdPartyAppAssociations.Contains(thirdParty.GetString());
		}

		/// <summary>
		/// Check if any credentials have been associated with this account, whether email, device ids or third party apps.
		/// </summary>
		/// <returns>true if any credentials exist, false otherwise</returns>
		public bool HasAnyCredentials()
		{
			return HasDBCredentials() || (thirdPartyAppAssociations != null && thirdPartyAppAssociations.Count > 0)
									  || (deviceIds != null && deviceIds.Count > 0);
		}

		/// <summary>
		/// Check if a specific scope exists for the player's permissions.
		/// If the user is an Admin, and has the * scope, then every scope check will return true.
		/// This method reads the scope data from the <see cref="scopes"/> list
		/// </summary>
		/// <param name="scope">The scope you want to check</param>
		/// <returns>true if the scope exists or if the user is an admin, false otherwise</returns>
		public bool HasScope(string scope)
		{
			return scopes.Contains(scope) || scopes.Contains("*");
		}

		/// <summary>
		/// The broadcast checksum is used by the various Player Centric SDKs to determine if an object has changed
		/// since the previous update event.
		/// </summary>
		/// <returns></returns>
		public int GetBroadcastChecksum()
		{
			unchecked
			{
				var hashCode = id.GetHashCode();
				hashCode = (hashCode * 397) ^ (email != null ? email.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (language != null ? language.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (scopes != null ? scopes.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (thirdPartyAppAssociations != null ? thirdPartyAppAssociations.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (deviceIds != null ? deviceIds.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (external != null ? external.GetHashCode() : 0);
				return hashCode;
			}
		}

	}

	[Serializable]
	public class OptionalTokenResponse : Optional<TokenResponse>
	{

	}

	/// <summary>
	/// This type defines the functionality for the %TokenResponse for the %AuthService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/accounts-feature">Accounts</a> feature documentation
	/// - See Beamable.Api.Auth.AuthService script reference
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class TokenResponse
	{
		/// <summary>
		/// The token that will become the <see cref="IAccessToken.Token"/> value.
		/// </summary>
		public string access_token;

		/// <summary>
		/// There are two different types of tokens that Beamable may issue. The possible values are "access", or "refresh"
		/// </summary>
		public string token_type;

		/// <summary>
		/// The number of milliseconds from when the <see cref="TokenResponse"/> was sent by the Beamable servers, to when the
		/// token will be expired. This value informs the <see cref="IAccessToken.ExpiresAt"/> property
		/// </summary>
		public long expires_in;

		/// <summary>
		/// The token that will become the <see cref="IAccessToken.RefreshToken"/> value
		/// </summary>
		public string refresh_token;
	}

	[Serializable]
	public class AvailabilityRequest
	{
		public string email;
	}

	[Serializable]
	public class AvailabilityResponse
	{
		public bool available;
	}

	/// <summary>
	/// The available set of third party apps that Beamable can associate with player accounts.
	/// Note that the serialized state of these values should use the <see cref="AuthThirdPartyMethods.GetString"/> method.
	/// </summary>
	public enum AuthThirdParty
	{
		Facebook,
		FacebookLimited,
		Apple,
		Google,
		GameCenter,
		GameCenterLimited,
		Steam,
		GoogleGamesServices
	}

	public static class AuthThirdPartyMethods
	{
		private static Dictionary<string, AuthThirdParty> _stringToEnum = new Dictionary<string, AuthThirdParty>();
		static AuthThirdPartyMethods()
		{
			foreach (var val in Enum.GetValues(typeof(AuthThirdParty)))
			{
				var enumVal = (AuthThirdParty)val;
				_stringToEnum[GetString(enumVal)] = enumVal;
			}
		}

		public static AuthThirdParty GetAuthThirdParty(string data)
		{
			if (!_stringToEnum.ContainsKey(data))
			{
				throw new InvalidEnumArgumentException(
					$"The given string is not a valid {nameof(AuthThirdParty)}, str=[{data}]");
			}
			return _stringToEnum[data];
		}

		/// <summary>
		/// Convert the given <see cref="AuthThirdParty"/> into a string format that can be sent to Beamable servers.
		/// Also, the Beamable servers treat these strings as special code names for the various third party apps.
		/// If you need to refer to a third party in Beamable's APIs, you should use this function to get the correct string value.
		/// </summary>
		/// <param name="thirdParty">The <see cref="AuthThirdParty"/> to convert to a string</param>
		/// <returns>The string format of the enum</returns>
		public static string GetString(this AuthThirdParty thirdParty)
		{
			switch (thirdParty)
			{
				case AuthThirdParty.Facebook:
					return "facebook";
				case AuthThirdParty.FacebookLimited:
					return "facebooklimited";
				case AuthThirdParty.Apple:
					return "apple";
				case AuthThirdParty.Google:
					return "google";
				case AuthThirdParty.GameCenter:
					return "gamecenter";
				case AuthThirdParty.GameCenterLimited:
					return "gamecenterlimited";
				case AuthThirdParty.Steam:
					return "steam";
				case AuthThirdParty.GoogleGamesServices:
					return "googlePlayServices";
				default:
					return null;
			}
		}
	}

	[System.Serializable]
	public class CustomerRegistrationRequest
	{
		public string email;
		public string password;
		public string projectName;
		public string customerName;
		public string alias;

		public CustomerRegistrationRequest(string email,
										   string password,
										   string projectName,
										   string customerName,
										   string alias)
		{
			this.email = email;
			this.password = password;
			this.projectName = projectName;
			this.customerName = customerName;
			this.alias = alias;
		}
	}

	[System.Serializable]
	public class CustomerRegistrationResponse
	{
		public long cid;
		public string pid;
		public TokenResponse token;
	}

	public class CurrentProjectResponse
	{
		public string cid, pid, projectName;
	}

	public interface IAuthSettings
	{
		CodeType PasswordResetCodeType { get; }
	}

	public class DefaultAuthSettings : IAuthSettings
	{
		public CodeType PasswordResetCodeType { get; set; } = CodeType.PIN;
	}

	public enum CodeType
	{
		UUID, PIN
	}

	public static class CodeTypeExtensions
	{
		public static string Serialize(this CodeType type)
		{
			switch (type)
			{
				case CodeType.PIN: return "PIN";
				default: return "UUID";
			}
		}
	}

	[Serializable]
	public class ExternalIdentity
	{
		public string providerNamespace;
		public string providerService;
		public string userId;
	}

	/// <summary>
	/// Class representing a solution for challenge given by a server. It needs to be send as a part of
	/// <see cref="ChallengedAttachExternalIdentityRequest"/> or <see cref="ChallengedExternalAuthenticationRequest"/>.
	/// To generate a solution You need to use only the first part of a challenge_token that can be retrieved using a
	/// <see cref="IAuthApi.ParseChallengeToken"/> method.
	/// <param name="challenge_token">Token received from a server</param>
	/// <param name="solution">Signed/solved solution that needs to be delivered to server to verify current identity</param>
	/// </summary>
	[Serializable]
	public class ChallengeSolution
	{
		public string challenge_token;
		public string solution;
	}

	[Serializable]
	public class AttachExternalIdentityRequest
	{
		public string provider_service;
		public string provider_namespace;
		public string external_token;
	}

	[Serializable]
	public class ChallengedAttachExternalIdentityRequest : AttachExternalIdentityRequest
	{
		public ChallengeSolution challenge_solution;
	}

	[Serializable]
	public class AttachExternalIdentityResponse
	{
		public string result;
		public string challenge_token;
	}

	[Serializable]
	public class ExternalLoginResponse
	{
		public OptionalExternalAuthenticationResponse challenge = new OptionalExternalAuthenticationResponse();
		public OptionalTokenResponse tokenResponse = new OptionalTokenResponse();
	}

	[Serializable]
	public class DetachExternalIdentityRequest
	{
		public string provider_service;
		public string provider_namespace;
		public string user_id;
	}

	[Serializable]
	public class DetachExternalIdentityResponse
	{
		public string result;
	}

	[Serializable]
	public class ExternalAuthenticationRequest
	{
		public string grant_type;
		public string provider_service;
		public string provider_namespace;
		public string external_token;
	}

	[Serializable]
	public class ChallengedExternalAuthenticationRequest : ExternalAuthenticationRequest
	{
		public ChallengeSolution challenge_solution;
	}

	[Serializable]
	public class OptionalExternalAuthenticationResponse : Optional<ExternalAuthenticationResponse>
	{

	}

	[Serializable]
	public class ExternalAuthenticationResponse
	{
		public string user_id;
		public string challenge;
		public int challenge_ttl;
	}

	[Serializable]
	public struct ChallengeToken
	{
		public string challenge;
		public long validUntil;
		public string signature;
	}
}
