using Beamable.Api.Auth;
using Beamable.Common;
using Beamable.Common.Api.Auth;

namespace Beamable.AccountManagement
{
	public static class AccountManagementHelper
	{
		public static Promise<bool> IsEmailRegistered(this IBeamableAPI beamableAPI, string email)
		{
			return beamableAPI.AuthService.IsEmailAvailable(email).Map(available => !available);
		}

		public static Promise<User> AttachThirdPartyToCurrentUser(this IBeamableAPI beamableAPI,
																  AuthThirdParty thirdParty,
																  string accessToken)
		{
			return beamableAPI.AuthService.RegisterThirdPartyCredentials(thirdParty, accessToken)
					 .Then(beamableAPI.UpdateUserData);
		}

		public static Promise<User> RemoveThirdPartyFromCurrentUser(this IBeamableAPI beamableAPI,
																	AuthThirdParty thirdParty,
																	string accessToken)
		{
			return beamableAPI.AuthService.RemoveThirdPartyAssociation(thirdParty, accessToken)
					 .Then(beamableAPI.UpdateUserData);
		}

		public static Promise<User> AttachEmailToCurrentUser(this IBeamableAPI beamableAPI, string email, string password)
		{
			return beamableAPI.AuthService.RegisterDBCredentials(email, password).Then(beamableAPI.UpdateUserData);
		}

		public static Promise<Unit> LoginToNewUser(this IBeamableAPI beamableAPI)
		{
			return beamableAPI.AuthService.CreateUser().FlatMap(beamableAPI.ApplyToken);
		}
	}
}
