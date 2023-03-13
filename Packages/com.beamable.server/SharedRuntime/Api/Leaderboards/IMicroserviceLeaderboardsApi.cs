using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Leaderboards;
using Beamable.Common.Content;
using Beamable.Common.Leaderboards;
using System.Collections.Generic;
#pragma warning disable CS0618

namespace Beamable.Server.Api.Leaderboards
{
	/// <summary>
	/// This type defines the %Client main entry point for the %Leaderboards feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/leaderboards-feature-overview">Leaderboards</a> feature documentation
	/// - See Beamable.Server.IBeamableServices script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public interface IMicroserviceLeaderboardsApi : ILeaderboardApi
	{
		/* admin only functions? */

		/// <summary>
		/// Call to create a new leaderboard with the given <paramref name="leaderboardId"/> using the parameters found in the <paramref name="templateLeaderboardContent"/>.
		/// Does not return an error when given an existing <paramref name="leaderboardId"/>. Instead, returns a success without any alteration made to the existing leaderboard that matches <paramref name="leaderboardId"/>.
		/// </summary>
		/// <param name="leaderboardId">Id for the new leaderboard. Caller must guarantee this to be unique.</param>
		/// <param name="templateLeaderboardContent">Template parameters that'll be used to create the new leaderboard.</param>
		/// <param name="ttl">When this leaderboard should expire.</param>
		/// <param name="derivatives">Board Ids for boards that must be recalculated when a entry is updated in this board.</param>
		/// <param name="freezeTime">An arbitrary time since jan 1st 1970 when this leaderboard should be frozen</param>
		Promise<EmptyResponse> CreateLeaderboard(string leaderboardId,
			LeaderboardContent templateLeaderboardContent,
			OptionalLong ttl = null,
			OptionalListString derivatives = null,
			OptionalLong freezeTime = null);

		/// <summary>
		/// Call to create a leaderboard without a template as a base.
		/// </summary>
		/// <param name="leaderboardId">Id for the new leaderboard. Caller must guarantee this to be unique.</param>
		/// <param name="maxEntries">Maximum number of players who can be in the leaderboard.</param>
		/// <param name="ttl">When this leaderboard should expire.</param>
		/// <param name="partitioned">Whether or not the leaderboard should be partitioned into N "maxEntries" boards.</param>
		/// <param name="cohortSettings">Stats-based filter that's used to group together leaderboard entries.</param>
		/// <param name="derivatives">Board Ids for boards that must be recalculated when a entry is updated in this board.</param>
		/// <param name="permissions">Whether or not a client can write to this leaderboard</param>
		/// <param name="freezeTime">An arbitrary time since jan 1st 1970 when this leaderboard should be frozen</param>
		Promise<EmptyResponse> CreateLeaderboard(string leaderboardId,
			OptionalInt maxEntries,
			OptionalLong ttl,
			OptionalBoolean partitioned,
			OptionalCohortSettings cohortSettings,
			OptionalListString derivatives,
			OptionalClientPermissions permissions,
			OptionalLong freezeTime);

		/// <summary>
		/// Call to create a leaderboard without a template as a base.
		/// </summary>
		Promise<EmptyResponse> CreateLeaderboard(string leaderboardId, CreateLeaderboardRequest req);

		/// <summary>
		/// Get a list of all available leaderboard ids.
		/// </summary>
		/// <param name="skip">An optional number of leaderboard ids to skip. By default, no leaderboards will be skipped. Use this with the <see cref="limit"/> argument to page the results.</param>
		/// <param name="limit">An optional maximum number of leaderboard ids to return. By default, the limit will be 50. Use this with the <see cref="skip"/> argument to page the results.</param>
		/// <returns>A <see cref="Promise"/> containing a <see cref="ListLeaderboardResult"/> that has the leaderboard ids.</returns>
		Promise<ListLeaderboardResult> ListLeaderboards(int? skip = null, int? limit = 50);

		/// <summary>
		/// Get a list of all the leaderboards that a specific player is participating in.
		/// </summary>
		/// <param name="gamerTag">The gamertag of the player to find leaderboards for.</param>
		/// <returns>A <see cref="Promise"/> containing a <see cref="GetPlayerLeaderboardsResponse"/> that has a set of <see cref="LeaderBoardView"/>s</returns>
		Promise<GetPlayerLeaderboardsResponse> GetPlayerLeaderboards(long gamerTag);

		/// <summary>
		/// Call to remove the player's entry from the leaderboard.
		/// </summary>
		/// <param name="leaderboardId">The ID of leaderboard from which the player's entry should be removed</param>
		/// <param name="gamerTag">The gamertag of the player.</param>
		/// <returns></returns>
		Promise<EmptyResponse> RemovePlayerEntry(string leaderboardId, long gamerTag);
	}

	[System.Serializable]
	public class ListLeaderboardResponse
	{
		public int total;
		public int offset;
		public List<string> nameList;
	}

	[System.Serializable]
	public class GetPlayerLeaderboardsResponse
	{
		public string result;
		public List<LeaderBoardView> lbs;
	}

	[System.Serializable]
	[Agnostic]
	public class CreateLeaderboardRequest
	{
		public int? maxEntries;
		public long? ttl;
		public bool? partitioned;
		public LeaderboardCohortSettings cohortSettings;
		public List<string> derivatives;
		public ClientPermissions permissions;
		public long? freezeTime;
	}

}
