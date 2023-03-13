using Beamable.Common.Spew;
using System;
using System.Collections.Generic;

namespace Beamable.Api.Notification.Internal
{
	/// <summary>
	/// Dummy local notification handler, for use as a stand-in within
	/// Editor and other contexts where OS notifications are not available.
	/// </summary>
	public class DummyLocalNotificationRelay : ILocalNotificationRelay
	{
		public void CreateNotificationChannel(string id, string name, string description)
		{
			NotificationLogger.LogFormat("[DummyLocalNote] Create notification channel. id={0}, name={1}, description={2}.", id, name, description);
		}

		public void ScheduleNotification(string channel, string key, string title, string message, DateTime when, Dictionary<string, string> data)
		{
			NotificationLogger.LogFormat("[DummyLocalNote] Schedule notification. channel={0}, key={1}, title={2}, message={3}.", channel, key, title, message);
		}

		public void CancelNotification(string key)
		{
			NotificationLogger.LogFormat("[DummyLocalNote] Cancel notification. key={0}.", key);
		}

		public void ClearDeliveredNotifications()
		{
			NotificationLogger.LogFormat("[DummyLocalNote] Cancel all notifications.");
		}
	}
}
