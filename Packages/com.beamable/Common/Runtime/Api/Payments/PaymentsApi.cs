using System;

namespace Beamable.Common.Api.Payments
{
	public class PaymentsApi : IPaymentsApi
	{
		protected IBeamableRequester _requester;

		public PaymentsApi(IBeamableRequester requester)
		{
			_requester = requester;
		}

		public Promise<EmptyResponse> VerifyReceipt(string provider, string receipt)
		{
			var data = new VerifyReceiptRequest(receipt);
			return _requester.Request<EmptyResponse>(Method.POST, $"/basic/payments/{provider}/purchase/verify", data);
		}
	}

	[Serializable]
	public class VerifyReceiptRequest
	{
		public string receipt;

		public VerifyReceiptRequest(string receipt)
		{
			this.receipt = receipt;
		}
	}
}
