using Beamable.Common;
using System.Collections.Generic;

namespace Beamable.Experimental.Api.Chat
{
	using Promise = Promise<Unit>;

	public abstract class Room
	{
		public readonly string Id;
		public readonly string Name;
		public readonly bool KeepSubscribed;
		public readonly List<Message> Messages;

		private bool _filterMessagesFromBlockedPlayers;

		public bool FilterMessagesFromBlockedPlayers
		{
			get { return _filterMessagesFromBlockedPlayers; }
			set { _filterMessagesFromBlockedPlayers = value; }
		}

		private OnMessageReceivedDelegate _onMessageReceived;

		protected Room(string id, string name, bool keepSubscribed, bool filterFromBlocked)
		{
			Id = id;
			Name = name;
			KeepSubscribed = keepSubscribed;
			Messages = new List<Message>();

			FilterMessagesFromBlockedPlayers = filterFromBlocked;
		}

		public bool ContainsMessage(Message message)
		{
			// XXX: We probably want to do something smarter to check if we've already seen a message with a particular
			// ID.
			return Messages.Exists(m => m.messageId == message.messageId);
		}

		/// <summary>
		/// Synchronize the local representation of the room to the server state.
		/// </summary>
		public abstract Promise Sync();

		/// <summary>
		/// Send a message to this room.
		/// </summary>
		/// <param name="message">Content of the message.</param>
		public abstract Promise<Message> SendMessage(string message);

		/// <summary>
		/// Join this room. After this call is successful, this player should start receiving messages for this room.
		/// <param name="callback">Associated callback when a message is received for this room.</param>
		/// </summary>
		public virtual Promise<Room> Join(OnMessageReceivedDelegate callback = null)
		{
			_onMessageReceived = callback;
			return Promise<Room>.Successful(this);
		}

		/// <summary>
		/// Leave this room. After this call is successful, this player no longer receive messages for this room.
		/// </summary>
		public virtual Promise<Room> Leave()
		{
			_onMessageReceived = null;
			return Promise<Room>.Successful(this);
		}

		/// <summary>
		/// Forget this room. This leaves the room and also removes it from saved rooms so it is no longer known
		/// </summary>
		public virtual Promise<Room> Forget()
		{
			return Leave();
		}

		/// <summary>
		/// Adds a new message to this room. This will not invoke the _onMessageReceived callback.
		/// <param name="message">The message to add to this room.</param>
		/// </summary>
		protected void MessageSent(Message message)
		{
			Messages.Add(message);
		}

		/// <summary>
		/// Adds a new message to this room. Will also invoke the _onMessageReceived callback if one is defined.
		/// <param name="message">The message to add to this room.</param>
		/// </summary>
		public void MessageReceived(Message message)
		{
			if (_filterMessagesFromBlockedPlayers)
			{
				MessageReceivedWithBlockFilter(message);
			}
			else
			{
				AddMessageAndInvokeCallback(message);
			}
		}

		private void MessageReceivedWithBlockFilter(Message message)
		{
			// XXX: Blockfilters don't make sense until we add back some sort of Social Apis.
			AddMessageAndInvokeCallback(message);
		}

		private void AddMessageAndInvokeCallback(Message message)
		{
			Messages.Add(message);

			if (_onMessageReceived != null)
			{
				_onMessageReceived(message);
			}
		}
	}
}
