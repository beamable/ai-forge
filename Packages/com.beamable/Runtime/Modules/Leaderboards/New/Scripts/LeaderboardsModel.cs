using Beamable.AccountManagement;
using Beamable.Api.Leaderboard;
using Beamable.Common.Api.Leaderboards;
using Beamable.Common.Leaderboards;
using Beamable.Modules.Generics;
using Beamable.Stats;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Beamable.Modules.Leaderboards
{
	public class LeaderboardsModel : Model
	{
		public event Action OnScrollRefresh;

		private IBeamableAPI _api;
		private LeaderboardService _leaderboardService;
		private LeaderboardRef _leaderboardRef;
		private StatObject _aliasStatObject;
		private int _firstEntryId;
		private int _entriesAmount;
		private bool _testMode;
		private long _dbid;
		private UnityEvent _backButtonAction;

		public List<RankEntry> CurrentRankEntries
		{
			get;
			private set;
		} = new List<RankEntry>();

		public RankEntry CurrentUserRankEntry
		{
			get;
			private set;
		}

		private int LastEntryId => _firstEntryId + _entriesAmount;

		public override async void Initialize(params object[] initParams)
		{
			_leaderboardRef = (LeaderboardRef)initParams[0];
			_entriesAmount = (int)initParams[1];
			_entriesAmount = Mathf.Clamp(_entriesAmount, 1, Int32.MaxValue);
			_testMode = (bool)initParams[2];
			_backButtonAction = (UnityEvent)initParams[3];

			_aliasStatObject = AccountManagementConfiguration.Instance.DisplayNameStat;
			_firstEntryId = 1;

			Validate();

			_api = await Beamable.API.Instance;
			_dbid = _api.User.id;
			_leaderboardService = _api.LeaderboardService;

			if (!_testMode)
			{
				await _leaderboardService.GetUser(_leaderboardRef, _dbid).Then(OnUserRankEntryReceived);
				await _leaderboardService.GetBoard(_leaderboardRef, _firstEntryId, LastEntryId).Then(OnLeaderboardReceived);
			}
			else
			{
				CurrentUserRankEntry =
					LeaderboardsModelHelper.GenerateCurrentUserRankEntryTestData(
						_aliasStatObject.StatKey, _aliasStatObject.DefaultValue);

				CurrentRankEntries = LeaderboardsModelHelper.GenerateLeaderboardsTestData(
					_firstEntryId, LastEntryId, CurrentUserRankEntry,
					_aliasStatObject.StatKey,
					_aliasStatObject.DefaultValue);

				InvokeRefresh();
			}
		}

		public void ScrollToTopButtonClicked()
		{
			if (IsBusy)
			{
				return;
			}

			OnScrollRefresh?.Invoke();
		}

		public void BackButtonClicked()
		{
			_backButtonAction?.Invoke();
		}

		private void OnUserRankEntryReceived(RankEntry rankEntry)
		{
			CurrentUserRankEntry = rankEntry;
		}

		private void OnLeaderboardReceived(LeaderBoardView leaderboardView)
		{
			CurrentRankEntries = leaderboardView.ToList();
			InvokeRefresh();
		}

		private void Validate()
		{
			if (!_testMode)
			{
				Assert.IsFalse(string.IsNullOrEmpty(_leaderboardRef.Id), "Leaderboard Ref has not been set");
			}

			Assert.IsNotNull(_aliasStatObject, "Display Name Stat in Project Settings/Beamable/Account Management has not been set");
		}
	}
}
