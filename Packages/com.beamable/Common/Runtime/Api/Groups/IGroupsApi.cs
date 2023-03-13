using Beamable.Common.Api.Inventory;
using System;
using System.Collections.Generic;

namespace Beamable.Common.Api.Groups
{
	public interface IGroupsApi : ISupportsGet<GroupsView>
	{
		/// <summary>
		/// Get the <see cref="GroupUser"/> for a player, which contains all the group metadata the player belongs to.
		/// Use the <see cref="GetGroup"/> method to resolve the full group data.
		/// </summary>
		/// <param name="gamerTag">The gamertag of a player</param>
		/// <returns>A <see cref="Promise{T}"/> containing the player's <see cref="GroupUser"/> data</returns>
		Promise<GroupUser> GetUser(long gamerTag);

		/// <summary>
		/// Get the <see cref="Group"/> data for some group.
		/// The resulting <see cref="Group"/> structure will be personalized for the current player making the call.
		/// </summary>
		/// <param name="groupId">
		/// The id of a group.
		/// The group id can be found with the <see cref="GetUser"/> or <see cref="Search"/> methods.
		/// </param>
		/// <returns>A <see cref="Promise{T}"/> containing the <see cref="Group"/> data</returns>
		Promise<Group> GetGroup(long groupId);

		/// <summary>
		/// Disbanding a group will delete the group data from Beamable.
		/// This method can only be called by an admin, from a microservice, or by the group Leader that has the <see cref="Group.canDisband"/> permission.
		/// </summary>
		/// <param name="group">The group id to disband.</param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call.</returns>
		Promise<EmptyResponse> DisbandGroup(long group);

		/// <summary>
		/// Remove the current player from a group.
		/// After a player leaves, the group will no longer appear in the <see cref="GetUser"/> method response.
		/// </summary>
		/// <param name="group">The group id to leave</param>
		/// <returns>A <see cref="Promise{T}"/> containing a <see cref="GroupMembershipResponse"/> to check that the Leave operation occurred correctly.</returns>
		Promise<GroupMembershipResponse> LeaveGroup(long group);

		/// <summary>
		/// Add the current player to a group.
		/// A player can only join a group when the group's <see cref="Group.enrollmentType"/> is set to "open".
		/// A player can only be in one group at a time. If the player is already in a group, they must
		/// use the <see cref="LeaveGroup"/> method before they can join a new group.
		/// </summary>
		/// <param name="group">The group id to join</param>
		/// <returns>A <see cref="Promise{T}"/> containing a <see cref="GroupMembershipResponse"/> to check that the Join operation occurred correctly.</returns>
		Promise<GroupMembershipResponse> JoinGroup(long group);


		/// <summary>
		/// This will be removed in a future version of Beamable. Please do not use.
		/// </summary>
		[Obsolete]
		Promise<EmptyResponse> Petition(long group);

		/// <summary>
		/// This will be removed in a future version of Beamable. Please do not use.
		/// </summary>
		[Obsolete]
		Promise<GroupSearchResponse> GetRecommendations();

		/// <summary>
		/// Get a list of <see cref="Group"/>s that match some criteria.
		/// The resulting <see cref="Group"/> structures will not be personalized to the current player, and
		/// as such, the various permission fields should not be trusted.
		/// </summary>
		/// <param name="name">An optional group name. This text will be a fuzzy text match</param>
		/// <param name="enrollmentTypes">valid values include "open", "closed", and "restricted".</param>
		/// <param name="hasSlots">By default, will be true. When true, only search for groups that still have room left for new members to join.</param>
		/// <param name="scoreMin">This will be removed in a future version of Beamable. </param>
		/// <param name="scoreMax">This will be removed in a future version of Beamable. </param>
		/// <param name="sortField">
		/// Sort the resulting groups on a particular field name.
		/// When this field is used, you must also specify a <see cref="sortValue"/>
		/// </param>
		/// <param name="sortValue">
		/// Use a positive number to sort the resulting groups from highest to lowest.
		/// Use a negative number to sort the resulting groups from lowest to highest.
		/// When this field is used, you must also specify a <see cref="sortField"/>
		/// </param>
		/// <param name="offset">The number of resulting groups to skip in the response. This can be used with <see cref="limit"/> to page the groups.</param>
		/// <param name="limit">The maximum number of resulting groups for the response. This can be used with the <see cref="offset"/> to page the groups.</param>
		/// <returns></returns>
		Promise<GroupSearchResponse> Search(
		   string name = null,
		   List<string> enrollmentTypes = null,
		   bool? hasSlots = null,
		   long? scoreMin = null,
		   long? scoreMax = null,
		   string sortField = null,
		   int? sortValue = null,
		   int? offset = null,
		   int? limit = null
		);

		/// <summary>
		/// Create a new group.
		/// The current player will become the group Leader for the new group.
		/// </summary>
		/// <param name="request">
		/// The <see cref="GroupCreateRequest"/> that will be used to seed the initial properties of the group.
		/// </param>
		/// <returns>A <see cref="Promise{T}"/> containing the <see cref="GroupCreateResponse"/> information the resulting group, including the see <see cref="GroupMetaData.id"/></returns>
		Promise<GroupCreateResponse> CreateGroup(GroupCreateRequest request);

		/// <summary>
		/// Check if the given name and tag are available to be used.
		/// Names and tags are required to be unique, so if another group is already using the name or tag given
		/// to the <see cref="CreateGroup"/> or <see cref="SetGroupProps"/> methods, those requests will fail.
		/// </summary>
		/// <param name="name">The name to check if other groups are using yet.</param>
		/// <param name="tag">The tag to check if other groups are using yet.</param>
		/// <returns>A <see cref="Promise{T}"/> containing a <see cref="AvailabilityResponse"/> that has information about the name and tag's availability. </returns>
		Promise<AvailabilityResponse> CheckAvailability(string name, string tag);

		/// <summary>
		/// Update the group's data.
		/// Only the group Leader can update group data.
		/// </summary>
		/// <param name="groupId">The id of the group to update</param>
		/// <param name="props">
		/// A <see cref="GroupUpdateProperties"/> structure that contains the data to update.
		/// The update can be partial. Any fields that are null will not cause updates.
		/// </param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call.</returns>
		Promise<EmptyResponse> SetGroupProps(long groupId, GroupUpdateProperties props);

		/// <summary>
		/// A <see cref="Member"/> can be kicked from the group if the current player's <see cref="Member.canKick"/> field is true.
		/// The group Leader and all Officers can kick regular group members.
		/// Once a member is kicked, they will no longer be a member.
		/// </summary>
		/// <param name="group">The group id to kick the member from.</param>
		/// <param name="gamerTag">The gamertag of the member to kick from the group.</param>
		/// <returns>A <see cref="Promise{T}"/> containing a <see cref="GroupMembershipResponse"/> to check that the Kick operation occurred correctly.</returns>
		Promise<GroupMembershipResponse> Kick(long group, long gamerTag);

		/// <summary>
		/// Set a player's role within a group.
		/// Valid roles are "leader", "officer", or an empty string.
		/// A group can only have 1 Leader, and the request will fail if the SetRole operation would result in 0 group Leaders.
		/// A group can have arbitrary many Officers.
		/// This method can be used to change the group Leader. The existing group Leader must call this method and assign a different
		/// user the role of "leader".
		/// The player that is running this method must have sufficient permissions to grant the role.
		/// </summary>
		/// <param name="group">The group id</param>
		/// <param name="gamerTag">the gamertag of the player whose role will be changed.</param>
		/// <param name="role">The role to assign the player to. Valid options are "leader", "officer", or ""</param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call.</returns>
		Promise<EmptyResponse> SetRole(long group, long gamerTag, string role);

		/// <summary>
		/// Send a request to the rest of the group to request an amount <see cref="Currency"/>.
		/// Other members in the group may decide to donate their personal currency to the requester by calling the <see cref="Donate"/> method.
		/// The status of donations can be observed in the <see cref="Group.donations"/> list.
		/// <b>ATTENTION!</b> For group donations to work, you must configure a <see cref="GroupDonationsContent"/> content object called "default".
		/// After a player makes a donation request, they must wait some number seconds (as configured in the donations.default) before making a new request.
		/// </summary>
		/// <param name="group">The group id</param>
		/// <param name="currency">A <see cref="Currency"/> structure to describe what type of donation the player is requesting.</param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call.</returns>
		Promise<EmptyResponse> MakeDonationRequest(long group, Currency currency);

		/// <summary>
		/// Send an amount of player currency to fulfil a pending donation request.
		/// Members of a group can request currency by running the <see cref="MakeDonationRequest"/> method.
		/// The status of donations can be observed in the <see cref="Group.donations"/> list.
		/// <b>ATTENTION!</b> For group donations to work, you must configure a <see cref="GroupDonationsContent"/> content object called "default".
		/// </summary>
		/// <param name="group">The group id</param>
		/// <param name="recipientId">The gamertag of the player that is asking for a donation.</param>
		/// <param name="amount">The amount of currency to donate to the player.</param>
		/// <param name="autoClaim">
		/// When true, the currency will automatically be transferred into the recipient's inventory.
		/// When false, the recipient will need to call <see cref="ClaimDonations"/>. However, the currency will leave the sender's inventory immediately.
		/// </param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call.</returns>
		Promise<EmptyResponse> Donate(long group, long recipientId, long amount, bool autoClaim = true);

		/// <summary>
		/// If a player has requested a donation with the <see cref="MakeDonationRequest"/> method,
		/// and other players have donated with the <see cref="Donate"/> method without autoClaim enabled,
		/// then this method must be called by the original player to complete the donation.
		///
		/// This method will claim all outstanding donations for the current player.
		/// <b>ATTENTION!</b> For group donations to work, you must configure a <see cref="GroupDonationsContent"/> content object called "default".
		/// </summary>
		/// <param name="group">the group id.</param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call.</returns>
		Promise<EmptyResponse> ClaimDonations(long group);

		/// <summary>
		/// This will be removed in a future version of Beamable. Please do not use.
		/// </summary>
		/// <param name="query"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		[Obsolete]
		string AddQuery(string query, string key, string value);
	}
}
