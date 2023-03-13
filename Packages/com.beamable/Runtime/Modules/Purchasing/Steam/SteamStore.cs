using Beamable.Common.Dependencies;
using Beamable.Common.Steam;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Beamable.Purchasing.Steam
{
	public class SteamStore : IStore
	{
		private readonly IDependencyProvider _provider;
		public const string Name = "SteamStore";

		public ISteamService steam;
		public IStoreCallback callback;
		public List<SteamProduct> steamProducts;

		private Dictionary<string, InProgressPurchase> _inProgress = new Dictionary<string, InProgressPurchase>();

		public SteamStore(IDependencyProvider provider)
		{
			_provider = provider;
		}

		public void Initialize(IStoreCallback callback)
		{
			this.callback = callback;

			this.steam = _provider.GetService<ISteamService>();

			if (this.steam == null)
			{
				OnInitializeFailed("Steam service unavailable. Provide ServiceManager an ISteamService instance.");
			}
			else
			{
				this.steam.RegisterTransactionCallback(OnTransactionAuthorized);
			}
		}

		public void RetrieveProducts(ReadOnlyCollection<ProductDefinition> productDefinitions)
		{
			steam.RegisterAuthTicket()
				.FlatMap(_ => steam.GetProducts())
				.Then(rsp => OnRetrieved(productDefinitions, rsp.products))
				.Error(ex => OnInitializeFailed("Failed to retrieve steam products.", ex));
		}

		private void OnRetrieved(ReadOnlyCollection<ProductDefinition> productDefinitions, List<SteamProduct> steamProducts)
		{
			this.steamProducts = steamProducts;

			var productDescriptions = new List<ProductDescription>();
			foreach (var definition in productDefinitions)
			{
				var steamProduct = steamProducts.Find(r => r.sku == definition.id);
				if (steamProduct != null)
				{
					var price = System.Convert.ToDecimal(steamProduct.localizedPrice);
					var metadata = new ProductMetadata(
						steamProduct.localizedPriceString,
						steamProduct.description,
						steamProduct.description,
						steamProduct.isoCurrencyCode,
						price);

					productDescriptions.Add(new ProductDescription(definition.storeSpecificId, metadata));
				}
			}

			callback.OnProductsRetrieved(productDescriptions);
		}

		private void OnInitializeFailed(string message, System.Exception ex = null)
		{
			Debug.LogError(message);
			if (ex != null)
			{
				Debug.LogError(ex);
			}

			callback.OnSetupFailed(InitializationFailureReason.PurchasingUnavailable);
		}

		public void OnTransactionAuthorized(SteamTransaction transaction)
		{
			InProgressPurchase currentPurchase;
			if (!_inProgress.TryGetValue(transaction.transactionId, out currentPurchase))
			{
				return;
			}

			if (transaction.authorized)
			{
				callback.OnPurchaseSucceeded(
					currentPurchase.product.storeSpecificId,
					currentPurchase.transactionId,
					currentPurchase.transactionId);
			}
			else
			{
				callback.OnPurchaseFailed(new PurchaseFailureDescription(
					currentPurchase.product.id,
					PurchaseFailureReason.UserCancelled,
					"Steam purchase cancelled."));
			}
		}

		public void Purchase(ProductDefinition product, string developerPayload)
		{
			if (this.steam == null)
			{
				callback.OnPurchaseFailed(new PurchaseFailureDescription(
					product.id,
					PurchaseFailureReason.PurchasingUnavailable,
					"Steam service unavailable. Provide ServiceManager an ISteamService instance."));

				return;
			}

			this._inProgress[developerPayload] = new InProgressPurchase(product, developerPayload);
		}

		public void FinishTransaction(ProductDefinition product, string transactionId)
		{

		}
	}

	public class InProgressPurchase
	{
		public ProductDefinition product;
		public string transactionId;

		public InProgressPurchase(ProductDefinition product, string transactionId)
		{
			this.product = product;
			this.transactionId = transactionId;
		}
	}
}

