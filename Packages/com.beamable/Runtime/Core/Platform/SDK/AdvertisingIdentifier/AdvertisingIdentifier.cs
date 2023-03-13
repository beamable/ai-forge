using Beamable.Common;
using UnityEngine;

namespace Beamable.Api.AdvertisingIdentifier
{
	public static class AdvertisingIdentifier
	{
		// TODO: should this just be inlined?
		public static Promise<string> GetIdentifier()
		{
			var result = new Promise<string>();

			void HandleResult(string advertisingId, bool trackingEnabled, string error)
			{
				result.CompleteSuccess(trackingEnabled ? advertisingId : null);
			}

			if (Application.isEditor)
			{
				result.CompleteSuccess("EDITOR_AD_ID");
			}
			else if (!Application.RequestAdvertisingIdentifierAsync(HandleResult))
			{
				result.CompleteSuccess(null);
			}

			return result;
		}
	}
}
