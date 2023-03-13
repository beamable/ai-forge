using Beamable.Common;
using Beamable.Common.Api.Notifications;
using Beamable.Common.Dependencies;
using Beamable.Common.Spew;
using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Beamable.Experimental.Api.Chat
{

	[Obsolete("Use ChatService instead")]
	public abstract class ChatProvider : MonoBehaviour
	{
		protected IDependencyProvider Provider;

		protected INotificationService NotificationService => Provider.GetService<INotificationService>();

		private readonly List<Room> _rooms = new List<Room>();
		private OnRoomAddedDelegate _onRoomAdded;

		/// <summary>
		/// Return the full list of rooms this player has access to.
		/// </summary>
		public List<Room> MyRooms
		{
			get { return _rooms; }
		}

		/// <summary>
		/// Return the general room for this game.
		/// </summary>
		public Room GeneralRoom
		{
			get { return MyRooms.Find(room => room.Name.StartsWith("general")); }
		}

		/// <summary>
		/// Return the guild room if the player is in a guild. Otherwise, this will return null.
		/// </summary>
		public Room GuildRoom
		{
			get { return MyRooms.Find(room => room.Name.StartsWith("group")); }
		}

		/// <summary>
		/// Return this player's direct messages.
		/// </summary>
		public List<Room> DirectMessageRooms
		{
			get { return MyRooms.FindAll(room => room.Name.StartsWith("direct")); }
		}

		/// <summary>
		/// Return all of the rooms the client should join immediately (and never leave).
		/// </summary>
		public List<Room> AlwaysSubscribedRooms
		{
			get { return MyRooms.FindAll(room => room.KeepSubscribed); }
		}

		/// <summary>
		/// Initializes the ChatProvider. This should connect to the service and populate the list of rooms accessible
		/// to the user.
		/// </summary>
		public void Initialize(IDependencyProvider provider)
		{
			Provider = provider;

			Connect()
			   .FlatMap(_ => FetchAndUpdateRooms())
			   .Then(joinedPrivateRooms =>
			   {
				   ChatLogger.LogFormat("ChatManager: Player has access to {0} rooms.", MyRooms.Count);
				   ChatLogger.LogFormat("ChatManager: Player joined {0} private rooms.", joinedPrivateRooms.Count);

				   // Subscribe for re-syncs
				   var ntfService = NotificationService;
				   ntfService.Subscribe(
				   "GROUP.MEMBERSHIP",
				   info => { FetchAndUpdateRooms(); }
				);

				   ntfService.Subscribe(
				   "CHAT.ROOM_CREATED",
				   info => { FetchAndUpdateRooms(); }
				);

				   ntfService.Subscribe(
				   "SOCIAL.UPDATE",
				   info => { FetchAndUpdateRooms(); }
				);
			   })
			   .Error(Debug.LogError);
		}

		public Promise<List<Room>> FetchAndUpdateRooms()
		{
			return FetchMyRooms().Then(rooms =>
			   {
				   for (int i = 0; i < _rooms.Count; i++)
				   {
					   _rooms[i].Leave();
				   }

				   _rooms.Clear();
				   _rooms.AddRange(rooms);
			   })
			   // We want to immediately join the AlwaysSubscribedRooms. This will allow us to show toasts for
			   // guild messages or direct messages if we wish. Other rooms we'll join only when the chat panel opens.
			   .FlatMap(_ => JoinRooms(AlwaysSubscribedRooms));
		}

		public void AddOnRoomAdded(OnRoomAddedDelegate callback)
		{
			_onRoomAdded = callback;
		}

		/// <summary>
		/// Replaces :shortcode: emoji instances with TMP rich sprite tags.
		/// </summary>
		/// <param name="original">The unaltered string</param>
		public string SubstituteEmoji(string original)
		{
			return original;
		}

		protected void AddRoom(Room room)
		{
			if (MyRooms.Exists(r => r.Id == room.Id))
			{
				return;
			}

			MyRooms.Add(room);
			if (_onRoomAdded != null)
			{
				_onRoomAdded(room);
			}
		}

		private static Promise<List<Room>> JoinRooms(List<Room> rooms)
		{
			var promisedRooms = new List<Promise<Room>>();

			foreach (var room in rooms)
			{
				promisedRooms.Add(room.Join());
			}

			return Promise.Sequence(promisedRooms);
		}

		protected string CreateRoomNameFromGamerTags(IEnumerable<long> gamerTags)
		{
			var playersForName = new List<string>();

			foreach (var gamerTag in gamerTags)
			{
				playersForName.Add(gamerTag.ToString());
			}

			playersForName.Sort();

			return string.Format("direct-{0}", string.Join("-", playersForName.ToArray()));
		}

		/// <summary>
		/// Ask the provider to create a private room consisting of the provided list of players.
		/// </summary>
		/// <param name="gamerTags">GamerTags of players who should be in the room.</param>
		public abstract Promise<Room> CreatePrivateRoom(List<long> gamerTags);

		/// <summary>
		/// Create a connection to the chat provider. This will be called when the ChatManager is initialized. The
		/// rest of the methods in this interface depend on a successfully connected player.
		/// </summary>
		protected abstract Promise<Unit> Connect();

		/// <summary>
		/// Return the list of rooms the currently connected player has access to.
		/// </summary>
		protected abstract Promise<List<Room>> FetchMyRooms();
	}

	public delegate void OnRoomAddedDelegate(Room room);

	public delegate void OnMessageReceivedDelegate(Message message);
}
