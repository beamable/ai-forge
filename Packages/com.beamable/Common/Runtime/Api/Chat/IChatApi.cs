using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Serialization;
using System;
using System.Collections.Generic;

namespace Beamable.Experimental.Api.Chat
{
	public interface IChatApi
	{
		/// <summary>
		/// Send a message to chat room.
		/// </summary>
		/// <param name="roomId">The room id</param>
		/// <param name="message">The message to send to the room</param>
		/// <returns>A <see cref="Promise"/> containing the sent <see cref="Message"/></returns>
		Promise<Message> SendMessage(string roomId, string message);

		/// <summary>
		/// Get the current player's set of <see cref="RoomInfo"/>.
		/// The player can create a new room using the <see cref="CreateRoom"/> method.
		/// </summary>
		/// <returns>A <see cref="Promise"/> containing the player's <see cref="RoomInfo"/></returns>
		Promise<List<RoomInfo>> GetMyRooms();

		/// <summary>
		/// Creates a new private chat room for the current player, and a set of other players.
		/// </summary>
		/// <param name="roomName">A name for the room</param>
		/// <param name="keepSubscribed">When true, the current player will receive messages for the room.</param>
		/// <param name="players">A list of gamertags of other players who will be included in the chat room.</param>
		/// <returns>A <see cref="Promise"/> containing the newly created <see cref="RoomInfo"/></returns>
		Promise<RoomInfo> CreateRoom(string roomName, bool keepSubscribed, List<long> players);

		/// <summary>
		/// Remove the current player from a room
		/// </summary>
		/// <param name="roomId">The room id to leave</param>
		/// <returns>A <see cref="Promise"/> representing the network call.</returns>
		Promise<EmptyResponse> LeaveRoom(string roomId);

		/// <summary>
		/// Check to see if a piece of text would trigger the Beamable profanity filter.
		/// </summary>
		/// <param name="text">some text</param>
		/// <returns>A <see cref="Promise"/> representing the network call.
		/// If the text contains profanity, the promise will fail with a PlatformRequesterException with an error of "ProfanityFilter" and a status of 400.
		/// </returns>
		Promise<EmptyResponse> ProfanityAssert(string text);
	}

	[Serializable]
	public class Message : JsonSerializable.ISerializable
	{
		/// <summary>
		/// The id of the message
		/// </summary>
		public string messageId;

		/// <summary>
		/// The id of the room where the message was sent
		/// </summary>
		public string roomId;

		/// <summary>
		/// The gamertag of the sender
		/// </summary>
		public long gamerTag;

		/// <summary>
		/// The message body. You should use the <see cref="censoredContent"/> to make sure the subject material is safe.
		/// </summary>
		public string content;

		/// <summary>
		/// The message body, similar to <see cref="content"/>. However, this string goes through a profanity filter on Beamable
		/// to remove unsafe material.
		/// </summary>
		public string censoredContent;

		/// <summary>
		/// The timestamp that this message was created.
		/// Number of milliseconds since 1970-01-01T00:00:00Z.
		/// </summary>
		public long timestampMillis;

		/// <summary>
		/// The <see cref="MessageType"/> of the message.
		/// </summary>
		public MessageType Type
		{
			get { return gamerTag == 0 ? MessageType.Admin : MessageType.User; }
		}

		public void Serialize(JsonSerializable.IStreamSerializer s)
		{
			s.Serialize("messageId", ref messageId);
			s.Serialize("roomId", ref roomId);
			s.Serialize("gamerTag", ref gamerTag);
			s.Serialize("content", ref content);
			s.Serialize("censoredContent", ref censoredContent);
			s.Serialize("timestampMillis", ref timestampMillis);
		}
	}

	public enum MessageType
	{
		Admin,
		User
	}

	[Serializable]
	public class SendChatRequest
	{
		public string roomId;
		public string content;

		public SendChatRequest(string roomId, string content)
		{
			this.roomId = roomId;
			this.content = content;
		}
	}

	[Serializable]
	public class SendChatResponse
	{
		public Message message;
	}

	[Serializable]
	public class GetMyRoomsResponse
	{
		public List<RoomInfo> rooms;
	}

	[Serializable]
	public class CreateRoomRequest
	{
		public string roomName;
		public bool keepSubscribed;
		public List<long> players;

		public CreateRoomRequest(string roomName, bool keepSubscribed, List<long> players)
		{
			this.roomName = roomName;
			this.keepSubscribed = keepSubscribed;
			this.players = players;
		}
	}

	[Serializable]
	public class CreateRoomResponse
	{
		public RoomInfo room;
	}

	[Serializable]
	public class RoomInfo
	{
		/// <summary>
		/// The id of the room
		/// </summary>
		public string id;

		/// <summary>
		/// The name of the room
		/// </summary>
		public string name;

		/// <summary>
		/// When true, the current player will receive messages from the room
		/// </summary>
		public bool keepSubscribed;

		/// <summary>
		/// A list of gamertags who are in the room
		/// </summary>
		public List<long> players;
	}
}
