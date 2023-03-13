using Beamable.Serialization;
using System.Collections.Generic;

namespace Beamable.Api.Payments
{
	public class UnfulfilledTransactionList : JsonSerializable.ISerializable
	{
		private List<CompletedTransaction> _unfulfilledTransactions;
		public List<CompletedTransaction> UnfulfilledTransactions
		{
			get { return _unfulfilledTransactions; }
		}

		public UnfulfilledTransactionList()
		{
			_unfulfilledTransactions = new List<CompletedTransaction>();
		}

		public UnfulfilledTransactionList(IEnumerable<CompletedTransaction> paymentTransactions)
		{
			_unfulfilledTransactions = new List<CompletedTransaction>(paymentTransactions);
		}
		public void Serialize(JsonSerializable.IStreamSerializer s)
		{
			s.SerializeList("unfulfilledTransactions", ref _unfulfilledTransactions);
		}
	}

	public class CompletedTransaction : JsonSerializable.ISerializable
	{
		long _txid;
		public long Txid { get { return _txid; } }

		string _receipt;
		public string Receipt { get { return _receipt; } }

		string _priceInLocalCurrency;
		public string PriceInLocalCurrency { get { return _priceInLocalCurrency; } }

		string _isoCurrencySymbol;
		public string IsoCurrencySymbol { get { return _isoCurrencySymbol; } }

		// These properties are for internal client use only. They should NOT go over the wire...
		public string ListingSymbol { get; private set; }
		public string SKUSymbol { get; private set; }
		public int Retries { get; set; }

		public CompletedTransaction() { }

		public CompletedTransaction(
		   long txid,
		   string receipt,
		   string priceInLocalCurrency,
		   string isoCurrencySymbol,
		   string listingSymbol = "",
		   string skuSymbol = ""
		)
		{
			_txid = txid;
			_receipt = receipt;
			_priceInLocalCurrency = priceInLocalCurrency;
			_isoCurrencySymbol = isoCurrencySymbol;
			ListingSymbol = listingSymbol;
			SKUSymbol = skuSymbol;
		}

		public void Serialize(JsonSerializable.IStreamSerializer s)
		{
			s.Serialize("txid", ref _txid);
			s.Serialize("receipt", ref _receipt);
			s.Serialize("priceInLocalCurrency", ref _priceInLocalCurrency);
			s.Serialize("isoCurrencySymbol", ref _isoCurrencySymbol);
		}
	}
}
