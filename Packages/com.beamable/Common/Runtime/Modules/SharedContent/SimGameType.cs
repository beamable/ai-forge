using Beamable.Common.Content.Validation;
using Beamable.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
#pragma warning disable CS0618

namespace Beamable.Common.Content
{
	/// <summary>
	/// This type defines a methodology for resolving a reference to a %Beamable %ContentObject.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code#contentlink-vs-contentref">ContentLink vs ContentRef</a> documentation
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[System.Serializable]
	public class SimGameTypeLink : ContentLink<SimGameType> { }

	/// <summary>
	/// This type defines a methodology for resolving a reference to a %Beamable %ContentObject.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code#contentlink-vs-contentref">ContentLink vs ContentRef</a> documentation
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class SimGameTypeRef : ContentRef<SimGameType> { }

	/// <summary>
	/// This type defines a %Beamable %ContentObject subclass for the %Multiplayer feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Content.ContentObject script reference
	/// - See Beamable.Experimental.Api.Matchmaking.MatchmakingService script reference
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/multiplayer-feature">Multiplayer</a> feature documentation
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[ContentType("game_types")]
	[Serializable]
	[Agnostic]
	public class SimGameType : ContentObject
	{
		[Obsolete("Use `CalculateMaxPlayers` instead.")]
		[IgnoreContentField]
		[HideInInspector]
		public int maxPlayers;

		//Hidden, so no tooltip needed
		[FormerlySerializedAs("teams")]
		[SerializeField]
		[HideInInspector]
		[IgnoreContentField]
		public List<TeamContent> legacyTeams;

		[Tooltip(ContentObject.TooltipTeamContent1)]
		[CannotBeEmpty]
		public TeamContentList teams;

		[Tooltip(ContentObject.TooltipMatchmakingRule1)]
		public List<NumericMatchmakingRule> numericRules;

		[Tooltip(ContentObject.TooltipMatchmakingRule1)]
		public List<StringMatchmakingRule> stringRules;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipWaitAfterMinReachedSecs1)]
		[MustBePositive]
		public OptionalInt waitAfterMinReachedSecs;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipWaitDurationSecsMax1)]
		[MustBePositive]
		public OptionalInt maxWaitDurationSecs;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipMatchingIntervalSecs1)]
		[MustBePositive]
		public OptionalInt matchingIntervalSecs;

		[Tooltip(ContentObject.TooltipLeaderboardUpdate1)]
		public List<LeaderboardUpdate> leaderboardUpdates;

		[Tooltip(ContentObject.TooltipRewardsPerRank1)]
		public List<RewardsPerRank> rewards;

		public int CalculateMaxPlayers()
		{
			int sum = 0;
			foreach (var team in teams.listData)
			{
				sum += team.maxPlayers;
			}

			return sum;
		}

		public void OnBeforeSerialize()
		{
			// never save the legacy teams...
			legacyTeams = null;
		}

		public void OnAfterDeserialize()
		{
			// if anything is in the legacy teams, move them into the new list.
			if (legacyTeams != null && legacyTeams.Count > 0)
			{
				teams = new TeamContentList()
				{
					listData = legacyTeams.ToList()
				};
			}

			legacyTeams = null;
		}
	}

	[Serializable]
	public class TeamContentList : DisplayableList<TeamContent>
	{
		public List<TeamContent> listData = new List<TeamContent>();

		protected override IList InternalList => listData;
		public override string GetListPropertyPath() => nameof(listData);
	}

	[Serializable]
	public class TeamContent
	{
		[Tooltip(ContentObject.TooltipName1)]
		public string name;

		[Tooltip(ContentObject.TooltipPlayersMax1)]
		[MustBePositive]
		public int maxPlayers;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipPlayersMin1)]
		[MustBePositive(allowZero: true)]
		public OptionalInt minPlayers;
	}

	[Serializable]
	public class NumericMatchmakingRule
	{
		[Tooltip(ContentObject.TooltipProperty1)]
		public string property;

		[Tooltip(ContentObject.TooltipDeltaMax1)]
		[MustBePositive]
		public double maxDelta;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.DefaultValue1)]
		public OptionalDouble Default;
	}

	[Serializable]
	public class StringMatchmakingRule
	{
		[Tooltip(ContentObject.TooltipProperty1)]
		public string property;

		[Tooltip(ContentObject.TooltipValue1)]
		public string value;
	}

	[Serializable]
	[Agnostic]
	public class RewardsPerRank
	{
		[Tooltip(ContentObject.TooltipStartRank1)]
		[MustBeNonNegative]
		public int startRank;

		[Tooltip(ContentObject.TooltipEndRank1)]
		[MustBeNonNegative]
		public int endRank;

		[Tooltip(ContentObject.TooltipReward1)]
		public List<Reward> rewards;
	}

	[System.Serializable]
	[Agnostic]
	public class Reward
	{
		[Tooltip(ContentObject.TooltipRewardType1)]
		public RewardType type;

		[Tooltip(ContentObject.TooltipName1)]
		[MustBeCurrency]
		// TODO: This should be a CurrencyRef but the serialization isn't supported on the backend.
		public string name;

		[Tooltip(ContentObject.TooltipAmount1)]
		public long amount;
	}

	[System.Serializable]
	[Agnostic]
	public enum RewardType
	{
		Currency
	}

	[System.Serializable]
	[Agnostic]
	public class LeaderboardUpdate
	{
		[Tooltip(ContentObject.TooltipLeaderboard1)]
		// TODO: This should be a LeaderboardRef but the serialization isn't supported on the backend.
		[MustBeLeaderboard]
		public string leaderboard;

		[Tooltip(ContentObject.TooltipScoringAlgorithm1)]
		public ScoringAlgorithm scoringAlgorithm;
	}

	[System.Serializable]
	[Agnostic]
	public class ScoringAlgorithm
	{
		[Tooltip(ContentObject.TooltipScoringAlgorithm1)]
		public AlgorithmType algorithm;

		[Tooltip(ContentObject.TooltipScoringAlgorithmOption1)]
		public List<ScoringAlgoOption> options; // TODO: [MustBeUnique(nameof(ScoringAlgoOption.key))]
	}

	[System.Serializable]
	[Agnostic]
	public class ScoringAlgoOption
	{
		[Tooltip(ContentObject.TooltipKey1)]
		[CannotBeBlank] // TODO: Add [MustBeUniqueInArray]
		public string key;

		[Tooltip(ContentObject.TooltipValue1)]
		public string value;
	}

	[System.Serializable]
	[Agnostic]
	public enum AlgorithmType
	{
		MultiplayerElo
	}
}
