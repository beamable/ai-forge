using Beamable.Api;
using Beamable.Api.Payments;
using Beamable.Common.Content;
using Beamable.Common.Inventory;
using Beamable.Signals;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Beamable.Shop.Defaults
{
	public class BasicListingRenderer : ListingRenderer
	{
		public TextMeshProUGUI Title;
		public TextMeshProUGUI Description;
		public TextMeshProUGUI ButtonText;
		public GameObject ObtainLayout;

		private PlayerListingView _listing;
		private PlayerStoreView _store;

		public override async void RenderListing(PlayerStoreView store, PlayerListingView listing)
		{
			var config = ShopConfiguration.Instance;

			// Basic info
			_listing = listing;
			_store = store;
			if (listing.offer.titles.Count > 0)
			{
				Title.text = listing.offer.titles[0];
			}

			if (listing.offer.descriptions.Count > 0)
			{
				Description.text = listing.offer.descriptions[0];
			}

			// Obtain
			var obtainRenderer = Instantiate(config.ObtainRenderer, ObtainLayout.transform);
			obtainRenderer.RenderObtain(listing);

			// RMT Price
			var beamable = await API.Instance;
			if (listing.offer.price.type == "sku")
			{
				var purchaser = await beamable.BeamableIAP;
				ButtonText.text = purchaser.GetLocalizedPrice(listing.offer.price.symbol);
			}
			else
			{
				var contentRef = new ContentRef<CurrencyContent>
				{
					Id = listing.offer.price.symbol
				};
				var currency = await beamable.ContentService.GetContent(contentRef);
				ButtonText.text = $"{listing.offer.price.amount} {currency.name}";
			}
		}

		/// <summary>
		/// Initiate the purchase of a store listing by way of either a SKU (real
		/// money) or in-game currency.
		/// </summary>
		public async void Buy()
		{
			switch (_listing.offer.price.type)
			{
				case "sku":
					var beamable = await API.Instance;
					var purchaser = await beamable.BeamableIAP;
					await purchaser.StartPurchase($"{_listing.symbol}:{_store.symbol}", _listing.offer.price.symbol);
					break;
				case "currency":
					var result = await PurchaseWithCurrency(_store.symbol, _listing.symbol);
					if (result == CurrencyPurchaseResult.Failure)
						return;
					break;
				default:
					throw new UnknownPriceTypeException(_listing.offer.price.type);
			}
			DeSignalTower.ForAll<ShopSignals>(s => s.OnPurchase?.Invoke(_listing));
		}

		/// <summary>
		/// Attempt to make a store purchase using in-game currency.
		/// </summary>
		/// <param name="storeSymbol">The store from which to buy the item.</param>
		/// <param name="listingSymbol">The listing being bought.</param>
		/// <returns>Success or failure.</returns>
		private static async Task<CurrencyPurchaseResult> PurchaseWithCurrency(string storeSymbol, string listingSymbol)
		{
			try
			{
				var beamable = await API.Instance;
				await beamable.CommerceService.Purchase(storeSymbol, listingSymbol);
				return CurrencyPurchaseResult.Success;
			}
			catch (PlatformRequesterException e) when (e.Error.status == 400 && e.Error.error == "InsufficientCurrency")
			{
				// TODO: Pop up a failed purchase dialog. See BEAM-1028
				Debug.Log($"Cannot purchase item: not enough '{e.Error.message}' currency!");
				return CurrencyPurchaseResult.Failure;
			}
		}

		private enum CurrencyPurchaseResult
		{
			Failure = 0,
			Success
		}
	}

	public class UnknownPriceTypeException : Exception
	{
		public string PriceType { get; }

		public UnknownPriceTypeException(string priceType) : base($"Unknown price type: {priceType}")
		{
			PriceType = priceType;
		}
	}
}
