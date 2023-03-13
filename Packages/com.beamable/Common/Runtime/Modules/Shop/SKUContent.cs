using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Content;
using System;
using UnityEngine;
using UnityEngine.Serialization;
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
	[ContentType("skus")]
	[System.Serializable]
	[Agnostic]
	public class SKUContent : ContentObject
	{
		[Tooltip(ContentObject.TooltipDescription1)]
		public string description;

		[Tooltip(ContentObject.TooltipListingPrice1)]
		[MustBePositive]
		public int realPrice;

		[Tooltip(ContentObject.TooltipSKUProductIds1)]
		public SKUProductIds productIds;
	}

	[Serializable]
	[Agnostic]
	public class SKUProductIds : ISerializationCallbackReceiver
	{
		[Tooltip(ContentObject.TooltipOptional0 + "The id for the Apple iTunes store")]
		[CannotBeBlank]
		public OptionalString itunes;

		[Tooltip(ContentObject.TooltipOptional0 + "The id for the Google Play store")]
		[CannotBeBlank]
		public OptionalString googleplay;

		[Tooltip(ContentObject.TooltipOptional0 + "The id for the Valve Steam store")]
		[CannotBeBlank]
		public OptionalInt steam;

		#region backwards compatability for pre-optional itunes and google play strings

		/* This code supports an old schema where the itunes and googleplay fields weren't OptionalString fields. Instead, they used to be string fields.
         * In order to support the old schema, we need to hi-jack Unity's serialization in a few ways..
         */

		// disable obsolete warning...
		[FormerlySerializedAs("googleplay")] // catch the old field name, and try and serialize it into this field
		[SerializeField] // tell Unity to serialize the field, even though its private
		[HideInInspector] // don't show this field in any Unity based inspector...
		[IgnoreContentField] // tell Beamable Content to not serialize this field
		[Obsolete("use the optional googleplay parameter instead.")] // tell any SDK consumer not to use this field
		private string googleplay_legacy;

		[FormerlySerializedAs("itunes")]
		[SerializeField]
		[HideInInspector]
		[IgnoreContentField]
		[Obsolete("use the optional itunes parameter instead.")]
		private string itunes_legacy;

#pragma warning disable 618
		public void OnBeforeSerialize()
		{
			// before Unity serializes the asset to yaml, we should erase any data in the legacy field.
			// By the time Unity serializes, the data should have been migrated to the new OptionalString fields
			itunes_legacy = null;
			googleplay_legacy = null;
		}

		public void OnAfterDeserialize()
		{
			// after Unity gets the asset from disk, we should take the legacy fields (if they exist), and move them into the new OptionalString fields.
			if (!string.IsNullOrEmpty(itunes_legacy))
			{
				itunes.SetValue(itunes_legacy);
				itunes_legacy = null;
			}
			if (!string.IsNullOrEmpty(googleplay_legacy))
			{
				googleplay.SetValue(googleplay_legacy);
				googleplay_legacy = null;
			}
		}
#pragma warning restore 618
		#endregion

	}
}
