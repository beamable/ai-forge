using Beamable.Common;
using Beamable.Experimental.Api.Parties;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.EasyFeatures.BasicParty
{
	public class JoinPartyView : MonoBehaviour, ISyncBeamableView
	{
		public static List<PartyInvite> ReceivedInvites = new List<PartyInvite>();
		[SerializeField] private GameObject _noInvitesPendingText;

		public PartyFeatureControl FeatureControl;
		public int EnrichOrder;

		public PlayersListPresenter InvitesList;
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

			Context.Party.OnPlayerInvited -= OnPlayerInvited;
			Context.Party.OnPlayerInvited += OnPlayerInvited;

			await RefreshInvitesList();

			BackButton.onClick.ReplaceOrAddListener(OnBackButtonClicked);
		}

		private void OnDisable()
		{
			Context.Party.OnPlayerInvited -= OnPlayerInvited;
		}

		protected virtual async Promise RefreshInvitesList()
		{
			var pendingInvites = await Context.Party.GetInvites();
			ReceivedInvites = pendingInvites.invitations;

			_noInvitesPendingText.SetActive(pendingInvites.invitations.Count == 0);

			List<long> playerIds = new List<long>(pendingInvites.invitations.Count);
			foreach (var invite in pendingInvites.invitations)
			{
				if (long.TryParse(invite.invitedBy, out long id))
				{
					playerIds.Add(id);
				}
			}

			await InvitesList.Setup(playerIds, false, OnInviteAccepted, null, null, null);
		}

		private async void OnPlayerInvited(PartyInviteNotification notification)
		{
			await RefreshInvitesList();
		}

		private async void OnInviteAccepted(string playerId)
		{
			for (int i = 0; i < ReceivedInvites.Count; i++)
			{
				var invite = ReceivedInvites[i];
				if (invite.invitedBy == playerId)
				{
					await Context.Party.Join(invite.partyId);
					FeatureControl.OpenPartyView();
					return;
				}
			}
		}

		private void OnBackButtonClicked()
		{
			FeatureControl.OpenCreatePartyView();
		}
	}
}
