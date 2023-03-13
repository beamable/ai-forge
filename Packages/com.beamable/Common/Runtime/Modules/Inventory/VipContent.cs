using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0618

namespace Beamable.Common.Inventory
{
	/// <summary>
	/// This type defines a methodology for resolving a reference to a %Beamable %ContentObject.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code#contentlink-vs-contentref">ContentLink vs ContentRef</a> documentation
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[System.Serializable]
	[Agnostic]
	public class VipLink : ContentLink<VipContent> { }

	/// <summary>
	/// This type defines a methodology for resolving a reference to a %Beamable %ContentObject.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code#contentlink-vs-contentref">ContentLink vs ContentRef</a> documentation
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[System.Serializable]
	[Agnostic]
	public class VipRef : ContentRef<VipContent> { }

	/// <summary>
	/// This type defines a %Beamable %ContentObject subclass for a %Vip.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[ContentType("vip")]
	[System.Serializable]
	[Agnostic]
	public class VipContent : ContentObject
	{
		[Tooltip(ContentObject.TooltipCurrency1)]
		[MustReferenceContent]
		public CurrencyRef currency;

		[Tooltip(ContentObject.TooltipVipTier1)]
		[CannotBeEmpty]
		public List<VipTier> tiers;
	}

	[System.Serializable]
	public class VipTier
	{
		[Tooltip(ContentObject.TooltipName1)]
		[CannotBeBlank]
		public string name;

		[Tooltip(ContentObject.TooltipQualifyThreshold1)]
		[MustBeNonNegative]
		public long qualifyThreshold;

		[Tooltip(ContentObject.TooltipDisqualifyThreshold1)]
		[MustBeNonNegative]
		public long disqualifyThreshold;

		[Tooltip(ContentObject.TooltipVipBonus1)]
		[CannotBeBlank]
		public List<VipBonus> multipliers;
	}

	[System.Serializable]
	public class VipBonus
	{
		[Tooltip(ContentObject.TooltipCurrency1)]
		[MustBeCurrency]
		public string currency;

		[Tooltip(ContentObject.TooltipMultiplier1)]
		[MustBePositive]
		public double multiplier;

		[Tooltip(ContentObject.TooltipRoundToNearest1)]
		[MustBePositive]
		public int roundToNearest;
	}
}
