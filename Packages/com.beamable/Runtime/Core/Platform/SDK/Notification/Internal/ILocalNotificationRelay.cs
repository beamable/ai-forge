using System;
using System.Collections.Generic;


namespace Beamable.Api.Notification.Internal
{
	/// <summary>
	/// Interface for local notification relay. See AppleLocalNotificationRelay
	/// and GoogleLocalNotificationRelay for concrete implementations.
	/// </summary>
	public interface ILocalNotificationRelay
	{
		/// <summary>
		/// Create a notification channel.
		/// </summary>
		/// <param name="id">Identifier of the channel (Android) or category (iOS) of the notification.</param>
		/// <param name="name">User facing name for this channel.</param>
		/// <param name="description">User facing description for this channel.</param>
		void CreateNotificationChannel(string id, string name, string description);

		/// <summary>
		/// Schedule a local notification for the future.
		/// </summary>
		/// <param name="channel">Identifier of the channel (Android) or category (iOS) of the notification.</param>
		/// <param name="key">Arbitrary string key that can be used for cancelling the notification.</param>
		/// <param name="title">Title text of the notification.</param>
		/// <param name="message">Body text of the notification.</param>
		/// <param name="when">Time when the notification should arrive. This should be a future timestamp.</param>
		/// <param name="data">Arbitrary key/value data to attach to the notification.</param>
		void ScheduleNotification(string channel, string key, string title, string message, DateTime when, Dictionary<string, string> data);

		/// <summary>
		/// Cancel a specific notification.
		/// </summary>
		/// <param name="key">Key for identifying which notification to cancel, like "DBCONSOLE" or "BUILD_TIMER".</param>
		void CancelNotification(string key);

		void ClearDeliveredNotifications();
	}
}
