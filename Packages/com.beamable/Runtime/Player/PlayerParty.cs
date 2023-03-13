using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Notifications;
using Beamable.Common.Player;
using Beamable.Experimental.Api.Parties;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Player
{
	/// <summary>
	/// Experimental API around managing a player's party state.
	/// </summary>
	[Serializable]
	public class PlayerParty : DefaultObservable
	{
		public Action<PartyInviteNotification> OnPlayerInvited;
		public Action<PlayerJoinedNotification> OnPlayerJoined;
		public Action<PlayerLeftNotification> OnPlayerLeft;
		public Action<PartyUpdatedNotification> OnPartyUpdated;
		public Action<PlayerPromotedNotification> OnPlayerPromoted;
		public Action<PlayerKickedNotification> OnPlayerKicked;

		private readonly IPartyApi _partyApi;
		private readonly INotificationService _notificationService;
		private readonly IUserContext _userContext;

		[SerializeField]
		private Party _state;

		/// <summary>
		/// This promise will complete when the first party data has arrived
		/// </summary>
		public Promise OnReady { get; private set; }

		public PlayerParty(IPartyApi partyApi, INotificationService notificationService, IUserContext userContext)
		{
			_partyApi = partyApi;
			_notificationService = notificationService;
			_userContext = userContext;
#pragma warning disable CS0618
			Members = new ObservableReadonlyList<string>(RefreshMembersList);
#pragma warning restore CS0618
			PartyMembers = new PartyMemberList(MembersListRefresh);
			ReceivedPartyInvites = new ReceivedPartyInviteList(ReceivedInviteListRefresh);
			_notificationService.Subscribe(PlayerInvitedName(), (Action<PartyInviteNotification>)PlayerInvited);

			OnReady = Refresh();
		}

		private Promise<List<PartyMember>> MembersListRefresh()
		{
			var friends = new List<PartyMember>(_state?.members?.Count ?? 0);
			if (_state?.members != null)
			{
				foreach (var friend in _state.members)
				{
					friends.Add(new PartyMember(this) { playerId = long.Parse(friend) });
				}
			}

			return Promise<List<PartyMember>>.Successful(friends);
		}

		private async Promise<List<ReceivedPartyInvite>> ReceivedInviteListRefresh()
		{
			var res = await _partyApi.GetPartyInvites();
			var invites = new List<ReceivedPartyInvite>(res?.invitations?.Count ?? 0);
			if (res?.invitations != null)
			{
				foreach (var invite in res.invitations)
				{
					invites.Add(new ReceivedPartyInvite(this)
					{
						partyId = invite.partyId,
						invitedBy = long.Parse(invite.invitedBy)
					});
				}
			}

			return invites;
		}

		private static string PlayersLeftName(string partyId) => $"party.players_left.{partyId}";
		private static string PlayersJoinedName(string partyId) => $"party.players_joined.{partyId}";
		private static string PlayerInvitedName() => "party.player_invited";
		private static string PartyUpdatedName(string partyId) => $"party.updated.{partyId}";
		private static string PlayerPromotedName(string partyId) => $"party.player_promoted_to_leader.{partyId}";
		private static string PlayerKickedName(string partyId) => $"party.player_kicked.{partyId}";

		[Obsolete("Use" + nameof(Refresh) + " instead")]
		public Promise PerformRefresh => Refresh();

		/// <summary>
		/// Refresh the state of the social service
		/// </summary>
		public async Promise Refresh()
		{
			if (!IsInParty) return;

			State = await _partyApi.GetParty(State.id);
			await RefreshMembersFromState();
			await ReceivedPartyInvites.Refresh();

			TriggerUpdate();
		}

		public override int GetBroadcastChecksum()
		{
			return (
					ReceivedPartyInvites.GetBroadcastChecksum(),
					PartyMembers.GetBroadcastChecksum(),
					State.GetBroadcastChecksum())
				.GetHashCode();
		}

		private async void PlayerJoined(PlayerJoinedNotification notification)
		{
			await Refresh();
			OnPlayerJoined?.Invoke(notification);
		}

		private async void PlayerLeft(PlayerLeftNotification notification)
		{
			await Refresh();
			OnPlayerLeft?.Invoke(notification);
		}

		private async void PlayerInvited(PartyInviteNotification data)
		{
			await ReceivedPartyInvites.Refresh();
			OnPlayerInvited?.Invoke(data);
		}

		private async void PartyUpdated(PartyUpdatedNotification notification)
		{
			await Refresh();
			OnPartyUpdated?.Invoke(notification);
		}

		private async void PlayerPromoted(PlayerPromotedNotification notification)
		{
			await Refresh();
			OnPlayerPromoted?.Invoke(notification);
		}

		private void PlayerKicked(PlayerKickedNotification notification)
		{
			long kickedPlayerId = long.Parse(notification.kickedPlayerId);
			if (kickedPlayerId == _userContext.UserId)
			{
				State = null;
			}

			OnPlayerKicked?.Invoke(notification);
		}

		private Promise<List<string>> RefreshMembersList() => Promise<List<string>>.Successful(_state?.members ?? new List<string>());


		protected async Promise RefreshMembersFromState()
		{
#pragma warning disable CS0618
			await Members.Refresh();
#pragma warning restore CS0618
			await PartyMembers.Refresh();
		}

		[Obsolete("use " + nameof(State) + " Instead")]
		public Party Value => State;

		/// <summary>
		/// The current <see cref="Party"/> the player is in. If the player is not in a party, then this field is null.
		/// A player can only be in one party at a time.
		/// </summary>
		public Party State
		{
			get => _state;
			private set
			{
				if (_state?.id != null && _state?.id != value?.id)
				{
					_notificationService.Unsubscribe(PlayersLeftName(_state.id), (Action<PlayerLeftNotification>)PlayerLeft);
					_notificationService.Unsubscribe(PlayersJoinedName(_state.id), (Action<PlayerJoinedNotification>)PlayerJoined);
					_notificationService.Unsubscribe(PartyUpdatedName(_state.id), (Action<PartyUpdatedNotification>)PartyUpdated);
					_notificationService.Unsubscribe(PlayerPromotedName(_state.id), (Action<PlayerPromotedNotification>)PlayerPromoted);
					_notificationService.Unsubscribe(PlayerKickedName(_state.id), (Action<PlayerKickedNotification>)PlayerKicked);
				}

				if (value?.id != null && _state?.id != value.id)
				{
					_notificationService.Subscribe(PlayersLeftName(value.id), (Action<PlayerLeftNotification>)PlayerLeft);
					_notificationService.Subscribe(PlayersJoinedName(value.id), (Action<PlayerJoinedNotification>)PlayerJoined);
					_notificationService.Subscribe(PartyUpdatedName(value.id), (Action<PartyUpdatedNotification>)PartyUpdated);
					_notificationService.Subscribe(PlayerPromotedName(value.id), (Action<PlayerPromotedNotification>)PlayerPromoted);
					_notificationService.Subscribe(PlayerKickedName(value.id), (Action<PlayerKickedNotification>)PlayerKicked);
				}

				if (_state == null)
				{
					_state = value;
				}
				else
				{
					_state.Set(value);
				}
			}
		}

		/// <summary>
		/// Checks if the player is in a party.
		/// </summary>
		public bool IsInParty => State != null && !string.IsNullOrEmpty(State.id);

		/// <inheritdoc cref="Party.id"/>
		/// <para>This references the data in the <see cref="State"/> field, which is the player's current party.</para>
		public string Id => SafeAccess(State?.id);

		/// <inheritdoc cref="Party.Restriction"/>
		/// <para>This references the data in the <see cref="State"/> field, which is the player's current party.</para>
		public PartyRestriction Restriction => SafeAccess(State.Restriction);

		/// <inheritdoc cref="Party.leader"/>
		/// <para>This references the data in the <see cref="State"/> field, which is the player's current party.</para>
		public string Leader => SafeAccess(State?.leader);

		/// <summary>
		/// This property checks if the current player is a party leader.
		/// </summary>
		public bool IsLeader => SafeAccess(State?.leader).Equals(_userContext.UserId.ToString());

		/// <inheritdoc cref="Party.members"/>
		/// <para>This references the data in the <see cref="State"/> field, which is the player's current party.</para>
		[Obsolete("Use " + nameof(PartyMembers) + " instead.")]
		public ObservableReadonlyList<string> Members { get; private set; }

		/// <summary>
		/// The set of <see cref="PartyMember"/>s that are in the party.
		/// </summary>
		public PartyMemberList PartyMembers;

		/// <summary>
		/// The set of <see cref="ReceivedPartyInvite"/> objects that the current player has received.
		/// </summary>
		public ReceivedPartyInviteList ReceivedPartyInvites;

		/// <inheritdoc cref="Party.maxSize"/>
		/// <para>This references the data in the <see cref="State"/> field, which is the player's current party.</para>
		public int MaxSize => SafeAccess(State?.maxSize) ?? 0;

		private T SafeAccess<T>(T value)
		{
			if (!IsInParty)
			{
				throw new NotInParty();
			}

			return value;
		}

		[Obsolete("Use individual public actions instead.")]
		public void RegisterCallbacks(Action<object> onPlayerJoined, Action<object> onPlayerLeft)
		{
			OnPlayerJoined = joinedEvt => onPlayerJoined?.Invoke(joinedEvt);
			OnPlayerLeft = leftEvt => onPlayerLeft?.Invoke(leftEvt);
		}

		/// <inheritdoc cref="IPartyApi.CreateParty"/>
		public async Promise Create(PartyRestriction restriction,
									int maxSize = 0,
									Action<PlayerJoinedNotification> onPlayerJoined = null,
									Action<PlayerLeftNotification> onPlayerLeft = null,
									Action<PartyUpdatedNotification> onPartyUpdated = null,
									Action<PlayerPromotedNotification> onPlayerPromoted = null,
									Action<PlayerKickedNotification> onPlayerKicked = null)
		{
			State = await _partyApi.CreateParty(restriction, maxSize);

			OnPlayerJoined = onPlayerJoined;
			OnPlayerLeft = onPlayerLeft;
			OnPartyUpdated = onPartyUpdated;
			OnPlayerPromoted = onPlayerPromoted;
			OnPlayerKicked = onPlayerKicked;

			await RefreshMembersFromState();
		}

		/// <inheritdoc cref="IPartyApi.UpdateParty"/>
		public async Promise Update(PartyRestriction restriction, int maxSize = 0)
		{
			if (State == null)
			{
				return;
			}

			State = await _partyApi.UpdateParty(Id, restriction, maxSize);
			await RefreshMembersFromState();
		}

		/// <inheritdoc cref="IPartyApi.JoinParty"/>
		public async Promise Join(string partyId)
		{
			State = await _partyApi.JoinParty(partyId);
			await RefreshMembersFromState();
		}

		/// <inheritdoc cref="IPartyApi.LeaveParty"/>
		public async Promise Leave()
		{
			if (State == null)
			{
				return;
			}

			try
			{
				var partyId = State.id;
				State = null;
				await _partyApi.LeaveParty(partyId);
			}
			finally
			{
				await RefreshMembersFromState();
			}
		}

		/// <inheritdoc cref="IPartyApi.InviteToParty"/>
		public async Promise Invite(string playerId)
		{
			if (State == null)
			{
				return;
			}

			await _partyApi.InviteToParty(State.id, playerId);
		}

		/// <inheritdoc cref="IPartyApi.InviteToParty"/>
		public async Promise Invite(long playerId)
		{
			if (State == null)
			{
				return;
			}

			await _partyApi.InviteToParty(State.id, playerId);
		}

		/// <inheritdoc cref="IPartyApi.GetPartyInvites"/>
		public Promise<InvitesResponse> GetInvites()
		{
			return _partyApi.GetPartyInvites();
		}

		/// <inheritdoc cref="IPartyApi.PromoteToLeader"/>
		public async Promise Promote(string playerId)
		{
			if (State == null)
			{
				return;
			}

			await _partyApi.PromoteToLeader(State.id, playerId);
		}

		/// <inheritdoc cref="IPartyApi.PromoteToLeader(string, long)"/>
		public async Promise Promote(long playerId)
		{
			if (State == null)
			{
				return;
			}

			await _partyApi.PromoteToLeader(State.id, playerId);
		}

		/// <inheritdoc cref="IPartyApi.KickPlayer"/>
		public async Promise Kick(string playerId)
		{
			if (State == null)
			{
				return;
			}

			await _partyApi.KickPlayer(State.id, playerId);
		}

		/// <inheritdoc cref="IPartyApi.KickPlayer(string, long)"/>
		public async Promise Kick(long playerId)
		{
			if (State == null)
			{
				return;
			}

			await _partyApi.KickPlayer(State.id, playerId);
		}
	}

	/// <summary>
	/// A party member is a player inside the current party.
	/// </summary>
	[Serializable]
	public class PartyMember
	{
		#region autogenerated equality members

		public Action OnLeft;
		public Action OnPromoted;
		public Action OnKicked;

		protected bool Equals(PartyMember other)
		{
			return playerId == other.playerId;
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

			return Equals((PartyMember)obj);
		}

		public override int GetHashCode()
		{
			return playerId.GetHashCode();
		}


		#endregion

		/// <summary>
		/// the gamerTag of the party member
		/// </summary>
		public long playerId;

		private readonly PlayerParty _sdk;
		internal PartyMember(PlayerParty sdk)
		{
			_sdk = sdk;

			_sdk.OnPlayerLeft += OnPlayerLeft;
			_sdk.OnPlayerPromoted += OnPlayerPromoted;
			_sdk.OnPlayerKicked += OnPlayerKicked;
		}

		/// <inheritdoc cref="PlayerParty.Kick(long)"/>
		public Promise Kick()
		{
			return _sdk.Kick(playerId);
		}

		/// <inheritdoc cref="PlayerParty.Promote(long)"/>
		public Promise Promote()
		{
			return _sdk.Promote(playerId);
		}

		private void OnPlayerLeft(PlayerLeftNotification obj)
		{
			if (CheckID(obj.playerThatLeftId))
			{
				OnLeft?.Invoke();
			}
		}

		private void OnPlayerPromoted(PlayerPromotedNotification obj)
		{
			if (CheckID(obj.playerPromotedId))
			{
				OnPromoted?.Invoke();
			}
		}

		private void OnPlayerKicked(PlayerKickedNotification obj)
		{
			if (CheckID(obj.kickedPlayerId))
			{
				OnKicked?.Invoke();
			}
		}

		private bool CheckID(string id)
		{
			return string.Equals(id, playerId.ToString());
		}
	}

	/// <summary>
	/// Represents an invite to a party that the current player can accept.
	/// </summary>
	[Serializable]
	public class ReceivedPartyInvite
	{
		#region auto generated equality members
		protected bool Equals(ReceivedPartyInvite other)
		{
			return invitedBy == other.invitedBy && partyId == other.partyId;
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

			return Equals((ReceivedPartyInvite)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (invitedBy.GetHashCode() * 397) ^ (partyId != null ? partyId.GetHashCode() : 0);
			}
		}
		#endregion

		private readonly PlayerParty _sdk;

		/// <summary>
		/// The gamerTag of the player that invited you to the party
		/// </summary>
		public long invitedBy;

		/// <summary>
		/// The id of the party that you have been invited to
		/// </summary>
		public string partyId;

		internal ReceivedPartyInvite(PlayerParty sdk)
		{
			_sdk = sdk;
		}

		/// <summary>
		/// Accept the invitation and join the party!
		/// </summary>
		public Promise Accept()
		{
			return _sdk.Join(partyId);
		}
	}

	[Serializable]
	public class PartyMemberList : ObservableReadonlyList<PartyMember>
	{
		public PartyMemberList(Func<Promise<List<PartyMember>>> refreshFunction) : base(refreshFunction) { }
	}

	[Serializable]
	public class ReceivedPartyInviteList : ObservableReadonlyList<ReceivedPartyInvite>
	{
		public ReceivedPartyInviteList(Func<Promise<List<ReceivedPartyInvite>>> refreshFunction) : base(refreshFunction) { }
	}
}
