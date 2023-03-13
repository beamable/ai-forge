using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Inventory;
using Beamable.Common.Api.Payments;
using System;
using System.Collections.Generic;
using UnityEngine;
using CometClientData = Beamable.Platform.SDK.CometClientData;

namespace Beamable.Api.Payments
{
	// Payments service.
	// This service provides an API to communicate with the Platform.


	/// <summary>
	/// This type defines the %Client main entry point for the %Payments feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/payments-feature">Payments</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class PaymentService : PaymentsApi
	{
		private IPlatformService _platform;

		public PaymentService(IPlatformService platform, IPlatformRequester requester) : base(requester)
		{
			_platform = platform;
			_requester = requester;
			platform.Notification.Subscribe("commerce.coupons_updated", payload =>
			{
				RefreshCoupons();
			});
		}

		private static string ProviderId
		{
			get
			{
				if (Application.platform == RuntimePlatform.IPhonePlayer) { return "itunes"; }
				else if (Application.platform == RuntimePlatform.Android) { return "googleplay"; }
				else
				{
#if UNITY_EDITOR
                    return "test";
#elif USE_STEAMWORKS
                    return "steam";
#else
					return "bogus";
#endif
				}
			}
		}

		/// <summary>
		/// Handles starting the coroutine to track a purchase request client-authoritatively
		/// Note: This is disabled by default, and requires the developer to enable it via the Portal.
		/// </summary>
		/// <param name="trackPurchaseRequest">The request structure for tracking the purchase.</param>
		public Promise<EmptyResponse> Track(TrackPurchaseRequest trackPurchaseRequest)
		{
			return _requester.Request<EmptyResponse>(
			   Method.POST,
			   $"/basic/payments/{ProviderId}/purchase/track",
			   trackPurchaseRequest
			);
		}

		/// <summary>
		/// Handles starting the coroutine to make the begin purchase request.
		/// </summary>
		/// <param name="purchaseId">The id of the item the player wishes to purchase.</param>
		public Promise<PurchaseResponse> BeginPurchase(string purchaseId)
		{
			return _requester.Request<PurchaseResponse>(
			   Method.POST,
			   $"/basic/payments/{ProviderId}/purchase/begin",
			   new BeginPurchaseRequest(purchaseId)
			);
		}

		/// <summary>
		/// This request is made after the provider has charged the player. It is used to verify the legitimacy of the
		/// purchase by verifying the receipt as well as completing the item fulfillment process.
		/// </summary>
		/// <param name="transaction">The completed transaction.</param>
		public Promise<EmptyResponse> CompletePurchase(CompletedTransaction transaction)
		{
			return _requester.Request<EmptyResponse>(
			   Method.POST,
			   $"/basic/payments/{ProviderId}/purchase/complete",
			   new CompleteTransactionRequest(transaction)
			);
		}

		/// <summary>
		/// Handles starting the coroutine to make the cancel purchase request.
		/// </summary>
		/// <param name="txid">The id of the transaction the player wishes to cancel.</param>
		public Promise<EmptyResponse> CancelPurchase(long txid)
		{
			return _requester.Request<EmptyResponse>(
			   Method.POST,
			   $"/basic/payments/{ProviderId}/purchase/cancel",
			   new CancelPurchaseRequest(txid)
			);
		}

		/// <summary>
		/// Handles starting the coroutine to make the fail purchase request.
		/// </summary>
		/// <param name="txid">The id of the transaction the player wishes to mark failed.</param>
		/// <param name="reason">The reason the transaction failed
		public Promise<EmptyResponse> FailPurchase(long txid, string reason)
		{
			return _requester.Request<EmptyResponse>(
			   Method.POST,
			   $"/basic/payments/{ProviderId}/purchase/fail",
			   new FailPurchaseRequest(txid, reason)
			);
		}

		/// <summary>
		/// Attempts to consume a coupon to redeem an offer
		/// </summary>
		/// <param name="purchaseId">The id of the item the player wishes to purchase.</param>
		public Promise<PurchaseResponse> RedeemCoupon(string purchaseId)
		{
			return _requester.Request<PurchaseResponse>(
			   Method.POST,
			   $"/basic/payments/coupon/purchase/begin",
			   new BeginPurchaseRequest(purchaseId)
			).FlatMap(beginRsp => _requester.Request<EmptyResponse>(
				  Method.POST,
				  $"/basic/payments/coupon/purchase/complete",
				  new CompleteTransactionRequest(beginRsp.txid)
			   ).Map(completeRsp => beginRsp)
			);
		}

		/// <summary>
		/// Get commerce SKUs from platform
		/// </summary>
		public Promise<GetSKUsResponse> GetSKUs()
		{
			return _requester.Request<GetSKUsResponse>(
			   Method.GET,
			   "/basic/commerce/skus"
			);
		}

		/// <summary>
		/// Get catalog from platform
		/// </summary>
		public Promise<GetOffersResponse> GetOffers(string[] stores = null)
		{
			string querystring = string.Empty;
			if (stores != null)
			{
				querystring = $"?stores={string.Join(",", stores)}";
			}

			return _requester.Request<GetOffersResponse>(
			   Method.GET,
			   $"/object/commerce/{_platform.User.id}/offers{querystring}"
			).Then(r => r.Init());
		}

		#region Coupons

		private CouponsCountResponse _couponsCount;

		/// <summary>
		/// Get the count of coupons
		/// </summary>
		public Promise<CouponsCountResponse> GetCouponsCount()
		{
			return _requester.Request<CouponsCountResponse>(
			   Method.GET,
			   $"/object/commerce/{_platform.User.id}/coupons/count"
			);
		}

		public void RefreshCoupons()
		{
			GetCouponsCount().Then(newCount =>
			{
				_couponsCount = newCount;
				CouponsCountUpdated?.Invoke(_couponsCount.count);
			});
		}

		private event Action<int> CouponsCountUpdated;

		public void SubscribeCoupons(Action<int> callback)
		{
			CouponsCountUpdated += callback;

			if (_couponsCount != null)
			{
				callback(_couponsCount.count);
			}
			else
			{
				RefreshCoupons();
			}
		}

		#endregion
	}

	[Serializable]
	public class TrackPurchaseRequest
	{
		public string purchaseId;
		public string skuName;
		public string skuProductId;
		public string store;
		public double priceInLocalCurrency;
		public string isoCurrencySymbol;
		public List<ObtainCurrency> obtainCurrency;
		public List<ObtainItem> obtainItems;

		public TrackPurchaseRequest(string purchaseId, string skuName, string skuProductId, string store, double priceInLocalCurrency, string isoCurrencySymbol, List<ObtainCurrency> obtainCurrency = null, List<ObtainItem> obtainItems = null)
		{
			this.purchaseId = purchaseId;
			this.skuName = skuName;
			this.skuProductId = skuProductId;
			this.store = store;
			this.priceInLocalCurrency = priceInLocalCurrency;
			this.isoCurrencySymbol = isoCurrencySymbol;
			this.obtainCurrency = obtainCurrency ?? new List<ObtainCurrency>();
			this.obtainItems = obtainItems ?? new List<ObtainItem>();
		}
	}

	[Serializable]
	public class BeginPurchaseRequest
	{
		public string purchaseId;
		public BeginPurchaseRequest(string purchaseId)
		{
			this.purchaseId = purchaseId;
		}
	}

	[Serializable]
	public class CancelPurchaseRequest
	{
		public long txid;
		public CancelPurchaseRequest(long txid)
		{
			this.txid = txid;
		}
	}

	[Serializable]
	public class FailPurchaseRequest
	{
		public long txid;
		public string reason;
		public FailPurchaseRequest(long txid, string reason)
		{
			this.txid = txid;
			this.reason = reason;
		}
	}

	[Serializable]
	public class CompleteTransactionRequest
	{
		public long txid;
		public string receipt;
		public string priceInLocalCurrency;
		public string isoCurrencySymbol;
		public CompleteTransactionRequest(CompletedTransaction txn)
		{
			txid = txn.Txid;
			receipt = txn.Receipt;
			priceInLocalCurrency = txn.PriceInLocalCurrency;
			isoCurrencySymbol = txn.IsoCurrencySymbol;
		}

		public CompleteTransactionRequest(long txid)
		{
			this.txid = txid;
		}
	}

	[Serializable]
	public class CouponsCountResponse
	{
		public int count;
	}

	[Serializable]
	public class PurchaseResponse
	{
		public long txid;
	}

	[Serializable]
	public class GetSKUsResponse
	{
		public SKUDefinitions skus;
	}

	[Serializable]
	public class SKUDefinitions
	{
		public long version;
		public string created;
		public List<SKU> definitions;
	}

	[Serializable]
	public class SKU
	{
		public string name;
		public string description;
		public int realPrice;
		public SKUProductIDs productIds;
	}

	[Serializable]
	public class SKUProductIDs
	{
		public string itunes;
		public string googleplay;
		public string facebook;
		public string steam;
	}

	[Serializable]
	public class GetOffersResponse
	{
		public List<PlayerStoreView> stores;

		internal void Init()
		{
			foreach (var s in stores)
			{
				s.Init();
			}
		}
	}

	[Serializable]
	public class PlayerStoreView
	{
		public string symbol;
		public string title;
		public List<PlayerListingView> listings;
		public long secondsRemain;
		public long nextDeltaSeconds;
		public DateTime refreshTime;

		internal void Init()
		{
			if (secondsRemain != 0)
			{
				refreshTime = DateTime.UtcNow.AddSeconds(secondsRemain);
			}

			foreach (var listing in listings)
			{
				listing.Init();
			}
		}
	}

	[Serializable]
	public class PlayerListingView : CometClientData
	{
		public string symbol;
		public PlayerOfferView offer;
		public long secondsRemain;
		public DateTime endTime;
		public bool active;
		public long cooldown;
		public int purchasesRemain;
		public bool queryAfterPurchase;

		internal void Init()
		{
			endTime = DateTime.UtcNow.AddSeconds(secondsRemain);
		}

		public bool IsCoolingDown => !active && secondsRemain > 0; // RESOLVE: Not strictly accurate. Update when we have backend support.
	}

	[Serializable]
	public class PlayerOfferView
	{
		public string symbol;
		public List<string> titles;
		public List<string> descriptions;
		public List<OfferImages> images;
		public List<Obtain> obtain;
		public List<ObtainCurrency> obtainCurrency;
		public List<ObtainItem> obtainItems;
		public Price price;
		public int coupons;
	}

	[Serializable]
	public class Price
	{
		public string type;
		public string symbol;
		public int amount;
		public int step;

		public bool IsFree => amount == 0;
	}

	[Serializable]
	public class OfferImages
	{
		public ImageRef small;
		public ImageRef medium;
		public ImageRef large;
	}

	[Serializable]
	public class ImageRef
	{
		public string file;
		public string atlas_info;
	}

	[Serializable]
	public class Obtain
	{
		public string ent;
		public string spec;
		public int count;
	}

	[Serializable]
	public class ObtainCurrency
	{
		public string symbol;
		public long amount;
		public long originalAmount;

		public bool HasBonus => originalAmount > 0;
		public long Delta => amount - originalAmount;
	}

	[Serializable]
	public class ObtainItem
	{
		public string contentId;
		public List<ItemProperty> properties;
	}
}
