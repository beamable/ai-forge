using Beamable.Avatars;
using Beamable.Common;
using Beamable.UI.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Beamable.EasyFeatures.BasicParty
{
	public class PlayersListPresenter : MonoBehaviour, PoolableScrollView.IContentProvider
	{
		public PartySlotPresenter PartySlotPrefab;
		public PoolableScrollView ScrollView;
		public float DefaultElementHeight = 150;

		protected List<PartySlotPresenter> SpawnedEntries = new List<PartySlotPresenter>();
		protected Action<string> OnAcceptButtonClicked;
		protected Action<string> OnAskToLeaveClicked;
		protected Action<string> OnPromoteClicked;
		protected Action OnAddMemberClicked;
		protected List<PartySlotPresenter.ViewData> Slots;
		protected bool AreElementsExpandable;

		public async Promise Setup(List<long> players, bool areElementsExpandable, Action<string> onPlayerAccepted, Action<string> onAskedToLeave, Action<string> onPromoted, Action onAddMember, int maxPlayers = 0)
		{
			PartySlotPresenter.ViewData[] viewData = new PartySlotPresenter.ViewData[players.Count];
			for (int i = 0; i < players.Count; i++)
			{
				var stats = await BeamContext.Default.Api.StatsService.GetStats("client", "public", "player", players[i]);
				string name = stats.ContainsKey("alias") ? stats["alias"] : players[i].ToString();
				string avatarName = stats.ContainsKey("avatar") ? stats["avatar"] : "";
				Sprite avatarSprite = AvatarConfiguration.Instance.Default.Sprite;
				if (!string.IsNullOrWhiteSpace(avatarName))
				{
					var avatar = AvatarConfiguration.Instance.Avatars.FirstOrDefault(av => av.Name == avatarName);
					if (avatar != null)
					{
						avatarSprite = avatar.Sprite;
					}
				}

				viewData[i] = new PartySlotPresenter.ViewData
				{
					Avatar = avatarSprite,
					PlayerId = players[i].ToString(),
					Name = name
				};
			}

			Slots = viewData.ToList();
			ScrollView.SetContentProvider(this);
			OnAcceptButtonClicked = onPlayerAccepted;
			OnAskToLeaveClicked = onAskedToLeave;
			OnPromoteClicked = onPromoted;
			OnAddMemberClicked = onAddMember;
			AreElementsExpandable = areElementsExpandable;

			ClearEntries();
			SpawnEntries(maxPlayers);
		}

		public void ClearEntries()
		{
			foreach (PartySlotPresenter slotPresenter in SpawnedEntries)
			{
				Destroy(slotPresenter.gameObject);
			}

			SpawnedEntries.Clear();
		}

		public void SpawnEntries(int maxPlayers)
		{
			var items = new List<PoolableScrollView.IItem>();
			for (var i = 0; i < Slots.Count; i++)
			{
				var data = Slots[i];
				var rankEntryPoolData = new PartySlotPresenter.PoolData
				{
					ViewData = data,
					Index = i,
					Height = DefaultElementHeight
				};
				items.Add(rankEntryPoolData);
			}

			if (AreElementsExpandable && (maxPlayers <= 0 || items.Count < maxPlayers))
			{
				items.Add(new PartySlotPresenter.PoolData
				{
					Height = DefaultElementHeight,
					Index = Slots.Count - 1,
					ViewData = new PartySlotPresenter.ViewData()
				});
			}

			ScrollView.SetContent(items);
		}

		public RectTransform Spawn(PoolableScrollView.IItem item, out int order)
		{
			// TODO: implement object pooling
			var spawned = Instantiate(PartySlotPrefab);
			SpawnedEntries.Add(spawned);
			order = -1;

			var data = item as PartySlotPresenter.PoolData;
			Assert.IsTrue(data != null, "All items in this scroll view MUST be PartySlotPresenters");

			spawned.Setup(data, this, AreElementsExpandable, OnAcceptButtonClicked, OnAskToLeaveClicked, OnPromoteClicked, OnAddMemberClicked);

			return spawned.GetComponent<RectTransform>();
		}

		public void Despawn(PoolableScrollView.IItem item, RectTransform rt)
		{
			if (rt == null) return;

			// TODO: implement object pooling
			var slotPresenter = rt.GetComponent<PartySlotPresenter>();
			SpawnedEntries.Remove(slotPresenter);
			Destroy(slotPresenter.gameObject);
		}

		public void UpdateContent()
		{
			ScrollView.SetDirtyContent();
		}
	}
}
