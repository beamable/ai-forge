using Beamable.Api.Auth;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Auth;
using System;
using UnityEngine;

namespace Beamable.Tests.Runtime.Api
{
	public class MockAuthService : IAuthService
	{
		public Func<string, Promise<bool>> IsEmailAvailableDelegate;
		public Func<string, string, Promise<User>> RegisterDbCredentialsDelegate;
		public Func<string, string, bool, Promise<TokenResponse>> LoginDelegate;
		public Func<TokenResponse, Promise<User>> GetUserDelegate;
		public Func<Promise<User>> GetCurrentUserDelegate;


		public Promise<User> GetUser()
		{
			return GetCurrentUserDelegate();
		}

		public Promise<User> SetLanguage(string languageCodeISO6391)
		{
			throw new NotImplementedException();
		}

		public Promise<User> GetUserForEditor()
		{
			return GetCurrentUserDelegate();
		}

		public Promise<User> GetUser(TokenResponse token)
		{
			return GetUserDelegate(token);
		}

		public Promise<bool> IsEmailAvailable(string email)
		{
			return IsEmailAvailableDelegate(email);
		}

		public Promise<bool> IsThirdPartyAvailable(AuthThirdParty thirdParty, string token)
		{
			throw new NotImplementedException();
		}

		public Promise<TokenResponse> CreateUser()
		{
			throw new System.NotImplementedException();
		}

		public Promise<TokenResponse> LoginRefreshToken(string refreshToken)
		{
			throw new System.NotImplementedException();
		}

		public Promise<TokenResponse> Login(string username, string password, bool mergeGamerTagToAccount = true)
		{
			return LoginDelegate(username, password, mergeGamerTagToAccount);
		}

		public Promise<TokenResponse> Login(
		   string username,
		   string password,
		   bool mergeGamerTagToAccount = true,
		   bool customerScoped = false
		)
		{
			return LoginDelegate(username, password, mergeGamerTagToAccount);
		}

		public Promise<TokenResponse> LoginThirdParty(AuthThirdParty thirdParty, string thirdPartyToken, bool includeAuthHeader = true)
		{
			throw new System.NotImplementedException();
		}

		public Promise<User> RegisterDBCredentials(string email, string password)
		{
			return RegisterDbCredentialsDelegate(email, password);
		}

		public Promise<User> RegisterThirdPartyCredentials(AuthThirdParty thirdParty, string accessToken)
		{
			throw new System.NotImplementedException();
		}

		public Promise<EmptyResponse> IssueEmailUpdate(string newEmail)
		{
			throw new System.NotImplementedException();
		}

		public Promise<EmptyResponse> ConfirmEmailUpdate(string code, string password)
		{
			throw new System.NotImplementedException();
		}

		public Promise<EmptyResponse> IssuePasswordUpdate(string email)
		{
			throw new System.NotImplementedException();
		}

		public Promise<EmptyResponse> ConfirmPasswordUpdate(string code, string newPassword)
		{
			throw new System.NotImplementedException();
		}

		public Promise<CustomerRegistrationResponse> RegisterCustomer(string email, string password, string projectName, string customerName, string alias)
		{
			throw new System.NotImplementedException();
		}
		public Promise<User> RemoveThirdPartyAssociation(AuthThirdParty thirdParty, string token)
		{
			throw new System.NotImplementedException();
		}

		public Promise<CurrentProjectResponse> GetCurrentProject()
		{
			throw new NotImplementedException();
		}

		public Promise<ExternalLoginResponse> LoginExternalIdentity(string externalToken,
															 string providerService,
															 string providerNamespace,
															 ChallengeSolution challengeSolution = null,
															 bool mergeGamerTagToAccount = true)
		{
			throw new NotImplementedException();
		}


		public Promise<AttachExternalIdentityResponse> AttachIdentity(string externalToken,
																	  string providerService,
																	  string providerNamespace = "",
																	  ChallengeSolution challengeSolution = null)
		{
			throw new NotImplementedException();
		}

		public Promise<DetachExternalIdentityResponse> DetachIdentity(string providerService, string userId, string providerNamespace = "")
		{
			throw new NotImplementedException();
		}

		public Promise<ExternalAuthenticationResponse> AuthorizeExternalIdentity(string externalToken,
																				 string providerService,
																				 string providerNamespace = "",
																				 ChallengeSolution challengeSolution = null)
		{
			throw new NotImplementedException();
		}

		public ChallengeToken ParseChallengeToken(string token)
		{
			throw new NotImplementedException();
		}

		public Promise<User> SetLanguage(SystemLanguage language)
		{
			throw new NotImplementedException();
		}

		public Promise<bool> IsThisDeviceIdAvailable()
		{
			throw new NotImplementedException();
		}

		public Promise<TokenResponse> LoginDeviceId(bool mergeGamerTagToAccount)
		{
			throw new NotImplementedException();
		}

		public Promise<User> RegisterDeviceId()
		{
			throw new NotImplementedException();
		}

		public Promise<User> RemoveDeviceId()
		{
			throw new NotImplementedException();
		}

		public Promise<User> RemoveDeviceIds(string[] deviceIds)
		{
			throw new NotImplementedException();
		}

		public Promise<User> RemoveAllDeviceIds()
		{
			throw new NotImplementedException();
		}

		public Promise<string> GetDeviceId()
		{
			throw new NotImplementedException();
		}

		public IBeamableRequester Requester { get; }
	}
}
