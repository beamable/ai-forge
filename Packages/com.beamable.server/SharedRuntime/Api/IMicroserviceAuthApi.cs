using Beamable.Common;
using Beamable.Common.Api.Auth;

namespace Beamable.Server.Api
{
	/// <summary>
	/// This type defines the %Microservice main entry point for the %Auth feature.
	/// 
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	/// 
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/identity">Identity</a> feature documentation
	/// - See Beamable.Server.IBeamableServices script reference
	/// 
	/// ![img beamable-logo]
	/// 
	/// </summary>
	public interface IMicroserviceAuthApi : IAuthApi
	{
		/// <summary>
		/// From a user's game-specific GamerTag, gets that user's account data. If you want the cross-game user account, see <see cref="AccountId"/> and <see cref="GetAccountId"/>.
		/// </summary>
		Promise<User> GetUser(long gamerTag);

		/// <summary>
		/// Get the assumed user's (see <see cref="Microservice.AssumeUser"/>) cross-game account id.
		/// This is different from <see cref="RequestContext.UserId"/>, which always resolve to the user's game-specific GamerTag.
		/// </summary>
		Promise<AccountId> GetAccountId();
	}

	/// <summary>
	/// This ID is different from <see cref="User.id"/>. In beamable, there are 2 ids:
	/// <list type="bullet">
	/// <item>A game-specific ID, which we call a gamertag and can be found in <see cref="User.id"/>.</item>
	/// <item>A customer-specific ID, which a single user shares across all your games/realms. This is the <see cref="AccountId"/>.</item>
	/// </list> 
	/// </summary>
	public struct AccountId
	{
		public long Id;
	}
}
