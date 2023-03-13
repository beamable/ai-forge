using Beamable.Common.Api.Stats;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Common.Api.Tournaments
{
	public class TournamentApi : ITournamentApi
	{
		const string SERVICE_PATH = "/basic/tournaments";

		private readonly IStatsApi _stats;
		private IBeamableRequester _requester;
		public TournamentApi(IStatsApi stats, IBeamableRequester requester, IUserContext ctx)
		{
			_stats = stats;
			_requester = requester;
		}

		public Promise<TournamentInfo> GetTournamentInfo(string tournamentContentId)
		{
			return GetAllTournaments().Map(resp =>
			   resp.tournaments.FirstOrDefault(tournament => string.Equals(tournament.contentId, tournamentContentId)));
		}

		public Promise<TournamentInfoResponse> GetAllTournaments(string contentId = null, int? cycle = null, bool? isRunning = null)
		{
			string queryArgs = "";
			if (!string.IsNullOrEmpty(contentId))
			{
				queryArgs += $"contentId={contentId}";
				if (cycle.HasValue)
				{
					queryArgs += $"&cycle={cycle.Value}";
				}
			}

			if (isRunning.HasValue)
			{
				if (!string.IsNullOrEmpty(queryArgs))
				{
					queryArgs += "&";
				}
				queryArgs += $"isRunning={isRunning.Value}";
			}

			var path = $"{SERVICE_PATH}?{queryArgs}";
			return _requester.Request<TournamentInfoResponse>(Method.GET, path);
		}



		private string ConstructStandingsURLArgs(string tournamentId, int cycle = -1, int from = -1, int max = -1, int focus = -1)
		{
			var cycleArg = cycle < 0 ? "" : $"&cycle={cycle}";
			var fromArg = from < 0 ? "" : $"&from={from}";
			var maxArg = max < 0 ? "" : $"&max={max}";
			var focusArg = focus < 0 ? "" : $"&focus={focus}";
			return $"?tournamentId={tournamentId}{cycleArg}{fromArg}{maxArg}{focusArg}";
		}

		public Promise<TournamentChampionsResponse> GetChampions(string tournamentId, int cycleLimit = 30)
		{
			var path = $"{SERVICE_PATH}/champions?tournamentId={tournamentId}&cycles={cycleLimit}";
			return _requester.Request<TournamentChampionsResponse>(Method.GET, path);
		}

		public Promise<TournamentStandingsResponse> GetGlobalStandings(string tournamentId, int cycle = -1, int from = -1,
		   int max = -1, int focus = -1)
		{
			var path = $"{SERVICE_PATH}/global{ConstructStandingsURLArgs(tournamentId, cycle, from, max, focus)}";
			return WithEmptyResultsOn404(_requester.Request<TournamentStandingsResponse>(Method.GET, path));
		}

		public Promise<TournamentStandingsResponse> GetStandings(string tournamentId, int cycle = -1, int from = -1,
		   int max = -1, int focus = -1)
		{
			var path = $"{SERVICE_PATH}/standings{ConstructStandingsURLArgs(tournamentId, cycle, from, max, focus)}";
			return WithEmptyResultsOn404(_requester.Request<TournamentStandingsResponse>(Method.GET, path));
		}

		private Promise<TournamentStandingsResponse> WithEmptyResultsOn404(Promise<TournamentStandingsResponse> promise)
		{
			return promise.Recover(ex =>
			{
				switch (ex)
				{
					case IRequestErrorWithStatus err when err.Status == 404:
						return new TournamentStandingsResponse
						{
							entries = new List<TournamentEntry>(),
							me = null
						};
					default:
						throw ex;
				}
			});
		}

		private Promise<TournamentGroupsResponse> WithEmptyResultsOn404(Promise<TournamentGroupsResponse> promise)
		{
			return promise.Recover(ex =>
			{
				switch (ex)
				{
					case IRequestErrorWithStatus err when err.Status == 404:
						return new TournamentGroupsResponse
						{
							entries = new List<TournamentGroupEntry>(),
							focus = null
						};
					default:
						throw ex;
				}
			});
		}

		public Promise<TournamentRewardsResponse> GetUnclaimedRewards(string tournamentId)
		{
			var path = $"{SERVICE_PATH}/rewards?tournamentId={tournamentId}";
			return _requester.Request<TournamentRewardsResponse>(Method.GET, path);
		}

		public Promise<TournamentRewardsResponse> ClaimAllRewards(string tournamentId)
		{
			var path = $"{SERVICE_PATH}/rewards?tournamentId={tournamentId}";
			return _requester.Request<TournamentRewardsResponse>(Method.POST, path);
		}

		public Promise<TournamentPlayerStatus> JoinTournament(string tournamentId, double startScore = 0)
		{
			return GetPlayerStatus().FlatMap(allStatus =>
			{
				var existing = allStatus.statuses.FirstOrDefault(status => status.tournamentId.Equals(tournamentId));
				if (existing != null)
				{
					// we have already joined the tournament. Don't do anything.
					return Promise<TournamentPlayerStatus>.Successful(existing);
				}
				else
				{
					// we actually need to join, and set a start score.
					var path = $"{SERVICE_PATH}";
					var body = new TournamentJoinRequest { tournamentId = tournamentId };
					return _requester.Request<TournamentPlayerStatus>(Method.POST, path, body).FlatMap(status =>
					   SetScore(tournamentId, status.playerId, startScore).Map(_ => status)
					);
				}
			});
		}

		public Promise<Unit> SetScore(string tournamentId, long dbid, double score, bool incrementScore = false)
		{
			var path = $"{SERVICE_PATH}/score";
			var body = new TournamentScoreRequest
			{
				tournamentId = tournamentId,
				score = score,
				playerId = dbid,
				increment = incrementScore
			};

			return _requester.Request<TournamentScoreResponse>(Method.POST, path, body).Map(_ => PromiseBase.Unit);
		}

		public Promise<TournamentPlayerStatusResponse> GetPlayerStatus(string tournamentId = null, string contentId = null, bool? hasUnclaimedRewards = null)
		{
			string queryArgs = "";
			if (!string.IsNullOrEmpty(tournamentId))
			{
				queryArgs += $"tournamentId={tournamentId}";
			}

			if (!string.IsNullOrEmpty(contentId))
			{
				if (!string.IsNullOrEmpty(queryArgs))
				{
					queryArgs += "&";
				}
				queryArgs += $"contentId={contentId}";
			}

			if (hasUnclaimedRewards.HasValue)
			{
				if (!string.IsNullOrEmpty(queryArgs))
				{
					queryArgs += "&";
				}
				queryArgs += $"hasUnclaimedRewards={hasUnclaimedRewards.Value}";
			}

			var path = $"{SERVICE_PATH}/me?{queryArgs}";
			return _requester.Request<TournamentPlayerStatusResponse>(Method.GET, path);
		}

		public Promise<TournamentGroupStatusResponse> GetGroupStatus(string tournamentId = null, string contentId = null)
		{
			string queryArgs = "";
			if (!string.IsNullOrEmpty(tournamentId))
			{
				queryArgs += $"tournamentId={tournamentId}";
			}

			if (!string.IsNullOrEmpty(contentId))
			{
				if (!string.IsNullOrEmpty(queryArgs))
				{
					queryArgs += "&";
				}
				queryArgs += $"contentId={contentId}";
			}

			var path = $"{SERVICE_PATH}/me/group?{queryArgs}";
			return _requester.Request<TournamentGroupStatusResponse>(Method.GET, path);
		}

		public Promise<TournamentGroupStatusResponse> GetGroupStatuses(List<long> groupIds, string contentId)
		{
			var path = $"{SERVICE_PATH}/search/groups";
			var body = new TournamentGetStatusesRequest
			{
				contentId = contentId,
				groupIds = groupIds
			};

			return _requester.Request<TournamentGroupStatusResponse>(Method.POST, path, body);
		}

		public Promise<string> GetPlayerAlias(long playerId, string statName = "alias")
		{
			return _stats.GetStats("client", "public", "player", playerId).Map(stats =>
			{
				var defaultAlias = "unknown";
				stats.TryGetValue(statName, out defaultAlias);
				return defaultAlias;
			});
		}

		public Promise<string> GetPlayerAvatar(long playerId, string statName = "avatar")
		{
			return _stats.GetStats("client", "public", "player", playerId).Map(stats =>
			{
				var defaultAlias = "0";
				stats.TryGetValue("avatar", out defaultAlias);
				return defaultAlias;
			});
		}

		public Promise<TournamentStandingsResponse> GetGroupPlayers(string tournamentId, int cycle = -1, int from = -1, int max = -1, int focus = -1)
		{
			var path = $"{SERVICE_PATH}/standings/group{ConstructStandingsURLArgs(tournamentId, cycle, from, max, focus)}";
			return WithEmptyResultsOn404(_requester.Request<TournamentStandingsResponse>(Method.GET, path));
		}

		public Promise<TournamentGroupsResponse> GetGroups(string tournamentId, int cycle = -1, int from = -1, int max = -1, int focus = -1)
		{
			var path = $"{SERVICE_PATH}/groups{ConstructStandingsURLArgs(tournamentId, cycle, from, max, focus)}";
			return WithEmptyResultsOn404(_requester.Request<TournamentGroupsResponse>(Method.GET, path));
		}
	}
}
