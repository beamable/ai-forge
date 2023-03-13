using Beamable.Serialization;
using System.Collections.Generic;

namespace Beamable.Api.Payments
{

	public class NonceTransactionPair : JsonSerializable.ISerializable
	{

		private string nonce;
		private long tx;

		public string Nonce
		{
			get { return nonce; }
		}

		public long TransactionId
		{
			get { return tx; }
		}

		public NonceTransactionPair() { }

		public NonceTransactionPair(string nonce, long transactionid)
		{
			this.nonce = nonce;
			this.tx = transactionid;
		}

		public void Serialize(JsonSerializable.IStreamSerializer s)
		{
			s.Serialize("nonce", ref nonce);
			s.Serialize("tx", ref tx);
		}
	}

	public class PaymentTransactionList : JsonSerializable.ISerializable
	{

		private List<PaymentTransaction> _paymentTransactions;
		public List<PaymentTransaction> PaymentTransactions
		{
			get { return _paymentTransactions; }
		}

		public PaymentTransactionList()
		{
			_paymentTransactions = new List<PaymentTransaction>();
		}

		public PaymentTransactionList(List<PaymentTransaction> paymentTransactions)
		{
			_paymentTransactions = paymentTransactions;
		}

		public PaymentTransactionList(PaymentTransaction[] paymentTransactions)
		{
			_paymentTransactions = new List<PaymentTransaction>();

			for (int i = 0; i < paymentTransactions.Length; i++)
			{
				_paymentTransactions.Add(paymentTransactions[i]);
			}
		}

		public void Serialize(JsonSerializable.IStreamSerializer s)
		{
			s.SerializeList("paymentTransactions", ref _paymentTransactions);
		}
	}

	public class PaymentTransaction : JsonSerializable.ISerializable
	{

		private string _dbid;
		private string _symbol;
		private string _receipt;
		private string _priceInLocalCurrency;
		private string _isoCurrencySymbol;
		private NonceTransactionPair _nonceTxPair;

		public string Dbid
		{
			get { return _dbid; }
		}

		public string Symbol
		{
			get { return _symbol; }
		}

		public string Receipt
		{
			get { return _receipt; }
		}

		public string PriceInLocalCurrency
		{
			get { return _priceInLocalCurrency; }
		}

		public string IsoCurrencySymbol
		{
			get { return _isoCurrencySymbol; }
		}

		public NonceTransactionPair NonceTxPair
		{
			get { return _nonceTxPair; }
			set { _nonceTxPair = value; }
		}

		public PaymentTransaction() { }

		public PaymentTransaction(string dbid, string symbol, string receipt, string priceInLocalCurrency, string isoCurrencySymbol, NonceTransactionPair nonceTxPair = null)
		{
			_dbid = dbid;
			_symbol = symbol;
			_receipt = receipt;
			_priceInLocalCurrency = priceInLocalCurrency;
			_isoCurrencySymbol = isoCurrencySymbol;
			_nonceTxPair = nonceTxPair;
		}

		public void Serialize(JsonSerializable.IStreamSerializer s)
		{
			s.Serialize("dbid", ref _dbid);
			s.Serialize("symbol", ref _symbol);
			s.Serialize("receipt", ref _receipt);
			s.Serialize("priceInLocalCurrency", ref _priceInLocalCurrency);
			s.Serialize("isoCurrencySymbol", ref _isoCurrencySymbol);
			s.Serialize("nonceTxPair", ref _nonceTxPair);
		}

	}

}
