using Beamable.Common.Api;
using Beamable.Common.Api.Social;

namespace Beamable.Experimental.Api.Social
{
	/// <summary>
	/// This type defines the %Client main entry point for the %Friends feature.
	/// 
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	/// 
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/friends-feature">Friends</a> feature documentation
	/// - See Beamable.API script reference
	/// 
	/// ![img beamable-logo]
	/// 
	/// </summary>
	public class SocialService : SocialApi
	{
		public SocialService(IUserContext ctx, IBeamableRequester requester) : base(ctx, requester) { }
	}
}
