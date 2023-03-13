using Beamable.Common.Api.Leaderboards;
using Beamable.Modules.Generics;
using Beamable.UI.Scripts;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Modules.Leaderboards
{
	public class LeaderboardsRankEntriesPresenter : DataPresenter<List<RankEntry>>, PoolableScrollView.IContentProvider
	{
#pragma warning disable CS0649
		[SerializeField] private LeaderboardsRankEntryPresenter _leaderboardsRankEntryPresenterPrefab;
		[SerializeField] private PoolableScrollView _poolableScrollView;
#pragma warning restore CS0649

		private readonly List<LeaderboardsRankEntryPresenter> _spawnedEntries =
			new List<LeaderboardsRankEntryPresenter>();

		private long _currentPlayerRank;

		public override void Setup(List<RankEntry> data, params object[] additionalParams)
		{
			_currentPlayerRank = (long)additionalParams[0];
			_poolableScrollView.SetContentProvider(this);
			base.Setup(data, additionalParams);
		}

		public override void ClearData()
		{
			base.ClearData();

			foreach (LeaderboardsRankEntryPresenter entryPresenter in _spawnedEntries)
			{
				Destroy(entryPresenter.gameObject);
			}

			_spawnedEntries.Clear();
		}

		protected override void Refresh()
		{
			// ScrollToTop();

			List<PoolableScrollView.IItem> items = new List<PoolableScrollView.IItem>();

			foreach (RankEntry rankEntry in Data)
			{
				LeaderboardsRankEntryPresenter.PoolData rankEntryPoolData =
					new LeaderboardsRankEntryPresenter.PoolData { RankEntry = rankEntry, Height = 50.0f };
				items.Add(rankEntryPoolData);
			}

			_poolableScrollView.SetContent(items);
		}

		public void ScrollToTop()
		{
			_poolableScrollView.Velocity = 0.0f;
			_poolableScrollView.SetPosition(0.0f);
		}

		public RectTransform Spawn(PoolableScrollView.IItem item, out int order)
		{
			// TODO: implement object pooling
			LeaderboardsRankEntryPresenter spawned = Instantiate(_leaderboardsRankEntryPresenterPrefab);
			_spawnedEntries.Add(spawned);
			order = -1;

			if (item is LeaderboardsRankEntryPresenter.PoolData data)
			{
				spawned.Setup(data.RankEntry, _currentPlayerRank);
			}

			return spawned.GetComponent<RectTransform>();
		}

		public void Despawn(PoolableScrollView.IItem item, RectTransform rt)
		{
			// TODO: implement object pooling
			LeaderboardsRankEntryPresenter rankEntryPresenter = rt.GetComponent<LeaderboardsRankEntryPresenter>();
			_spawnedEntries.Remove(rankEntryPresenter);
			Destroy(rankEntryPresenter.gameObject);
		}
	}
}
