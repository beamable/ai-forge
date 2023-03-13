using Beamable.UI.Scripts;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Beamable.EasyFeatures.BasicLobby
{
	public class LobbiesListPresenter : MonoBehaviour, PoolableScrollView.IContentProvider
	{
		[Header("Prefabs")]
		public LobbiesListEntryPresenter LobbyEntryPrefab;

		[Header("Components")]
		public PoolableScrollView PoolableScrollView;

		private readonly List<LobbiesListEntryPresenter> _spawnedEntries = new List<LobbiesListEntryPresenter>();
		private List<LobbiesListEntryPresenter.ViewData> _entriesList = new List<LobbiesListEntryPresenter.ViewData>();
		private Action<int?> _onLobbySelected;

		private LobbiesListEntryPresenter _currentlySelectedLobby;

		public void Setup(List<LobbiesListEntryPresenter.ViewData> entries, Action<int?> onLobbySelected)
		{
			PoolableScrollView.SetContentProvider(this);
			_entriesList = entries;
			_onLobbySelected = onLobbySelected;
		}

		public void ClearPooledRankedEntries()
		{
			foreach (LobbiesListEntryPresenter entryPresenter in _spawnedEntries)
			{
				Destroy(entryPresenter.gameObject);
			}

			_spawnedEntries.Clear();
		}

		public void RebuildPooledLobbiesEntries()
		{
			var items = new List<PoolableScrollView.IItem>();
			for (var i = 0; i < _entriesList.Count; i++)
			{
				LobbiesListEntryPresenter.ViewData data = _entriesList[i];
				var rankEntryPoolData = new LobbiesListEntryPresenter.PoolData
				{
					ViewData = data,
					Index = i,
					Height = 100.0f // TODO: expose this somewhere in inspector
				};
				items.Add(rankEntryPoolData);
			}

			PoolableScrollView.SetContent(items);
		}

		public RectTransform Spawn(PoolableScrollView.IItem item, out int order)
		{
			// TODO: implement object pooling
			LobbiesListEntryPresenter spawned = Instantiate(LobbyEntryPrefab);
			_spawnedEntries.Add(spawned);
			order = -1;

			LobbiesListEntryPresenter.PoolData poolData = item as LobbiesListEntryPresenter.PoolData;
			Assert.IsTrue(poolData != null, "All items in this scroll view MUST be LobbiesListEntryPresenter");

			spawned.Setup(poolData.ViewData, (presenter) =>
			{
				if (_currentlySelectedLobby != null)
				{
					_currentlySelectedLobby.SetSelected(false);
				}

				_currentlySelectedLobby = presenter;
				_currentlySelectedLobby.SetSelected(true);
				_onLobbySelected.Invoke(poolData.Index);
			});

			return spawned.GetComponent<RectTransform>();
		}

		public void Despawn(PoolableScrollView.IItem item, RectTransform rt)
		{
			if (rt == null) return;

			// TODO: implement object pooling
			LobbiesListEntryPresenter rankEntryPresenter = rt.GetComponent<LobbiesListEntryPresenter>();
			_spawnedEntries.Remove(rankEntryPresenter);
			Destroy(rankEntryPresenter.gameObject);
		}
	}
}
