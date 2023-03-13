using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Common.Inventory;
using Beamable.Content;
using System;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0618

namespace Beamable.Common.Tournaments
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
	[Agnostic]
	public class TournamentLink : ContentLink<TournamentContent> { }

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
	[Agnostic]
	public class TournamentRef : ContentRef<TournamentContent> { }

	[System.Serializable]
	[Agnostic]
	public class TournamentRankReward
	{
		[Tooltip(ContentObject.TooltipName1)]
		public string name;

		[Tooltip("The index of the tier you want, in the tiers array")]
		public int tier; // should line up with tiers

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipStageMin1)]
		[MustBeNonNegative]
		public OptionalInt stageMin;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipStageMax1)]
		[MustBeNonNegative]
		public OptionalInt stageMax;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipRankMin1)]
		[MustBePositive]
		public OptionalInt minRank;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipRankMax1)]
		[MustBePositive]
		public OptionalInt maxRank;

		[Tooltip(ContentObject.TooltipCurrencyAmount1)]
		public List<CurrencyAmount> currencyRewards;
	}

	[System.Serializable]
	[Agnostic]
	public class TournamentScoreReward
	{
		[Tooltip(ContentObject.TooltipName1)]
		public string name;

		[Tooltip("The index of the tier you want, in the tiers array")]
		public int tier; // should line up with tiers

		[Tooltip(ContentObject.TooltipScoreMin1)]
		[MustBePositive]
		public double minScore;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipScoreMax1)]
		[MustBePositive]
		public OptionalDouble maxScore;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipStageMin1)]
		[MustBeNonNegative]
		public OptionalInt stageMin;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipStageMax1)]
		[MustBeNonNegative]
		public OptionalInt stageMax;

		[Tooltip(ContentObject.TooltipCurrencyAmount1)]
		public List<CurrencyAmount> currencyRewards;
	}

	[System.Serializable]
	[Agnostic]
	public class TournamentGroupRewards
	{
		[Tooltip(ContentObject.TooltipTournamentRankReward1)]
		public List<TournamentRankReward> rankRewards;
	}

	[System.Serializable]
	[Agnostic]
	public class TournamentStageChange
	{
		[Tooltip(ContentObject.TooltipRankMin1)]
		[MustBePositive]
		public int minRank;

		[Tooltip(ContentObject.TooltipRankMax1)]
		[MustBePositive]
		public int maxRank;

		[Tooltip(ContentObject.TooltipDelta1)]
		[Range(-1, 2)]
		public int delta;

		[Tooltip(ContentObject.TooltipColor1)]
		public Color color;

		public bool AcceptsRank(long rank)
		{
			return rank >= minRank && rank <= maxRank;
		}
	}

	[System.Serializable]
	[Agnostic]
	public class TournamentTier
	{
		[Tooltip(ContentObject.TooltipName1)]
		[CannotBeBlank]
		public string name;

		[Tooltip(ContentObject.TooltipColor1)]
		public Color color;
	}

	/// <summary>
	/// This type defines a %Beamable %ContentObject subclass for an %TournamentService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Content.ContentObject script reference
	/// - See Beamable.Api.Tournaments.TournamentService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[ContentType("tournaments")]
	[System.Serializable]
	[Agnostic(new[] { typeof(TournamentColorConstants) })]
	public class TournamentContent : ContentObject
	{
		[Tooltip(ContentObject.TooltipName1)]
		public new string name = "sample";

		/*
         * every day relative from August 11th, 2021 2:05PM
         * every 3 hours relative from August 11th, 1999 2:05PM
         * every day at 2:05PM
         * every 3 days relative to August 11th, ????
         * every week relative to August 19th 2020, 1:00 PM (UTC)
         */

		//       [Tooltip("Cron-like string. https://crontab.guru/")]
		//       public string schedule = "30 14 * * *";
		//

		[Tooltip("ISO UTC Anchor time. From what time are the cycles relative to?")]
		[MustBeDateString]
		public string anchorTimeUTC = "2020-01-01T12:00:00Z";

		[IgnoreContentField]
		[SerializeField]
		[TimeSpanDisplay(nameof(cycleDuration))]
		private int cycleText;

		[Tooltip("ISO duration string. How long does each tournament cycle last? Default is 1 Day.")]
		[MustBeTimeSpanDuration]
		public string cycleDuration = "P1D";

		[Tooltip("The number of players allowed to be in each stage")]
		[MustBePositive]
		public int playerLimit;

		[Tooltip("The names of the stages, from worst to best")]
		public List<TournamentTier> tiers = new List<TournamentTier>
		{
			new TournamentTier {color = TournamentColorConstants.BRONZE, name = "Bronze"},
			new TournamentTier {color = TournamentColorConstants.SILVER, name = "Silver"},
			new TournamentTier {color = TournamentColorConstants.GOLD, name = "Gold"}
		};

		[Tooltip("The stages per tier")]
		[MustBePositive]
		public int stagesPerTier;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipDivider +
				 "The number of a stages a player regresses if they do not participate in a cycle.")]
		[MustBePositive]
		public OptionalInt passiveDecayStages;

		[Tooltip(ContentObject.TooltipColor1 + ContentObject.TooltipDivider + " For the default entry")]
		public Color DefaultEntryColor = TournamentColorConstants.RED; // TODO: Add Purple color

		[Tooltip(ContentObject.TooltipColor1 + ContentObject.TooltipDivider + " For the champion entry")]
		public Color ChampionColor = TournamentColorConstants.GOLD;

		[Tooltip(ContentObject.TooltipTheListOf1 + nameof(TournamentStageChange) + "s")]
		public List<TournamentStageChange> stageChanges = new List<TournamentStageChange>
		{
            //1-3 goes up 2stages, 4-10 goes up 1 stage, and 11-40 stay same. 41-50 goes down 1 stage.
            new TournamentStageChange {minRank = 1, maxRank = 1, delta = 2, color = TournamentColorConstants.GOLD},
			new TournamentStageChange {minRank = 2, maxRank = 2, delta = 2, color = TournamentColorConstants.SILVER},
			new TournamentStageChange {minRank = 3, maxRank = 3, delta = 2, color = TournamentColorConstants.BRONZE},
			new TournamentStageChange {minRank = 4, maxRank = 10, delta = 1},
			new TournamentStageChange {minRank = 11, maxRank = 20, delta = 0, color = TournamentColorConstants.GREY},
			new TournamentStageChange {minRank = 41, maxRank = 50, delta = -1, color = TournamentColorConstants.RED}
		};

		[Tooltip(ContentObject.TooltipTheListOf1 + nameof(TournamentRankReward) + "s")]
		public List<TournamentRankReward> rankRewards = new List<TournamentRankReward>
		{
			new TournamentRankReward
			{
				name = "Winner",
				tier = 0,
				minRank = new OptionalInt { HasValue = true, Value = 1},
				maxRank = new OptionalInt { HasValue = true, Value = 1},
				stageMin = new OptionalInt { HasValue = true, Value = 1},
				stageMax = new OptionalInt { HasValue = true, Value = 1},
				currencyRewards = new List<CurrencyAmount>()
			}
		};

		[Tooltip(ContentObject.TooltipTheListOf1 + nameof(TournamentScoreReward) + "s")]
		public List<TournamentScoreReward> scoreRewards = new List<TournamentScoreReward>
		{
			new TournamentScoreReward
			{
				name = "Winner Score",
				tier = 0,
				minScore = 1.0,
				maxScore = new OptionalDouble { HasValue = true, Value = 1.0},
				stageMin = new OptionalInt { HasValue = true, Value = 1},
				stageMax = new OptionalInt { HasValue = true, Value = 1},
				currencyRewards = new List<CurrencyAmount>()
			}
		};

		[Tooltip(ContentObject.TooltipTheListOf1 + nameof(TournamentGroupRewards))]
		public TournamentGroupRewards groupRewards;

		public TournamentTier GetTier(int tierIndex)
		{
			if (tierIndex < 0 || tierIndex >= tiers.Count)
				return new TournamentTier
				{
					color = new Color(.2f, .2f, .2f),
					name = "Void"
				};

			return tiers[tierIndex];
		}

		public DateTime GetUTCOfCycle(int cycle)
		{
			// TODO: HACKED TO ONLY WORK FOR DAYS ATM.
			var date = DateTime.Parse(anchorTimeUTC);
			return date.AddDays(cycle);
		}

		public int GetCurrentCycleNumber()
		{
			var anchor = DateTime.Parse(anchorTimeUTC);
			var now = DateTime.UtcNow;
			var daysDiff = now.Subtract(anchor);
			return (int)daysDiff.TotalDays;
		}

		public DateTime GetUTCOfCyclesPrior(int cyclesPrior)
		{
			// TODO: Hacked To only work for days atm.
			var currentCycle = GetCurrentCycleNumber();
			return GetUTCOfCycle(currentCycle - cyclesPrior);
		}
	}
}
