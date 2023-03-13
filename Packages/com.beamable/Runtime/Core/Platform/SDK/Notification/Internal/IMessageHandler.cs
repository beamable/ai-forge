using UnityEngine;

namespace Beamable.Api.Notification.Internal
{
	public interface IMessageHandler<T>
	{
		int ChannelHistory();
		bool ShouldHandleMessage(T message);
		void OnMessageReceived(MonoBehaviour owner, T message);
	}
}
