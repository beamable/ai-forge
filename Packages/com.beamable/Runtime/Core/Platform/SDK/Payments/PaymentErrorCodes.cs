
namespace Beamable.Api.Payments
{
	public static class PaymentErrorCodes
	{
		public const int ERROR_IN_COMPLETE_RESPONSE = 1;
		public const int UNSUPPORTED_PLATFORM = 2;
		public const int BILLING_ERROR = 3;
		public const int SKU_SYMBOL_IS_NULL = 4;
		public const int FAILED_TO_START_TX = 5;
		public const int EXCEPTION_HANDLING_COMPLETE_RESPONSE = 6;
		public const int EXCEPTION_MAKING_COMPLETE_REQUEST = 7;
		public const int FAILED_TO_CANCEL_TRANSACTION = 8;
		public const int FAILED_TO_FAIL_TRANSACTION = 9;
	}
}

