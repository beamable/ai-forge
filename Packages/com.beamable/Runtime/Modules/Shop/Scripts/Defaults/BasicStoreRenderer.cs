using Beamable.Api.Payments;
using Beamable.UI.Layouts;
using Beamable.UI.Scripts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Shop.Defaults
{
	public class BasicStoreRenderer : MenuBase
	{
		public ReparenterBehaviour Layout;
		private List<ListingRenderer> allListings = new List<ListingRenderer>();
		public GameObject portrait;

		public PlayerStoreView Store;

		public override string GetTitleText()
		{
			return Store.title;
		}

		public override void OnOpened()
		{
			base.OnOpened();

			if (Store == null)
			{
				Debug.LogError("Cannot render store without data.");
				return;
			}

			var config = ShopConfiguration.Instance;
			var parent = Layout.CurrentParent;

			// Clear all existing listing views
			foreach (var listingRenderer in allListings)
			{
				Destroy(listingRenderer.gameObject);
			}

			allListings.Clear();

			// Rebuild with latest data
			foreach (var listing in Store.listings)
			{
				var listingRenderer = Instantiate(config.ListingRenderer, parent);
				listingRenderer.RenderListing(Store, listing);
				allListings.Add(listingRenderer);
			}

			var scrollRect = portrait.GetComponent<ScrollRect>();
			var normalPos = scrollRect.normalizedPosition;
			if (scrollRect.horizontal)
				scrollRect.normalizedPosition = new Vector2(0f, normalPos.y);
			else
				scrollRect.normalizedPosition = new Vector2(normalPos.x, 1f);
		}
	}
}
