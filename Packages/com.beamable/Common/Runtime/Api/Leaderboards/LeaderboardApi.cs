using Beamable.Common.Dependencies;
using Beamable.Common.Leaderboards;
using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Beamable.Common.Api.Leaderboards
{
	public class LeaderboardApi : ILeaderboardApi
	{
		private readonly UserDataCache<RankEntry>.FactoryFunction _factoryFunction;
		public IBeamableRequester Requester { get; }
		public IUserContext UserContext { get; }
		protected IDependencyProvider Provider { get; }

		private static long TTL_MS = 60 * 1000;
		private Dictionary<string, UserDataCache<RankEntry>> caches = new Dictionary<string, UserDataCache<RankEntry>>();

		private Dictionary<string, LeaderboardAssignmentInfo> _assignmentCache =
		   new Dictionary<string, LeaderboardAssignmentInfo>();

		public LeaderboardApi(IBeamableRequester requester, IUserContext userContext, IDependencyProvider provider, UserDataCache<RankEntry>.FactoryFunction factoryFunction)
		{
			_factoryFunction = factoryFunction;
			Requester = requester;
			UserContext = userContext;
			Provider = provider;
		}

		public UserDataCache<RankEntry> GetCache(string boardId)
		{
			UserDataCache<RankEntry> cache;
			if (!caches.TryGetValue(boardId, out cache))
			{
				cache = _factoryFunction(
				   $"Leaderboard.{boardId}",
				   TTL_MS,
				   (gamerTags => Resolve(boardId, gamerTags)), Provider
				);
				caches.Add(boardId, cache);
			}

			return cache;
		}

		public Promise<LeaderboardAssignmentInfo> GetAssignment(string boardId, bool joinBoard)
		{
			string encodedBoardId = Requester.EscapeURL(boardId);
			return Requester.Request<LeaderboardAssignmentInfo>(
			   Method.GET,
			   $"/basic/leaderboards/assignment?boardId={encodedBoardId}&joinBoard={joinBoard}"
			);
		}

		public Promise<LeaderboardAssignmentInfo> ResolveAssignment(string boardId, long gamerTag)
		{
			LeaderboardAssignmentInfo info;
			if (_assignmentCache.TryGetValue($"{gamerTag}:{boardId}", out info))
			{
				return Promise<LeaderboardAssignmentInfo>.Successful(info);
			}
			else
			{
				return GetAssignment(boardId, true).Then(assignment =>
				{
					_assignmentCache[$"{gamerTag}:{boardId}"] = assignment;
				}).Recover(ex => new LeaderboardAssignmentInfo(boardId, gamerTag));
			}
		}

		public Promise<RankEntry> GetUser(LeaderboardRef leaderBoard, long gamerTag)
		   => GetUser(leaderBoard.Id, gamerTag);
		public Promise<RankEntry> GetUser(string boardId, long gamerTag)
		{
			return GetCache(boardId).Get(gamerTag);
		}

		public Promise<LeaderBoardView> GetBoard(LeaderboardRef leaderBoard, int @from, int max, long? focus = null, long? outlier = null)
		   => GetBoard(leaderBoard.Id, from, max, focus, outlier);

		public Promise<LeaderBoardView> GetBoard(string boardId, int @from, int max, long? focus = null, long? outlier = null)
		{
			if (string.IsNullOrEmpty(boardId))
			{
				return Promise<LeaderBoardView>.Failed(new Exception("Leaderboard ID cannot be uninitialized."));
			}

			string query = $"from={from}&max={max}";
			if (focus.HasValue)
			{
				query += $"&focus={focus.Value}";
			}
			if (outlier.HasValue)
			{
				query += $"&outlier={outlier.Value}";
			}

			string encodedBoardId = Requester.EscapeURL(boardId);
			return Requester.Request<LeaderBoardV2ViewResponse>(
			   Method.GET,
			   $"/object/leaderboards/{encodedBoardId}/view?{query}"
			).Map(rsp => rsp.lb)
			 .Then(lbv => lbv.userId = UserContext.UserId);
		}

		/// <inheritdoc/>
		public Promise<LeaderBoardView> GetAssignedBoard(string boardId, int @from, int max, long? focus = null, long? outlier = null)
		{
			return ResolveAssignment(boardId, UserContext.UserId).FlatMap(assignment =>
			   GetBoard(assignment.leaderboardId, @from, max, focus, outlier));
		}

		/// <inheritdoc/>
		public Promise<LeaderBoardView> GetAssignedBoard(LeaderboardRef leaderBoard, int @from, int max,
		   long? focus = null, long? outlier = null)
		   => GetAssignedBoard(leaderBoard.Id, @from, max, focus, outlier);

		public Promise<LeaderBoardView> GetFriendRanks(LeaderboardRef leaderboard) => GetFriendRanks(leaderboard.Id);

		public Promise<LeaderBoardView> GetFriendRanks(string boardId)
		{
			string encodedBoardId = Requester.EscapeURL(boardId);
			return Requester.Request<LeaderBoardV2ViewResponse>(
			   Method.GET,
			   $"/object/leaderboards/{encodedBoardId}/friends"
			).Map(rsp => rsp.lb);
		}

		public Promise<LeaderBoardView> GetRanks(LeaderboardRef leaderBoard, List<long> ids)
		   => GetRanks(leaderBoard.Id, ids);

		public Promise<LeaderBoardView> GetRanks(string boardId, List<long> ids)
		{
			var query = "";
			if (ids != null && ids.Count > 0)
			{
				query = $"&ids={string.Join(",", ids)}";
			}

			string encodedBoardId = Requester.EscapeURL(boardId);
			return Requester.Request<LeaderBoardV2ViewResponse>(
			   Method.GET,
			   $"/object/leaderboards/{encodedBoardId}/ranks?{query}"
			).Map(rsp => rsp.lb);
		}

		public Promise<EmptyResponse> SetScore(LeaderboardRef leaderBoard, double score, IDictionary<string, object> stats = null)
			  => SetScore(leaderBoard.Id, score, stats);

		public Promise<EmptyResponse> SetScore(string boardId, double score, IDictionary<string, object> stats = null)
		{
			return Update(boardId, score, increment: false, stats);
		}

		public Promise<EmptyResponse> IncrementScore(LeaderboardRef leaderBoard, double score, IDictionary<string, object> stats = null)
		   => IncrementScore(leaderBoard.Id, score, stats);

		public Promise<EmptyResponse> IncrementScore(string boardId, double score, IDictionary<string, object> stats = null)
		{
			return Update(boardId, score, true, stats);
		}

		protected Promise<EmptyResponse> Update(string boardId, double score, bool increment = false, IDictionary<string, object> stats = null)
		{
			return ResolveAssignment(boardId, UserContext.UserId).FlatMap(assignment =>
			{
				var req = new ArrayDict
			   {
			   {"score", score},
			   {"id", UserContext.UserId},
			   {"increment", increment}
			   };
				if (stats != null)
				{
					req["stats"] = new ArrayDict(stats);
				}

				var body = Json.Serialize(req, new StringBuilder());
				string encodedBoardId = Requester.EscapeURL(assignment.leaderboardId);
				return Requester.Request<EmptyResponse>(
				Method.PUT,
				$"/object/leaderboards/{encodedBoardId}/entry",
				body
			 ).Then(_ => GetCache(boardId).Remove(UserContext.UserId));
			});
		}

		private Promise<Dictionary<long, RankEntry>> Resolve(string boardId, List<long> gamerTags)
		{
			string queryString = "";
			for (int i = 0; i < gamerTags.Count; i++)
			{
				if (i > 0)
				{
					queryString += ",";
				}

				queryString += gamerTags[i].ToString();
			}

			string encodedBoardId = Requester.EscapeURL(boardId);
			return Requester.Request<LeaderBoardV2ViewResponse>(
			   Method.GET,
			   $"/object/leaderboards/{encodedBoardId}/ranks?ids={queryString}"
			).Map(rsp =>
			{
				Dictionary<long, RankEntry> result = new Dictionary<long, RankEntry>();
				var rankings = rsp.lb.ToDictionary();
				for (int i = 0; i < gamerTags.Count; i++)
				{
					RankEntry entry;
					if (!rankings.TryGetValue(gamerTags[i], out entry))
					{
						entry = new RankEntry();
						entry.gt = gamerTags[i];
#pragma warning disable CS0612
						entry.columns = new RankEntryColumns();
#pragma warning restore CS0612
					}

					result.Add(gamerTags[i], entry);
				}
				return result;
			});
		}

		public Promise<EmptyResponse> FreezeLeaderboard(string boardId)
		{
			return Requester.Request<EmptyResponse>(Method.PUT, $"/object/leaderboards/{boardId}/freeze");
		}
	}
}
