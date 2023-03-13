using Beamable.Common.Spew;
using System;

namespace Beamable.Api.Notification.Internal
{
	internal class InGameNotification
	{
		string key;
		string message;
		NotificationService.InGameNotificationCB callback;

		DateTime endpoint;

		public DateTime Endpoint
		{
			get { return endpoint; }
		}

		public InGameNotification(string key, string message, TimeSpan secondsFromNow, NotificationService.InGameNotificationCB callback)
		{
			this.key = key;
			this.message = message;
			this.callback = callback;

			DateTime currentTime = DateTime.UtcNow;
			this.endpoint = currentTime.AddSeconds(secondsFromNow.TotalSeconds);
		}

		public void Notify()
		{
			NotificationLogger.LogFormat("In Game Notification {0}: {1}", key, message);

			if (callback != null)
			{
				callback(key, message);
			}
		}
	}
}
