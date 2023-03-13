using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.EasyFeatures.Components;
using Beamable.Experimental.Api.Lobbies;
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
	public class CreateLobbyView : MonoBehaviour, ISyncBeamableView
	{
		public interface IDependencies : IBeamableViewDeps
		{
			List<SimGameType> GameTypes { get; }
			int SelectedGameTypeIndex { get; set; }
			Dictionary<string, LobbyRestriction> AccessOptions { get; } // TODO: remove this dependency
			int SelectedAccessOption { get; set; }
			string Name { get; set; }
			string Description { get; set; }
			bool ValidateConfirmButton();
			void ResetData();
			Promise CreateLobby();
		}

		[Header("View Configuration")]
		public int EnrichOrder;

		[Header("Components")]
		public MultiToggleComponent TypesToggle;
		public MultiToggleComponent AccessToggle;
		public TMP_InputField Name;
		public TMP_InputField Description;
		public Button ConfirmButton;
		public Button CancelButton;
		public Button BackButton;

		public BussElement ConfirmButtonBussElement;

		[Header("Callbacks")]
		public UnityEvent OnCreateLobbyRequestSent;
		public UnityEvent OnCreateLobbyResponseReceived;
		public UnityEvent OnCancelButtonClicked;

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
			BeamContext ctx = managedPlayers.GetSinglePlayerContext();
			System = ctx.ServiceProvider.GetService<IDependencies>();

			// We don't need to perform anything in case if view is not visible. Visibility is controlled by a feature control script.
			if (!IsVisible)
			{
				return;
			}

			// Setting up all components
			TypesToggle.Setup(System.GameTypes.Select(gameType => gameType.name).ToList(), OnGameTypeSelected, System.SelectedGameTypeIndex);
			AccessToggle.Setup(System.AccessOptions.Select(pair => pair.Key).ToList(), OnAccessOptionSelected,
							   System.SelectedAccessOption);

			Name.SetTextWithoutNotify(System.Name);
			Description.SetTextWithoutNotify(System.Description);

			Name.onValueChanged.ReplaceOrAddListener(OnNameChanged);
			Description.onValueChanged.ReplaceOrAddListener(OnDescriptionChanged);
			ConfirmButton.onClick.ReplaceOrAddListener(CreateLobbyButtonClicked);

			ValidateConfirmButton();

			CancelButton.onClick.ReplaceOrAddListener(CancelButtonClicked);
			BackButton.onClick.ReplaceOrAddListener(CancelButtonClicked);
		}

		public virtual void ValidateConfirmButton()
		{
			bool canJoinLobby = System.ValidateConfirmButton();

			ConfirmButton.interactable = canJoinLobby;

			if (canJoinLobby)
			{
				ConfirmButtonBussElement.SetButtonPrimary();
			}
			else
			{
				ConfirmButtonBussElement.SetButtonDisabled();
			}
		}

		private void CancelButtonClicked()
		{
			System.ResetData();
			OnCancelButtonClicked?.Invoke();
		}

		public virtual void OnNameChanged(string value)
		{
			System.Name = value;
			ValidateConfirmButton();
		}

		public virtual void OnDescriptionChanged(string value)
		{
			System.Description = value;
		}

		public virtual void OnAccessOptionSelected(int optionId)
		{
			if (optionId == System.SelectedAccessOption)
			{
				return;
			}

			System.SelectedAccessOption = optionId;
		}

		public virtual void OnGameTypeSelected(int optionId)
		{
			if (optionId == System.SelectedGameTypeIndex)
			{
				return;
			}

			System.SelectedGameTypeIndex = optionId;
		}

		public virtual async void CreateLobbyButtonClicked()
		{
			OnCreateLobbyRequestSent?.Invoke();

			try
			{
				await System.CreateLobby();
				OnCreateLobbyResponseReceived?.Invoke();
			}
			catch (Exception e)
			{
				if (e is PlatformRequesterException pre)
				{
					OnError?.Invoke(pre.Error.error);
				}
			}
		}
	}
}
