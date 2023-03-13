using Beamable.Common;
using Beamable.UI.Buss;
using EasyFeatures.Components;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Beamable.EasyFeatures.BasicLobby
{
	public class LobbyView : MonoBehaviour, ISyncBeamableView
	{
		public interface IDependencies : IBeamableViewDeps
		{
			List<LobbySlotPresenter.ViewData> SlotsData { get; }
			string Id { get; }
			string Name { get; }
			string Description { get; }
			int MaxPlayers { get; }
			int CurrentPlayers { get; }
			int? CurrentlySelectedPlayerIndex { get; set; }
			bool IsPlayerAdmin { get; }
			bool IsPlayerReady { get; }
			bool IsMatchStarting { get; }
			Promise LeaveLobby();
			void SetPlayerReady(bool value);
			void SetCurrentSelectedPlayer(int slotIndex);
			void UpdateLobby(string name, string description, string host);
			Promise StartMatch();
			bool IsServerReady();
		}

		[Header("View Configuration")]
		public int EnrichOrder;

		[Header("Components")]
		public TextMeshProUGUI Name;

		public TextMeshProUGUI Counter;
		public LobbySlotsListPresenter LobbySlotsList;
		public Button SettingsButton;

		public Button BackButton;
		public Button ReadyButton;
		public Button NotReadyButton;
		public Button StartButton;
		public Button LeaveButton;

		public BussElement StartButtonBussElement;

		[Header("Callbacks")]
		public UnityEvent OnRebuildRequested;

		public UnityEvent OnAdminLeaveLobbyRequestSent;
		public UnityEvent OnPlayerLeaveLobbyRequestSent;
		public UnityEvent OnLobbyLeft;
		public UnityEvent OnKickPlayerClicked;
		public UnityEvent OnPassLeadershipClicked;
		public UnityEvent OnSettingButtonClicked;
		public UnityEvent OnStartMatchRequestSent;
		public UnityEvent OnStartMatchResponseReceived;

		public Action<string> OnError;

		protected IDependencies System;

		public bool IsVisible
		{
			get => gameObject.activeSelf;
			set => gameObject.SetActive(value);
		}

		public int GetEnrichOrder() => EnrichOrder;

		public virtual void EnrichWithContext(BeamContextGroup managedPlayers)
		{
			var ctx = managedPlayers.GetSinglePlayerContext();
			System = ctx.ServiceProvider.GetService<IDependencies>();

			if (!IsVisible)
			{
				return;
			}

			Name.text = System.Name;
			Counter.text = $"{System.CurrentPlayers}/{System.MaxPlayers}";

			// Buttons' callbacks
			SettingsButton.onClick.ReplaceOrAddListener(SettingsButtonClicked);
			ReadyButton.onClick.ReplaceOrAddListener(ReadyButtonClicked);
			NotReadyButton.onClick.ReplaceOrAddListener(NotReadyButtonClicked);
			StartButton.onClick.ReplaceOrAddListener(StartButtonClicked);
			LeaveButton.onClick.ReplaceOrAddListener(LeaveButtonClicked);
			BackButton.onClick.ReplaceOrAddListener(LeaveButtonClicked);

			// Buttons' visibility
			SettingsButton.gameObject.SetActive(System.IsPlayerAdmin);
			ReadyButton.gameObject.SetActive(!System.IsPlayerReady);
			NotReadyButton.gameObject.SetActive(System.IsPlayerReady);

			ValidateStartButton();

			LobbySlotsList.ClearPooledRankedEntries();
			LobbySlotsList.Setup(System.SlotsData, System.IsPlayerAdmin, OnAdminButtonClicked, OnKickButtonClicked,
								 OnPassLeadershipButtonClicked);
			LobbySlotsList.RebuildPooledLobbiesEntries();
		}

		public virtual void ValidateStartButton()
		{
			bool buttonValid = System.IsPlayerAdmin && System.IsServerReady();
			StartButton.interactable = buttonValid;

			if (buttonValid)
			{
				StartButtonBussElement.SetButtonPrimary();
			}
			else
			{
				StartButtonBussElement.SetButtonDisabled();
			}
		}

		private void OnAdminButtonClicked(int slotIndex)
		{
			if (!System.IsPlayerAdmin)
			{
				return;
			}

			System.SetCurrentSelectedPlayer(slotIndex);
			OnRebuildRequested?.Invoke();
		}

		private void OnKickButtonClicked(int slotIndex)
		{
			if (!System.IsPlayerAdmin || System.IsMatchStarting)
			{
				return;
			}

			OnKickPlayerClicked?.Invoke();
		}

		private void OnPassLeadershipButtonClicked(int slotIndex)
		{
			if (!System.IsPlayerAdmin || System.IsMatchStarting)
			{
				return;
			}

			OnPassLeadershipClicked?.Invoke();
		}

		private void SettingsButtonClicked()
		{
			if (!System.IsPlayerAdmin || System.IsMatchStarting)
			{
				return;
			}

			OnSettingButtonClicked?.Invoke();
		}

		public virtual void ReadyButtonClicked()
		{
			if (System.IsMatchStarting)
			{
				return;
			}

			System.SetPlayerReady(true);
		}

		public virtual void NotReadyButtonClicked()
		{
			if (System.IsMatchStarting)
			{
				return;
			}

			System.SetPlayerReady(false);
		}

		public virtual async void StartButtonClicked()
		{
			if (!System.IsPlayerAdmin || System.IsMatchStarting)
			{
				return;
			}

			try
			{
				OnStartMatchRequestSent?.Invoke();
				await System.StartMatch();
				OnStartMatchResponseReceived?.Invoke();
			}
			catch (Exception e)
			{
				OnError?.Invoke(e.Message);
			}
		}

		public virtual async void LeaveButtonClicked()
		{
			if (System.IsMatchStarting)
			{
				return;
			}

			if (System.IsPlayerAdmin)
			{
				OnAdminLeaveLobbyRequestSent?.Invoke();
			}
			else
			{
				try
				{
					OnPlayerLeaveLobbyRequestSent?.Invoke();
					await System.LeaveLobby();
					OnLobbyLeft?.Invoke();
				}
				catch (Exception e)
				{
					OnError?.Invoke(e.Message);
				}
			}
		}
	}
}
