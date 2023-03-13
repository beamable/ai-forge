using Beamable.Experimental.Api.Parties;
using Beamable.UI.Buss;
using EasyFeatures.Components;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace Beamable.EasyFeatures.BasicParty
{
	public class CreatePartyView : MonoBehaviour, ISyncBeamableView
	{
		public interface IDependencies : IBeamableViewDeps
		{
			int MaxPlayers { get; set; }
			bool ValidateConfirmButton(int maxPlayers);
		}

		public PartyFeatureControl FeatureControl;
		public int _enrichOrder;

		[Header("Components")]
		public TextMeshProUGUI HeaderText;

		public GameObject PartyIdObject;
		public TMP_InputField PartyIdInputField;
		public TMP_InputField MaxPlayersTextField;
		public Button CopyIdButton;
		public Button NextButton;
		public Button BackButton;
		public BussElement NextButtonBussElement;

		protected BeamContext Context;
		protected IDependencies System;
		protected bool CreateNewParty;

		public bool IsVisible
		{
			get => gameObject.activeSelf;
			set => gameObject.SetActive(value);
		}

		public int GetEnrichOrder() => _enrichOrder;

		public void EnrichWithContext(BeamContextGroup managedPlayers)
		{
			Context = managedPlayers.GetSinglePlayerContext();
			System = Context.ServiceProvider.GetService<IDependencies>();

			if (!IsVisible)
			{
				return;
			}

			CreateNewParty = !Context.Party.IsInParty;
			System.MaxPlayers = CreateNewParty ? 0 : Context.Party.MaxSize;
			HeaderText.text = CreateNewParty ? "CREATE" : "SETTINGS";
			PartyIdObject.gameObject.SetActive(!CreateNewParty);
			PartyIdInputField.text = CreateNewParty ? "" : Context.Party.Id;
			MaxPlayersTextField.text = CreateNewParty ? "" : System.MaxPlayers.ToString();
			MaxPlayersValueChanged(System.MaxPlayers.ToString());

			// set callbacks
			MaxPlayersTextField.onValueChanged.ReplaceOrAddListener(MaxPlayersValueChanged);
			CopyIdButton.onClick.ReplaceOrAddListener(OnCopyIdButtonClicked);
			NextButton.onClick.ReplaceOrAddListener(OnNextButtonClicked);
			BackButton.onClick.ReplaceOrAddListener(OnBackButtonClicked);
		}

		private void ValidateNextButton()
		{
			bool canCreateParty = System.ValidateConfirmButton(System.MaxPlayers);

			NextButton.interactable = canCreateParty;

			if (canCreateParty)
			{
				NextButtonBussElement.SetButtonPrimary();
			}
			else
			{
				NextButtonBussElement.SetButtonDisabled();
			}
		}

		private void OnCopyIdButtonClicked()
		{
			GUIUtility.systemCopyBuffer = Context.Party.Id;
			FeatureControl.OverlaysController.ShowToast("Party ID was copied");
		}

		private void MaxPlayersValueChanged(string value)
		{
			if (int.TryParse(value, out int maxPlayers))
			{
				System.MaxPlayers = maxPlayers;
			}
			else
			{
				System.MaxPlayers = 0;
			}

			ValidateNextButton();
		}

		private void OnBackButtonClicked()
		{
			ReturnToPartyView();
		}

		private void ReturnToPartyView()
		{
			if (!Context.Party.IsInParty)
			{
				FeatureControl.OpenJoinView();
			}

			FeatureControl.OpenPartyView();
		}

		private async void OnNextButtonClicked()
		{
			if (Context.Party.IsInParty)
			{
				if (System.MaxPlayers < Context.Party.PartyMembers.Count)
				{
					FeatureControl.OverlaysController.ShowError($"There's currently {Context.Party.PartyMembers.Count} players in the party. You can't set max size to less than that.");
					return;
				}

				// update party settings
				await Context.Party.Update(PartyRestriction.InviteOnly, System.MaxPlayers);
			}
			else
			{
				// show loading
				await Context.Party.Create(PartyRestriction.InviteOnly, System.MaxPlayers);
			}

			FeatureControl.OpenPartyView();
		}
	}
}
