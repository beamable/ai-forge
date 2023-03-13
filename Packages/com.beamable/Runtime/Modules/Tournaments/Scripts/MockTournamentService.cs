using Beamable.Common;
using Beamable.Common.Api.Tournaments;
using Beamable.Common.Tournaments;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Tournaments
{
	public class MockTournamentService : ITournamentApi
	{
		private const float LATENCY_MIN = .2f;
		private const float LATENCY_MAX = .7f;

		public TournamentRef ContentRef;

		readonly TournamentInfo Info = new TournamentInfo
		{
			tournamentId = "mockTournament",
			contentId = "mockContent",
			secondsRemaining = (5 * 60) + (1 * 60 * 60)
		};

		private readonly TournamentPlayerStatus Status = new TournamentPlayerStatus
		{
			playerId = 0,
			stage = 6,
			tier = 0,
			contentId = "mockContent"
		};

		private readonly List<TournamentRewardCurrency> Rewards = new List<TournamentRewardCurrency>
	  {
		 new TournamentRewardCurrency {amount = 5000, symbol = "currency.gems"},
		 new TournamentRewardCurrency {amount = 15000, symbol = "currency.gems"},
		 new TournamentRewardCurrency {amount = 20, symbol = "currency.gems"},
		 new TournamentRewardCurrency {amount = 833224, symbol = "currency.gems"},
		 new TournamentRewardCurrency {amount = 5000, symbol = "currency.gems"},
	  };

		private readonly string[] Names = new[]
		{
		 "Rocky",
		 "Bullwinkle",
		 "Natasha",
		 "Boris",
		 "Fearless Leader",
		 "Narrator",
	  };

		private float RandomLatencyTime => UnityEngine.Random.Range(LATENCY_MIN, LATENCY_MAX);

		public Promise<TournamentInfo> GetTournamentInfo(string tournamentContentId)
		{
			return Promise<TournamentInfo>.Successful(Info).WaitForSeconds(RandomLatencyTime);
		}

		public Promise<TournamentInfoResponse> GetAllTournaments(string contentId = null, int? cycle = null, bool? isRunning = null)
		{
			return Promise<TournamentInfoResponse>.Successful(new TournamentInfoResponse
			{
				tournaments = new List<TournamentInfo> { Info }
			}).WaitForSeconds(RandomLatencyTime);
		}

		public Promise<TournamentChampionsResponse> GetChampions(string tournamentId, int cycleLimit = 30)
		{
			if (!string.Equals(tournamentId, Info.tournamentId)) throw new Exception("Invalid tournament id for mock");


			var entries = new List<TournamentChampionEntry>();
			for (var i = 0; i < cycleLimit; i++)
			{

				entries.Add(new TournamentChampionEntry
				{
					playerId = i,
					cyclesPrior = i,
					score = (100 - i) * (1234 + i),
				});
			}


			return Promise<TournamentChampionsResponse>.Successful(new TournamentChampionsResponse
			{
				entries = entries,
			}).WaitForSeconds(RandomLatencyTime);
		}

		public Promise<TournamentStandingsResponse> GetGlobalStandings(string tournamentId, int cycle = -1, int @from = -1, int max = -1, int focus = -1)
		{
			return GetStandings(tournamentId, cycle);
		}

		public Promise<TournamentStandingsResponse> GetStandings(string tournamentId, int cycle = -1, int @from = -1, int max = -1, int focus = -1)
		{
			if (!string.Equals(tournamentId, Info.tournamentId)) throw new Exception("Invalid tournament id for mock");


			return ContentRef.Resolve().FlatMap(content =>
			{
				var entries = new List<TournamentEntry>();
				for (var i = 0; i < content.playerLimit; i++)
				{
					var rewards = new List<TournamentRewardCurrency>();
					for (var j = 0; j < UnityEngine.Random.Range(0, 3); j++)
					{
						rewards.Add(new TournamentRewardCurrency
						{
							amount = (100 - i) * 10050,
							symbol = "currency.gems"
						});
					}

					var rank = (i + 1);
					var stage = content.stageChanges.FirstOrDefault(x => x.AcceptsRank(rank));
					var stageChange = 0;
					if (stage != null)
					{
						stageChange = stage.delta;
					}


					entries.Add(new TournamentEntry
					{
						playerId = i,
						rank = rank,
						score = (100 - i) * (1234 + i),
						currencyRewards = rewards,
						stageChange = stageChange
					});
				}

				var self = entries[content.playerLimit / 2];

				return Promise<TournamentStandingsResponse>.Successful(new TournamentStandingsResponse
				{
					entries = entries,
					me = self
				});

			}).WaitForSeconds(RandomLatencyTime);
		}

		public Promise<TournamentRewardsResponse> GetUnclaimedRewards(string tournamentId)
		{
			if (!string.Equals(tournamentId, Info.tournamentId)) throw new Exception("Invalid tournament id for mock");

			return Promise<TournamentRewardsResponse>.Successful(new TournamentRewardsResponse
			{
				rewardCurrencies = Rewards
			}).WaitForSeconds(RandomLatencyTime);
		}

		public Promise<TournamentRewardsResponse> ClaimAllRewards(string tournamentId)
		{
			return GetUnclaimedRewards(tournamentId);
		}

		public Promise<TournamentPlayerStatus> JoinTournament(string tournamentId, double startScore = 0)
		{
			throw new System.NotImplementedException();
		}

		public Promise<Unit> SetScore(string tournamentId, long dbid, double score, bool incrementScore = false)
		{
			throw new System.NotImplementedException();
		}

		public Promise<bool> HasJoinedTournament(string tournamentId)
		{
			throw new System.NotImplementedException();
		}

		public Promise<TournamentPlayerStatusResponse> GetPlayerStatus(string tournamentId = null, string contentId = null, bool? hasUnclaimedRewards = null)
		{
			return Promise<TournamentPlayerStatusResponse>.Successful(new TournamentPlayerStatusResponse
			{
				statuses = new List<TournamentPlayerStatus> { Status }
			}).WaitForSeconds(RandomLatencyTime);
		}


		public Promise<string> GetPlayerAlias(long playerId, string statName = "alias")
		{
			var picked = Names[(int)Mathf.Abs(playerId) % Names.Length] + "_" + (int)((playerId * 100));
			return Promise<string>.Successful(picked);
		}

		public Promise<string> GetPlayerAvatar(long playerId, string statName = "avatar")
		{
			return Promise<string>.Successful("" + (1 + (playerId + 5) % 6));
		}

		public Promise<TournamentStandingsResponse> GetGroupPlayers(string tournamentId, int cycle = -1, int from = -1, int max = -1, int focus = -1)
		{
			throw new NotImplementedException();
		}

		public Promise<TournamentGroupsResponse> GetGroups(string tournamentId, int cycle = -1, int from = -1, int max = -1, int focus = -1)
		{
			throw new NotImplementedException();
		}

		public Promise<TournamentGroupStatusResponse> GetGroupStatus(string tournamentId, string contentId)
		{
			throw new NotImplementedException();
		}

		public Promise<TournamentGroupStatusResponse> GetGroupStatuses(List<long> groupIds, string contentId)
		{
			throw new NotImplementedException();
		}
	}
}
