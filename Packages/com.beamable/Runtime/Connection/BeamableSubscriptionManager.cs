using Beamable.Common.Api.Notifications;
using Beamable.Serialization.SmallerJSON;
using UnityEngine;

namespace Beamable.Connection
{
	public class BeamableSubscriptionManager
	{
		private readonly INotificationService _notificationService;

		public BeamableSubscriptionManager(IBeamableConnection connection, INotificationService notificationService)
		{
			_notificationService = notificationService;

			connection.Message += HandleMessage;
		}

		private void HandleMessage(string message)
		{
			// XXX: This merely mimics what we had in the initial pubnub implementation. Eventually this should be made
			// a lot smarter and more type safe.
			var deserialized = (ArrayDict)Json.Deserialize(message);
			bool hasContext = deserialized.TryGetValue("context", out object context);
			bool hasMessage = deserialized.TryGetValue("messageFull", out object messageFull);

			bool isValidMessage = hasContext && hasMessage;
			if (!isValidMessage)
			{
				Debug.LogWarning("Unable to handle incoming notification");
				return;
			}

			// Invoke notification service
			object parsedPayload = Json.Deserialize(messageFull as string);
			_notificationService.Publish(context as string, parsedPayload ?? messageFull);
		}
	}
}
