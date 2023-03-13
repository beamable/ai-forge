using Beamable.Common;
using Beamable.Common.Api.Leaderboards;
using System.Collections.Generic;
using UnityEngine;
using static Beamable.Common.Constants.Features.Leaderboards;

namespace Beamable.Modules.Leaderboards
{
	public static class LeaderboardsModelHelper
	{
		public static RankEntry GenerateCurrentUserRankEntryTestData(string statKey, string statDefaultValue)
		{
			RankEntryStat[] stats = { new RankEntryStat { name = statKey, value = statDefaultValue } };

			return new RankEntry
			{
				gt = (long)Random.Range(0, TEST_DATA_MAX_GAMER_TAG),
				rank = 1,
				score = (long)Random.Range(0, TEST_DATA_MAX_SCORE),
				stats = stats
			};
		}

		public static Promise<List<RankEntry>> GetTestData(int firstEntryId,
														   int lastEntryId,
														   RankEntry currentUserRankEntry,
														   string statKey,
														   string defaultValue)
		{
			List<RankEntry> rankEntries =
				GenerateLeaderboardsTestData(firstEntryId, lastEntryId, currentUserRankEntry, statKey, defaultValue);

			Promise<List<RankEntry>> promise = new Promise<List<RankEntry>>();
			promise.CompleteSuccess(rankEntries);

			return promise;
		}

		public static List<RankEntry> GenerateLeaderboardsTestData(int firstId,
																	int lastId,
																	RankEntry currentUserEntry,
																	string statKey,
																	string statDefaultValue)
		{
			List<RankEntry> entries = new List<RankEntry>();

			for (int i = 0; i < lastId - firstId; i++)
			{
				int currentRank = firstId + i;

				if (currentRank == currentUserEntry.rank)
				{
					entries.Add(currentUserEntry);
				}
				else
				{
					RankEntryStat[] stats =
					{
						new RankEntryStat {name = statKey, value = $"{statDefaultValue} {currentRank}"}
					};

					entries.Add(new RankEntry
					{
						rank = currentRank,
						score = (long)Random.Range(0, TEST_DATA_MAX_SCORE),
						stats = stats
					});
				}
			}

			return entries;
		}
	}
}
