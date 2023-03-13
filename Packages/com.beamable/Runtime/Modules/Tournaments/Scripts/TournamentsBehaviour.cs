using Beamable.AccountManagement;
using Beamable.Common;
using Beamable.Common.Api.Auth;
using Beamable.Common.Api.Tournaments;
using Beamable.Common.Content;
using Beamable.Common.Inventory;
using Beamable.Common.Shop;
using Beamable.Common.Tournaments;
using Beamable.UI.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VirtualList;
using Debug = UnityEngine.Debug;

namespace Beamable.Tournaments
{
	public class TournamentsBehaviour : MonoBehaviour
	{
		public TournamentRef Tournament;


		public TournamentInfoBehaviour InfoPrefab;
		public TournamentEntryBehavior EntryPrefab;
		public TournamentEntryPlayerBehaviour PlayerInstance;
		public ScrollRect InfoScroller;
		public AbstractVirtualList List;
		public GameObject InfoPage, MainPage, InfoDetailPage;
		public TournamentInfoDetailContainerBehaviour InfoDetailContainer;
		public TournamentType Type;
		public TournamentDay Day;
		public LoadingIndicator LoadingIndicator;
		public CanvasGroup ViewportCanvasGroup, TopBarCanvasGroup;
		public TournamentRewardPanelBehaviour RewardsPage;
		public TournamentNumbersBehaviour RewardCount;
		public Button RewardsButton;
		public CountdownTextBehaviour DailyCountdown, GlobalCountdown;
		public TextMeshProUGUI StageText, ChampionText;
		public List<TournamentStageGainDefinition> StagePrefabs;
		public List<GameObject> ActiveOnlyForDaily,
			ActiveOnlyForGlobal,
			ActiveOnlyForChampion,
			DeactivedOnlyForChampion, ActiveOnlyForToday, ActiveOnlyForYesterday;
		public TabBehaviour DayTabs, TypeTodayTabs, TypeYesterdayTabs;
		public GameObject GeneralErrorPage;

		private Color BackgroundColor;
		private Color EntryColor;
		public Color YesterdayEntryColor, NormalEntryColor, DisabledButtonColor, TodayBackgroundColor, YesterdayBackgroundColor;

		private TournamentEntryProvider _dataProvider;
		private TournamentTier _playerTier;
		private TournamentEntryBehavior _playerEntryInstance;
		private Coroutine _viewportFadeRoutine;
		private Promise<TournamentBundle> _dataPromise;
		private Promise<TournamentPlayerStatus> _playerStatusPromise;

		private ColorBlock _activeTabColorBlock, _inactiveTabColorBlock;

		private Promise<TournamentBundle> DataPromise => _dataPromise ?? (_dataPromise = Refresh());

		public bool ShouldShowStageGain => Type == TournamentType.DAILY;
		public bool ShouldShowRewards => Type == TournamentType.DAILY;
		public bool ShouldShowRank => Type != TournamentType.CHAMPION;


		private IBeamableAPI _beamableApi;

		// Start is called before the first frame update
		void OnEnable()
		{
			TopBarCanvasGroup.alpha = 0;
			RewardCount.Set(0);
			RefreshColors();
			RefreshColorConstraints();
			DataPromise.Then(data =>
			{
				FadeTopBar(1);
				CreateInfoPages();
			});

		}

		void Awake()
		{
			API.Instance.Then(beamable =>
			{
				_beamableApi = beamable;
				_beamableApi.OnUserChanged -= HandleUserChange;
				_beamableApi.OnUserChanged += HandleUserChange;
			});
		}

		private void OnDestroy()
		{
			if (_beamableApi == null) return;
			_beamableApi.OnUserChanged -= HandleUserChange;
		}

		void HandleUserChange(User nextUser)
		{
			_dataPromise = Refresh();
			OnEnable();
		}

		private void OnDisable()
		{
			ViewportCanvasGroup.alpha = 0; // hide everything!
			_dataPromise = null; // force a refresh if we come back to the page.
		}


		// Update is called once per frame
		void Update()
		{

		}

		Promise<TournamentBundle> Refresh()
		{
			return GetAllBasicData().Then(data =>
			{
				// set the time remaining.
				DailyCountdown.SetSecondsLeft(data.TournamentInfo.secondsRemaining);
				GlobalCountdown.SetSecondsLeft(data.TournamentInfo.secondsRemaining);

				// if the time runs out, we want to invalidate everything, and refresh again.
				BeamableAnimationUtil.Animate((i, n) =>
				{
					_dataPromise = null;
					OnEnable();
				}, data.TournamentInfo.secondsRemaining, 1);


				// set tier
				_playerTier = data.Content.GetTier(data.Status.tier);

				// set the stage number
				StageText.text = $"Stage {data.Status.stage + 1}/{data.Content.stagesPerTier}";

				// TODO: set the cycle names

				// set the color scheme, which is based from tournament data
				RefreshColors();

				// set the rewards
				RefreshRewards(data);

				// resolve the score data itself
				RefreshEntries(data);
			});
		}

		public void SetType(TournamentType nextType)
		{
			if (nextType == TournamentType.CHAMPION)
			{
				Day = TournamentDay.TODAY;
				//SetDay(TournamentDay.TODAY);
			}
			Type = nextType;
			// XXX: Gross.

			DataPromise.Then(data =>
			{
				RefreshActivations();
				RefreshColors();
				RefreshEntries(data);
				RefreshColorConstraints();
				if (nextType == TournamentType.CHAMPION)
				{
					DayTabs.SetActiveTab(0);
				}
			});

		}

		public void SetDay(TournamentDay nextDay)
		{
			Day = nextDay;

			DataPromise.Then(data =>
			{
				RefreshActivations();
				RefreshColors();
				RefreshEntries(data);
				RefreshColorConstraints();
			});
		}


		public void ToggleInfoPage()
		{
			var isInfoOpen = InfoPage.activeInHierarchy || InfoDetailPage.activeInHierarchy;
			var shouldInfoBeOpen = !isInfoOpen;
			var shouldMainBeOpen = isInfoOpen;
			InfoPage.SetActive(shouldInfoBeOpen);
			MainPage.SetActive(shouldMainBeOpen);
			RewardsPage.gameObject.SetActive(false);
			InfoDetailPage.SetActive(false); // always turn off the detail page.
			TypeTodayTabs.Tabs.ForEach(t => t.Image.gameObject.SetActive(isInfoOpen));

		}

		public void ShowMainPage()
		{

			InfoDetailPage.SetActive(false);
			InfoPage.SetActive(false);
			RewardsPage.gameObject.SetActive(false);
			MainPage.SetActive(true);
		}

		public void ShowInfoDetailPage(TournamentInfoPageSection infoPageSection)
		{
			InfoDetailPage.SetActive(true);
			InfoPage.SetActive(false);
			MainPage.SetActive(false);
			RewardsPage.gameObject.SetActive(false);
			InfoDetailContainer.Set(infoPageSection);
		}

		public void ShowInfoPage()
		{
			TypeTodayTabs.Tabs.ForEach(t => t.Image.gameObject.SetActive(false));
			InfoDetailPage.SetActive(false);
			InfoPage.SetActive(true);
			RewardsPage.gameObject.SetActive(false);
			MainPage.SetActive(false);
		}

		public void ShowRewardPage()
		{
			InfoDetailPage.SetActive(false);
			InfoPage.SetActive(false);
			RewardsPage.gameObject.SetActive(true);
			MainPage.SetActive(false);

			RewardsPage.ShowPage();

		}

		public void ClaimAllRewards()
		{
			// do networky thing?
			DataPromise.Then(data =>
			{
				data.Service.ClaimAllRewards(data.TournamentInfo.tournamentId).Then(response =>
				{
					RefreshRewardCount(response.rewardCurrencies.Count);
					ShowMainPage();
				});
			});
		}

		public void ScrollInfoContainer(float speed)
		{
			// https://answers.unity.com/questions/1184410/scrollrect-changing-velocity-with-script.html
			InfoScroller.horizontalNormalizedPosition = Mathf.Clamp(InfoScroller.horizontalNormalizedPosition, 0.0001f, 0.9999f); //Clamping between 0 and 1 just didn't do...
			InfoScroller.velocity = Vector2.right * InfoScroller.viewport.rect.width * speed * 2;
		}

		public void CreateInfoPages()
		{
			var allInfo = TournamentsConfiguration.Instance.Info;
			foreach (var info in allInfo)
			{
				var infoInstance = Instantiate(InfoPrefab, InfoScroller.content);
				infoInstance.Set(this, info);
			}
		}

		public void HandleDayTabChange(TabChangeEventArgs args)
		{
			SetDay(args.index == 0 ? TournamentDay.TODAY : TournamentDay.YESTERDAY);
		}

		public void HandleTypeTabChange(TabChangeEventArgs args)
		{
			var nextType = TournamentType.DAILY;
			switch (args.index)
			{
				case 0:
					nextType = TournamentType.DAILY;
					break;
				case 1:
					nextType = TournamentType.GLOBAL;
					break;
				case 2:
					nextType = TournamentType.CHAMPION;
					break;
			}
			SetType(nextType);

		}

		public void RefreshRewards(TournamentBundle data)
		{
			RewardCount.Set(0);
			RewardsButton.gameObject.SetActive(false);
			data.Service.GetUnclaimedRewards(data.TournamentInfo.tournamentId).Then(response =>
			{
				var rewards = new TournamentPlayerRewards
				{
					UnclaimedRewards = response.rewardCurrencies.Select(x => new OfferObtainCurrency
					{
						amount = x.amount,
						symbol = new CurrencyRef(x.symbol)
					}).ToList()
				};
				RewardsPage.Set(rewards);
				RefreshRewardCount(response.rewardCurrencies.Count);
			});
		}

		void RefreshRewardCount(int count)
		{
			RewardCount.Set(count);
			RewardsButton.gameObject.SetActive(count > 0);
		}

		public Promise<TournamentInfo> ResolveTournamentInfo()
		{
			return API.Instance.FlatMap(de =>
			{
				return Tournament.Resolve().FlatMap(content => de.TournamentsService.GetTournamentInfo(content.Id));
			});

		}

		void RefreshEntries(TournamentBundle bundle)
		{
			FadeContent(0);


			Promise<Unit> promise = null;
			switch (Type)
			{
				case TournamentType.DAILY:
					promise = RefreshForDaily(bundle);
					break;
				case TournamentType.GLOBAL:
					promise = RefreshForGlobal(bundle);
					break;
				case TournamentType.CHAMPION:
					promise = RefreshForChampions(bundle);
					break;
			}

			var loadingArg = promise.ToLoadingArg();

			LoadingIndicator.Show(loadingArg);
			loadingArg.Promise.Then(_ => FadeContent(1));

		}

		Promise<Unit> RefreshForDaily(TournamentBundle data)
		{
			var cycle = Day == TournamentDay.TODAY ? 0 : 1; //today or yesterday?

			return data.Service.GetStandings(data.TournamentInfo.tournamentId, cycle).Then(standingsResponse =>
			{
				var others = standingsResponse.entries.ToViewData();
				var playerEntry = standingsResponse.me?.ToViewData() ?? new TournamentEntryData
				{
					Dbid = data.Status.playerId,
					Rank = others.LastOrDefault()?.Rank + 1 ?? 1,
					RewardCurrencies = new List<OfferObtainCurrency>(),
					Score = 0,
					StageGain = 0
				};
				RefreshEntryObjects(data, playerEntry, others);
			}).Error(ex =>
			{
				throw ex;
			}).ToUnit();
		}

		Promise<Unit> RefreshForGlobal(TournamentBundle data)
		{
			var cycle = Day == TournamentDay.TODAY ? 0 : 1; //today or yesterday?

			return data.Service.GetGlobalStandings(data.TournamentInfo.tournamentId, cycle).Then(standingsResponse =>
			{
				var others = standingsResponse.entries.ToViewData();
				var playerEntry = standingsResponse.me?.ToViewData() ?? new TournamentEntryData
				{
					Dbid = data.Status.playerId,
					Rank = others.LastOrDefault()?.Rank + 1 ?? 1,
					RewardCurrencies = new List<OfferObtainCurrency>(),
					Score = 0,
					StageGain = 0
				};
				RefreshEntryObjects(data, playerEntry, others);
			}).ToUnit();
		}

		Promise<Unit> RefreshForChampions(TournamentBundle data)
		{
			var limit = 30;
			ChampionText.text = $"Fetching Champions...";
			return data.Service.GetChampions(data.TournamentInfo.tournamentId, limit).Map(championsResponse =>
				{
					var others = championsResponse.entries.ToViewData();
					RefreshEntryObjects(data, null, others);
					return others.Count;
				})
				.Then(size =>
				{
					if (size == 0)
					{
						ChampionText.text = $"There are no Champions yet.";
					}
					else
					{
						var displaySize = Mathf.Min(limit, size);
						var displayUnit = displaySize == 1 ? "day" : "days";
						ChampionText.text = $"Champions from the last {displaySize} {displayUnit}";
					}
				})
				.ToUnit();
		}


		void FadeContent(float alpha)
		{
			var startAlpha = ViewportCanvasGroup.alpha;
			this.RunAnimation(i =>
			{
				ViewportCanvasGroup.alpha = Mathf.Lerp(startAlpha, alpha, i);
			}, .1f);
		}

		void FadeTopBar(float alpha)
		{
			var startAlpha = TopBarCanvasGroup.alpha;
			this.RunAnimation(i => { TopBarCanvasGroup.alpha = Mathf.Lerp(startAlpha, alpha, i); }, .1f);
		}

		private void SetActivation(List<GameObject> gameObjects, bool active)
		{
			SetActivation(gameObjects, _ => active);
		}
		private void SetActivation(List<GameObject> gameObjects, Func<GameObject, bool> shouldActivate)
		{
			foreach (var gob in gameObjects)
			{
				var active = shouldActivate(gob);
				gob.SetActive(active);
			}
		}

		public Color GetColorForClass(TournamentColorClass colorClass)
		{
			switch (colorClass)
			{
				case TournamentColorClass.ENTRY:
					return EntryColor;
				case TournamentColorClass.BACKGROUND:
					return BackgroundColor;
				case TournamentColorClass.STAGE:
					if (Day == TournamentDay.YESTERDAY)
					{
						return EntryColor;
					}
					return _playerTier?.color ?? Color.grey;
				default:
					return Color.white;
			}
		}

		private void RefreshActivations()
		{

			var todayActive = Day == TournamentDay.TODAY;
			TypeTodayTabs.gameObject.SetActive(todayActive);
			TypeYesterdayTabs.gameObject.SetActive(!todayActive);

			/*
             * There can be things that are in the ActiveOnlyForDaily, and also in the ActiveOnlyToday.
             * Since those are distinct sets, we can merge their constraints together.
             */

			SetActivation(ActiveOnlyForDaily, Type == TournamentType.DAILY);
			SetActivation(ActiveOnlyForGlobal, Type == TournamentType.GLOBAL);
			SetActivation(ActiveOnlyForChampion, Type == TournamentType.CHAMPION);
			SetActivation(DeactivedOnlyForChampion, Type != TournamentType.CHAMPION);
			SetActivation(ActiveOnlyForToday, x => x.activeInHierarchy && Day == TournamentDay.TODAY);
			SetActivation(ActiveOnlyForYesterday, x => x.activeInHierarchy && Day == TournamentDay.YESTERDAY);
		}

		private void RefreshColors()
		{
			var isToday = Day == TournamentDay.TODAY;
			switch (Type)
			{
				case TournamentType.DAILY:
				case TournamentType.GLOBAL:
					BackgroundColor = isToday ? TodayBackgroundColor : YesterdayBackgroundColor;
					EntryColor = isToday ? NormalEntryColor : YesterdayEntryColor;
					break;

				case TournamentType.CHAMPION:
					BackgroundColor = TodayBackgroundColor;
					EntryColor = NormalEntryColor; // TODO change to gold
					break;
			}
		}

		private void RefreshColorConstraints()
		{
			var colorConstraints = FindObjectsOfType<TournamentColorConstraint>();
			foreach (var constrained in colorConstraints)
			{
				if (constrained.isActiveAndEnabled)
				{
					constrained.Refresh(this);
				}
			}
		}

		TournamentEntryViewData UpgradeEntryData(TournamentBundle bundle, TournamentEntryData data)
		{
			return new TournamentEntryViewData
			{
				Data = data,
				IsGrey = Day == TournamentDay.YESTERDAY,
				Master = this,
				Tournament = bundle
			};
		}

		private void RefreshEntryObjects(TournamentBundle bundle, TournamentEntryData playerData, List<TournamentEntryData> entries)
		{
			var viewData = entries.Select(e => UpgradeEntryData(bundle, e)).ToList();

			var playerIndex = playerData != null ? entries.FindIndex(e => e.Dbid == playerData.Dbid) : -1;

			if (_dataProvider == null)
			{
				_dataProvider = new TournamentEntryProvider(playerIndex,
					viewData,
					EntryPrefab,
					PlayerInstance);
			}
			else
			{
				_dataProvider.Reset(playerIndex, viewData);
			}

			if (Type == TournamentType.CHAMPION)
			{
				PlayerInstance.gameObject.SetActive(false);
			}
			else
			{
				PlayerInstance.gameObject.SetActive(true);
				PlayerInstance.EntryBehavior.Set(UpgradeEntryData(bundle, playerData));
			}


			var focusIndex = Type == TournamentType.DAILY ? playerIndex : 0;
			List.SetSourceAndCenterOn(_dataProvider, focusIndex);
		}


		Promise<TournamentBundle> GetAllBasicData()
		{
			// TODO: maybe make parallel instead of serial?
			return API.Instance.FlatMap(de =>
				Tournament.Resolve()
					.RecoverWith(err =>
					{
						if (!(err is ContentNotFoundException)) throw err;

						GeneralErrorPage.SetActive(true);
						Debug.LogException(err);
						Debug.LogError("Make sure you have a tournament content reference set on the tournament Game Object", this);

						var emptyContent = ScriptableObject.CreateInstance<TournamentContent>();
						return Promise<TournamentContent>.Successful(emptyContent);
					})
					.FlatMap(content =>
					de.TournamentsService.GetTournamentInfo(content.Id)
						.FlatMap(info => de.TournamentsService.JoinTournament(info.tournamentId)
							.Map(status => new TournamentBundle
							{
								Content = content,
								Service = de.TournamentsService,
								TournamentInfo = info,
								Status = status
							})
						)
					)
				);
		}
	}

	[System.Serializable]
	public enum TournamentType
	{
		DAILY,
		GLOBAL,
		CHAMPION
	}

	[System.Serializable]
	public enum TournamentDay
	{
		TODAY,
		YESTERDAY
	}

	public class TournamentBundle
	{
		public ITournamentApi Service;
		public TournamentInfo TournamentInfo;
		public TournamentContent Content;
		public TournamentPlayerStatus Status;
	}

}
