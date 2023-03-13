using Beamable.UI.Scripts;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Beamable.EasyFeatures.BasicLobby
{
	public class LobbySlotsListPresenter : MonoBehaviour, PoolableScrollView.IContentProvider
	{
		[Header("Prefabs")]
		public LobbySlotPresenter LobbySlotPrefab;

		[Header("Components")]
		public PoolableScrollView PoolableScrollView;

		private List<LobbySlotPresenter.ViewData> _slots;
		private bool _isAdmin;
		private Action<int> _onAdminButtonClicked;
		private Action<int> _onKickButtonClicked;
		private Action<int> _onPassLeadershipButtonClicked;
		private readonly List<LobbySlotPresenter> _spawnedSlots = new List<LobbySlotPresenter>();

		public void Setup(List<LobbySlotPresenter.ViewData> slots,
						  bool isAdmin,
						  Action<int> onAdminButtonClicked,
						  Action<int> onKickButtonClicked,
						  Action<int> onPassLeadershipButtonClicked)
		{
			PoolableScrollView.SetContentProvider(this);

			_slots = slots;
			_isAdmin = isAdmin;
			_onAdminButtonClicked = onAdminButtonClicked;
			_onKickButtonClicked = onKickButtonClicked;
			_onPassLeadershipButtonClicked = onPassLeadershipButtonClicked;
		}

		public void ClearPooledRankedEntries()
		{
			foreach (LobbySlotPresenter entryPresenter in _spawnedSlots)
			{
				Destroy(entryPresenter.gameObject);
			}

			_spawnedSlots.Clear();
		}

		public void RebuildPooledLobbiesEntries()
		{
			var items = new List<PoolableScrollView.IItem>();
			for (var i = 0; i < _slots.Count; i++)
			{
				var data = _slots[i];
				LobbySlotPresenter.ViewData rankEntryPoolData = new LobbySlotPresenter.ViewData
				{
					PlayerId = data.PlayerId,
					IsReady = data.IsReady,
					IsUnfolded = data.IsUnfolded,
					Index = i,
					Height = data.IsUnfolded ? data.UnfoldedHeight : data.FoldedHeight
				};
				items.Add(rankEntryPoolData);
			}

			PoolableScrollView.SetContent(items);
		}

		public RectTransform Spawn(PoolableScrollView.IItem item, out int order)
		{
			// TODO: implement object pooling
			LobbySlotPresenter spawned = Instantiate(LobbySlotPrefab);
			_spawnedSlots.Add(spawned);
			order = -1;

			LobbySlotPresenter.ViewData poolData = item as LobbySlotPresenter.ViewData;
			Assert.IsTrue(poolData != null, "All items in this scroll view MUST be LobbySlotPresenter");

			if (poolData.PlayerId != String.Empty) // Temporarily Name is set to playerId
			{
				spawned.SetupFilled(poolData.PlayerId, poolData.IsReady, _isAdmin, poolData.IsUnfolded,
									() =>
									{
										_onAdminButtonClicked.Invoke(poolData.Index);
									},
									() =>
									{
										_onKickButtonClicked.Invoke(poolData.Index);
									},
									() =>
									{
										_onPassLeadershipButtonClicked.Invoke(poolData.Index);
									});
			}
			else
			{
				spawned.SetupEmpty();
			}

			return spawned.GetComponent<RectTransform>();
		}

		public void Despawn(PoolableScrollView.IItem item, RectTransform rt)
		{
			if (rt == null) return;

			// TODO: implement object pooling
			LobbySlotPresenter slotPresenter = rt.GetComponent<LobbySlotPresenter>();
			_spawnedSlots.Remove(slotPresenter);
			Destroy(slotPresenter.gameObject);
		}
	}
}
