using Beamable.Common;
using Beamable.Common.Api;
using System;

namespace Beamable.Api
{
	public interface IPubnubNotificationService
	{
		/// <summary>
		/// Get the pubnub subscription details.
		/// </summary>
		/// <returns>A <see cref="Promise"/> containing a <see cref="SubscriberDetailsResponse"/></returns>
		Promise<SubscriberDetailsResponse> GetSubscriberDetails();
	}

	public class PubnubNotificationService : IPubnubNotificationService
	{
		private IBeamableRequester _requester;

		public PubnubNotificationService(IBeamableRequester requester)
		{
			_requester = requester;
		}

		public Promise<SubscriberDetailsResponse> GetSubscriberDetails()
		{
			return _requester.Request<SubscriberDetailsResponse>(Method.GET, "/basic/notification");
		}
	}

	[Serializable]
	public class SubscriberDetailsResponse
	{
		public string subscribeKey;
		public string gameNotificationChannel;
		public string gameGlobalNotificationChannel;
		public string playerChannel;
		public string playerForRealmChannel;
		public string customChannelPrefix;
		public string authenticationKey;
	}
}
