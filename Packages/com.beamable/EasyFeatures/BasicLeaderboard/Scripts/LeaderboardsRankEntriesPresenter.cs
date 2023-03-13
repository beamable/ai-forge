using Beamable.Common.Api.Leaderboards;
using Beamable.UI.Scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Beamable.EasyFeatures.BasicLeaderboard
{
	public class LeaderboardsRankEntriesPresenter : MonoBehaviour, PoolableScrollView.IContentProvider
	{
		public LeaderboardsRankEntryPresenter LeaderboardsRankEntryPresenterPrefab;
		public PoolableScrollView PoolableScrollView;
		public GameObjectToggler LoadingIndicator;

		private readonly List<LeaderboardsRankEntryPresenter> _spawnedEntries = new List<LeaderboardsRankEntryPresenter>();
		private long _currentPlayerRank;

		private IReadOnlyList<string> _aliases;
		private IReadOnlyList<Sprite> _avatars;
		private IReadOnlyList<long> _ranks;
		private IReadOnlyList<double> _scores;
		private int _totalRankCount;

		public void Enrich(IEnumerable<BasicLeaderboardView.BasicLeaderboardViewEntry> entries, long currentPlayerRank)
		{
			PoolableScrollView.SetContentProvider(this);

			var entriesList = entries.ToList();
			_aliases = entriesList.Select(e => e.Alias).ToList();
			_avatars = entriesList.Select(e => e.Avatar).ToList();
			_ranks = entriesList.Select(e => e.Rank).ToList();
			_scores = entriesList.Select(e => e.Score).ToList();

			_totalRankCount = entriesList.Count;

			_currentPlayerRank = currentPlayerRank;
		}

		public void Enrich(IReadOnlyList<string> aliases, IReadOnlyList<Sprite> avatars, IReadOnlyList<long> ranks, IReadOnlyList<double> scores, long currentPlayerRank)
		{
			PoolableScrollView.SetContentProvider(this);

			Assert.IsTrue(aliases.Count == avatars.Count && aliases.Count == ranks.Count && aliases.Count == scores.Count, "These arrays must be parallel!");
			_totalRankCount = aliases.Count;

			_aliases = aliases;
			_avatars = avatars;
			_ranks = ranks;
			_scores = scores;

			_currentPlayerRank = currentPlayerRank;
		}

		public void ClearPooledRankedEntries()
		{
			LoadingIndicator.Toggle(true);

			foreach (LeaderboardsRankEntryPresenter entryPresenter in _spawnedEntries)
			{
				Destroy(entryPresenter.gameObject);
			}

			_spawnedEntries.Clear();
		}

		public void RebuildPooledRankEntries()
		{
			var items = new List<PoolableScrollView.IItem>();
			for (var rank = 0; rank < _totalRankCount; rank++)
			{
				var rankEntryPoolData = new LeaderboardsRankEntryPresenter.PoolData
				{
					Name = _aliases[rank],
					Avatar = _avatars[rank],
					Rank = _ranks[rank],
					Score = _scores[rank],
					Height = 50.0f
				};
				items.Add(rankEntryPoolData);
			}

			PoolableScrollView.SetContent(items);
			LoadingIndicator.Toggle(false);
		}

		public void ScrollToTop()
		{
			PoolableScrollView.Velocity = 0.0f;
			PoolableScrollView.SetPosition(0.0f);
		}

		RectTransform PoolableScrollView.IContentProvider.Spawn(PoolableScrollView.IItem item, out int order)
		{
			// TODO: implement object pooling
			var spawned = Instantiate(LeaderboardsRankEntryPresenterPrefab);
			_spawnedEntries.Add(spawned);
			order = -1;

			var data = item as LeaderboardsRankEntryPresenter.PoolData;
			Assert.IsTrue(data != null, "All items in this scroll view MUST be LeaderboardEntryPresenters");

			spawned.Enrich(data.Name, data.Avatar, data.Rank, data.Score, _currentPlayerRank);
			spawned.RebuildRankEntry();

			return spawned.GetComponent<RectTransform>();
		}

		void PoolableScrollView.IContentProvider.Despawn(PoolableScrollView.IItem item, RectTransform rt)
		{
			if (rt == null) return;

			// TODO: implement object pooling
			var rankEntryPresenter = rt.GetComponent<LeaderboardsRankEntryPresenter>();
			_spawnedEntries.Remove(rankEntryPresenter);
			Destroy(rankEntryPresenter.gameObject);
		}
	}
}
