using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

#pragma warning disable CS0618

namespace Beamable.Common.Inventory
{
	/// <summary>
	/// This type defines a %Beamable %ContentObject subclass for %Currency related to the %InventoryService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Content.ContentObject script reference
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[ContentType("currency")]
	[System.Serializable]
	[Agnostic]
	public class CurrencyContent : ContentObject
	{
		[Tooltip(TooltipIcon1)]
		[FormerlySerializedAs("Icon")]
		[ContentField("icon", FormerlySerializedAs = new[] { "Icon" })]
		public AssetReferenceSprite icon;

		/// <summary>
		/// <inheritdoc cref="ClientPermissions"/>
		/// </summary>
		[Tooltip(TooltipClientPermission1)]
		public ClientPermissions clientPermission;

		[Tooltip(TooltipAmount1)]
		[MustBeNonNegative]
		public long startingAmount;

		[ContentField("external")]
		[Tooltip(TooltipFederation)]
		[FederationMustBeValid]
		public OptionalFederation federation;
	}

	[System.Serializable]
	public class CurrencyChange
	{
		public string symbol;
		public long amount;
	}

	[System.Serializable]
	public class CurrencyReward
	{
		[MustReferenceContent]
		public CurrencyRef symbol;
		public long amount;
	}

	[System.Serializable]
	public class ListOfCurrencyChanges : DisplayableList<CurrencyReward>
	{
		public List<CurrencyReward> listData = new List<CurrencyReward>();

		protected override IList InternalList => listData;
		public override string GetListPropertyPath() => nameof(listData);
	}

	[System.Serializable]
	public class OptionalCurrencyChangeList : Optional<ListOfCurrencyChanges> { }
}
