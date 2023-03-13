namespace Beamable.Common.Api.Auth
{
	public interface IAuthApi : IHasBeamableRequester
	{
		/// <summary>
		/// Get the main <see cref="User"/> for the current instance.
		/// </summary>
		/// <returns>A <see cref="Promise{User}"/> representing when the <see cref="User"/> has been retrieved.</returns>
		Promise<User> GetUser();

		/// <summary>
		/// Set the current user's language preference for Beamable.
		/// </summary>
		/// <param name="languageCodeISO6391">
		/// The language code should be an ISO6391 string.
		/// </param>
		/// <returns>A <see cref="Promise{User"/> with the updated language information</returns>
		Promise<User> SetLanguage(string languageCodeISO6391);

		/// <summary>
		/// Get a specific <see cref="User"/> by passing in a <see cref="TokenResponse"/> structure.
		/// The <see cref="User"/> this method returns will be the one that has the token given.
		/// In order to get a <see cref="TokenResponse"/>, you can use the
		/// <see cref="CreateUser"/>, <see cref="Login"/>, <see cref="LoginThirdParty"/>, or <see cref="LoginRefreshToken"/> methods.
		/// </summary>
		/// <param name="token">The <see cref="TokenResponse"/> structure containing token information for the <see cref="User"/> you want to get.</param>
		/// <returns>A <see cref="Promise{User}"/> representing when the <see cref="User"/> has been retrieved.</returns>
		Promise<User> GetUser(TokenResponse token);

		/// <summary>
		/// Check if a given email address is available to be associated with a <see cref="User"/>.
		/// Each email address is only assignable to one <see cref="User"/>, so if one player registers an email,
		/// that email cannot be registered by any other players.
		/// </summary>
		/// <param name="email">The email address to check</param>
		/// <returns>A promise that will result in true if the address is available, false otherwise.</returns>
		Promise<bool> IsEmailAvailable(string email);

		/// <summary>
		/// Check if a given <see cref="AuthThirdParty"/>'s token is available to be associated with a <see cref="User"/>.
		/// Each third party issues a unique access token per player that will be stored on the Beamable servers.
		/// However, if the access token from the third party has already been associated with a <see cref="User"/>,
		/// then it cannot be associated with any other players.
		/// </summary>
		/// <param name="thirdParty">the <see cref="AuthThirdParty"/> that issued the token</param>
		/// <param name="token">the token that was issued by the third party. You should get this directly from the third party itself.</param>
		/// <returns>A promise that will result in true if the token is available, false otherwise.</returns>
		Promise<bool> IsThirdPartyAvailable(AuthThirdParty thirdParty, string token);

		/// <summary>
		/// Create a new <see cref="User"/> on the Beamable server, and return that player's <see cref="TokenResponse"/>.
		/// To log into the new account, use the token response structure.
		/// </summary>
		/// <returns>A <see cref="Promise{TokenResponse}"/> that results in the <see cref="TokenResponse"/> for the new <see cref="User"/></returns>
		Promise<TokenResponse> CreateUser();

		/// <summary>
		/// Use a refresh token string to retrieve a <see cref="TokenResponse"/>. The resulting token response can be used
		/// to change the current <see cref="User"/>.
		/// </summary>
		/// <param name="refreshToken">The refresh token of a <see cref="User"/>. This value can be found in the <see cref="IAccessToken.RefreshToken"/> field.</param>
		/// <returns>A <see cref="Promise{TokenResponse}"/> that results in the <see cref="TokenResponse"/> for the requested <see cref="User"/>'s refresh token</returns>
		Promise<TokenResponse> LoginRefreshToken(string refreshToken);

		/// <summary>
		/// Use email and password credentials to retrieve a <see cref="TokenResponse"/>. The resulting token response can
		/// be used to change the current <see cref="User"/>
		/// A login will only work after the email and password have been registered by using the <see cref="RegisterDBCredentials"/> method.
		/// </summary>
		/// <param name="username">The email address of the <see cref="User"/></param>
		/// <param name="password">The password the player registered when they associated their email address</param>
		/// <param name="mergeGamerTagToAccount">
		/// Since this function can only be called from a <see cref="IAuthApi"/> that already exists,
		/// there must already be some associated <see cref="User"/> account. If the <see cref="mergeGamerTagToAccount"/> is enabled,
		/// then the current player will be merged with the player associated with the given email and password.
		/// </param>
		/// <param name="customerScoped">
		/// The email and password login can return a <see cref="TokenResponse"/> that works for a specific CID / PID combo, or for
		/// an entire CID, across all PIDs. When <see cref="customerScoped"/> is enabled, the resulting token will be eligible for
		/// the entire CID, regardless of PID. However, this type of token will only work for <see cref="User"/>'s who are at least
		/// testers, developers, or admins.
		/// </param>
		/// <returns>A <see cref="Promise{TokenResponse}"/> that results in the <see cref="TokenResponse"/> for the requested <see cref="User"/>'s email/password</returns>
		Promise<TokenResponse> Login(string username,
									 string password,
									 bool mergeGamerTagToAccount = true,
									 bool customerScoped = false);

		/// <summary>
		/// Use a token issued by a third party to retrieve a <see cref="TokenResponse"/>. The resulting token response
		/// can be used to change the current <see cref="User"/>. You should get the <see cref="thirdPartyToken"/> directly
		/// from the third party itself.
		/// A login will only work after the third party has been registered by using the <see cref="RegisterThirdPartyCredentials"/> method.
		/// </summary>
		/// <param name="thirdParty">The <see cref="AuthThirdParty"/> that issued the <see cref="thirdPartyToken"/></param>
		/// <param name="thirdPartyToken">The token that you received from the given <see cref="AuthThirdParty"/></param>
		/// <param name="includeAuthHeader">
		/// Since this function can only be called from a <see cref="IAuthApi"/> that already exists,
		/// there must already be some associated <see cref="User"/> account. If the <see cref="includeAuthHeader"/> is enabled,
		/// then the current player will be merged with the player associated with the given third party credential.
		/// </param>
		/// <returns></returns>
		Promise<TokenResponse> LoginThirdParty(AuthThirdParty thirdParty,
											   string thirdPartyToken,
											   bool includeAuthHeader = true);

		/// <summary>
		/// A <see cref="User"/> can associate an email and password credential to their account.
		/// Once the player registers the credential, they can use the email and password to retrieve
		/// a <see cref="TokenResponse"/> for the account by using the <see cref="Login"/> method.
		/// This method will associate the email and password with the <i>current</i> <see cref="User"/>.
		/// </summary>
		/// <param name="email">
		/// An email address to associate with the player. Email addresses must be unique to each player. You
		/// can use the <see cref="IsEmailAvailable"/> method to check if the given email is available before
		/// attempting the <see cref="RegisterDBCredentials"/>.
		/// </param>
		/// <param name="password">A password for the player to use later to recover their account.</param>
		/// <returns>
		/// A <see cref="Promise{User}"/> that will have the updated <see cref="User"/> data for the current user.
		/// The resulting <see cref="User"/> object will have the <see cref="User.email"/> field filled out.
		/// </returns>
		Promise<User> RegisterDBCredentials(string email, string password);

		/// <summary>
		/// A <see cref="User"/> can associate a third party credential to their account.
		/// The available third party apps can be explored by looking at the <see cref="AuthThirdParty"/> enum.
		/// Once the player registers the credential, they can request a token from the third party and then submit that
		/// token to the <see cref="LoginThirdParty"/> method to retrieve a <see cref="TokenResponse"/>.
		/// This method will associate the third party token with the <i>current</i> <see cref="User"/>.
		/// </summary>
		/// <param name="thirdParty">The <see cref="AuthThirdParty"/> that issued the <see cref="accessToken"/></param>
		/// <param name="accessToken">The token issued by the <see cref="thirdParty"/></param>
		/// <returns>
		/// A <see cref="Promise{User}"/> that will have the updated <see cref="User"/> data for the current user.
		/// The resulting <see cref="User"/> object will have the <see cref="User.thirdPartyAppAssociations"/> field updated.
		/// </returns>
		Promise<User> RegisterThirdPartyCredentials(AuthThirdParty thirdParty, string accessToken);

		/// <summary>
		/// It is possible that a player may forget the email address they registered with
		/// their account when using the <see cref="RegisterDBCredentials"/> method.
		/// In that event, you can update the <see cref="User.email"/> address using this method.
		/// <para></para>
		/// Once this method executes, the player should receive an email to the given address that contains a unique code.
		/// That code needs to be given to the <see cref="ConfirmEmailUpdate"/> function to complete the email transfer.
		/// </summary>
		/// <param name="newEmail">The new email address</param>
		/// <returns>An empty <see cref="Promise"/> representing the network request's completion. </returns>
		Promise<EmptyResponse> IssueEmailUpdate(string newEmail);

		/// <summary>
		/// It is possible that a player may forget the email address they registered with their account
		/// when using the <see cref="RegisterDBCredentials"/> method.
		/// In that even, you can update the <see cref="User.email"/> address with the <see cref="IssueEmailUpdate"/> method.
		/// A unique code will be sent to the player's new email address. That code needs to be given to this method to complete
		/// the email transfer.
		/// </summary>
		/// <param name="code">The code that was sent to the player's new email</param>
		/// <param name="password">The player's password, to confirm the email transfer</param>
		/// <returns>An empty <see cref="Promise"/> representing the network request's completion. </returns>
		Promise<EmptyResponse> ConfirmEmailUpdate(string code, string password);

		/// <summary>
		/// If the player forgets the password they registered with their email during the <see cref="RegisterDBCredentials"/> method,
		/// you can use this method to issue them a password update email.
		/// This method will send the player an email containing a unique code. That code needs to be given to the
		/// <see cref="ConfirmPasswordUpdate"/> method to complete the password update.
		/// </summary>
		/// <param name="email">The email address to send the code to. This must always be the same address registered as the User's <see cref="User.email"/></param>
		/// <returns>An empty <see cref="Promise"/> representing the network request's completion. </returns>
		Promise<EmptyResponse> IssuePasswordUpdate(string email);

		/// <summary>
		/// If the player forgets the password they registered with their email during the <see cref="RegisterDBCredentials"/> method,
		/// you can use the <see cref="IssuePasswordUpdate"/> method to send the player a one time password reset code.
		/// That code needs to be given to this method, as well as a new password, to complete the password update process.
		/// </summary>
		/// <param name="code">The code that was sent to the player's email address</param>
		/// <param name="newPassword">A new password that the player can use to sign in. This password will replace the old password.</param>
		/// <returns>An empty <see cref="Promise"/> representing the network request's completion. </returns>
		Promise<EmptyResponse> ConfirmPasswordUpdate(string code, string newPassword);

		/// <summary>
		/// This method should not be used by Developers, and will soon be marked as obsolete.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <param name="projectName"></param>
		/// <param name="customerName"></param>
		/// <param name="alias"></param>
		/// <returns></returns>
		Promise<CustomerRegistrationResponse> RegisterCustomer(string email,
															   string password,
															   string projectName,
															   string customerName,
															   string alias);

		/// <summary>
		/// If a <see cref="User"/> has associated third party credentials to their account by using the <see cref="RegisterThirdPartyCredentials"/> method,
		/// you can remove the credentials with this method.
		/// In order to remove a third party association, you need to retrieve the third party's auth token for the player one last time, so that Beamable
		/// can verify that the remove operation is secure and authorized by the actual player.
		/// </summary>
		/// <param name="thirdParty">The <see cref="AuthThirdParty"/> that issues the <see cref="token"/></param>
		/// <param name="token">The token issued by the <see cref="thirdParty"/></param>
		/// <returns>
		/// A <see cref="Promise{User}"/> that will have the updated <see cref="User"/> data for the current user.
		/// The resulting <see cref="User"/> object will have the <see cref="User.thirdPartyAppAssociations"/> field updated.
		/// </returns>
		Promise<User> RemoveThirdPartyAssociation(AuthThirdParty thirdParty, string token);

		/// <summary>
		/// Based on the logged in user, gets the current CID, PID and project name.
		/// </summary>
		Promise<CurrentProjectResponse> GetCurrentProject();

		/// <summary>
		/// Use a token issued by an external identity provider to retrieve a <see cref="TokenResponse"/>. The resulting token response

		/// can be used to change the current <see cref="User"/>.
		///
		/// This method returns a <see cref="ExternalLoginResponse"/>, which will contain a <see cref="TokenResponse"/> if the
		/// <see cref="challengeSolution"/> field is given.
		/// Otherwise, this method's <see cref="ExternalLoginResponse"/> will contain a <see cref="ExternalAuthenticationResponse"/>
		/// which has a challenge that needs to be signed.
		/// 
		/// A login will only work after the external identity has been registered by using the <see cref="AttachIdentity"/> method.
		/// </summary>
		/// <param name="externalToken">Unique token identifying player.</param>
		/// <param name="providerService">Provider (microservice) name with custom verification logic. It is required to
		/// implement Authenticate(string token, string challenge, string solution) method there</param>
		/// <param name="providerNamespace">Optional parameter to differentiate paths to a provider authenticate method
		/// in case of having more than one authenticate method in a microservice. Method in microservice should have
		/// ClientCallable attribute with pathnameOverrider set to "{providerNamespace}/authenticate"</param>
		/// <param name="challengeSolution"><see cref="ChallengeSolution"/> that contains full challenge token received
		/// from server and signed/solved solution for that challenge.</param>
		/// <param name="mergeGamerTagToAccount"></param>
		/// <returns></returns>
		Promise<ExternalLoginResponse> LoginExternalIdentity(string externalToken,
													   string providerService,
													   string providerNamespace,
													   ChallengeSolution challengeSolution = null,
													   bool mergeGamerTagToAccount = true);


		/// <summary>
		/// Method for registering external identity.
		/// </summary>
		/// <param name="externalToken">Unique token identifying player.</param>
		/// <param name="providerService">Provider (microservice) name with custom verification logic. It is required to
		/// implement Authenticate(string token, string challenge, string solution) method there</param>
		/// <param name="providerNamespace">Optional parameter to differentiate paths to a provider authenticate method
		/// in case of having more than one authenticate method in a microservice. Method in microservice should have
		/// ClientCallable attribute with pathnameOverrider set to "{providerNamespace}/authenticate"</param>
		/// <param name="challengeSolution"><see cref="ChallengeSolution"/> that contains full challenge token received
		/// from server and signed/solved solution for that challenge.</param>
		/// <returns><see cref="AttachExternalIdentityResponse"/></returns>
		Promise<AttachExternalIdentityResponse> AttachIdentity(string externalToken,
															   string providerService,
															   string providerNamespace = "",
															   ChallengeSolution challengeSolution = null);

		/// <summary>
		/// Method for unregistering previously registered external identity.
		/// </summary>
		/// <param name="providerService">Provider (microservice) name with custom verification logic. It is required to
		/// implement Authenticate(string token, string challenge, string solution) method there</param>
		/// <param name="userId">Identity we want to unregister for.</param>
		/// <param name="providerNamespace">Optional parameter to differentiate paths to a provider authenticate method
		/// in case of having more than one authenticate method in a microservice. Method in microservice should have
		/// ClientCallable attribute with pathnameOverrider set to "{providerNamespace}/authenticate"</param>
		/// <returns><see cref="DetachExternalIdentityResponse"/></returns>
		Promise<DetachExternalIdentityResponse> DetachIdentity(string providerService,
															   string userId,
															   string providerNamespace = "");

		/// <summary>
		/// Method for authorizing previously attached identity.
		/// </summary>
		/// <param name="externalToken">Unique token identifying player.</param>
		/// <param name="providerService">Provider (microservice) name with custom verification logic. It is required to
		/// implement Authenticate(string token, string challenge, string solution) method there</param>
		/// <param name="providerNamespace">Optional parameter to differentiate paths to a provider authenticate method
		/// in case of having more than one authenticate method in a microservice. Method in microservice should have
		/// ClientCallable attribute with pathnameOverrider set to "{providerNamespace}/authenticate"</param>
		/// <param name="challengeSolution"><see cref="ChallengeSolution"/> that contains full challenge token received
		/// from server and signed/solved solution for that challenge.</param>
		/// <returns><see cref="ExternalAuthenticationResponse"/></returns>
		Promise<ExternalAuthenticationResponse> AuthorizeExternalIdentity(string externalToken,
																		  string providerService,
																		  string providerNamespace = "",
																		  ChallengeSolution challengeSolution = null);

		/// <summary>
		/// Method to extract specific part of a challenge token received from a server. Challenge token is a three-part,
		/// dot-separated string and it has following structure:
		/// {challenge}.{validUntilEpoch}.{signature} where 
		/// challenge is Base64 encoded challenge to sign/solve and resend back as a part of <see cref="ChallengeSolution"/>,
		/// validUntilEpoch is Int64 value with time in miliseconds and
		/// signature - Base64 encoded token signature.
		/// </summary>
		/// <param name="token">Challenge token received from a server.</param>
		/// <returns><see cref="ChallengeToken"/> structure</returns>
		ChallengeToken ParseChallengeToken(string token);
	}
}
