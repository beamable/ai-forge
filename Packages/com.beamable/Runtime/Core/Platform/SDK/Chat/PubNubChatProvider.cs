using Beamable.Api;
using Beamable.Common;
using System;
using System.Collections.Generic;

namespace Beamable.Experimental.Api.Chat
{
	using Promise = Promise<Unit>;

	[Obsolete("Use ChatService instead")]
	public class PubNubChatProvider : ChatProvider
	{
		protected ChatService Chat => Provider.GetService<ChatService>();

		public override Promise<Room> CreatePrivateRoom(List<long> gamerTags)
		{
			var userId = Provider.GetService<IPlatformService>().UserId;
			var gamerTagsPlusMe = new List<long> { userId };
			gamerTagsPlusMe.AddRange(gamerTags);

			var roomName = CreateRoomNameFromGamerTags(gamerTagsPlusMe);
			return Chat.CreateRoom(roomName, true, gamerTagsPlusMe).Map<Room>(roomInfo =>
			{
				var room = new PubNubRoom(roomInfo, Provider);
				AddRoom(room);
				return room;
			});
		}

		protected override Promise Connect()
		{
			// The game will already be connected to PubNub by the time that this chat provider is initialized.
			return Promise.Successful(Promise.Unit);
		}

		protected override Promise<List<Room>> FetchMyRooms()
		{
			return Chat.GetMyRooms()
			   .Map(roomInfos =>
			   {
				   var rooms = new List<Room>();
				   foreach (var roomInfo in roomInfos)
				   {
					   rooms.Add(new PubNubRoom(roomInfo, Provider));
				   }

				   return rooms;
			   });
		}
	}
}
