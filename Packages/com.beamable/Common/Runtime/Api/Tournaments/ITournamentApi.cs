using Beamable.Common.Content;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Common.Api.Tournaments
{
	/// <summary>
	/// This type defines the %Client main entry point for the %Tournaments feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/tournaments-feature-overview">Tournaments</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public interface ITournamentApi
	{
		/// <summary>
		/// Find the first <see cref="TournamentInfo"/> that matches the given tournament content id.
		/// </summary>
		/// <param name="tournamentContentId">A tournament content id.</param>
		/// <returns>
		/// A <see cref="Promise{T}"/> containing the first <see cref="TournamentInfo"/> whose <see cref="TournamentInfo.contentId"/> matches the <see cref="tournamentContentId"/>
		/// </returns>
		Promise<TournamentInfo> GetTournamentInfo(string tournamentContentId);

		/// <summary>
		/// Find all the tournaments that match given criteria.
		/// </summary>
		/// <param name="contentId">If included, only tournaments that were created from the given tournament content id will be included.</param>
		/// <param name="cycle">If included, only tournaments with a matching <see cref="TournamentInfo.cycle"/> will be included. </param>
		/// <param name="isRunning">If included, only tournaments that are running will be included.</param>
		/// <returns>A <see cref="Promise"/> containing the <see cref="TournamentInfoResponse"/> will all the matching <see cref="TournamentInfo"/> objects.</returns>
		Promise<TournamentInfoResponse> GetAllTournaments(string contentId = null, int? cycle = null, bool? isRunning = null);

		/// <summary>
		/// Tournament champions are players who came in first across all stages and tiers for a given cycle.
		/// This method will fetch the latest cycle winners.
		/// Use the <see cref="GetStandings"/> method to get the standings for the current player's stage and tier.
		/// </summary>
		/// <param name="tournamentId">The runtime id of a tournament.</param>
		/// <param name="cycleLimit">The number of cycles in history to look back to see champions for.</param>
		/// <returns>A <see cref="Promise"/> containing a <see cref="TournamentChampionsResponse"/> which contains the champions for the number cycles requested.</returns>
		Promise<TournamentChampionsResponse> GetChampions(string tournamentId, int cycleLimit = 30);

		/// <summary>
		/// Players participating in a tournament are segmented into different leaderboard partitions
		/// to limit the size of the leaderboard any given player sees. Global standings are the scores
		/// across all stages, tiers, and partitions.
		/// Use the <see cref="GetStandings"/> method to get the standings for the current player's stage and tier.
		/// </summary>
		/// <param name="tournamentId">The runtime id of a tournament.</param>
		/// <param name="cycle">
		/// The relative of number cycles in the past to look for the global standings.
		/// Leaving this blank, or passing 0, will get the global standings for the <i>current</i> cycle.
		/// For example, if the value is 2, the resulting global standings will be from 2 cycles ago.
		/// </param>
		/// <param name="from">How many entries from the top of the list to skip before returning data. Used with <see cref="max"/>, this can be used to page the global standings.</param>
		/// <param name="max">Limit the number of entries that will be returned. used with <see cref="from"/>, this can be used to page the global standings.</param>
		/// <param name="focus">The gamertag of a player to focus the results for. A focused response will include the given player, and surrounding scores.</param>
		/// <returns>A <see cref="Promise{T}"/> containing a <see cref="TournamentStandingsResponse"/> where the inner standings are global.</returns>
		Promise<TournamentStandingsResponse> GetGlobalStandings(string tournamentId, int cycle = -1, int from = -1,
		   int max = -1, int focus = -1);

		/// <summary>
		/// Retrieve the scores for the current player's stage and tier in the tournament.
		/// </summary>
		/// <param name="tournamentId">The runtime id of a tournament.</param>
		/// <param name="cycle">
		/// The relative of number cycles in the past to look for the standings.
		/// Leaving this blank, or passing 0, will get the standings for the <i>current</i> cycle.
		/// For example, if the value is 2, the resulting standings will be from 2 cycles ago.
		/// </param>
		/// <param name="from">How many entries from the top of the list to skip before returning data. Used with <see cref="max"/>, this can be used to page the standings.</param>
		/// <param name="max">Limit the number of entries that will be returned. used with <see cref="from"/>, this can be used to page the standings.</param>
		/// <param name="focus">The gamertag of a player to focus the results for. A focused response will include the given player, and surrounding scores.</param>
		/// <returns>A <see cref="Promise{T}"/> containing a <see cref="TournamentStandingsResponse"/> where the inner standings are relative the current player's tier and stage.</returns>
		Promise<TournamentStandingsResponse> GetStandings(string tournamentId, int cycle = -1, int from = -1,
														  int max = -1, int focus = -1);

		/// <summary>
		/// Retrieve scores for the the group members in the current player's group.
		/// </summary>
		/// <param name="tournamentId">The runtime id of a tournament.</param>
		/// <param name="cycle">
		/// The relative of number cycles in the past to look for the standings.
		/// Leaving this blank, or passing 0, will get the standings for the <i>current</i> cycle.
		/// For example, if the value is 2, the resulting standings will be from 2 cycles ago.
		/// </param>
		/// <param name="from">How many entries from the top of the list to skip before returning data. Used with <see cref="max"/>, this can be used to page the standings.</param>
		/// <param name="max">Limit the number of entries that will be returned. used with <see cref="from"/>, this can be used to page the standings.</param>
		/// <param name="focus">The gamertag of a player to focus the results for. A focused response will include the given player, and surrounding scores.</param>
		/// <returns>A <see cref="Promise{T}"/> containing a <see cref="TournamentStandingsResponse"/> where the inner standings are for the player's group members.</returns>
		Promise<TournamentStandingsResponse> GetGroupPlayers(string tournamentId, int cycle = -1, int from = -1,
															 int max = -1, int focus = -1);

		/// <summary>
		/// Retrieve the tournament scores for player groups.
		/// When groups participate in leaderboards, the group's score is the sum of all participating members in the group.
		/// </summary>
		/// <param name="tournamentId">The runtime id of a tournament.</param>
		/// <param name="cycle">
		/// The relative of number cycles in the past to look for the group standings.
		/// Leaving this blank, or passing 0, will get the standings for the <i>current</i> cycle.
		/// For example, if the value is 2, the resulting standings will be from 2 cycles ago.
		/// </param>
		/// <param name="from">How many entries from the top of the list to skip before returning data. Used with <see cref="max"/>, this can be used to page the group standings.</param>
		/// <param name="max">Limit the number of entries that will be returned. used with <see cref="from"/>, this can be used to page the group standings.</param>
		/// <param name="focus">The id of a group to focus the results for. A focused response will include the given group, and surrounding scores.</param>
		/// <returns>A <see cref="Promise{T}"/> containing a <see cref="TournamentGroupsResponse"/> where the inner standings are relative the current player's tier and stage.</returns>
		Promise<TournamentGroupsResponse> GetGroups(string tournamentId, int cycle = -1, int from = -1,
													int max = -1, int focus = -1);

		/// <summary>
		/// Retrieve a list of unclaimed rewards that the current player has earned from previous cycles of a tournament.
		/// This method can be used to show a player what rewards they will acquire before they are claimed.
		/// Use the <see cref="ClaimAllRewards"/> method to actually claim the rewards.
		/// Use the <see cref="GetPlayerStatus"/> method to identify tournaments that the player is participating in.
		/// </summary>
		/// <param name="tournamentId">The runtime id of a tournament.</param>
		/// <returns>A <see cref="Promise"/> containing a <see cref="TournamentRewardsResponse"/> that has a list of <see cref="TournamentRewardCurrency"/>s the player has earned.</returns>
		Promise<TournamentRewardsResponse> GetUnclaimedRewards(string tournamentId);

		/// <summary>
		/// Claim all rewards that the current player has earned for previous cycles of a tournament.
		/// If you need to show the rewards before claiming them, use the <see cref="GetUnclaimedRewards"/> method.
		/// Use the <see cref="GetPlayerStatus"/> method to identify tournaments that the player is participating in.
		/// </summary>
		/// <param name="tournamentId">The runtime id of a tournament.</param>
		/// <returns>A <see cref="Promise"/> containing a <see cref="TournamentRewardsResponse"/> that has a list of <see cref="TournamentRewardCurrency"/>s the player claimed.</returns>
		Promise<TournamentRewardsResponse> ClaimAllRewards(string tournamentId);

		/// <summary>
		/// A player must join a tournament before they can submit scores with the <see cref="SetScore"/> method.
		/// Once a player has joined a tournament, they will be given scores for all future cycles of the tournament.
		/// There is no way to leave a tournament.
		/// </summary>
		/// <param name="tournamentId">The runtime id of a tournament.</param>
		/// <param name="startScore">An initial score for the player. </param>
		/// <returns>A <see cref="Promise{T}"/> containing a <see cref="TournamentPlayerStatus"/> for the current player's view of the tournament.</returns>
		Promise<TournamentPlayerStatus> JoinTournament(string tournamentId, double startScore = 0);

		/// <summary>
		/// Set the tournament score for the given <see cref="dbid"/>. The player must have joined the tournament using the <see cref="JoinTournament"/>
		/// method before they can submit scores.
		/// </summary>
		/// <param name="tournamentId">The runtime id of a tournament.</param>
		/// <param name="dbid">The gamertag of the player that will have their score updated.</param>
		/// <param name="score">
		/// If the <see cref="incrementScore"/> is false (which it is by default),
		///  then the score will be the player's new score.
		/// However, if the <see cref="incrementScore"/> is true,
		///  then the score will be added to the player's existing score. Negative values would lower the player's score.
		/// </param>
		/// <param name="incrementScore">
		/// When true, the <see cref="score"/> value will be added to the player's existing score.
		/// When false, the <see cref="score"/> value will become the player's new score.
		/// </param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call.</returns>
		Promise<Unit> SetScore(string tournamentId, long dbid, double score, bool incrementScore = false);

		/// <summary>
		/// Retrieve a set of <see cref="TournamentPlayerStatus"/>s for all matching tournaments.
		/// The <see cref="TournamentPlayerStatus"/> is a view of a tournament for the current player.
		/// </summary>
		/// <param name="tournamentId">
		/// A tournament runtime id. If this is argument is provided, then the resulting list may only contain up to one
		/// <see cref="TournamentPlayerStatus"/>, which represents the current player's status on the given tournament id.
		/// </param>
		/// <param name="contentId">
		/// A tournament content id. If this argument is provided, then the resulting list may contain
		/// all <see cref="TournamentPlayerStatus"/>s for each tournament that was spawned from the given content id.
		/// </param>
		/// <param name="hasUnclaimedRewards">
		/// When true, the resulting list will only contain <see cref="TournamentPlayerStatus"/>s for tournaments that have
		/// unclaimed rewards. This can be used in conjunction with the <see cref="ClaimAllRewards"/> to claim all pending rewards for the player.
		/// </param>
		/// <returns>A <see cref="Promise{T}"/> containing a <see cref="TournamentPlayerStatusResponse"/> which contains a list of
		/// <see cref="TournamentPlayerStatus"/> for each matching tournament.
		/// </returns>
		Promise<TournamentPlayerStatusResponse> GetPlayerStatus(string tournamentId = null, string contentId = null, bool? hasUnclaimedRewards = null);

		/// <summary>
		/// Retrieve a set of <see cref="TournamentGroupStatus"/>s for all matching tournaments.
		/// The <see cref="TournamentGroupStatus"/> is a view of a tournament for the current player, for the player's group.
		/// </summary>
		/// <param name="tournamentId">
		/// A tournament runtime id. If this is argument is provided, then the resulting list may only contain up to one
		/// <see cref="TournamentGroupStatus"/>, which represents the current player's group status on the given tournament id.
		/// </param>
		/// <param name="contentId">
		/// A tournament content id. If this argument is provided, then the resulting list may contain
		/// all <see cref="TournamentGroupStatus"/>s for each tournament that was spawned from the given content id.
		/// </param>
		/// <returns></returns>
		Promise<TournamentGroupStatusResponse> GetGroupStatus(string tournamentId = null, string contentId = null);

		/// <summary>
		/// Retrieve a set of <see cref="TournamentGroupStatus"/>s for some set of groups for all tournaments
		/// that were spawned from the given content id.
		/// </summary>
		/// <param name="groupIds">
		/// A list of group ids to get <see cref="TournamentGroupStatus"/> for.
		/// There should be a <see cref="TournamentGroupStatus"/> instance per group id, per tournament.
		/// </param>
		/// <param name="contentId">
		/// A tournament content id.
		/// </param>
		/// <returns></returns>
		Promise<TournamentGroupStatusResponse> GetGroupStatuses(List<long> groupIds, string contentId);

		/// <summary>
		/// A utility function to get the alias for a given player.
		/// The alias will be found by looking at the player's public stat values.
		/// </summary>
		/// <param name="playerId">The gamertag of the player to get the alias for.</param>
		/// <param name="statName">The stat name where the alias is kept. By default, this is "alias"</param>
		/// <returns>A <see cref="Promise{T}"/> containing the player's alias. </returns>
		Promise<string> GetPlayerAlias(long playerId, string statName = "alias");

		/// <summary>
		/// A utility function to get the avatar key for a given player.
		/// The avatar key will be found by looking at the player's public stat values.
		/// </summary>
		/// <param name="playerId">The gamertag of the player to get the avatar key for.</param>
		/// <param name="statName">The stat name where the avatar key is kept. By default, this is "avatar"</param>
		/// <returns>A <see cref="Promise{T}"/> containing the player's avatar key. </returns>
		Promise<string> GetPlayerAvatar(long playerId, string statName = "avatar");
	}


	[System.Serializable]
	public class TournamentEntry
	{
		/// <summary>
		/// The gamertag of the player this entry represents
		/// </summary>
		public long playerId;

		/// <summary>
		/// The player's rank in the tournament. The lower the rank, the better the player is performing. Think of this as "first place"
		/// </summary>
		public long rank;

		/// <summary>
		/// How many stages will the player advance after the currency cycle completes? If the player is doing well, then the player will
		/// advance according to the tournament content configuration. Players can also lose stages. If a player will lose a stage, this field
		/// value will be negative.
		/// </summary>
		public int stageChange;

		/// <summary>
		/// The player's score. The score can be updated with the <see cref="ITournamentApi.SetScore"/> method.
		/// </summary>
		public double score;

		/// <summary>
		/// The <see cref="TournamentRewardCurrency"/>s that the player will be granted after the cycle completes.
		/// In order to claim the currency, the player must call <see cref="ITournamentApi.ClaimAllRewards"/>
		/// </summary>
		public List<TournamentRewardCurrency> currencyRewards;
	}

	[System.Serializable]
	public class TournamentGroupEntry
	{
		/// <summary>
		/// The group id for this entry in the tournament
		/// </summary>
		public long groupId;

		/// <summary>
		/// The group's rank in the tournament. Low ranks are best. Think of this like being in "first place".
		/// </summary>
		public long rank;

		/// <summary>
		/// How many stages will the group advance after the currency cycle completes? If the group is doing well, then the group will
		/// advance according to the tournament content configuration. Groups can also lose stages. If a group will lose a stage, this field
		/// value will be negative.
		/// </summary>
		public int stageChange;

		/// <summary>
		/// The group's score in the tournament. The group score is the sum of all participating group members.
		/// </summary>
		public double score;

		/// <summary>
		/// The rewards that will be given to all members of the group.
		/// </summary>
		public List<TournamentRewardCurrency> currencyRewards;
	}

	[System.Serializable]
	public class TournamentChampionEntry
	{
		/// <summary>
		/// The gamertag of the champion
		/// </summary>
		public long playerId;

		/// <summary>
		/// The tournament score the champion earned for the cycle
		/// </summary>
		public double score;

		/// <summary>
		/// The number of cycles ago that this entry is for
		/// </summary>
		public int cyclesPrior;
	}

	[System.Serializable]
	public class TournamentStandingsResponse
	{
		/// <summary>
		/// The current player's <see cref="TournamentEntry"/>
		/// </summary>
		public TournamentEntry me;

		/// <summary>
		/// A set of <see cref="TournamentEntry"/>s
		/// </summary>
		public List<TournamentEntry> entries;
	}

	[System.Serializable]
	public class TournamentGroupsResponse
	{
		/// <summary>
		/// The <see cref="TournamentGroupEntry"/> corresponding to the given focus.
		/// </summary>
		public TournamentGroupEntry focus;

		/// <summary>
		/// A set of <see cref="TournamentGroupEntry"/>s
		/// </summary>
		public List<TournamentGroupEntry> entries;
	}

	[System.Serializable]
	public class TournamentChampionsResponse
	{
		public List<TournamentChampionEntry> entries;
	}

	[System.Serializable]
	public class TournamentRewardCurrency
	{
		/// <summary>
		/// A currency content id
		/// </summary>
		[Tooltip(ContentObject.TooltipSymbol1)]
		public string symbol;

		/// <summary>
		/// The amount of currency that will be given for the reward.
		/// </summary>
		[Tooltip(ContentObject.TooltipAmount1)]
		public int amount;
	}

	[System.Serializable]
	public class TournamentRewardsResponse
	{
		[Tooltip(ContentObject.TooltipTournamentRewardCurrency1)]
		public List<TournamentRewardCurrency> rewardCurrencies;
	}

	[System.Serializable]
	public class TournamentJoinRequest
	{
		[Tooltip(ContentObject.TooltipId1)]
		public string tournamentId;
	}

	[System.Serializable]
	public class TournamentScoreRequest
	{
		[Tooltip(ContentObject.TooltipId1)]
		public string tournamentId;

		[Tooltip(ContentObject.TooltipPlayerDbid1)]
		public long playerId;

		[Tooltip(ContentObject.TooltipScore1)]
		public double score;

		[Tooltip(ContentObject.TooltipIncrement1)]
		public bool increment;
		//TODO: Add optional stats to set score request
	}

	[System.Serializable]
	public class TournamentInfo
	{
		/// <summary>
		/// The runtime id of the tournament.
		/// </summary>
		[Tooltip(ContentObject.TooltipId1)]
		public string tournamentId;

		/// <summary>
		/// The tournament content id that created this tournament.
		/// </summary>
		[Tooltip(ContentObject.TooltipId1)]
		public string contentId;

		/// <summary>
		/// The number of seconds remaining before the next cycle of the tournament begins.
		/// </summary>
		[Tooltip(ContentObject.TooltipSecondsRemaining1)]
		public long secondsRemaining;

		/// <summary>
		/// The current cycle that the tournament is running. A tournament is a repeating event, and it repeats after a fixed amount of time.
		/// Each time the tournament repeats, the cycle increases by 1.
		/// </summary>
		[Tooltip(ContentObject.TooltipCycle1)]
		public int cycle;

		/// <summary>
		/// The start date of the cycle
		/// </summary>
		[Tooltip(ContentObject.TooltipStartDate1 + ContentObject.TooltipStartDate2)]
		public string startTimeUtc;

		/// <summary>
		/// The end date of the cycle
		/// </summary>
		[Tooltip(ContentObject.TooltipEndDate1 + ContentObject.TooltipEndDate2)]
		public string endTimeUtc;
	}

	[System.Serializable]
	public class TournamentInfoResponse
	{
		[Tooltip(ContentObject.TooltipTournamentInfo1)]
		public List<TournamentInfo> tournaments;
	}

	[System.Serializable]
	public class TournamentPlayerStatus
	{
		/// <summary>
		/// The tournament content id that spawned this tournament.
		/// </summary>
		[Tooltip(ContentObject.TooltipId1)]
		public string contentId;

		/// <summary>
		/// The runtime id of the tournament.
		/// </summary>
		[Tooltip(ContentObject.TooltipId1)]
		public string tournamentId;

		/// <summary>
		/// The gamertag of this player's tournament status.
		/// </summary>
		[Tooltip(ContentObject.TooltipPlayerDbid1)]
		public long playerId;

		/// <summary>
		/// The tournament tier the player is participating on.
		/// If the player does well, they will gain stages.
		/// If a player does poorly, they will loose stages.
		/// Once a player has reached the final stage for their tier, they will advance a tier.
		/// </summary>
		[Tooltip(ContentObject.TooltipTier1)]
		public int tier;

		/// <summary>
		/// The tournament stage the player is participating on.
		/// There are multiple stages per tier.
		/// If the player does well, they will gain stages.
		/// If a player does poorly, they will loose stages.
		/// </summary>
		[Tooltip(ContentObject.TooltipStage1)]
		public int stage;

		/// <summary>
		/// The group id that the player is working for in the tournament. If the player isn't
		/// participating in a group, this will be 0.
		/// </summary>
		[Tooltip(ContentObject.TooltipId1)]
		public long groupId;

		/// <summary>
		/// A set of <see cref="TournamentRewardCurrency"/> that the player would earn at the end of the tournament cycle.
		/// These rewards aren't allowed to be claimed until the end of the cycle. If a player's score is overtaken at the
		/// end of a tournament, the rewards may change. Use the <see cref="ITournamentApi.GetUnclaimedRewards"/> method to see the final rewards.
		/// </summary>
		[Tooltip(ContentObject.TooltipTournamentRewardCurrency1)]
		public List<TournamentRewardCurrency> unclaimedRewards;
	}

	[System.Serializable]
	public class TournamentGroupStatus
	{
		[Tooltip(ContentObject.TooltipId1)]
		public long groupId;

		[Tooltip(ContentObject.TooltipId1)]
		public string contentId;

		[Tooltip(ContentObject.TooltipId1)]
		public string tournamentId;

		[Tooltip(ContentObject.TooltipTier1)]
		public int tier;

		[Tooltip(ContentObject.TooltipStage1)]
		public int stage;
	}

	[System.Serializable]
	public class TournamentScoreResponse
	{
		[Tooltip(ContentObject.TooltipResult1)]
		public string result;
	}

	[System.Serializable]
	public class TournamentPlayerStatusResponse
	{
		[Tooltip(ContentObject.TooltipStatus)]
		public List<TournamentPlayerStatus> statuses;
	}

	[System.Serializable]
	public class TournamentGroupStatusResponse
	{
		[Tooltip(ContentObject.TooltipStatus)]
		public List<TournamentGroupStatus> statuses;
	}

	[System.Serializable]
	public class TournamentGetStatusesRequest
	{
		[Tooltip(ContentObject.TooltipId1)]
		public List<long> groupIds;

		[Tooltip(ContentObject.TooltipId1)]
		public string contentId;
	}
}
