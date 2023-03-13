namespace Beamable.Common.Api.Presence
{
	public interface IPresenceApi : IConnectivityChecker
	{
		Promise<EmptyResponse> SendHeartbeat();

		/// <summary>
		/// Get current <see cref="PlayerPresence"/> of the given player.
		/// </summary>
		Promise<PlayerPresence> GetPlayerPresence(long playerId);

		/// <summary>
		/// Set status of the current player.
		/// </summary>
		/// <param name="status"><see cref="PresenceStatus"/> to set.</param>
		/// <param name="description">Status description to set.</param>
		Promise<EmptyResponse> SetPlayerStatus(PresenceStatus status, string description);

		/// <summary>
		/// Get status of multiple players.
		/// </summary>
		/// <param name="playerIds">Ids of players to check.</param>
		/// <returns>A <see cref="MultiplePlayersStatus"/> containing a list of statuses of queried players.</returns>
		Promise<MultiplePlayersStatus> GetManyStatuses(params long[] playerIds);

		/// <inheritdoc cref="GetManyStatuses(long[])"/>
		Promise<MultiplePlayersStatus> GetManyStatuses(params string[] playerIds);
	}
}
