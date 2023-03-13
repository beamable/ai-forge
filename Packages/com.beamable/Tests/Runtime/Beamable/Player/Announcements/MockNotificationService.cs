using Beamable.Common.Api.Notifications;
using System;
using System.Collections.Generic;

namespace Beamable.Tests.Runtime.Player.Notifications
{
	public class MockNotificationService : INotificationService
	{
		public void Subscribe(string name, Action<object> callback)
		{

		}

		public void Unsubscribe(string name, Action<object> handler)
		{

		}

		public void UnsubscribeAll(string name)
		{

		}

		public void Subscribe<T>(string name, Action<T> handler)
		{
			throw new NotImplementedException();
		}

		public void Unsubscribe<T>(string name, Action<T> handler)
		{
			throw new NotImplementedException();
		}

		public void Publish(string name, object payload)
		{

		}

		public void Pause(string name)
		{

		}

		public void Resume(string name)
		{

		}


		public void CreateNotificationChannel(string id, string name, string description)
		{

		}

		public void ScheduleLocalNotification(string channel,
											  string key,
											  int trackingId,
											  string title,
											  string message,
											  TimeSpan timeFromNow,
											  bool restrictTime,
											  Dictionary<string, string> customData = null)
		{
		}

		public void RegisterForNotifications()
		{
		}
	}
}
