using Beamable.Api;
using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections.Generic;

namespace Beamable.Common.Api.Presence
{
	public class PresenceService : IPresenceApi
	{
		private readonly IRequester _requester;
		private readonly IUserContext _userContext;

		public PresenceService(IRequester requester, IUserContext userContext)
		{
			_requester = requester;
			_userContext = userContext;
		}

		public Promise<EmptyResponse> SendHeartbeat()
		{
			return ForceCheck().Map(_ => EmptyResponse.Unit);
		}

		public Promise<PlayerPresence> GetPlayerPresence(long playerId)
		{
			return _requester.Request<PlayerPresence>(
				Method.GET,
				$"/players/{playerId}/presence"
			);
		}

		public Promise<EmptyResponse> SetPlayerStatus(PresenceStatus status, string description)
		{
			string json = $"{{ \"status\": {(int)status}, \"description\": \"{description}\" }}";

			return _requester.Request<EmptyResponse>(
				Method.PUT,
				$"/players/{_userContext.UserId}/presence/status",
				json
			);
		}

		public Promise<MultiplePlayersStatus> GetManyStatuses(params long[] playerIds)
		{
			string[] ids = Array.ConvertAll(playerIds, id => id.ToString());
			return GetManyStatuses(ids);
		}

		public Promise<MultiplePlayersStatus> GetManyStatuses(params string[] playerIds)
		{
			ArrayDict dict = new ArrayDict { { "playerIds", playerIds } };
			string json = Json.Serialize(dict, null);

			return _requester.Request<MultiplePlayersStatus>(
				Method.POST,
				"/presence/query",
				json
			);
		}

		public bool ConnectivityCheckingEnabled { get; set; }
		public Promise<bool> ForceCheck()
		{
			/*
			 * if the ConnectivityCheckingEnabled is enabled, then we DON'T want the request
			 * to include the pre-check. But if the ConnectivityCheckingEnabled is disabled,
			 * then we should include the pre-check.
			 */
			var useConnectivityPreCheck = !ConnectivityCheckingEnabled;

			return _requester.BeamableRequest(new SDKRequesterOptions<EmptyResponse>
			{
				method = Method.PUT,
				uri = $"/players/{_userContext.UserId}/presence",
				includeAuthHeader = true,
				useConnectivityPreCheck =
					useConnectivityPreCheck // the magic sauce to allow this to ignore the connectivity
			})
			 .Map(_ => true)
			 .RecoverFromNoConnectivity(() => false); // if no connection happens, that is fine, just carry on.
		}
	}

	[Serializable]
	public class PlayerPresence
	{
		public bool online;
		public string lastOnline;
		public long playerId;
		public string status;
		public string description;

		public PresenceStatus Status => (PresenceStatus)Enum.Parse(typeof(PresenceStatus), status);
		public DateTime LastOnline => DateTime.Parse(lastOnline);

		#region auto-generated-equality
		protected bool Equals(PlayerPresence other)
		{
			return online == other.online && lastOnline == other.lastOnline && playerId == other.playerId && status == other.status && description == other.description;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != this.GetType())
			{
				return false;
			}

			return Equals((PlayerPresence)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = online.GetHashCode();
				hashCode = (hashCode * 397) ^ (lastOnline != null ? lastOnline.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ playerId.GetHashCode();
				hashCode = (hashCode * 397) ^ (status != null ? status.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (description != null ? description.GetHashCode() : 0);
				return hashCode;
			}
		}
		#endregion
	}

	public enum PresenceStatus
	{
		Visible = 0,
		Invisible = 1,
		Dnd = 2,
		Away = 3,
	}

	[Serializable]
	public class MultiplePlayersStatus
	{
		public List<PlayerPresence> playersStatus;
	}

	[Serializable]
	public class FriendStatusChangedNotification
	{
		// TODO: [TD000007] Change those fields according to the new notification structure
		public long friendId;
		public string onlineStatus;
		public string lastOnline;
		public string description;

		public DateTime LastOnline => DateTime.Parse(lastOnline);
	}
}
