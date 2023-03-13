using System.Collections.Generic;

namespace Beamable.Api.Analytics
{

	public class BillerPurchaseFailedEvent : CoreEvent
	{
		public BillerPurchaseFailedEvent(string sku, ErrorCode error)
			: base("commerce", "purchase_failed", new Dictionary<string, object>
			{
				["sku"] = sku,
				["error"] = error.ToString()
			})
		{
		}
	}
}
