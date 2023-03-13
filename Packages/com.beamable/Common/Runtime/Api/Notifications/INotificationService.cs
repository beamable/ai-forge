using System;
using System.Collections.Generic;

namespace Beamable.Common.Api.Notifications
{
	public interface INotificationService
	{
		/// <summary>
		/// Register a callback handler for notifications.
		/// Note that the object given to the callback may not be the exact type you expect. Instead, it may be an ArrayDict.
		/// Use the <see cref="Subscribe{T}"/> method to have the callback deliver an object of the desired type.
		/// These callbacks will be triggered anytime an event with same name is sent to the <see cref="Publish"/> method.
		/// </summary>
		/// <param name="name">The event name to receive a callback on</param>
		/// <param name="callback">The callback to invoke when the event is received</param>
		void Subscribe(string name, Action<object> callback);

		/// <summary>
		/// Unregister a callback handler for a notification.
		/// </summary>
		/// <param name="name">The event name to remove a callback handler from.</param>
		/// <param name="handler">The same instance of the callback that was registered with the <see cref="Subscribe"/> method.</param>
		void Unsubscribe(string name, Action<object> handler);

		/// <summary>
		/// Unregister all callback handlers for a notification.
		/// </summary>
		/// <param name="name">The event name to remove all callback handlers from.</param>
		void UnsubscribeAll(string name);

		/// <summary>
		/// Register a callback handler for notifications.
		/// These callbacks will be triggered anytime an event with same name is sent to the <see cref="Publish"/> method.
		/// </summary>
		/// <param name="name">The event name to receive a callback on</param>
		/// <param name="callback">The callback to invoke when the event is received</param>
		/// <typeparam name="T">
		/// The type of argument that was sent with the original event.
		/// If a type of "string" is given, the inner event will assumed to have an inner "stringValue" field containing the raw string.
		/// </typeparam>
		void Subscribe<T>(string name, Action<T> callback);

		/// <summary>
		/// Unregister a callback handler for a notification.
		/// </summary>
		/// <param name="name">The event name to remove a callback handler from.</param>
		/// <param name="handler">The same instance of the callback that was registered with the <see cref="Subscribe{T}"/> method.</param>
		/// <typeparam name="T"></typeparam>
		void Unsubscribe<T>(string name, Action<T> handler);

		/// <summary>
		/// Trigger the callbacks for a given notification.
		/// Callbacks can be registered using the <see cref="Subscribe"/> method.
		/// </summary>
		/// <param name="name">The event name to publish</param>
		/// <param name="payload">The data to to make available to all subscribers</param>
		void Publish(string name, object payload);

		/// <summary>
		/// Pause all callback handlers for a notification.
		/// </summary>
		/// <param name="name">The event name to pause all callback handlers from.</param>
		void Pause(string name);

		/// <summary>
		/// Resume all callback handlers for a notification.
		/// </summary>
		/// <param name="name">The event name to resume all callback handlers from.</param>
		void Resume(string name);

		/// <summary>
		/// Create a notification channel.
		/// </summary>
		/// <param name="id">Identifier of the notification channel.</param>
		/// <param name="name">Arbitrary identifier that can be used to cancel the notification.</param>
		/// <param name="description">Arbitrary ID used for analytics.</param>
		void CreateNotificationChannel(string id, string name, string description);

		/// <summary>
		/// Schedule a local notification. This will overwrite any
		/// previous notification with the same key that may exist.
		/// </summary>
		/// <param name="channel">Identifier of the notification channel.</param>
		/// <param name="key">Arbitrary identifier that can be used to cancel the notification.</param>
		/// <param name="trackingId">Arbitrary ID used for analytics.</param>
		/// <param name="title">The title of the action button or slider.</param>
		/// <param name="message">The message body text.</param>
		/// <param name="timeFromNow">How long before the notification should appear.</param>
		/// <param name="restrictTime">If true the notification will be placed inside the restricted time window.</param>
		/// <param name="customData">Optional list of custom data to store in the notification for later use.</param>
		void ScheduleLocalNotification(string channel,
									   string key,
									   int trackingId,
									   string title,
									   string message,
									   TimeSpan timeFromNow,
									   bool restrictTime,
									   Dictionary<string, string> customData = null);

		/// <summary>
		/// Register for remote and local notifications
		/// </summary>
		void RegisterForNotifications();
	}

	public static class NotificationServiceExtensions
	{
		public static string GetRefreshEventNameForService(this INotificationService _, string service) =>
		   $"{service}.refresh";
	}
}
