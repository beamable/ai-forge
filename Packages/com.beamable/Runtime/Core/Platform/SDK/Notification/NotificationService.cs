using Beamable.Api.Notification.Internal;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Notifications;
using Beamable.Common.Dependencies;
using Beamable.Common.Spew;
using Beamable.Pooling;
using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections.Generic;
using UnityEngine;
using StringBuilderPool = Beamable.Common.Pooling.StringBuilderPool;

#if UNITY_IOS
using NotificationServices = UnityEngine.iOS.NotificationServices;
using NotificationType = UnityEngine.iOS.NotificationType;
using LocalNotification = UnityEngine.iOS.LocalNotification;
using RemoteNotification = UnityEngine.iOS.RemoteNotification;
#endif

#if USE_FIREBASE
using Firebase.Messaging;
using System.Text;
#endif

namespace Beamable.Api.Notification
{
	/// <summary>
	/// This type defines the %Client main entry point for the %Notifications feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/notifications-feature">Mail</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class NotificationService : MonoBehaviour, INotificationService
	{
		DateTime timeOfLastUpdate = DateTime.MinValue;
		Dictionary<string, InGameNotification> inGameNotifications = new Dictionary<string, InGameNotification>();
		public delegate void InGameNotificationCB(string notificationKey, string message);

		private HashSet<string> pausedHandlers = new HashSet<string>();
		private Dictionary<string, List<Action<object>>> handlers = new Dictionary<string, List<Action<object>>>();
		private HashSet<object> typedHandlerObjects = new HashSet<object>();

		private Dictionary<object, Action<object>> _boxedCallbackToActualCallback =
			new Dictionary<object, Action<object>>();

		/* Maximum number of local notifications allowed to be scheduled at the same time. */
		public const int MaxLocalNotifications = 25;
		// Scheduled notifications must fall between 10AM - 10PM
		const int MinHour = 10;
		const int MaxHour = 22;
		// #if USE_FIREBASE
		// private IPlatformService _platform;
		// #endif

		/* Relay to use for local notifications. */
		private ILocalNotificationRelay LocalRelay;

		/// <summary>
		/// Register for remote and local notifications
		/// </summary>
		public void RegisterForNotifications()
		{
			// #if USE_FIREBASE
			// _platform = null; // TODO: Fix this by just using the push service directly.
			// #endif
			RegisterForLocalNotifications();
			ClearDeliveredLocalNotifications();
			RegisterForPushNotifications();
		}

		private void RegisterForLocalNotifications()
		{
			// ReSharper disable once SwitchStatementMissingSomeCases
			switch (Application.platform)
			{
				case RuntimePlatform.Android:
					LocalRelay = new GoogleLocalNotificationRelay();
					NotificationLogger.Log("Local notifications using Google provider.");
					break;
#if UNITY_IOS
            case RuntimePlatform.IPhonePlayer:
                NotificationServices.RegisterForNotifications(NotificationType.Alert | NotificationType.Badge | NotificationType.Sound);
                LocalRelay = new AppleLocalNotificationRelay();
               NotificationLogger.Log("Local notifications using Apple provider.");
               break;
#endif // UNITY_IOS
				default:
					LocalRelay = new DummyLocalNotificationRelay();
					NotificationLogger.LogFormat("Local notifications using dummy provider on platform='{0}'.", Application.platform);
					break;
			}
		}

		/// <inheritdoc cref="INotificationService.Subscribe{T}(string, Action{T})"/>
		public void Subscribe<T>(string name, Action<T> callback)
		{
			void Handler(object raw)
			{
				if (raw == null)
				{
					callback?.Invoke(default);
					return;
				}

				bool isString = typeof(T) == typeof(string);
				switch (raw)
				{
					case ArrayDict dict:
						// special handling for the string case, because in 1.1, we didn't force the string case to be in a wrapped object.
						if (isString)
						{
							var objResult = dict[Constants.Features.Notifications.PRIMITIVE_STRING_PAYLOAD_FIELD]; // the "stringValue" is a custom name from the C#MS base image.
							string strResult = (string)objResult;
							//strResult = strResult.Substring(1, strResult.Length - 2); // strip off the required escape quotes.
							objResult = strResult; // rebox the type for casting.
							callback?.Invoke((T)objResult);
							return;
						}

						var json = Json.Serialize(raw, StringBuilderPool.StaticPool.Spawn().Builder);
						var typedResult = JsonUtility.FromJson<T>(json);
						callback?.Invoke(typedResult);
						break;
					default:
						Debug.LogWarning($"Unknown type sent to Notification Service. type=[{raw?.GetType().FullName}] data=[{raw}]");
						break;
				}
			}

			object boxedCallback = callback;
			typedHandlerObjects.Add(boxedCallback);
			_boxedCallbackToActualCallback.Add(callback, Handler);
			Subscribe(name, Handler);
		}


		/// <summary>
		/// Register a callback handler for push notifications.
		/// </summary>
		/// <param name="name">The event name to receive a callback on</param>
		/// <param name="callback">The callback to invoke when the event is received</param>
		public void Subscribe(string name, Action<object> callback)
		{
			if (!handlers.TryGetValue(name, out var found))
			{
				found = new List<Action<object>>();
				handlers.Add(name, found);
			}
			found.Add(callback);
		}

		public void Unsubscribe(string name, Action<object> handler)
		{
			if (handlers.TryGetValue(name, out var found))
			{
				found.Remove(handler);
			}
		}

		/// <summary>
		/// Unregister all callback handlers for push notifications.
		/// </summary>
		/// <param name="name">The event name to unsubscribe all callbacks</param>
		public void UnsubscribeAll(string name)
		{
			if (handlers.TryGetValue(name, out var found))
			{
				found.Clear();
			}
		}

		/// <inheritdoc cref="INotificationService.Unsubscribe{T}(string, Action{T})"/>
		public void Unsubscribe<T>(string name, Action<T> handler)
		{
			object boxedCallback = handler;
			if (!typedHandlerObjects.Remove(boxedCallback))
			{
				Debug.LogWarning("No existing handler was found for the given handler.");
			}

			if (!_boxedCallbackToActualCallback.TryGetValue(handler, out var actualHandler))
			{
				Debug.LogWarning("No existing wrapped handler was found for the given typed handler.");
			}

			if (handlers.TryGetValue(name, out var found))
			{
				found.Remove(actualHandler);
				_boxedCallbackToActualCallback.Remove(handler);
			}
		}

		/// <summary>
		/// Trigger the callbacks for a given notification.
		/// </summary>
		/// <param name="name">The event name to publish</param>
		/// <param name="payload">The data to to make available to all subscribers</param>
		public void Publish(string name, object payload)
		{
			if (pausedHandlers.Contains(name))
				return;

			if (handlers.TryGetValue(name, out var found))
			{
				for (var i = found.Count - 1; i > -1; i--)
				{
					found[i](payload);
				}
			}
		}

		/// <summary>
		/// Pause the callbacks for a given notification.
		/// </summary>
		/// <param name="name">The event name to pause</param>
		public void Pause(string name)
		{
			pausedHandlers.Add(name);
		}

		/// <summary>
		/// Resume the callbacks for a given notification.
		/// </summary>
		/// <param name="name">The event name to resume</param>
		public void Resume(string name)
		{
			if (pausedHandlers.Contains(name))
				pausedHandlers.Remove(name);
		}

		#region Push notifications

#if USE_FIREBASE

#if UNITY_ANDROID
        private const PushProvider provider = PushProvider.Google;
#elif UNITY_IOS
        private const PushProvider provider = PushProvider.Apple;
#else
        private const PushProvider provider = PushProvider.Unknown;
#endif // UNITY_ANDROID | UNITY_IOS

        public void FirebaseTokenReceived(object sender, TokenReceivedEventArgs token)
        {
            NotificationLogger.Log("FCM token: " + token.Token);
            _platform.Push.Register(provider, token.Token);
        }

        public void FirebaseMessageReceived(object sender, MessageReceivedEventArgs message)
        {
            string title = "";
            string body = "";
            if (message.Message.Notification != null) {
                title = message.Message.Notification.Title;
                body = message.Message.Notification.Body;
            }

            var messageType = message.Message.MessageType;

            NotificationLogger.LogFormat("FCM message: title={0} body='{1}' type={2}.", title, body, messageType);

            Publish(messageType, message.Message.Data);
        }
#endif // USE_FIREBASE
		#endregion // Push notifications

		#region In-game notifications
		/**
         * Schedule an in-game notification. If an existing notification is found, then it
         * will be replaced.
         * Use this method to schedule an notification 1971 seconds from now.
         * Note that In game notifications don't persist after game restarts.
         *
         * @param key: an arbitrary identifier for this notification that is used for cancelling later
         * @param timeFromNow: The amount of time into the future to fire this notification
         */
		public void ScheduleInGameNotification(string key, string message, TimeSpan timeFromNow, InGameNotificationCB callback)
		{
			// Check for an existing one
			if (inGameNotifications.ContainsKey(key))
			{
				CancelInGameNotification(key);
			}

			inGameNotifications.Add(key, new InGameNotification(key, message, timeFromNow, callback));
		}

		/**
         * Cancel an in-game notification.
         *
         * @param key: an arbitrary identifier for this notification
         */
		public void CancelInGameNotification(string key)
		{
			InGameNotification notification;
			inGameNotifications.TryGetValue(key, out notification);
			if (notification != null)
			{
				inGameNotifications.Remove(key);
			}
		}

		public void RemoveAllInGameNotifications()
		{
			inGameNotifications.Clear();
		}

		public void Update()
		{
			// This loop handles all scheduled in game notifications. This was originally performed by using
			// coroutines that awoke X seconds later. Unfortunately in iOS, the WaitForSeconds pauses when the game
			// pauses skewing the time by the amount the app was backgrounded.
			// This function checks the list of notifications every second and acts on any that have expired.
			// NOTE: WaitForSecondsRealtime is an alternative
			var now = DateTime.UtcNow;
			TimeSpan updateSpan = now - timeOfLastUpdate;
			if (updateSpan.TotalSeconds >= 1)
			{
				timeOfLastUpdate = now;

				if (inGameNotifications.Count > 0)
				{
					// Get the key list so we can iterate while notifications are added and removed.
					var keyList = new string[inGameNotifications.Count];
					try
					{
						inGameNotifications.Keys.CopyTo(keyList, 0);
						for (int i = 0; i < keyList.Length; i++)
						{
							InGameNotification inGameNote;
							if (inGameNotifications.TryGetValue(keyList[i], out inGameNote))
							{
								try
								{
									if (DateTime.Compare(inGameNote.Endpoint, timeOfLastUpdate) <= 0)
									{
										// time has triggered
										inGameNotifications.Remove(keyList[i]);
										inGameNote.Notify();
									}
								}
								catch (Exception exc)
								{
									Debug.LogError(string.Format("Exception processing In Game Notification {0}: {1}", keyList[i], exc));
								}
							}
							else
							{
								NotificationLogger.LogFormat("In Game Notification {0} was removed before being processed", keyList[i]);
							}
						}
					}
					catch (ArgumentException)
					{
						// This occurs when the CopyTo has more elements than the length of the array
						NotificationLogger.Log("In Game Notification inserted during processing, skipping to next second");
					}
				}
			}
		}
		#endregion

		#region "Local/OS supported notifications"
		/// <summary>
		/// Create a notification channel.
		/// </summary>
		/// <param name="id">Identifier of the notification channel.</param>
		/// <param name="name">Arbitrary identifier that can be used to cancel the notification.</param>
		/// <param name="description">Arbitrary ID used for analytics.</param>
		public void CreateNotificationChannel(string id, string name, string description)
		{
			LocalRelay.CreateNotificationChannel(id, name, description);
			NotificationLogger.LogFormat("Create notification channel. id={0}, name={1}, description={2}.", id, name, description);
		}

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
		public void ScheduleLocalNotification(
		   string channel,
		   string key,
		   int trackingId,
		   string title,
		   string message,
		   TimeSpan timeFromNow,
		   bool restrictTime,
		   Dictionary<string, string> customData = null
		)
		{
			var fireTime = CalculateNotificationFireDateTime(timeFromNow, restrictTime);
			LocalRelay.ScheduleNotification(channel, key, title, message, fireTime, customData);

			// Removing Notification Custom Events because they are rather noisy
			//new CustomEvent(instance, key).Category("note:schedule").Subcategory("local").Value(trackingId).Send();

			NotificationLogger.LogFormat("Local Notification {0} ({1}) scheduled for {2} on channel {3}.", key, key.GetHashCode(), fireTime, channel);
		}

		/// <summary>
		/// Cancel a pending local notification that has not yet been shown.
		/// </summary>
		/// <param name="key">String key identifying the notification.</param>
		public void CancelLocalNotification(string key)
		{
			LocalRelay.CancelNotification(key);
			NotificationLogger.LogFormat("Remove Scheduled Notification '{0}'.", key);
		}

		private void ClearDeliveredLocalNotifications()
		{
			LocalRelay.ClearDeliveredNotifications();
			NotificationLogger.LogFormat("Remove All Scheduled Notifications.");
		}

		/// <summary>
		/// Calculate an absolute date-time for the notification to fire using 'timeFromNow' as an offset.
		/// </summary>
		/// <param name="timeFromNow">Time interval until the notification would fire.</param>
		/// <param name="restrictTime">True if the notification should be restricted to "daylight hours".</param>
		/// <returns>Time for the notification, which may be adjusted to fit in restricted hours.</returns>
		private DateTime CalculateNotificationFireDateTime(TimeSpan timeFromNow, bool restrictTime)
		{
			var currentTime = DateTime.Now;
			var fireTime = currentTime.Add(timeFromNow);
			if (!restrictTime)
			{
				return fireTime;
			}

			if (fireTime.Hour < MinHour)
			{
				// round up to 10 AM same day
				fireTime = new DateTime(fireTime.Year, fireTime.Month, fireTime.Day, MinHour, 0, 0);
			}
			else if (fireTime.Hour > MaxHour)
			{
				// round up to 10 AM next day
				fireTime = fireTime.AddDays(1);
				fireTime = new DateTime(fireTime.Year, fireTime.Month, fireTime.Day, MinHour, 0, 0);
			}
			return fireTime;
		}
		#endregion // "Local/OS supported notifications"

		#region "Remote Push Notifications"
		private void RegisterForPushNotifications()
		{
			// Initialize Firebase for push notifications
#if USE_FIREBASE
            try
            {
                FirebaseMessaging.TokenReceived += FirebaseTokenReceived;
                FirebaseMessaging.MessageReceived += FirebaseMessageReceived;
            }
            catch(Exception exc)
            {
                Debug.LogError(string.Format("Exception initializing Firebase in Notification Manager {0}", exc));
            }
#endif
		}

		#endregion // "Remote Push Notifications"
	}
}
