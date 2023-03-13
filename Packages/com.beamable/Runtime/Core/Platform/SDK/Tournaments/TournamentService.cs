using Beamable.Api.Stats;
using Beamable.Common.Api;
using Beamable.Common.Api.Tournaments;

namespace Beamable.Api.Tournaments
{
	/// <summary>
	/// This type defines the %Client main entry point for the %Tournaments feature.
	/// 
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	/// 
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/tournaments-feature">Tournaments</a> feature documentation
	/// - See Beamable.API script reference
	/// 
	/// ![img beamable-logo]
	/// 
	/// </summary>
	public class TournamentService : TournamentApi
	{
		public TournamentService(StatsService stats, IBeamableRequester requester, IUserContext ctx) : base(stats, requester, ctx)
		{
		}
	}

}
