using Beamable.Pooling;
using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Api.Notification.Internal
{
	/// <summary>
	/// Google local notification relay, for scheduling and canceling
	/// background notifications that are local to the device.
	/// </summary>
	public class GoogleLocalNotificationRelay : ILocalNotificationRelay
	{
		/* Instance of LocalNotificationScheduler to use for handling notifications. */
		private readonly AndroidJavaObject _scheduler;

		public GoogleLocalNotificationRelay()
		{
			var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			var currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
			_scheduler = new AndroidJavaObject("com.disruptorbeam.LocalNotificationScheduler", currentActivity);
		}

		public void CreateNotificationChannel(string id, string name, string description)
		{
			try
			{
				_scheduler.Call("createNotificationChannel", id, name, description);
			}
			catch (Exception e)
			{
				Debug.LogErrorFormat("Failed to create notification channel: {0}", e);
			}
		}

		public void ScheduleNotification(string channel, string key, string title, string message, DateTime when, Dictionary<string, string> data)
		{
			try
			{
				using (var pb = StringBuilderPool.StaticPool.Spawn())
				{
					var delay = (long)(when - DateTime.Now).TotalMilliseconds;
					string jsonData = Json.Serialize(data, pb.Builder);
					_scheduler.Call("scheduleNotification", channel, GetId(key), title, message, delay, jsonData);
				}
			}
			catch (Exception e)
			{
				Debug.LogErrorFormat("Local notification scheduling failed: {0}", e);
			}
		}

		public void CancelNotification(string key)
		{
			try
			{
				_scheduler.Call("cancelNotification", GetId(key));
			}
			catch (Exception e)
			{
				Debug.LogErrorFormat("Local notification canceling failed: {0}", e);
			}
		}

		public void ClearDeliveredNotifications()
		{
			try
			{
				_scheduler.Call("cancelAll");
			}
			catch (Exception e)
			{
				Debug.LogErrorFormat("Local notification canceling failed: {0}", e);
			}
		}

		/// <summary>
		/// Produce a numeric ID for a message, based on its string key.
		/// </summary>
		private static int GetId(string key)
		{
			return key.GetHashCode();
		}
	}
}
