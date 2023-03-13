using Beamable.AccountManagement;
using Beamable.Common;
using Beamable.Common.Api.Tournaments;
using Beamable.Common.Inventory;
using Beamable.Common.Shop;
using Beamable.Shop;
using Beamable.Stats;
using Beamable.UI.Scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VirtualList;

namespace Beamable.Tournaments
{

	public class TournamentEntryViewData
	{
		public TournamentEntryData Data;
		public TournamentsBehaviour Master;
		public TournamentBundle Tournament;
		public bool IsGrey;
	}

	public class TournamentEntryBehavior : MonoBehaviour, IViewFor<TournamentEntryViewData>
	{
		public TournamentEntryViewData Data;

		public Image MainBackgroundImage;
		public TextReference RankReference;
		public TextReference ScoreReference;
		public TextReference AliasReference;
		public TextReference CycleLabel;
		public TextReference CycleValue;
		public AccountAvatarBehaviour AvatarBehaviour;
		public StatBehaviour AvatarStatBehaviour;
		public Image StageImage;
		public TournamentColorConstraint ColorConstraint;
		public TournamentColorConstraint BackgroundConstraint;

		public RectTransform RewardContainer;
		public TournamentRewardEntryBehaviour RewardPrefab;

		public RectTransform StageContainer;
		public FixedCharacterFitter RankFitter;

		private List<TournamentRewardEntryBehaviour> _rewardEntries = new List<TournamentRewardEntryBehaviour>();


		public void Set(TournamentEntryViewData viewData)
		{
			Data = viewData;
			var data = viewData.Data;

			if (viewData.Master.ShouldShowRank)
			{
				CycleLabel.gameObject.SetActive(false);
				CycleValue.gameObject.SetActive(false);
				RankReference.gameObject.SetActive(true);
				RankReference.Value = "" + data.Rank;
				RankFitter.Refresh();
			}
			else
			{
				RankReference.gameObject.SetActive(false);
				RankReference.Value = "123";
				RankFitter.Refresh();

				CycleLabel.gameObject.SetActive(true);
				CycleValue.gameObject.SetActive(true);
				// TODO: implement this better...

				var cycleDate = viewData.Tournament.Content.GetUTCOfCyclesPrior(data.Cycle);
				CycleLabel.Value = cycleDate.ToString("MMM");
				CycleValue.Value = cycleDate.ToString("dd");

			}

			ScoreReference.Value = TournamentScoreUtil.GetCommaString((long)data.Score);

			viewData.Tournament.Service.GetPlayerAlias(data.Dbid).Then(alias => AliasReference.Value = alias);
			AvatarStatBehaviour.SetForUser(data.Dbid);
			viewData.Tournament.Service.GetPlayerAvatar(data.Dbid).Then(AvatarStatBehaviour.SetCurrentValue);

			var isChampion = viewData.Master.Type == TournamentType.CHAMPION;

			var stageChange = viewData.Tournament.Content.stageChanges.FirstOrDefault(x => x.AcceptsRank(data.Rank));
			var rankColor = stageChange?.color ?? new Color(0, 0, 0, 0);
			if (isChampion)
			{
				rankColor = viewData.Tournament.Content.ChampionColor;
			}
			var shouldOverrideColor = stageChange != null || isChampion;

			if (shouldOverrideColor && rankColor.a > 0 && !viewData.IsGrey)
			{
				ColorConstraint.OverrideColor(rankColor);
			}
			else
			{
				ColorConstraint.ReleaseOverride();
			}
			BackgroundConstraint.Refresh(viewData.Master);

			SetStageGain(viewData);
			SetRewards(viewData);

			ColorConstraint.Refresh();


		}

		void SetStageGain(TournamentEntryViewData viewData)
		{
			for (var i = 0; i < StageContainer.childCount; i++)
			{
				Destroy(StageContainer.GetChild(i).gameObject);
			}

			if (!viewData.Master.ShouldShowStageGain) return;

			var stageGainDef = viewData.Master.StagePrefabs.FirstOrDefault(x => x.StageGain == viewData.Data.StageGain);

			if (stageGainDef.Prefab != null)
			{
				var instance = Instantiate(stageGainDef.Prefab, StageContainer);
				instance.SetEffect(true); // always supposed to be grey
			}
		}

		void SetRewards(TournamentEntryViewData viewData)
		{

			for (var i = 0; i < RewardContainer.childCount; i++)
			{
				Destroy(RewardContainer.GetChild(i).gameObject);
			}
			if (!viewData.Master.ShouldShowRewards) return;

			_rewardEntries.Clear();
			foreach (var rewardData in viewData.Data.RewardCurrencies)
			{
				var instance = Instantiate(RewardPrefab, RewardContainer);
				instance.Set(viewData, rewardData);
				_rewardEntries.Add(instance);
			}
		}

		public void SetColor(Color color)
		{
			if (MainBackgroundImage != null)
			{
				MainBackgroundImage.color = color;
			}
		}

	}

	[System.Serializable]
	public class TournamentEntryData
	{
		public long Dbid;
		public long Rank;
		public double Score;
		public int StageGain;
		public int Cycle;

		public List<OfferObtainCurrency> RewardCurrencies;
	}

	public static class TournamentEntryExtensions
	{
		public static TournamentEntryData ToViewData(this TournamentEntry self)
		{
			if (self == null) return null;
			return new TournamentEntryData
			{
				Dbid = self.playerId,
				Rank = self.rank,
				Score = self.score,
				StageGain = self.stageChange,
				RewardCurrencies = self.currencyRewards?.Select(x => new OfferObtainCurrency { amount = x.amount, symbol = new CurrencyRef(x.symbol) }).ToList() ?? new List<OfferObtainCurrency>()
			};
		}

		public static TournamentEntryData ToViewData(this TournamentChampionEntry self)
		{
			return new TournamentEntryData
			{
				Dbid = self.playerId,
				Score = self.score,
				Cycle = self.cyclesPrior
			};
		}

		public static List<TournamentEntryData> ToViewData(this List<TournamentEntry> self)
		{
			return self.Select(x => x.ToViewData()).ToList();
		}

		public static List<TournamentEntryData> ToViewData(this List<TournamentChampionEntry> self)
		{
			return self.Select(x => x.ToViewData()).ToList();
		}
	}

	[System.Serializable]
	public class TournamentPlayerRewards
	{
		public List<OfferObtainCurrency> UnclaimedRewards;

		private Promise<Dictionary<string, Sprite>> _currencyPromise;

		public Promise<Dictionary<string, Sprite>> ResolveAllIcons()
		{
			return _currencyPromise ?? (_currencyPromise = UnclaimedRewards.ResolveAllIcons());
		}
	}

}
