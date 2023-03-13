using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.EasyFeatures.Components;
using Beamable.UI.Buss;
using EasyFeatures.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Beamable.EasyFeatures.BasicLobby
{
	public class JoinLobbyView : MonoBehaviour, ISyncBeamableView
	{
		public interface IDependencies : IBeamableViewDeps
		{
			bool HasInitialData { get; set; }
			bool IsLoading { get; set; }
			int SelectedGameTypeIndex { get; set; }
			int? SelectedLobbyIndex { get; }
			string NameFilter { get; }
			string Passcode { get; }
			int MinPasscodeLength { get; }
			int CurrentPlayersFilter { get; }
			int MaxPlayersFilter { get; }
			List<SimGameType> GameTypes { get; }
			List<LobbiesListEntryPresenter.ViewData> LobbiesData { get; }
			void ApplyPasscode(string passcode);
			void ApplyFilter(string name);
			void ApplyFilter(string name, int currentPlayers, int maxPlayers);
			void OnLobbySelected(int? obj);
			bool CanJoinLobby();
			Promise JoinLobby();
			Promise GetLobbies();
		}

		[Header("View Configuration")]
		public int EnrichOrder;
		public BeamableViewGroup ViewGroup;

		[Header("Components")]
		public MultiToggleComponent TypesToggle;
		public GameObject NoLobbiesIndicator;
		public LobbiesListPresenter LobbiesList;
		public TMP_InputField FilterField;
		public TMP_InputField PasscodeField;
		public Button ClearFilterButton;
		public Button JoinLobbyButton;
		public Button BackButton;

		public BussElement JoinLobbyButtonBussElement;

		[Header("Callbacks")]
		public UnityEvent OnGetLobbiesRequestSent;
		public UnityEvent OnGetLobbiesRequestReceived;
		public UnityEvent OnJoinLobbyRequestSent;
		public UnityEvent OnJoinLobbyRequestReceived;
		public UnityEvent OnBackButtonClicked;

		public Action<string> OnError;

		protected IDependencies System;

		public bool IsVisible
		{
			get => gameObject.activeSelf;
			set => gameObject.SetActive(value);
		}

		public int GetEnrichOrder() => EnrichOrder;

		public virtual async void EnrichWithContext(BeamContextGroup managedPlayers)
		{
			BeamContext ctx = managedPlayers.GetSinglePlayerContext();
			System = ctx.ServiceProvider.GetService<IDependencies>();

			// We don't need to perform anything in case if view is not visible. Visibility is controlled by a feature control script.
			if (!IsVisible)
			{
				return;
			}

			// Setting up all components
			TypesToggle.Setup(System.GameTypes.Select(gameType => gameType.name).ToList(), OnGameTypeSelected,
							  System.SelectedGameTypeIndex);

			if (!System.HasInitialData)
			{
				await System.GetLobbies();
				System.HasInitialData = true;
			}

			FilterField.onValueChanged.ReplaceOrAddListener(OnFilterApplied);
			PasscodeField.onValueChanged.ReplaceOrAddListener(OnPasscodeEntered);
			ClearFilterButton.onClick.ReplaceOrAddListener(ClearButtonClicked);
			JoinLobbyButton.onClick.ReplaceOrAddListener(JoinLobbyButtonClicked);
			ValidateJoinButton();
			BackButton.onClick.ReplaceOrAddListener(BackButtonClicked);

			FilterField.SetTextWithoutNotify(System.NameFilter);
			PasscodeField.SetTextWithoutNotify(System.Passcode);

			LobbiesList.gameObject.SetActive(!System.IsLoading);
			NoLobbiesIndicator.SetActive(System.LobbiesData.Count == 0 && !System.IsLoading);

			if (System.IsLoading)
			{
				return;
			}

			LobbiesList.ClearPooledRankedEntries();
			LobbiesList.Setup(System.LobbiesData, OnLobbySelected);
			LobbiesList.RebuildPooledLobbiesEntries();
		}

		public virtual void ValidateJoinButton()
		{
			bool canJoinLobby = System.CanJoinLobby();

			JoinLobbyButton.interactable = canJoinLobby;

			if (canJoinLobby)
			{
				JoinLobbyButtonBussElement.SetButtonPrimary();
			}
			else
			{
				JoinLobbyButtonBussElement.SetButtonDisabled();
			}
		}

		public virtual async void JoinLobbyButtonClicked()
		{
			try
			{
				OnJoinLobbyRequestSent?.Invoke();
				await System.JoinLobby();
				OnJoinLobbyRequestReceived?.Invoke();
			}
			catch (Exception e)
			{
				System.ApplyPasscode(string.Empty);
				OnLobbySelected(null);
				await ViewGroup.Enrich();

				if (e is PlatformRequesterException pre)
				{
					OnError?.Invoke(pre.Error.error);
				}
			}
		}

		public virtual async void OnGameTypeSelected(int optionId)
		{
			if (optionId == System.SelectedGameTypeIndex)
			{
				return;
			}

			OnLobbySelected(null);

			System.SelectedGameTypeIndex = optionId;

			OnGetLobbiesRequestSent?.Invoke();
			System.IsLoading = true;
			await ViewGroup.Enrich();

			try
			{
				await System.GetLobbies();
				OnGetLobbiesRequestReceived?.Invoke();
				System.IsLoading = false;
				await ViewGroup.Enrich();
			}
			catch (Exception e)
			{
				OnError?.Invoke(e.Message);
				// if (e is PlatformRequesterException pre)
				// {
				//		OnError?.Invoke(pre.Error.error);
				// }
			}
		}

		private void BackButtonClicked()
		{
			OnLobbySelected(null);
			OnBackButtonClicked?.Invoke();
		}

		public virtual void OnLobbySelected(int? lobbyId)
		{
			System.OnLobbySelected(lobbyId);
			ValidateJoinButton();
		}

		public virtual async void ClearButtonClicked()
		{
			System.ApplyFilter(String.Empty);
			await ViewGroup.Enrich();
		}

		public virtual async void OnPasscodeEntered(string passcode)
		{
			System.ApplyPasscode(passcode);
			await ViewGroup.Enrich();
		}

		public virtual async void OnFilterApplied(string filter)
		{
			System.ApplyFilter(filter);
			await ViewGroup.Enrich();
		}
	}
}
