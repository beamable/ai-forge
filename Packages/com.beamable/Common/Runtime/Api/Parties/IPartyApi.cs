using Beamable.Common;

namespace Beamable.Experimental.Api.Parties
{
	public interface IPartyApi
	{
		/// <summary>
		/// Create a new <see cref="Party"/> with the current player as the host.
		/// </summary>
		/// <param name="restriction">The privacy value for the created party.</param>
		/// <param name="maxSize">Maximum number of players in the party. Value of '0' means default limit - 25.</param>
		/// <returns><see cref="Promise{Party}"/> representing the created party.</returns>
		Promise<Party> CreateParty(PartyRestriction restriction, int maxSize = 0);

		/// <summary>
		/// Update state of an existing party.
		/// </summary>
		/// <param name="partyId">The id of the <see cref="Party"/> to update.</param>
		/// <param name="restriction">New privacy value.</param>
		/// <param name="maxSize">New max players value. Value of '0' means default limit - 25.</param>
		/// <returns><see cref="Promise{Party}"/> representing the updated party.</returns>
		Promise<Party> UpdateParty(string partyId, PartyRestriction restriction, int maxSize = 0);

		/// <summary>
		/// Join a <see cref="Party"/> given its id.
		/// </summary>
		/// <param name="partyId">The id of the <see cref="Party"/> to join.</param>
		/// <returns>A <see cref="Promise{Party}"/> representing the joined party.</returns>
		Promise<Party> JoinParty(string partyId);

		/// <summary>
		/// Fetch the current status of a <see cref="Party"/>.
		/// </summary>
		/// <param name="partyId">The id of the <see cref="Party"/>.</param>
		/// <returns>A <see cref="Party"/> representing the fetched party.</returns>
		Promise<Party> GetParty(string partyId);

		/// <summary>
		/// Notify the given party that the player intends to leave.
		/// </summary>
		/// <param name="partyId">The id of the <see cref="Party"/> to leave.</param>
		Promise LeaveParty(string partyId);

		/// <summary>
		/// Send a request to the given <see cref="Party"/> to remove the player with the given playerId.
		/// If the requesting player doesn't have the capability to boot players, this will throw an exception.
		/// </summary>
		/// <param name="partyId">The id of the <see cref="Party"/>.</param>
		/// <param name="playerId">The id of the player to remove.</param>
		Promise KickPlayer(string partyId, string playerId);

		/// <summary>
		/// Send a request to the given <see cref="Party"/> to promote the player with the given playerId to leader.
		/// </summary>
		/// <param name="partyId">The id of the <see cref="Party"/>.</param>
		/// <param name="playerId">The id of the player to promote.</param>
		Promise PromoteToLeader(string partyId, string playerId);

		/// <summary>
		/// Send a request to the given <see cref="Party"/> to invite the player with the given playerId.
		/// </summary>
		/// <param name="partyId">The id of the <see cref="Party"/>.</param>
		/// <param name="playerId">The id of the player to invite.</param>
		Promise InviteToParty(string partyId, string playerId);

		/// <summary>
		/// Send a request to the given <see cref="Party"/> to remove the player with the given playerId.
		/// If the requesting player doesn't have the capability to boot players, this will throw an exception.
		/// </summary>
		/// <param name="partyId">The id of the <see cref="Party"/>.</param>
		/// <param name="playerId">The id of the player to remove.</param>
		Promise KickPlayer(string partyId, long playerId);

		/// <summary>
		/// Send a request to the given <see cref="Party"/> to promote the player with the given playerId to leader.
		/// </summary>
		/// <param name="partyId">The id of the <see cref="Party"/>.</param>
		/// <param name="playerId">The id of the player to promote.</param>
		Promise PromoteToLeader(string partyId, long playerId);

		/// <summary>
		/// Send a request to the given <see cref="Party"/> to invite the player with the given playerId.
		/// </summary>
		/// <param name="partyId">The id of the <see cref="Party"/>.</param>
		/// <param name="playerId">The id of the player to invite.</param>
		Promise InviteToParty(string partyId, long playerId);

		/// <summary>
		/// Request a list of pending <see cref="PartyInvite"/>.
		/// </summary>
		/// <returns>List of <see cref="PartyInvite"/>.</returns>
		Promise<InvitesResponse> GetPartyInvites();
	}
}
