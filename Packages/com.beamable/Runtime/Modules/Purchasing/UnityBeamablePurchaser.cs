using Beamable.Api;
using Beamable.Api.Payments;
using Beamable.Common;
using Beamable.Common.Dependencies;
using Beamable.Common.Spew;
using Beamable.Coroutines;
using Beamable.Service;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Beamable.Purchasing
{
	/// <summary>
	/// Implementation of Unity IAP for Beamable purchasing.
	/// </summary>
	public class UnityBeamablePurchaser : IStoreListener, IBeamablePurchaser
	{
		private IStoreController _storeController;
#pragma warning disable CS0649
		private IAppleExtensions _appleExtensions;
		private IGooglePlayStoreExtensions _googleExtensions;
#pragma warning restore CS0649

		private readonly Promise<Unit> _initPromise = new Promise<Unit>();
		private long _txid;
		private Action<CompletedTransaction> _success;
		private Action<ErrorCode> _fail;
		private Action _cancelled;
		private IDependencyProvider _serviceProvider;

		static readonly int[] RETRY_DELAYS = { 1, 2, 5, 10, 20 }; // TODO: Just double a few times. ~ACM 2021-03-10


		public Promise<Unit> Initialize(IDependencyProvider provider)
		{
			_serviceProvider = provider;
			var paymentService = GetPaymentService();

			var skuPromise = paymentService.GetSKUs(); // XXX: This is failing, but nothing is listening for it.
			return skuPromise.FlatMap(rsp =>
			{
				var noSkusAvailable = rsp.skus.definitions.Count == 0;
				if (noSkusAvailable)
				{
					// If there are no SKUs available, we will short-circuit the rest of the init-flow.
					// Most importantly, we don't call `UnityPurchasing.Initialize`, so that we don't receive the Purchase Finished callbacks.
					_initPromise.CompleteSuccess(PromiseBase.Unit);
					return _initPromise;
				}

#if USE_STEAMWORKS && !UNITY_EDITOR
                var builder = ConfigurationBuilder.Instance(new Steam.SteamPurchasingModule(_serviceProvider));
                foreach (var sku in rsp.skus.definitions)
                {
                    builder.AddProduct(sku.name, ProductType.Consumable, new IDs
                    {
                        { sku.productIds.steam, Steam.SteamStore.Name }
                    });
                }
#else
				var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
				foreach (var sku in rsp.skus.definitions)
				{
					builder.AddProduct(sku.name, ProductType.Consumable, new IDs
					{
						{ sku.productIds.itunes, AppleAppStore.Name },
						{ sku.productIds.googleplay, GooglePlay.Name },
					});
				}
#endif

				// Kick off the remainder of the set-up with an asynchrounous call,
				// passing the configuration and this class's instance. Expect a
				// response either in OnInitialized or OnInitializeFailed.
				UnityPurchasing.Initialize(this, builder);
				return _initPromise;
			});

		}

		/// <summary>
		/// Clear all the callbacks and zero out the transaction ID.
		/// </summary>
		private void ClearCallbacks()
		{
			_success = null;
			_fail = null;
			_cancelled = null;
			_txid = 0;
		}

		private PaymentService GetPaymentService()
		{
			return _serviceProvider.GetService<PaymentService>();
		}

		private CoroutineService GetCoroutineService()
		{
			return _serviceProvider.GetService<CoroutineService>();
		}

		#region "IBeamablePurchaser"
		/// <summary>
		/// Get the localized price string for a given SKU.
		/// </summary>
		public string GetLocalizedPrice(string skuSymbol)
		{
			var product = _storeController?.products.WithID(skuSymbol);
			return product?.metadata.localizedPriceString ?? "???";
		}

		/// <summary>
		/// Start a purchase for the given listing using the given SKU.
		/// </summary>
		/// <param name="listingSymbol">Store listing that should be purchased.</param>
		/// <param name="skuSymbol">Platform specific SKU of the item being purchased.</param>
		/// <returns>Promise containing completed transaction.</returns>
		public Promise<CompletedTransaction> StartPurchase(string listingSymbol, string skuSymbol)
		{
			var result = new Promise<CompletedTransaction>();
			_txid = 0;
			_success = result.CompleteSuccess;
			_fail = result.CompleteError;
			if (_cancelled == null) _cancelled = () =>
			{
				result.CompleteError(
				 new ErrorCode(400, GameSystem.GAME_CLIENT, "Purchase Cancelled"));
			};

			GetPaymentService().BeginPurchase(listingSymbol).Then(rsp =>
			{
				_txid = rsp.txid;
				_storeController.InitiatePurchase(skuSymbol, _txid.ToString());
			}).Error(err =>
			{
				Debug.LogError($"There was an exception making the begin purchase request: {err}");
				_fail?.Invoke(err as ErrorCode);
			});

			return result;
		}
		#endregion

		/// <summary>
		/// Initiate transaction restoration if needed.
		/// </summary>
		public void RestorePurchases()
		{
			if (Application.platform == RuntimePlatform.IPhonePlayer ||
				Application.platform == RuntimePlatform.OSXPlayer)
			{
				InAppPurchaseLogger.Log("RestorePurchases started ...");

				// Begin the asynchronous process of restoring purchases. Expect a confirmation response in
				// the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
				_appleExtensions.RestoreTransactions(result =>
				{
					// The first phase of restoration. If no more responses are received on ProcessPurchase then
					// no purchases are available to be restored.
					InAppPurchaseLogger.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
				});
			}
			else
			{
				// If we are not running on an Apple device, no work is necessary to restore purchases.
				InAppPurchaseLogger.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
			}
		}

		#region "IStoreListener"
		/// <summary>
		/// React to successful Unity IAP initialization.
		/// </summary>
		/// <param name="controller"></param>
		/// <param name="extensions"></param>
		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			InAppPurchaseLogger.Log("Successfully initialized IAP.");
			_storeController = controller;
#if !USE_STEAMWORKS || UNITY_EDITOR
			_appleExtensions = extensions.GetExtension<IAppleExtensions>();
			_googleExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();
#endif
			RestorePurchases();
			_initPromise.CompleteSuccess(PromiseBase.Unit);
		}

		/// <summary>
		/// Handle failed initialization by logging the error.
		/// </summary>
		public void OnInitializeFailed(InitializationFailureReason error)
		{
			Debug.LogError("Billing failed to initialize!");
			switch (error)
			{
				case InitializationFailureReason.AppNotKnown:
					InAppPurchaseLogger.Log("Is your App correctly uploaded on the relevant publisher console?");
					break;
				case InitializationFailureReason.PurchasingUnavailable:
					InAppPurchaseLogger.Log("Billing disabled!");
					break;
				case InitializationFailureReason.NoProductsAvailable:
					InAppPurchaseLogger.Log("No products available for purchase!");
					break;
				default:
					InAppPurchaseLogger.Log("Unknown billing error: '{error}'");
					break;
			}
			_initPromise.CompleteError(new BeamableIAPInitializationException(error));

		}

		/// <summary>
		/// Process a completed purchase by fulfilling it.
		/// </summary>
		/// <param name="args">Purchase event information from Unity IAP</param>
		/// <returns>Successful or failed result of processing this purchase</returns>
		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
		{
			string rawReceipt;
			if (args.purchasedProduct.hasReceipt)
			{
				var receipt = JsonUtility.FromJson<UnityPurchaseReceipt>(args.purchasedProduct.receipt);
				rawReceipt = receipt.Payload;
				InAppPurchaseLogger.Log($"UnityIAP Payload: {receipt.Payload}");
				InAppPurchaseLogger.Log($"UnityIAP Raw Receipt: {args.purchasedProduct.receipt}");
			}
			else
			{
				rawReceipt = args.purchasedProduct.receipt;
			}

			var transaction = new CompletedTransaction(
			   _txid,
			   rawReceipt,
			   args.purchasedProduct.metadata.localizedPrice.ToString(),
			   args.purchasedProduct.metadata.isoCurrencyCode
			);
			FulfillTransaction(transaction, args.purchasedProduct);

			return PurchaseProcessingResult.Pending;
		}

		/// <summary>
		/// Handle a purchase failure event from Unity IAP.
		/// </summary>
		/// <param name="product">The product whose purchase was attempted</param>
		/// <param name="failureReason">Information about why the purchase failed</param>
		public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
		{
			// A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing
			// this reason with the user to guide their troubleshooting actions.
			InAppPurchaseLogger.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}",
			   product.definition.storeSpecificId, failureReason));
			var paymentService = GetPaymentService();
			var reasonInt = (int)failureReason;
			if (failureReason == PurchaseFailureReason.UserCancelled)
			{
				paymentService.CancelPurchase(_txid);
				_cancelled?.Invoke();
			}
			else
			{
				paymentService.FailPurchase(_txid, product.definition.storeSpecificId + ":" + failureReason);
				var errorCode = new ErrorCode(reasonInt, GameSystem.GAME_CLIENT,
				   failureReason.ToString() + $" ({product.definition.storeSpecificId})");
				_fail?.Invoke(errorCode);
			}

			ClearCallbacks();
		}
		#endregion

		/// <summary>
		/// Fulfill a completed transaction by completing the purchase in the
		/// payments service and informing Unity IAP of completion.
		/// </summary>
		/// <param name="transaction">Completed IAP transaction</param>
		/// <param name="purchasedProduct">The product being purchased</param>
		private void FulfillTransaction(CompletedTransaction transaction, Product purchasedProduct)
		{
			GetPaymentService().CompletePurchase(transaction).Then(_ =>
			{
				_storeController.ConfirmPendingPurchase(purchasedProduct);
				_success?.Invoke(transaction);
				ClearCallbacks();
			}).Error(ex =>
			{
				Debug.LogError($"There was an exception making the complete purchase request: {ex}");
				var err = ex as ErrorCode;

				if (err == null)
				{
					return;
				}

				var retryable = err.Code >= 500 || err.Code == 429 || err.Code == 0;   // Server error or rate limiting or network error
				if (retryable)
				{
					GetCoroutineService().StartCoroutine(RetryTransaction(transaction, purchasedProduct));
				}
				else
				{
					_storeController.ConfirmPendingPurchase(purchasedProduct);
					_fail?.Invoke(err);
					ClearCallbacks();
				}
			});
		}

		/// <summary>
		/// If fulfillment failed, retry fulfillment with a backoff, as a coroutine.
		/// </summary>
		/// <param name="transaction">The failed transaction</param>
		/// <param name="purchasedProduct">The product that should have been fulfilled</param>
		private IEnumerator RetryTransaction(CompletedTransaction transaction, Product purchasedProduct)
		{
			// This block should only be hit when the error returned from the request is retryable. This lives down here
			// because C# doesn't allow you to yield return from inside a try..catch block.
			var waitTime = RETRY_DELAYS[Math.Min(transaction.Retries, RETRY_DELAYS.Length - 1)];
			InAppPurchaseLogger.Log($"Got a retryable error from platform. Retrying complete purchase request in {waitTime} seconds.");

			// Avoid incrementing the backoff if the device is definitely not connected to the network at all.
			// This is narrow, and would still increment if the device is connected, but the internet has other problems

			if (Application.internetReachability != NetworkReachability.NotReachable)
			{
				transaction.Retries += 1;
			}

			yield return new WaitForSeconds(waitTime);

			FulfillTransaction(transaction, purchasedProduct);
		}
	}

	/// <summary>
	/// Unity IAP Beamable service resolver.
	/// </summary>
	[BeamContextSystem]
	public class UnityBeamableIAPServiceResolver : IServiceResolver<IBeamablePurchaser>
	{
		private UnityBeamablePurchaser _unityBeamablePurchaser;

		public void OnTeardown()
		{
			_unityBeamablePurchaser = null;
		}

		public bool CanResolve() => true;

		public bool Exists() => _unityBeamablePurchaser != null;

		public IBeamablePurchaser Resolve()
		{
			return _unityBeamablePurchaser ?? (_unityBeamablePurchaser = new UnityBeamablePurchaser());
		}

		[RegisterBeamableDependencies(Constants.SYSTEM_DEPENDENCY_ORDER)]
		public static void Register(IDependencyBuilder builder)
		{
			builder.AddSingleton<IBeamablePurchaser, UnityBeamablePurchaser>();
		}
	}

	public class BeamableIAPInitializationException : Exception
	{
		public InitializationFailureReason Reason { get; }

		public BeamableIAPInitializationException(InitializationFailureReason reason) : base(
			$"Beamable IAP failed due to: {reason}")
		{
			Reason = reason;
		}
	}


	[Serializable]
	public class UnityPurchaseReceipt
	{
		public string Store;
		public string TransactionID;
		public string Payload;
	}
}
