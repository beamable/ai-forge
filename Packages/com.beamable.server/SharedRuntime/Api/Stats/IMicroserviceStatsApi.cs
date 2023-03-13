using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Stats;
using System;
using System.Collections.Generic;

namespace Beamable.Server.Api.Stats
{
	/// <summary>
	/// This type defines the %Microservice main entry point for the %Stats feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/stats-feature-overview">Stats</a> feature documentation
	/// - See Beamable.Server.IBeamableServices script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public interface IMicroserviceStatsApi : IStatsApi
	{
		/// <summary>
		/// Retrieve a stat value, by key
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="stat"></param>
		/// <returns></returns>
		Promise<string> GetPublicPlayerStat(long userId, string stat);

		/// <summary>
		/// Retrieve one or more stat values, each by key
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="stats"></param>
		/// <returns></returns>
		Promise<Dictionary<string, string>> GetPublicPlayerStats(long userId, string[] stats);

		/// <summary>
		/// Retrieve all stat values, each by key
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		Promise<Dictionary<string, string>> GetPublicPlayerStats(long userId);

		/// <summary>
		/// Retrieve a stat value, by key
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		Promise<string> GetProtectedPlayerStat(long userId, string key);

		/// <summary>
		/// Retrieve one or more stat values, each by key
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="stats"></param>
		/// <returns></returns>
		Promise<Dictionary<string, string>> GetProtectedPlayerStats(long userId, string[] stats);

		/// <summary>
		/// Retrieve all stat values, each by key
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		Promise<Dictionary<string, string>> GetProtectedPlayerStats(long userId);

		/// <summary>
		/// Retrieve all stat values, each by key
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		[Obsolete("Use GetProtectedPlayerStats(long userId) instead")]
		Promise<Dictionary<string, string>> GetAllProtectedPlayerStats(long userId);

		/// <summary>
		/// Set a stat value, by key
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		Promise<EmptyResponse> SetProtectedPlayerStat(long userId, string key, string value);

		/// <summary>
		/// Set one or more stat values, by key
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="stats"></param>
		/// <returns></returns>
		Promise<EmptyResponse> SetProtectedPlayerStats(long userId, Dictionary<string, string> stats);

		Promise<EmptyResponse> SetStats(string domain, string access, string type, long userId,
		   Dictionary<string, string> stats);

		Promise<Dictionary<string, string>> GetStats(string domain, string access, string type, long userId,
		   string[] stats);

		/// <summary>
		/// Queries the player base for matches against specific stats defined by the given <paramref name="criteria"/>.
		/// </summary>
		/// <param name="domain">"game" or "player".</param>
		/// <param name="access">"public" or "private"</param>
		/// <param name="type">Should always be "player" (exists for legacy reasons).</param>
		/// <param name="criteria">List of all <see cref="Criteria"/> that must match.</param>
		/// <returns>The list of DBIDs for all users that match ALL of the criteria provided.</returns>
		Promise<StatsSearchResponse> SearchStats(string domain, string access, string type, List<Criteria> criteria);

		/// <summary>
		/// Deletes a player's game private stats (<see cref="DeleteStats"/>).
		/// </summary>
		/// <param name="userId">A player's realm-specific GamerTag (for example, <see cref="RequestContext.UserId"/>).</param>
		/// <param name="stats">The list of stats to delete.</param>
		/// <returns></returns>
		Promise DeleteProtectedPlayerStats(long userId, string[] stats);

		/// <summary>
		/// Deletes the given stats.
		/// </summary>
		/// <param name="domain">"game" or "player".</param>
		/// <param name="access">"public" or "private"</param>
		/// <param name="type">Should always be "player" (exists for legacy reasons).</param>
		/// <param name="userId">A player's realm-specific GamerTag (for example, <see cref="RequestContext.UserId"/>).</param>
		/// <param name="stats">The list of stats to delete.</param>
		Promise DeleteStats(string domain, string access, string type, long userId, string[] stats);
	}
}
