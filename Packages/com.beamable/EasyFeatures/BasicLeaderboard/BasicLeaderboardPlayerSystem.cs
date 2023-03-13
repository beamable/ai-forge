using Beamable;
using Beamable.Api.Leaderboard;
using Beamable.Avatars;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Leaderboards;
using Beamable.Common.Dependencies;
using Beamable.Common.Leaderboards;
using Beamable.Modules.Leaderboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

namespace Beamable.EasyFeatures.BasicLeaderboard
{
	/// <summary>
	/// This is our basic leaderboard system --- It exposes some APIs to fetch data from the backend and rearranges that API with basic information from our backend into a
	/// format more easily usable by UI.
	/// It gets <see cref="RankEntry"/>s from our platform, parses them, loads data relevant and caches that data in a format that's easier to work with for the things we want to do. In this case,
	/// it simply caches the leaderboards names, avatar sprites, rank and score values in sequential parallel list.
	/// </summary>
	public class BasicLeaderboardPlayerSystem : BasicLeaderboardView.ILeaderboardDeps
	{
		public virtual IEnumerable<BasicLeaderboardView.BasicLeaderboardViewEntry> Entries
		{
			get
			{
				var entryCount = Ranks.Count;
				for (int i = 0; i < entryCount; i++)
					yield return new BasicLeaderboardView.BasicLeaderboardViewEntry() { Alias = Aliases[i], Rank = Ranks[i], Score = Scores[i], Avatar = Avatars[i] };
			}
		}

		public virtual IReadOnlyList<string> Aliases => PerLeaderboardAliases[FocusedLeaderboardId];
		public virtual IReadOnlyList<long> Ranks => PerLeaderboardRanks[FocusedLeaderboardId];
		public virtual IReadOnlyList<double> Scores => PerLeaderboardScores[FocusedLeaderboardId];
		public virtual IReadOnlyList<Sprite> Avatars => PerLeaderboardAvatars[FocusedLeaderboardId];
		public virtual int PlayerIndexInLeaderboard => PerLeaderboardUserIdx[FocusedLeaderboardId].HasValue ? PerLeaderboardUserIdx[FocusedLeaderboardId].Value : -1;

		public virtual string PlayerAlias => Aliases[PlayerIndexInLeaderboard];
		public virtual long PlayerRank => Ranks[PlayerIndexInLeaderboard];
		public virtual double PlayerScore => Scores[PlayerIndexInLeaderboard];
		public virtual Sprite PlayerAvatar => Avatars[PlayerIndexInLeaderboard];

		public string FocusedLeaderboardId;

		public readonly Dictionary<string, List<string>> PerLeaderboardAliases;
		public readonly Dictionary<string, List<Sprite>> PerLeaderboardAvatars;
		public readonly Dictionary<string, List<long>> PerLeaderboardRanks;
		public readonly Dictionary<string, List<double>> PerLeaderboardScores;
		public readonly Dictionary<string, int?> PerLeaderboardUserIdx;

		/// <summary>
		/// Reference to the current user's data.
		/// </summary>
		protected readonly IUserContext _userContext;

		/// <summary>
		/// Reference to our platform's leaderboard API.
		/// </summary>
		protected readonly LeaderboardService _leaderboardService;

		/// <summary>
		/// Constructs with the appropriate dependencies. Is injected by <see cref="BeamContext"/> dependency injection framework.
		/// </summary>
		public BasicLeaderboardPlayerSystem(LeaderboardService leaderboardService, IUserContext ctx)
		{
			_leaderboardService = leaderboardService;
			_userContext = ctx;

			FocusedLeaderboardId = string.Empty;

			PerLeaderboardAliases = new Dictionary<string, List<string>>();
			PerLeaderboardRanks = new Dictionary<string, List<long>>();
			PerLeaderboardScores = new Dictionary<string, List<double>>();
			PerLeaderboardAvatars = new Dictionary<string, List<Sprite>>();
			PerLeaderboardUserIdx = new Dictionary<string, int?>();
		}

		public virtual async Promise FetchLeaderboardData(string leaderboardId, int firstEntryId, int entriesAmount, bool focus = true, Action<BasicLeaderboardView.ILeaderboardDeps> onComplete = null)
		{
			var userRankEntry = await _leaderboardService.GetUser(leaderboardId, _userContext.UserId);
			var leaderBoardView = await _leaderboardService.GetBoard(leaderboardId, firstEntryId, firstEntryId + entriesAmount);

			var rankEntries = leaderBoardView.ToList();
			RegisterLeaderboardEntries(leaderboardId, rankEntries, userRankEntry);

			FocusedLeaderboardId = focus ? leaderboardId : FocusedLeaderboardId;

			onComplete?.Invoke(this);
		}

		public virtual async Promise FetchLeaderboardData(LeaderboardRef leaderboardRef,
														  int firstEntryId,
														  int entriesAmount,
														  bool focus = true,
														  Action<BasicLeaderboardView.ILeaderboardDeps> onComplete = null) =>
			await FetchLeaderboardData(leaderboardRef.Id, firstEntryId, entriesAmount, focus, onComplete);

		public virtual void RegisterLeaderboardEntries(LeaderboardRef leaderboardRef, List<RankEntry> rankEntries, RankEntry userRankEntry) =>
			RegisterLeaderboardEntries(leaderboardRef.Id, rankEntries, userRankEntry);

		public virtual void RegisterLeaderboardEntries(string leaderboardId, List<RankEntry> rankEntries, RankEntry userRankEntry)
		{
			_ = PerLeaderboardAliases.TryGetValue(leaderboardId, out var aliases);
			_ = PerLeaderboardRanks.TryGetValue(leaderboardId, out var ranks);
			_ = PerLeaderboardScores.TryGetValue(leaderboardId, out var scores);
			_ = PerLeaderboardAvatars.TryGetValue(leaderboardId, out var avatars);
			_ = PerLeaderboardUserIdx.TryGetValue(leaderboardId, out var userRank);

			BuildLeaderboardClientData(rankEntries, userRankEntry, ref aliases, ref ranks, ref scores, ref avatars, ref userRank);

			if (PerLeaderboardAliases.ContainsKey(leaderboardId))
				PerLeaderboardAliases[leaderboardId] = aliases;
			else
				PerLeaderboardAliases.Add(leaderboardId, aliases);

			if (PerLeaderboardRanks.ContainsKey(leaderboardId))
				PerLeaderboardRanks[leaderboardId] = ranks;
			else
				PerLeaderboardRanks.Add(leaderboardId, ranks);

			if (PerLeaderboardScores.ContainsKey(leaderboardId))
				PerLeaderboardScores[leaderboardId] = scores;
			else
				PerLeaderboardScores.Add(leaderboardId, scores);

			if (PerLeaderboardAvatars.ContainsKey(leaderboardId))
				PerLeaderboardAvatars[leaderboardId] = avatars;
			else
				PerLeaderboardAvatars.Add(leaderboardId, avatars);

			if (PerLeaderboardUserIdx.ContainsKey(leaderboardId))
				PerLeaderboardUserIdx[leaderboardId] = userRank;
			else
				PerLeaderboardUserIdx.Add(leaderboardId, userRank);
		}

		public virtual void ClearLeaderboardData(LeaderboardRef leaderboardRef) => ClearLeaderboardData(leaderboardRef.Id);

		public virtual void ClearLeaderboardData(string leaderboardId)
		{
			PerLeaderboardAliases.Remove(leaderboardId);
			PerLeaderboardRanks.Remove(leaderboardId);
			PerLeaderboardScores.Remove(leaderboardId);
			PerLeaderboardAvatars.Remove(leaderboardId);
			PerLeaderboardUserIdx.Remove(leaderboardId);
		}

		/// <summary>
		/// The actual data transformation function that converts rank entries into data that is relevant for our <see cref="BasicLeaderboardView.ILeaderboardDeps"/>. 
		/// </summary>
		public virtual void BuildLeaderboardClientData(List<RankEntry> rankEntries,
													   RankEntry userRankEntry,
													   ref List<string> aliases,
													   ref List<long> ranks,
													   ref List<double> scores,
													   ref List<Sprite> avatars,
													   ref int? userRank)
		{
			void GuaranteeInitList<T>(ref List<T> toInit)
			{
				if (toInit != null) toInit.Clear();
				else toInit = new List<T>();
			}

			GuaranteeInitList(ref aliases);
			aliases.AddRange(rankEntries.Select(re => re.GetStat("alias") == null ? "Null Alias" : re.GetStat("alias")));

			GuaranteeInitList(ref ranks);
			ranks.AddRange(rankEntries.Select(re => re.rank));

			GuaranteeInitList(ref scores);
			scores.AddRange(rankEntries.Select(re => re.score));

			GuaranteeInitList(ref avatars);
			avatars.AddRange(rankEntries.Select(re => re.GetStat("avatar")).Select(GetAvatar));

			userRank = rankEntries.FindIndex(r => r.rank == userRankEntry.rank);
		}

		/// <summary>
		/// Just a helper that gets a reference to the avatar image that matches the given id. 
		/// </summary>
		protected virtual Sprite GetAvatar(string id)
		{
			var accountAvatar = AvatarConfiguration.Instance.Avatars.FirstOrDefault(av => av.Name == id);
			return accountAvatar != null ? accountAvatar.Sprite : AvatarConfiguration.Instance.Default.Sprite;
		}
	}
}
