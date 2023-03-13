using System.Collections.Generic;
using UnityEngine;
using VirtualList;

namespace Beamable.Tournaments
{
	// TODO: atm, this class cannot handle infinite scroll. We should make it work that way.
	public class TournamentEntryProvider : SimpleSource<TournamentEntryViewData, TournamentEntryBehavior>, IPrefabSource
	{
		public int PlayerIndex;
		public TournamentEntryBehavior EntryPrefab;
		public TournamentEntryPlayerBehaviour PlayerInstance;
		private GameObject _emptySlotGameObject;

		public TournamentEntryProvider(int playerIndex,
		   IList<TournamentEntryViewData> list,
		   TournamentEntryBehavior entryPrefab,
		   TournamentEntryPlayerBehaviour playerInstance) : base(list)
		{
			EntryPrefab = entryPrefab;
			PlayerIndex = playerIndex;
			PlayerInstance = playerInstance;
		}

		public void Reset(int playerIndex, IList<TournamentEntryViewData> list)
		{
			PlayerIndex = playerIndex;
			_list = list;
		}

		public override void SetItem(GameObject view, int index)
		{
			if (index == PlayerIndex)
			{
				/*
				 * the player index has an empty gameobject.
				 * It needs no configuration.
				 *
				 * However, we should notify the player entry about the empty slot,
				 *  so that the player entry can manually track it and decide its own placement.
				 */
				PlayerInstance.ProvideTrackingElement(view);
				return;
			}
			var element = _list[index];
			var display = view.GetComponent<TournamentEntryBehavior>();

			display.Set(element);
		}

		public GameObject GetEmptySlot()
		{
			if (_emptySlotGameObject == null)
			{
				_emptySlotGameObject = new GameObject("EmptySlotObject", typeof(RectTransform));
			}

			return _emptySlotGameObject;
		}

		public GameObject PrefabAt(int index)
		{
			if (index == PlayerIndex)
			{
				return GetEmptySlot();
			}
			else
			{
				return EntryPrefab.gameObject;

			}
		}
	}
}
