using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Common.Inventory;
using Beamable.Content;
using System;
using System.Collections;
using System.Collections.Generic;
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
	[ContentType("listings")]
	[System.Serializable]
	[Agnostic]
	public class ListingContent : ContentObject
	{
		[Tooltip(ContentObject.TooltipListingPrice1)]
		public ListingPrice price;

		[Tooltip(ContentObject.TooltipListingOffer1)]
		public ListingOffer offer;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipActivePeriod1)]
		public OptionalPeriod activePeriod;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipPurchaseLimit1)]
		[MustBePositive]
		public OptionalInt purchaseLimit;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipCohort1 + ContentObject.TooltipRequirement2)]
		public OptionalCohort cohortRequirements;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipStat1 + ContentObject.TooltipRequirement2)]
		public OptionalStats playerStatRequirements;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipActivePeriod1 + ContentObject.TooltipRequirement2)]
		public OptionalOffers offerRequirements;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipActivePeriod1)]
		public OptionalSerializableDictionaryStringToString clientData;

		[MustBePositive]
		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipDurationSeconds1 + ContentObject.TooltipActive2)]
		public OptionalInt activeDurationSeconds;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipDurationSeconds1 + ContentObject.TooltipActiveCooldown2)]
		[MustBePositive]
		public OptionalInt activeDurationCoolDownSeconds;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipDurationPurchaseLimit1)]
		[MustBePositive]
		public OptionalInt activeDurationPurchaseLimit;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.ButtonText1)]
		public OptionalString buttonText; // TODO: This is a dictionary, not a string!

		[Tooltip(ContentObject.TooltipOptional0 + "schedule for when the listing will be active")]
		public OptionalListingSchedule schedule;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipScheduleInstancePurchaseLimit)]
		[MustBePositive]
		public OptionalInt scheduleInstancePurchaseLimit;
	}

	[System.Serializable]
	public class ListingOffer
	{
		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipTitle1)]
		[CannotBeEmpty]
		public OptionalNonBlankStringList titles;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipDescription1)]
		[CannotBeEmpty]
		public OptionalNonBlankStringList descriptions;

		[Tooltip(ContentObject.TooltipObtainCurrency1)]
		public List<OfferObtainCurrency> obtainCurrency;

		[Tooltip(ContentObject.TooltipObtainItem1)]
		public List<OfferObtainItem> obtainItems;
	}

	[System.Serializable]
	public class OptionalNonBlankStringList : Optional<NonBlankStringList> { }

	[System.Serializable]
	public class NonBlankStringList : DisplayableList
	{
		[CannotBeBlank]
		public List<string> listData = new List<string>();

		protected override IList InternalList => listData;
		public override string GetListPropertyPath() => nameof(listData);
	}

	[System.Serializable]
	[Agnostic]
	public class OfferObtainCurrency : ISerializationCallbackReceiver
	{
		public CurrencyRef symbol;
		public int amount;

		#region backwards compatability

		[FormerlySerializedAs("symbol")]
		[SerializeField]
		[HideInInspector]
		[IgnoreContentField]
		[Obsolete("use the symbol parameter instead.")]
		private string symbol_legacy;

		// disable obsolete warning...
#pragma warning disable 618
		public void OnBeforeSerialize()
		{
			symbol_legacy = null;
		}

		public void OnAfterDeserialize()
		{
			if (!string.IsNullOrEmpty(symbol_legacy))
			{
				symbol = new CurrencyRef(symbol_legacy);
				symbol_legacy = null;
			}
		}
#pragma warning restore 618
		#endregion
	}

	[System.Serializable]
	public class OfferObtainItem : ISerializationCallbackReceiver
	{
		public ItemRef contentId;
		public List<OfferObtainItemProperty> properties;

		#region backwards compatability

		[FormerlySerializedAs("contentId")]
		[SerializeField]
		[HideInInspector]
		[IgnoreContentField]
		[Obsolete("use the content parameter instead.")]
		private string content_legacy;

		// disable obsolete warning...
#pragma warning disable 618
		public void OnBeforeSerialize()
		{
			content_legacy = null;
		}

		public void OnAfterDeserialize()
		{
			if (!string.IsNullOrEmpty(content_legacy))
			{
				contentId = new ItemRef(content_legacy);
				content_legacy = null;
			}
		}
#pragma warning restore 618
		#endregion
	}

	[System.Serializable]
	public class OfferObtainItemProperty
	{
		[Tooltip(ContentObject.TooltipName1)]
		[CannotBeBlank]
		public string name;

		[Tooltip(ContentObject.TooltipValue1)]
		public string value;
	}

	[System.Serializable]
	public class ListingPrice : ISerializationCallbackReceiver
	{
		[Tooltip(ContentObject.TooltipType1)]
		[MustBeOneOf("skus", "currency")]
		public string type;

		[Tooltip(ContentObject.TooltipSymbol1)]
		[MustReferenceContent(false, typeof(CurrencyContent), typeof(SKUContent))]
		public string symbol;

		[Tooltip(ContentObject.TooltipAmount1)]
		[MustBeNonNegative]
		public int amount;

		public void OnBeforeSerialize()
		{
			if (type == "sku")
			{
				type = "skus";
			}
		}

		public void OnAfterDeserialize()
		{
			if (type == "sku")
			{
				type = "skus";
			}
		}
	}

	[System.Serializable]
	public class ActivePeriod
	{
		[Tooltip(ContentObject.TooltipStartDate1 + ContentObject.TooltipStartDate2)]
		[MustBeDateString]
		public string start;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipEndDate1 + ContentObject.TooltipEndDate2)]
		[MustBeDateString]
		public OptionalString end;
	}

	[System.Serializable]
	public class OfferRequirement
	{
		[Tooltip(ContentObject.TooltipSymbol1)]
		[MustReferenceContent(AllowedTypes = new[] { typeof(ListingContent) })]
		public string offerSymbol;

		[Tooltip(ContentObject.TooltipPurchase1)]
		public OfferConstraint purchases;
	}

	[System.Serializable]
	public class StatRequirement
	{
		// TODO: StatRequirement, by way of OptionalStats, is used by AnnouncementContent too. Should this be in a shared location? ~ACM 2021-04-22

		[Tooltip("Domain of the stat (e.g. 'platform', 'game', 'client'). Default is 'game'.")]
		[MustBeOneOf("platform", "game", "client")]
		public OptionalString domain;

		[Tooltip("Visibility of the stat (e.g. 'private', 'public'). Default is 'private'.")]
		[MustBeOneOf("private", "public")]
		public OptionalString access;

		[Tooltip(ContentObject.TooltipStat1)]
		[CannotBeBlank]
		public string stat;

		[Tooltip(ContentObject.TooltipConstraint1)]
		[MustBeComparatorString]
		public string constraint;

		[Tooltip(ContentObject.TooltipValue1)] public int value;
	}

	[System.Serializable]
	public class CohortRequirement
	{
		[Tooltip(ContentObject.TooltipCohortTrial1)]
		[CannotBeBlank]
		public string trial;

		[Tooltip(ContentObject.TooltipCohort1)]
		[CannotBeBlank]
		public string cohort;

		[Tooltip(ContentObject.TooltipConstraint1)]
		[MustBeComparatorString]
		public string constraint;
	}

	[System.Serializable]
	public class ContentDictionary
	{
		public List<KVPair> keyValues;
	}

	[System.Serializable]
	public class OfferConstraint
	{
		[Tooltip(ContentObject.TooltipConstraint1)]
		[MustBeComparatorString]
		public string constraint;

		[Tooltip(ContentObject.TooltipValue1)]
		public int value;
	}

	[System.Serializable]
	public class OptionalColor : Optional<Color>
	{
		public static OptionalColor From(Color color)
		{
			return new OptionalColor { HasValue = true, Value = color };
		}
	}

	[System.Serializable]
	public class OptionalPeriod : Optional<ActivePeriod> { }

	[System.Serializable]
	public class OptionalStats : Optional<List<StatRequirement>> { }

	[System.Serializable]
	public class OptionalOffers : Optional<List<OfferRequirement>> { }

	[System.Serializable]
	public class OptionalDict : Optional<ContentDictionary> { }

	[System.Serializable]
	public class OptionalCohort : Optional<List<CohortRequirement>> { }
}
