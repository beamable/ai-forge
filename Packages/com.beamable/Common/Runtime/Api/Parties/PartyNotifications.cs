using System;

namespace Beamable.Experimental.Api.Parties
{
	[Serializable]
	public class PlayerJoinedNotification
	{
		public string partyId;
		public string playerThatJoinedId;
	}

	[Serializable]
	public class PlayerLeftNotification
	{
		public string partyId;
		public string playerThatLeftId;
	}

	[Serializable]
	public class PartyInviteNotification
	{
		public string partyId;
		public string invitingPlayerId;
	}

	[Serializable]
	public class PartyUpdatedNotification
	{
		public string partyId;
		public long oldMaxSize;
		public long newMaxSize;
		public string oldRestriction;
		public string newRestriction;

		public PartyRestriction OldRestriction => (PartyRestriction)Enum.Parse(typeof(PartyRestriction), oldRestriction);
		public PartyRestriction NewRestriction => (PartyRestriction)Enum.Parse(typeof(PartyRestriction), newRestriction);
	}

	[Serializable]
	public class PlayerPromotedNotification
	{
		public string partyId;
		public string playerPromotedId;
	}

	[Serializable]
	public class PlayerKickedNotification
	{
		public string partyId;
		public string kickedPlayerId;
	}
}
