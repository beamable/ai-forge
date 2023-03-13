using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0618

namespace Beamable.Common.Shop
{
	/// <summary>
	/// This type defines a %Beamable %ContentObject subclass for the %CommerceService and %Store feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/store-feature">Store</a> feature documentation
	/// - See Beamable.Api.Commerce.CommerceService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[ContentType("stores")]
	[System.Serializable]
	[Agnostic]
	public class StoreContent : ContentObject
	{
		[Tooltip(ContentObject.TooltipTitle1)]
		[CannotBeBlank]
		public string title;

		[Tooltip(ContentObject.TooltipListing1)]
		[MustReferenceContent]
		public List<ListingLink> listings;

		[Tooltip(ContentObject.TooltipShowInactive1)]
		public bool showInactiveListings;

		/// <summary>
		/// The default value is 20. If you need to show more than 20 listings at a time, change this field.
		/// </summary>
		[Tooltip(ContentObject.TooltipOptional0 + "The default value is 20. If you need to show more than 20 listings at a time, change this field. ")]
		[MustBePositive]
		public OptionalInt activeListingLimit;
	}
}
