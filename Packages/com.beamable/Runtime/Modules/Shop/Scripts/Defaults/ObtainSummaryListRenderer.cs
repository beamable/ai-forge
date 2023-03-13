using Beamable.Api.Payments;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Shop.Defaults
{
	public class ObtainSummaryListRenderer : ObtainRenderer
	{
		public GameObject Layout;
		public ObtainSummaryListCurrencyRenderer CurrencyLine;
		public ObtainSummaryListItemRenderer ItemLine;
		private List<GameObject> _allEntries = new List<GameObject>();

		public override void RenderObtain(PlayerListingView data)
		{
			// Clear all existing listing views
			foreach (var entry in _allEntries)
			{
				Destroy(entry);
			}

			foreach (var obtain in data.offer.obtainCurrency)
			{
				var obtainRenderer = Instantiate(CurrencyLine, Layout.transform);
				_allEntries.Add(obtainRenderer.gameObject);
				obtainRenderer.RenderObtainCurrency(obtain);
			}

			foreach (var obtain in data.offer.obtainItems)
			{
				var obtainRenderer = Instantiate(ItemLine, Layout.transform);
				_allEntries.Add(obtainRenderer.gameObject);
				obtainRenderer.RenderObtainItem(obtain);
			}
		}
	}
}
