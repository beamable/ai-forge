using Beamable.Common.Leaderboards;
using Beamable.Modules.Generics;
using UnityEngine;
using UnityEngine.Events;

namespace Beamable.Modules.Leaderboards
{
	public class LeaderboardsPresenter : ModelPresenter<LeaderboardsModel>
	{
#pragma warning disable CS0649
		[SerializeField] private LeaderboardRef _leaderboardRef;
		[SerializeField] private int _entriesAmount;
		[SerializeField] private GenericButton _backButton;
		[SerializeField] private GenericButton _topButton;
		[SerializeField] private LeaderboardsRankEntriesPresenter _rankEntries;
		[SerializeField] private LeaderboardsRankEntryPresenter _currentUserRankEntry;

		[Space]
		[SerializeField] private UnityEvent _backButtonAction;

		[Header("Debug")]
		[SerializeField] private bool _testMode;
#pragma warning restore CS0649

		protected override void Awake()
		{
			base.Awake();

			if (_testMode)
			{
				Debug.LogWarning($"You are using a Leaderboard in test mode", this);
			}

			Model.Initialize(_leaderboardRef, _entriesAmount, _testMode, _backButtonAction);

			Model.OnScrollRefresh += OnScrollRefresh;

			_topButton.Setup(Model.ScrollToTopButtonClicked);
			_backButton.Setup(Model.BackButtonClicked);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			Model.OnScrollRefresh -= OnScrollRefresh;
		}

		protected override void RefreshRequested()
		{
			_rankEntries.ClearData();
		}

		protected override void Refresh()
		{
			_rankEntries.Setup(Model.CurrentRankEntries, Model.CurrentUserRankEntry.rank);
			_currentUserRankEntry.Setup(Model.CurrentUserRankEntry, Model.CurrentUserRankEntry.rank);
		}

		private void OnScrollRefresh()
		{
			_rankEntries.ScrollToTop();
		}
	}
}
