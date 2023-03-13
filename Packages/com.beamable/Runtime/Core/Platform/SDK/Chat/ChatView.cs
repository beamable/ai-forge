using Beamable.Api;
using Beamable.Api.Notification;
using Beamable.Common;
using Beamable.Common.Api.Notifications;
using Beamable.Common.Dependencies;
using Beamable.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Experimental.Api.Chat
{
	[Serializable]
	public class ChatView
	{
		/// <summary>
		/// A list of <see cref="RoomHandle"/>s that represent the rooms the current player is in.
		/// </summary>
		public readonly List<RoomHandle> roomHandles;

		/// <summary>
		/// The first <see cref="RoomHandle"/> from the <see cref="roomHandles"/> list that has a name of "general"
		/// </summary>
		public RoomHandle GeneralRoom
		{
			get;
			private set;
		}

		/// <summary>
		/// A set of <see cref="RoomHandle"/> from the <see cref="roomHandles"/> list that have names that start with "group"
		/// </summary>
		public List<RoomHandle> GuildRooms
		{
			get;
			private set;
		}

		/// <summary>
		/// A set of <see cref="RoomHandle"/> from the <see cref="roomHandles"/> list that have names that start with "direct"
		/// </summary>
		public List<RoomHandle> DirectMessageRooms
		{
			get;
			private set;
		}

		public ChatView()
		{
			this.roomHandles = new List<RoomHandle>();
		}

		public void Update(List<RoomInfo> roomInfo, IDependencyProvider dependencyProvider)
		{
			HashSet<string> remove = new HashSet<string>();
			foreach (var handle in roomHandles)
			{
				var room = roomInfo.Find(info => info.id == handle.Id);
				if (room == null)
				{
					remove.Add(handle.Id);
					handle.Terminate();
				}
			}
			roomHandles.RemoveAll(handle => remove.Contains(handle.Id));

			foreach (var info in roomInfo)
			{
				var room = roomHandles.Find(handle => handle.Id == info.id);
				if (room == null)
				{
					roomHandles.Add(new RoomHandle(info, dependencyProvider));
				}
			}

			GeneralRoom = roomHandles.Find(room => room.Name.StartsWith("general"));
			GuildRooms = roomHandles.FindAll(room => room.Name.StartsWith("group"));
			DirectMessageRooms = roomHandles.FindAll(room => room.Name.StartsWith("direct"));
		}
	}

	[Serializable]
	public class RoomHandle
	{
		private readonly IDependencyProvider _dependencyProvider;
		private const string ChatEvent = "CHAT.RECEIVED";

		/// <summary>
		/// The room id
		/// </summary>
		public readonly string Id;

		/// <summary>
		/// The name of the room
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// When true, the player will get message updates in real time for this room
		/// </summary>
		public readonly bool KeepSubscribed;

		/// <summary>
		/// A list of gamertags that are in the room.
		/// The list may be null if there are no tracked players.
		/// </summary>
		public readonly List<long> Players;

		/// <summary>
		/// A list of <see cref="Message"/>s that are in the room.
		/// Use the <see cref="OnMessageReceived"/> to see messages as they appear.
		/// </summary>
		public readonly List<Message> Messages;

		/// <summary>
		/// True when there are tracked players in the room
		/// </summary>
		public bool ShowPlayerList => Players == null;

		/// <summary>
		/// True when the <see cref="Subscribe"/> call has completed.
		/// </summary>
		public bool IsSubscribed
		{
			get
			{
				if (_subscribe == null)
					return false;
				else
					return _subscribe.IsCompleted;
			}
		}

		/// <summary>
		/// An event that will trigger after the <see cref="Terminate"/> method has been called.
		/// This will not remove the player from the room.
		/// </summary>
		public Action OnRemoved;

		/// <summary>
		/// An event that triggers anytime a new <see cref="Message"/> is received for the room.
		/// This event will only trigger when <see cref="IsSubscribed"/> is true.
		/// </summary>
		public Action<Message> OnMessageReceived;

		private Promise<Unit> _subscribe;

		private ChatService ChatService => _dependencyProvider.GetService<ChatService>();
		private IPubnubSubscriptionManager Pubnub => _dependencyProvider.GetService<IPubnubSubscriptionManager>();
		private INotificationService NotificationService => _dependencyProvider.GetService<INotificationService>();

		public RoomHandle(RoomInfo room, IDependencyProvider dependencyProvider)
		{
			_dependencyProvider = dependencyProvider;
			this.Id = room.id;
			this.Name = room.name;
			this.KeepSubscribed = room.keepSubscribed;
			this.Players = room.players;
			this.Messages = new List<Message>();

			if (KeepSubscribed)
			{
				Subscribe();
			}
		}

		/// <summary>
		/// Subscribe to the room to get a live feed of <see cref="Message"/> with the <see cref="Messages"/> list.
		/// The room will automatically subscribe if the <see cref="KeepSubscribed"/> property is true.
		/// </summary>
		/// <returns>A <see cref="Promise"/> representing when the subscription has been initialized.</returns>
		public Promise<Unit> Subscribe()
		{
			if (_subscribe != null)
			{
				return _subscribe;
			}

			var promise = new Promise<Unit>();
			_subscribe = promise.FlatMap(_ =>
			{
				return LoadHistory();
			});


			Pubnub.EnqueueOperation(new PubNubOp(PubNubOp.PNO.OpSubscribe, Id, () =>
			{
				NotificationService.Subscribe(ChatEvent, OnChatEvent);
				promise.CompleteSuccess(PromiseBase.Unit);
			}), shouldRunNextOp: true);

			return _subscribe;
		}

		/// <summary>
		/// Stop listening for new messages in this room.
		/// This will not leave the room.
		/// </summary>
		/// <returns></returns>
		public Promise<Unit> Unsubscribe()
		{
			var promise = new Promise<Unit>();

			Pubnub.EnqueueOperation(new PubNubOp(PubNubOp.PNO.OpUnsubscribe, Id, () =>
			{
				_subscribe = null;
				NotificationService.Unsubscribe(ChatEvent, OnChatEvent);
				promise.CompleteSuccess(PromiseBase.Unit);
			}), shouldRunNextOp: true);

			return promise;
		}

		/// <summary>
		/// Remove the current player from this room.
		/// </summary>
		/// <returns>A <see cref="Promise"/> representing the network call.</returns>
		public Promise<Unit> LeaveRoom()
		{
			return ChatService.LeaveRoom(Id).Map(_ => PromiseBase.Unit);
		}

		/// <summary>
		/// Send a message to this room.
		/// </summary>
		/// <param name="message">The message body</param>
		/// <returns>A <see cref="Promise"/> representing the network call.</returns>
		public Promise<Unit> SendMessage(string message)
		{
			return ChatService.SendMessage(Id, message).Map(_ => PromiseBase.Unit);
		}

		/// <summary>
		/// <inheritdoc cref="Unsubscribe"/>
		/// Triggers the <see cref="OnRemoved"/> event.
		/// </summary>
		public void Terminate()
		{
			Unsubscribe().Then(_ =>
			{
				OnRemoved?.Invoke();
			});
		}

		private Promise<Unit> LoadHistory()
		{
			var promise = new Promise<Unit>();
			Pubnub.LoadChannelHistory(Id, 50,
			   pubnubMessages =>
			   {
				   Messages.Clear();
				   foreach (var message in pubnubMessages)
				   {
					   Messages.Add(ToMessage(message));
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

		private void OnChatEvent(object payload)
		{
			var message = ToMessage(payload);
			if (message.roomId == Id)
			{
				bool foundMessage = Messages.Exists(m => m.messageId == message.messageId);
				if (!foundMessage)
				{
					Messages.Add(message);
					OnMessageReceived?.Invoke(message);
				}
			}
		}

		private Message ToMessage(object payload)
		{
			var result = new Message();
			var data = payload as IDictionary<string, object>;
			JsonSerializable.Deserialize(result, data, JsonSerializable.ListMode.kReplace);

			return result;
		}
	}
}
