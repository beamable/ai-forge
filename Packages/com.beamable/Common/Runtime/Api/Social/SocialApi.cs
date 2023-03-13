using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections.Generic;

namespace Beamable.Common.Api.Social
{
	/// <summary>
	/// This type defines the %Client main entry point for the %Friends feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/social-networking">Social</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class SocialApi : ISocialApi
	{
		public IBeamableRequester Requester { get; }
		private IUserContext Ctx { get; }

		private Promise<SocialList> _socialList;

		public SocialApi(IUserContext ctx, IBeamableRequester requester)
		{
			Requester = requester;
			Ctx = ctx;
		}

		public Promise<SocialList> Get()
		{
			if (_socialList == null)
			{
				return RefreshSocialList();
			}

			return _socialList;
		}

		public Promise<EmptyResponse> ImportFriends(SocialThirdParty source, string token)
		{
			return Requester.Request<EmptyResponse>(
			   Method.POST,
			   "/basic/social/friends/import",
			   new ImportFriendsRequest { source = source.GetString(), token = token }
			);
		}

		public Promise<FriendStatus> BlockPlayer(long playerId)
		{
			return Requester.Request<FriendStatus>(
			   Method.POST,
			   "/basic/social/blocked",
			   new PlayerIdRequest { playerId = playerId.ToString() }
			);
		}

		public Promise<FriendStatus> UnblockPlayer(long playerId)
		{
			return Requester.Request<FriendStatus>(
			   Method.DELETE,
			   "/basic/social/blocked",
			   new PlayerIdRequest { playerId = playerId.ToString() }
			);
		}

		public Promise<EmptyResponse> SendFriendRequest(long gamerTag)
		{
			return Requester.Request<EmptyResponse>(
			  Method.POST,
			  "/basic/social/friends/invite",
			  new GamerTagRequest { gamerTag = gamerTag }
		   );
		}

		public Promise<EmptyResponse> AcceptFriendRequest(long gamerTag)
		{
			return Requester.Request<EmptyResponse>(
				Method.POST,
				"/basic/social/friends/make",
				new GamerTagRequest { gamerTag = gamerTag }
			);
		}

		public Promise<EmptyResponse> CancelFriendRequest(long gamerTag)
		{
			return Requester.Request<EmptyResponse>(
			   Method.DELETE,
			   "/basic/social/friends/invite",
			   new GamerTagRequest { gamerTag = gamerTag }
			);
		}

		public Promise<EmptyResponse> RemoveFriend(long playerId)
		{
			return Requester.Request<EmptyResponse>(
			   Method.DELETE,
			   "/basic/social/friends",
			   new PlayerIdRequest { playerId = playerId.ToString() }
			);
		}

		public Promise<SocialList> RefreshSocialList()
		{
			_socialList = Requester.Request<SocialList>(
			   Method.GET,
			   "/basic/social/my"
			);
			return _socialList;
		}
	}

	[Serializable]
	public class PlayerIdRequest
	{
		public string playerId;
	}

	[Serializable]
	public class GamerTagRequest
	{
		public long gamerTag;
	}

	[Serializable]
	public class ImportFriendsRequest
	{
		public string source;
		public string token;
	}


	[Serializable]
	public class SocialList
	{
		/// <summary>
		/// The owner of this social list.
		/// </summary>
		public long PlayerId;

		/// <summary>
		/// A list of the player's <see cref="Friend"/>s.
		/// </summary>
		public List<Friend> friends;

		/// <summary>
		/// A list of the player's blocked <see cref="Player"/>s.
		/// </summary>
		public List<Player> blocked;

		/// <summary>
		/// A list of all pending invites, both <see cref="FriendInviteDirection.Incoming"/> and <see cref="FriendInviteDirection.Outgoing"/>.
		/// </summary>
		public List<FriendInvite> invites;

		/// <summary>
		/// Check if a given gamertag is in the <see cref="blocked"/> list.
		/// </summary>
		/// <param name="dbid">a gamertag</param>
		/// <returns>true if the given gamertag is in the <see cref="blocked"/> list</returns>
		public bool IsBlocked(long dbid)
		{
			return blocked.Find(p => p.playerId == dbid.ToString()) != null;
		}

		/// <summary>
		/// Check if a given gamertag is in the <see cref="friends"/> list.
		/// </summary>
		/// <param name="dbid">a gamertag</param>
		/// <returns>true if the given gamertag is in the <see cref="friends"/> list</returns>
		public bool IsFriend(long dbid)
		{
			return friends.Find(f => f.playerId == dbid.ToString()) != null;
		}

	}

	[Serializable]
	public class Friend
	{
		/// <summary>
		/// The gamertag of this friend
		/// </summary>
		public string playerId;

		/// <summary>
		/// Where the friend was discovered. Use the <see cref="Source"/> property for a type safe source.
		/// </summary>
		public string source;

		/// <summary>
		/// Where the friend was discovered. This value is derived from the <see cref="source"/> field, but should
		/// be Facebook, or Native.
		/// </summary>
		public FriendSource Source => (FriendSource)Enum.Parse(typeof(FriendSource), source, ignoreCase: true);
	}

	[Serializable]
	public class Player
	{
		/// <summary>
		/// The gamertag of this player
		/// </summary>
		public string playerId;
	}

	public enum FriendSource
	{
		Facebook,
		Native
	}

	[Serializable]
	public class FriendStatus
	{
		/// <summary>
		/// true if the current player has blocked this player.
		/// </summary>
		public bool isBlocked;
	}

	[Serializable]
	public class FriendInvite
	{
		/// <summary>
		/// The player that is inviting the authenticated player, when <see cref="Direction"/> is <see cref="FriendInviteDirection.Incoming"/>.
		/// The player that was invited by the authenticated player, when <see cref="Direction"/> is <see cref="FriendInviteDirection.Outgoing"/>.
		/// </summary>
		public long playerId;

		/// <summary>
		/// <see cref="FriendInviteDirection.Incoming"/> means the authenticated player is being invited.
		/// <see cref="FriendInviteDirection.Outgoing"/> means the authenticated player sending an invite to another player.
		/// </summary>
		public string direction;

		public FriendInviteDirection Direction => (FriendInviteDirection)Enum.Parse(typeof(FriendInviteDirection), direction, ignoreCase: true);
	}


	/// <summary>
	/// Type that you can subscribe to receive
	/// </summary>
	public class FriendRequestUpdateNotification
	{
		/// <summary>
		/// Use the <see cref="EventType"/> property for type safe access.
		/// One of the following values:
		/// <list type="bullet">
		/// <item><b>friend<b> => A friend request related to the subscribed player was accepted.</item>
		/// <item><b>cancel-friend-request<b> => A friend request related to the subscribed player was cancelled or declined.</item>
		/// <item><b>unfriend<b> => ?? </item>
		/// <item><b>block<b> => ?? </item>
		/// <item><b>unblock<b> => ?? </item>
		/// <item><b>mute<b> => ?? </item>
		/// <item><b>unmute<b> => ?? </item>
		/// </list>
		/// </summary>
		public string etype;

		/// <summary>
		/// The player that made the friend request.
		/// </summary>
		public long player;

		/// <summary>
		/// The player that received the friend request.
		/// </summary>
		public long friend;

		/// <summary>
		/// The type of event being broadcast
		/// </summary>
		public FriendEventType EventType => GetEventType(etype);

		/// <summary>
		/// Get the event type for some <see cref="etype"/>
		/// </summary>
		/// <param name="eType"></param>
		/// <returns></returns>
		/// <exception cref="Exception">An exception if the event type is unknown</exception>
		public static FriendEventType GetEventType(string eType)
		{
			switch (eType?.ToLower() ?? null)
			{
				case "block": return FriendEventType.Block;
				case "unblock": return FriendEventType.Unblock;
				case "friend": return FriendEventType.Friend;
				case "unfriend": return FriendEventType.Unfriend;
				case "mute": return FriendEventType.Mute;
				case "unmute": return FriendEventType.Unmute;
				case "cancel-friend-request": return FriendEventType.CancelFriendRequest;
				default:
					throw new Exception($"Unknown friend event type. etype=[{eType}]");
			}
		}
	}

	public enum FriendInviteDirection { Incoming, Outgoing }

	public enum FriendEventType
	{
		Friend, Unfriend, Block, Unblock, Mute, Unmute, CancelFriendRequest
	}

	public enum SocialThirdParty
	{
		Facebook
	}

	public static class SocialThirdPartyMethods
	{
		public static string GetString(this SocialThirdParty thirdParty)
		{
			switch (thirdParty)
			{
				case SocialThirdParty.Facebook:
					return "facebook";
				default:
					return null;
			}
		}
	}
}
