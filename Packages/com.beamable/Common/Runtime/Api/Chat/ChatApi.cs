using Beamable.Common;
using Beamable.Common.Api;
using System.Collections.Generic;

namespace Beamable.Experimental.Api.Chat
{
	public class ChatApi : IChatApi
	{
		protected const string BaseUri = "/object/chatV2";
		private readonly IBeamableRequester _requester;
		private readonly IUserContext _userContext;

		public ChatApi(IBeamableRequester requester, IUserContext userContext)
		{
			_requester = requester;
			_userContext = userContext;
		}

		/// <inheritdoc cref="IChatApi.SendMessage"/>
		public Promise<Message> SendMessage(string roomId, string message)
		{
			return _requester.Request<SendChatResponse>(
				Method.POST,
				string.Format("{0}/{1}/messages", BaseUri, _userContext.UserId),
				new SendChatRequest(roomId, message)
			).Map(response => response.message);
		}

		/// <inheritdoc cref="IChatApi.GetMyRooms"/>
		public Promise<List<RoomInfo>> GetMyRooms()
		{
			return _requester.Request<GetMyRoomsResponse>(
				Method.GET,
				string.Format("{0}/{1}/rooms", BaseUri, _userContext.UserId)
			).Map(response => response.rooms);
		}

		/// <inheritdoc cref="IChatApi.CreateRoom"/>
		public Promise<RoomInfo> CreateRoom(string roomName, bool keepSubscribed, List<long> players)
		{
			return _requester.Request<CreateRoomResponse>(
				Method.POST,
				string.Format("{0}/{1}/rooms", BaseUri, _userContext.UserId),
				new CreateRoomRequest(roomName, keepSubscribed, players)
			).Map(response => response.room);
		}

		/// <inheritdoc cref="IChatApi.LeaveRoom"/>
		public Promise<EmptyResponse> LeaveRoom(string roomId) => PlayerLeaveRoom(_userContext.UserId, roomId);

		/// <inheritdoc cref="IChatApi.ProfanityAssert"/>
		public Promise<EmptyResponse> ProfanityAssert(string text)
		{
			return _requester.Request<EmptyResponse>(
				Method.GET,
				$"/basic/chat/profanityAssert?text={text}"
			);
		}

		protected Promise<EmptyResponse> PlayerLeaveRoom(long gamerTag, string roomId)
		{
			return _requester.Request<EmptyResponse>(
				Method.DELETE,
				string.Format("{0}/{1}/rooms?roomId={2}", BaseUri, gamerTag, roomId)
			);
		}
	}
}
