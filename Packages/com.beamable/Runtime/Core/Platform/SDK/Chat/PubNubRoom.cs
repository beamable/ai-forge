using Beamable.Api;
using Beamable.Api.Notification;
using Beamable.Common;
using Beamable.Common.Api.Notifications;
using Beamable.Common.Dependencies;
using Beamable.Common.Spew;
using Beamable.Serialization;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;

namespace Beamable.Experimental.Api.Chat
{
	using Promise = Promise<Unit>;

	public class PubNubRoom : Room
	{
		private readonly IDependencyProvider _provider;
		private const string ChatEvent = "CHAT.RECEIVED";
		private bool _isSubscribed;

		private IPubnubSubscriptionManager PubnubSubscriptionManager =>
			_provider.GetService<IPubnubSubscriptionManager>();

		private ChatService ChatService => _provider.GetService<ChatService>();
		private INotificationService NotificationService => _provider.GetService<INotificationService>();

		public PubNubRoom(RoomInfo roomInfo, IDependencyProvider provider) : this(roomInfo.id, roomInfo.name, roomInfo.keepSubscribed, provider) { }

		private PubNubRoom(string id, string name, bool keepSubscribed, IDependencyProvider provider) : base(id, name, keepSubscribed, true)
		{
			_provider = provider;
		}

		public override Promise Sync()
		{
			// XXX: This should be a bit smarter in when and/or how often it fetches the history.
			var promise = new Promise();

			PubnubSubscriptionManager.LoadChannelHistory(
			   Id,
			   50,
			   pubnubMessages =>
			   {
				   Messages.Clear();
				   foreach (var message in pubnubMessages)
				   {
					   OnChatEvent(message);
				   }

				   promise.CompleteSuccess(PromiseBase.Unit);
			   },
			   error =>
			   {
				   Debug.LogError(error.Message);
				   promise.CompleteError(new ErrorCode(error.StatusCode));
			   }
			);
			return promise;
		}

		public override Promise<Message> SendMessage(string message)
		{
			return ChatService.SendMessage(Id, message);
		}

		public override Promise<Room> Join(OnMessageReceivedDelegate callback = null)
		{
			var basePromise = base.Join(callback);

			if (_isSubscribed)
			{
				return basePromise;
			}

			var promise = new Promise<Room>();

			basePromise.Then(_ =>
			{
				PubnubSubscriptionManager.EnqueueOperation(new PubNubOp(PubNubOp.PNO.OpSubscribe, Id, () =>
				{
					_isSubscribed = true;
					NotificationService.Subscribe(ChatEvent, OnChatEvent);
					promise.CompleteSuccess(this);
				}), shouldRunNextOp: true);
			});

			return promise;
		}

		public override Promise<Room> Leave()
		{
			var basePromise = base.Leave();

			// Stay in the room if keep subscribe
			if (KeepSubscribed)
			{
				ChatLogger.Log("PubNubRoom: Will not unsubscribe from room marked as 'KeepSubscribed'.");
				return basePromise;
			}

			var promise = new Promise<Room>();

			basePromise.Then(_ =>
			{
				PubnubSubscriptionManager.EnqueueOperation(new PubNubOp(PubNubOp.PNO.OpUnsubscribe, Id, () =>
				{
					_isSubscribed = false;
					NotificationService.Unsubscribe(ChatEvent, OnChatEvent);
					promise.CompleteSuccess(this);
				}), shouldRunNextOp: true);
			}).Error(promise.CompleteError);

			return promise;
		}

		public override Promise<Room> Forget()
		{
			var basePromise = base.Forget();

			return basePromise.FlatMap(baseRsp => ChatService.LeaveRoom(Id).Map<Room>(rsp => this));
		}

		private void OnChatEvent(object payload)
		{
			var result = new Message();
			var data = payload as IDictionary<string, object>;
			JsonSerializable.Deserialize(result, data, JsonSerializable.ListMode.kReplace);

			if (result.roomId == Id && !ContainsMessage(result))
			{
				MessageReceived(result);
			}
		}
	}
}
