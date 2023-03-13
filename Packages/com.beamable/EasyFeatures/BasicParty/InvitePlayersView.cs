using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.EasyFeatures.BasicParty
{
	public class InvitePlayersView : MonoBehaviour, ISyncBeamableView
	{
		public PartyFeatureControl FeatureControl;
		public int EnrichOrder;

		public PlayersListPresenter PartyList;
		public Button BackButton;

		protected BeamContext Context;

		public bool IsVisible
		{
			get => gameObject.activeSelf;
			set => gameObject.SetActive(value);
		}

		public int GetEnrichOrder() => EnrichOrder;

		public async void EnrichWithContext(BeamContextGroup managedPlayers)
		{
			Context = managedPlayers.GetSinglePlayerContext();

			if (!IsVisible)
			{
				return;
			}

			// set callbacks
			BackButton.onClick.ReplaceOrAddListener(OnBackButtonClicked);

			// prepare friends list
			await Context.Social.OnReady;   // show loading
			var friendsList = Context.Social.Friends;
			List<long> friends = new List<long>(friendsList.Count);
			for (int i = 0; i < friendsList.Count; i++)
			{
				if (Context.Party.PartyMembers.Any(member => member.playerId.Equals(friendsList[i].playerId)))
					continue;

				friends.Add(friendsList[i].playerId);
			}

			await PartyList.Setup(friends, false, OnPlayerInvited, null, null, null);
		}

		private async void OnPlayerInvited(string id)
		{
			await Context.Party.Invite(id);
		}

		private void OnBackButtonClicked()
		{
			FeatureControl.OpenPartyView();
		}
	}
}
