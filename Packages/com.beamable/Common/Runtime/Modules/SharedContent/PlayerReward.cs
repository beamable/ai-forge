using Beamable.Common.Inventory;
using System;
using UnityEngine;

namespace Beamable.Common.Content
{
	/// <summary>
	/// A <see cref="PlayerReward"/> describes a set of rewards that a player can claim.
	/// </summary>
	[Serializable]
	public class PlayerReward
	{
		/// <summary>
		/// An optional textual description of the reward. Use this to quickly summarize for development what the reward grants.
		/// </summary>
		[ContentField("description")]
		[Tooltip("An optional textual description of the reward. Use this to quickly summarize for development what the reward grants.")]
		public OptionalString description;

		/// <summary>
		/// Optionally, each reward can grant a set of currencies.
		/// </summary>
		[ContentField("changeCurrencies")]
		[Tooltip("Optionally, each reward can grant a set of currencies. ")]
		public OptionalCurrencyChangeList currencies;

		/// <summary>
		/// Optionally, each reward can grant a set of items with properties.
		/// </summary>
		[ContentField("addItems")]
		[Tooltip("Optionally, each reward can grant a set of items with properties. ")]
		public OptionalNewItemList items;

		/// <summary>
		/// Optionally, when a reward is claimed, the vip bonus can be applied to the currencies.
		/// </summary>
		[ContentField("applyVipBonus")]
		[Tooltip("Optionally, when a reward is claimed, the vip bonus can be applied to the currencies")]
		public OptionalBoolean applyVipBonus;

		/// <summary>
		/// Check if there are any currencies or items in this <see cref="PlayerReward"/>
		/// </summary>
		/// <returns>true if there are any rewards, false otherwise.</returns>
		public virtual bool HasAnyReward()
		{
			var anyCurrencies = currencies.GetOrElse(() => null)?.Count > 0;
			var anyItems = items.GetOrElse(() => null)?.Count > 0;
			return anyCurrencies || anyItems;
		}
	}

	[Serializable]
	public class PlayerReward<TOptionalApiRewards> : PlayerReward
	{
		[ContentField("callWebhooks")]
		[HideUnlessServerPackageInstalled]
		public TOptionalApiRewards webhooks;
	}

}
