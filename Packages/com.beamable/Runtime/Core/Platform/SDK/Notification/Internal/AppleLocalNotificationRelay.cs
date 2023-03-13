#if UNITY_IOS
#pragma warning disable CS0618
using System;
using System.Collections.Generic;
using UnityEngine.iOS;
using Beamable.Common.Spew;

namespace Beamable.Api.Notification.Internal
{
   /// <summary>
   /// Apple local notification relay, for scheduling or cancelling
   /// background notifications that are local to the device.
   /// </summary>
   public class AppleLocalNotificationRelay : ILocalNotificationRelay
   {
      public const string CancellationKey = "NOTIFICATION_KEY";

      public void CreateNotificationChannel(string id, string name, string description)
      {
          NotificationLogger.LogFormat("Create Notification Channel not implemented on this platform.");
      }

      public void ScheduleNotification(string channel, string key, string title, string message, DateTime when, Dictionary<string, string> data)
      {
         // Make certain we haven't reached our maximum.
         if (NotificationServices.scheduledLocalNotifications.Length >= NotificationService.MaxLocalNotifications)
         {
            NotificationLogger.LogFormat("Local Notification Limit of {0} has been reached. Ignoring {1}", NotificationService.MaxLocalNotifications, key);
            return;
         }
         // Unless we cancel previous ones, the device will show multiple notifications.
         CancelNotification(key);
         Dictionary<string, string> userInfo = null;
         if (data != null)
         {
            userInfo = new Dictionary<string, string>(data) {{CancellationKey, key}};
         }
         else
         {
            userInfo = new Dictionary<string, string>() {{CancellationKey, key}};
         }

         var note = new LocalNotification
         {
            alertBody = message,
            alertAction = title,
            fireDate = when,
            userInfo = userInfo
         };
         NotificationServices.ScheduleLocalNotification(note);
      }

      public void CancelNotification(string key)
      {
         var notifications = NotificationServices.scheduledLocalNotifications;
         for (var i = 0; i < notifications.Length; i++)
         {
            var note = notifications[i];
            if (key == (string)note.userInfo[CancellationKey])
            {
               NotificationServices.CancelLocalNotification(note);
            }
         }
      }

      public void ClearDeliveredNotifications()
      {
         //clearAllNotification();
      }

      //[DllImport("__Internal")]
      //private static extern void clearAllNotification();
   }
}
#pragma warning restore CS0618
#endif
