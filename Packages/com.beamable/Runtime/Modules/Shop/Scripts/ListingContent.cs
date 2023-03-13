using Beamable.Common;
using Beamable.Common.Inventory;
using Beamable.Common.Shop;
using Beamable.UI.Scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Shop
{
	public static class OfferObtainCurrencyExtensions
	{
		public static Promise<Dictionary<string, Sprite>> ResolveAllIcons(this List<OfferObtainCurrency> self)
		{
			List<Promise<CurrencyContent>> toContentPromises = self
			   .Select(x => x.symbol)
			   .Distinct()
			   .Select(x => x.Resolve())
			   .ToList();

			var z = Promise.Sequence(toContentPromises)
			   .Map(contentSet => contentSet.ToDictionary(
				  content => content.Id,
				  content => content.icon.LoadSprite())
			   ).FlatMap(dict =>
				  Promise
					 .Sequence(dict.Values.ToList())
					 .Map(_ => dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetResult()))
			   );

			return z;
		}
	}


}
