namespace Beamable.Common.Api.Social
{
	public interface ISocialApi : IHasBeamableRequester
	{
		/// <summary>
		/// Get the current player's <see cref="SocialList"/>.
		/// If the social list has never been downloaded, this will use the <see cref="RefreshSocialList"/> method.
		/// However, once the social list has been downloaded, this method will return immediately with the cached data.
		/// </summary>
		/// <returns>A <see cref="Promise"/> containing the player's <see cref="SocialList"/></returns>
		Promise<SocialList> Get();

		/// <summary>
		/// Import friends from a third party provider.
		/// </summary>
		/// <param name="source">
		/// The third party to find friends from.
		/// </param>
		/// <param name="token">
		/// An access token issued from the third party that can be sent to Beamable so that the Beamable Cloud can perform the friend import.
		/// </param>
		/// <returns>A <see cref="Promise"/> representing the network call.</returns>
		Promise<EmptyResponse> ImportFriends(SocialThirdParty source, string token);

		/// <summary>
		/// Block a player from the current player's social list.
		/// When a player is blocked, they'll appear in the <see cref="SocialList.blocked"/> list.
		/// Use the <see cref="UnblockPlayer"/> method to revert.
		/// </summary>
		/// <param name="gamerTag">The gamertag of the player to block</param>
		/// <returns>A <see cref="Promise"/> containing a <see cref="FriendStatus"/> for the given gamertag</returns>
		Promise<FriendStatus> BlockPlayer(long gamerTag);

		/// <summary>
		/// Unblock a player from the current player's <see cref="SocialList.blocked"/> list.
		/// </summary>
		/// <param name="gamerTag">The gamertag of the player to unblock</param>
		/// <returns>A <see cref="Promise"/> containing a <see cref="FriendStatus"/> for the given gamertag</returns>
		Promise<FriendStatus> UnblockPlayer(long gamerTag);

		/// <summary>
		/// Cancel a pending <see cref="FriendInvite"/> from the given <paramref name="gamerTag"/> associated with the authenticated player.
		/// If the <paramref name="gamerTag"/> player is online, they will receive a <see cref="FriendRequestUpdateNotification"/> on the "SOCIAL.UPDATE" channel.  
		/// </summary>
		Promise<EmptyResponse> CancelFriendRequest(long gamerTag);

		/// <summary>
		/// Sends out a friend request to the given <paramref name="gamerTag"/> and notifies them.
		/// If the given <paramref name="gamerTag"/> is already a friend or has a pending invite, this endpoint will still notify them, but the invite will not be duplicated. 
		/// If the given <paramref name="gamerTag"/> does not exist or is invalid, will fail the promise with an "AccountNotFound" error.
		/// </summary>
		Promise<EmptyResponse> SendFriendRequest(long gamerTag);

		/// <summary>
		/// Accepts an <see cref="FriendInviteDirection.Incoming"/> friend invite from the given <paramref name="gamerTag"/>.
		/// If the involved players are online, they will receive a <see cref="FriendRequestUpdateNotification"/> on the "SOCIAL.UPDATE" channel.  
		/// If the given <paramref name="gamerTag"/> has not invited me, will fail the promise with a "NoInviteError" error. 
		/// </summary>
		Promise<EmptyResponse> AcceptFriendRequest(long gamerTag);

		/// <summary>
		/// Remove a player from the current player's <see cref="SocialList.friends"/> list.
		/// </summary>
		/// <param name="gamerTag">The gamertag of the player to remove.</param>
		/// <returns>A <see cref="Promise"/> representing the network call.</returns>
		Promise<EmptyResponse> RemoveFriend(long gamerTag);

		/// <summary>
		/// Get the current player's <see cref="SocialList"/>
		/// </summary>
		/// <returns>A <see cref="Promise"/> containing the player's <see cref="SocialList"/></returns>
		Promise<SocialList> RefreshSocialList();
	}
}
