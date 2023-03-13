using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Dependencies;
using Beamable.Common.Spew;
using Beamable.Coroutines;
using Beamable.Pooling;
using Beamable.Serialization;
using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Beamable.Api.Payments
{
	[Obsolete("This is no longer a supported Beamable flow. Please migrate to Beamable Purchaser")]
	public class TransactionManager
	{
		private readonly IDependencyProvider _provider;
		public event Action<CompletedTransaction> OnFulfillmentComplete;
		public event Action<ErrorCode, CompletedTransaction> OnFulfillmentFailed;

		private readonly Dictionary<string, PurchaseInfo> pendingPurchases = new Dictionary<string, PurchaseInfo>();

		private readonly Queue<CompletedTransaction> pendingFulfillment = new Queue<CompletedTransaction>();

		private const string pendingPurchasesStorageKey = "pending_purchases";

		private const string unfulfilledStorageKey = "unfulfilled_transactions";


		private PaymentService PaymentService => _provider.GetService<PaymentService>();
		private CoroutineService CoroutineService => _provider.GetService<CoroutineService>();

		public TransactionManager(IDependencyProvider provider)
		{
			_provider = provider;
		}

		public void Initialize()
		{
			RestoreUnfulfilledTransactions();
			if (pendingFulfillment.Count > 0)
			{
				RestorePendingPurchases();
			}
		}

		private void FulfillTransaction(CompletedTransaction transaction)
		{
			InAppPurchaseLogger.Log($"[TransactionManager] FulfillTransaction: Fulfilling Transaction: {transaction.Txid}");
			PaymentService.CompletePurchase(transaction).Then(rsp =>
			{
				FulfillmentCompleted(transaction);
			}).Error(error =>
			{
				Debug.LogError($"There was an exception making the complete purchase request: {error}");

				//XXX: This should be an ErrorCode.
				var err = error as PlatformRequesterException;
				if (err?.Error == null)
				{
					return;
				}

				var retryable = err.Status >= 500 || err.Status == 429;   // Server error or rate limiting
				if (retryable)
				{
					transaction.Retries += 1;
					CoroutineService.StartCoroutine(RetryTransaction(transaction));
				}
				else
				{
					FulfillmentFailed(new ErrorCode(err.Error), transaction);
				}
			});
		}

		private IEnumerator RetryTransaction(CompletedTransaction transaction)
		{
			// This block should only be hit when the error returned from the request is retryable. This lives down here
			// because C# doesn't allow you to yield return from inside a try..catch block.
			float waitTime = (float)System.Math.Min(System.Math.Pow(2, transaction.Retries), 60);
			Debug.LogWarning($"Got a retryable error from platform. Retrying complete purchase request in {waitTime} seconds.");
			yield return new WaitForSeconds(waitTime);
			FulfillTransaction(transaction);
			yield break;
		}

		public void AddToPendingPurchases(long txid, string listingSymbol, string skuSymbol)
		{
			pendingPurchases[skuSymbol] = new PurchaseInfo(txid, listingSymbol);

			BackupPendingPurchases();
		}

		public PurchaseInfo PopFromPendingPurchases(string skuSymbol)
		{
			PurchaseInfo purchaseInfo;
			if (pendingPurchases.TryGetValue(skuSymbol, out purchaseInfo))
			{
				pendingPurchases.Remove(skuSymbol);
				BackupPendingPurchases();
				return purchaseInfo;
			}
			return new PurchaseInfo(-1L, "");
		}

		/// <summary>
		/// Add a completed transaction to the pendingFulfillment queue.
		/// </summary>
		/// <param name="transaction">The completed transaction to enqueue for fulfillment.</param>
		public void AddToPendingFulfillment(CompletedTransaction transaction)
		{
			pendingFulfillment.Enqueue(transaction);
			BackupUnfulfilledTransactions();

			// If this is the head, start fulfilling
			if (pendingFulfillment.Count == 1)
			{
				FulfillTransaction(transaction);
			}
		}

		private void PopFromPendingFulfillment()
		{
			pendingFulfillment.Dequeue();
			BackupUnfulfilledTransactions();

			// If there's more, start fulfilling
			if (pendingFulfillment.Count > 0)
			{
				var transaction = pendingFulfillment.Peek();
				FulfillTransaction(transaction);
			}
		}

		private void FulfillmentCompleted(CompletedTransaction transaction)
		{
			PopFromPendingFulfillment();

			InAppPurchaseLogger.Log($"[TransactionManager] Purchase Completed for Txid: {transaction.Txid}");
			OnFulfillmentComplete?.Invoke(transaction);
		}

		private void FulfillmentFailed(ErrorCode errorCode, CompletedTransaction transaction)
		{
			PopFromPendingFulfillment();

			InAppPurchaseLogger.LogFormat($"[TransactionManager] Purchase Failed for Txid: {transaction.Txid}");

			OnFulfillmentFailed?.Invoke(errorCode, transaction);
		}

		private void BackupPendingPurchases()
		{
			InAppPurchaseLogger.Log("[TransactionManager] BackupPendingPurchases: Saving pending purchases to disk, count: " + pendingPurchases.Count.ToString());

			try
			{
				//Serialize into json string, and base64 encode
				Dictionary<string, object> dict = new Dictionary<string, object>();
				foreach (var entry in pendingPurchases)
				{
					dict[entry.Key] = JsonSerializable.Serialize(entry.Value);
				}

				using (var pooledBuilder = StringBuilderPool.StaticPool.Spawn())
				{
					string s = Json.Serialize(dict, pooledBuilder.Builder);
					string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(s));

					//Save to Disk
					PlayerPrefs.SetString(pendingPurchasesStorageKey, encoded);
					PlayerPrefs.Save();
				}
			}
			catch (Exception e)
			{
				InAppPurchaseLogger.Log("[TransactionManager] BackupPendingPurchases: An error occurred during saving of pending purchases => " + e.Message);
			}
		}

		private void RestorePendingPurchases()
		{
			InAppPurchaseLogger.Log("[TransactionManager] RestorePendingPurchases: Restoring unfulfilled transactions from disk...");

			try
			{
				string encodedPending = PlayerPrefs.GetString(pendingPurchasesStorageKey);
				InAppPurchaseLogger.Log("[TransactionManager] RestorePendingPurchases: Decoding " + encodedPending);
				if (!string.IsNullOrEmpty(encodedPending))
				{
					string decodedPending = FromBase64(encodedPending);

					if (!string.IsNullOrEmpty(decodedPending))
					{
						var pending = Json.Deserialize(decodedPending) as Dictionary<string, object>;
						if (pending != null)
						{
							foreach (var entry in pending)
							{
								var purchaseInfo = new PurchaseInfo();
								JsonSerializable.Deserialize(purchaseInfo, entry.Value as IDictionary<string, object>);
								var txid = purchaseInfo.Txid;
								var listingSymbol = purchaseInfo.ListingSymbol;
								InAppPurchaseLogger.Log($"[TransactionManager] RestorePendingPurchases: Adding {txid} {listingSymbol} {entry.Key}");
								AddToPendingPurchases(txid, listingSymbol, entry.Key);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				InAppPurchaseLogger.Log("[TransactionManager] RestorePendingPurchases: An error occurred during loading pending purchases.");
				Debug.LogException(e);
			}
			InAppPurchaseLogger.Log("[TransactionManager] RestorePendingPurchases: Done.");
		}

		private void BackupUnfulfilledTransactions()
		{
			InAppPurchaseLogger.Log($"[TransactionManager] BackupUnfulfilledTransactions: Saving Unfulfilled transactions to disk, count: {pendingFulfillment.Count}");

			try
			{
				var unfulfilledTransactionsList = new UnfulfilledTransactionList(pendingFulfillment);
				Dictionary<string, object> unfulfilledTransactions = JsonSerializable.Serialize(unfulfilledTransactionsList);

				//Serialize into json string, and base64 encode
				using (var pooledBuilder = StringBuilderPool.StaticPool.Spawn())
				{
					string s = Json.Serialize(unfulfilledTransactions, pooledBuilder.Builder);
					string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(s));

					//Save to Disk
					PlayerPrefs.SetString(unfulfilledStorageKey, encoded);
					PlayerPrefs.Save();
				}
			}
			catch (Exception e)
			{
				InAppPurchaseLogger.Log($"[TransactionManager] BackupUnfulfilledTransactions: An error occurred during saving of a transaction => {e.Message}");
			}
		}

		//This exists to ensure that payments are not lost in the event of a crash or network glitch
		private void RestoreUnfulfilledTransactions()
		{
			InAppPurchaseLogger.Log("[TransactionManager] RestoreUnfulfilledTransactions: Restoring unfulfilled transactions from disk...");

			try
			{
				string encodedUnfulfilled = PlayerPrefs.GetString(unfulfilledStorageKey);
				InAppPurchaseLogger.Log($"[TransactionManager] RestoreUnfulfilledTransactions: Decoding {encodedUnfulfilled}");
				if (!string.IsNullOrEmpty(encodedUnfulfilled))
				{
					string retrievedUnfulfilled = FromBase64(encodedUnfulfilled);

					if (!string.IsNullOrEmpty(retrievedUnfulfilled))
					{
						var ts = Json.Deserialize(retrievedUnfulfilled) as Dictionary<string, object>;
						if (ts != null)
						{
							var transactions = new UnfulfilledTransactionList();
							JsonSerializable.Deserialize(transactions, ts);

							if (transactions.UnfulfilledTransactions != null)
							{
								InAppPurchaseLogger.Log($"[TransactionManager] RestoreUnfulfilledTransactions: Transactions restored, count: {transactions.UnfulfilledTransactions.Count}");

								foreach (var t in transactions.UnfulfilledTransactions)
								{
									AddToPendingFulfillment(t);
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				InAppPurchaseLogger.Log("[TransactionManager] RestoreUnfulfilledTransactions: An error occurred during loading Unfulfilled transactions.");
				Debug.LogException(e);
			}
			InAppPurchaseLogger.Log("[TransactionManager] RestoreUnfulfilledTransactions: Done.");
		}

		public Promise<PurchaseResponse> BeginPurchase(string purchaseId)
		{
			return PaymentService.BeginPurchase(purchaseId);
		}

		public Promise<EmptyResponse> CancelPurchase(long txid)
		{
			return PaymentService.CancelPurchase(txid);
		}

		public void FailPurchase(long txid, string reason, string sku)
		{
			if (txid != -1)
			{
				PaymentService.FailPurchase(txid, reason);
			}
		}

		private string FromBase64(string input)
		{
			try
			{
				return Encoding.UTF8.GetString(Convert.FromBase64String(input));
			}
			catch
			{
				return null;
			}
		}
	}

	public class PurchaseInfo : JsonSerializable.ISerializable
	{
		long _txid;
		public long Txid { get { return _txid; } }

		string _listingSymbol;
		public string ListingSymbol { get { return _listingSymbol; } }

		public PurchaseInfo() { }

		public PurchaseInfo(
		   long txid,
		   string listingSymbol
		)
		{
			_txid = txid;
			_listingSymbol = listingSymbol;
		}

		public void Serialize(JsonSerializable.IStreamSerializer s)
		{
			s.Serialize("txid", ref _txid);
			s.Serialize("listingSymbol", ref _listingSymbol);
		}
	}
}
