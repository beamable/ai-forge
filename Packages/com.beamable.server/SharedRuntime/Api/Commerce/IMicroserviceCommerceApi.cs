using Beamable.Common;
using Beamable.Common.Api.Commerce;
using System.Collections.Generic;

namespace Beamable.Server.Api.Commerce
{
	/// <summary>
	/// This type defines the %Microservice main entry point for the %Commerce feature.
	/// 
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	/// 
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/reference/commerce-overview">Commerce</a> feature documentation
	/// - See Beamable.Server.IBeamableServices script reference
	/// 
	/// ![img beamable-logo]
	/// 
	/// </summary>
	public interface IMicroserviceCommerceApi : ICommerceApi
	{
		Promise<Unit> AccelerateListingCooldown(long gamerTag, List<CooldownReductionRequest> cooldownReductions);
	}

	[System.Serializable]
	public class UpdateListingCooldownRequest
	{
		public long gamerTag;
		public List<CooldownReductionRequest> updateListingCooldownRequests;

		public UpdateListingCooldownRequest(long gamerTag, List<CooldownReductionRequest> updateListingCooldownRequests)
		{
			this.gamerTag = gamerTag;
			this.updateListingCooldownRequests = updateListingCooldownRequests;
		}
	}

	[System.Serializable]

	public class CooldownReductionRequest
	{
		public string symbol;
		public int cooldownReduction;

		public CooldownReductionRequest(string symbol, int cooldownReduction)
		{
			this.symbol = symbol;
			this.cooldownReduction = cooldownReduction;
		}
	}

}
