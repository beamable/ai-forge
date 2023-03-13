using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Common.Dependencies;
using Beamable.EasyFeatures.Components;
using Beamable.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Beamable.EasyFeatures.BasicLobby
{
	[BeamContextSystem]
	public class LobbyFeatureControl : MonoBehaviour, IBeamableFeatureControl, IOverlayController
	{
		protected enum View
		{
			MainMenu,
			CreateLobby,
			JoinLobby,
			InsideLobby
		}

		[Header("Feature Control")]
		[SerializeField] private bool _runOnEnable = true;

		public BeamableViewGroup ViewGroup;
		public LobbyOverlaysController OverlaysController;

		[Header("Components")]
		public GameObject LoadingIndicator;

		[Header("Fast-Path Configuration")]
		public List<SimGameTypeRef> GameTypesRefs;

		public UnityEvent OnMatchStarted;

		public BeamContext BeamContext;

		protected CreateLobbyPlayerSystem CreateLobbyPlayerSystem;
		protected LobbyPlayerSystem LobbyPlayerSystem;
		protected JoinLobbyPlayerSystem JoinLobbyPlayerSystem;

		private IBeamableView _currentView;
		private readonly Dictionary<View, IBeamableView> views = new Dictionary<View, IBeamableView>();

		public bool RunOnEnable { get => _runOnEnable; set => _runOnEnable = value; }

		public IEnumerable<BeamableViewGroup> ManagedViewGroups
		{
			get => new[] { ViewGroup };
			set => ViewGroup = value.FirstOrDefault();
		}

		public List<SimGameType> GameTypes { get; set; }

		[RegisterBeamableDependencies(Constants.SYSTEM_DEPENDENCY_ORDER)]
		public static void RegisterDefaultViewDeps(IDependencyBuilder builder)
		{
			builder.SetupUnderlyingSystemSingleton<JoinLobbyPlayerSystem, JoinLobbyView.IDependencies>();
			builder.SetupUnderlyingSystemSingleton<CreateLobbyPlayerSystem, CreateLobbyView.IDependencies>();
			builder.SetupUnderlyingSystemSingleton<LobbyPlayerSystem, LobbyView.IDependencies>();
		}

		public void OnEnable()
		{
			ViewGroup.RebuildManagedViews();

			if (!RunOnEnable)
			{
				return;
			}

			Run();
		}

		public async void Run()
		{
			LoadingIndicator.SetActive(true);

			// Ensures the player contexts this view is configured to use are ready (frictionless login flow completed). 
			await ViewGroup.RebuildPlayerContexts(ViewGroup.AllPlayerCodes);

			BeamContext = ViewGroup.AllPlayerContexts[0];
			await BeamContext.OnReady;

			JoinLobbyPlayerSystem = BeamContext.ServiceProvider.GetService<JoinLobbyPlayerSystem>();
			CreateLobbyPlayerSystem = BeamContext.ServiceProvider.GetService<CreateLobbyPlayerSystem>();
			LobbyPlayerSystem = BeamContext.ServiceProvider.GetService<LobbyPlayerSystem>();

			GameTypes = await FetchGameTypes();

			JoinLobbyPlayerSystem.Setup(GameTypes);
			CreateLobbyPlayerSystem.Setup(GameTypes);

			JoinLobbyView joinLobbyView = ViewGroup.ManagedViews.OfType<JoinLobbyView>().First();
			joinLobbyView.OnError = ShowErrorWindow;

			CreateLobbyView createLobbyView = ViewGroup.ManagedViews.OfType<CreateLobbyView>().First();
			createLobbyView.OnError = ShowErrorWindow;

			LobbyView insideLobbyView = ViewGroup.ManagedViews.OfType<LobbyView>().First();
			insideLobbyView.OnError = ShowErrorWindow;

			foreach (var view in ViewGroup.ManagedViews)
			{
				views.Add(TypeToViewEnum(view.GetType()), view);
			}

			OpenView(View.MainMenu);
		}

		protected virtual View TypeToViewEnum(Type type)
		{
			if (type == typeof(CreateLobbyView))
			{
				return View.CreateLobby;
			}

			if (type == typeof(LobbyView))
			{
				return View.InsideLobby;
			}

			if (type == typeof(MainLobbyView))
			{
				return View.MainMenu;
			}

			if (type == typeof(JoinLobbyView))
			{
				return View.JoinLobby;
			}

			throw new ArgumentException("View enum does not support provided type.");
		}

		protected virtual async void OpenView(View newView)
		{
			if (_currentView != null)
			{
				_currentView.IsVisible = false;
			}

			_currentView = views[newView];
			_currentView.IsVisible = true;

			await ViewGroup.Enrich();
			LoadingIndicator.SetActive(false);
		}

		public virtual async Promise<List<SimGameType>> FetchGameTypes()
		{
			Assert.IsTrue(GameTypesRefs.Count > 0, "Game types count configured in inspector must be greater than 0");

			List<SimGameType> gameTypes = new List<SimGameType>();

			foreach (SimGameTypeRef simGameTypeRef in GameTypesRefs)
			{
				SimGameType simGameType = await simGameTypeRef.Resolve();
				gameTypes.Add(simGameType);
			}

			return gameTypes;
		}

		public void OpenMainView()
		{
			OpenView(View.MainMenu);
		}

		public void OpenJoinLobbyView()
		{
			OpenView(View.JoinLobby);
		}

		public void OpenCreateLobbyView()
		{
			JoinLobbyPlayerSystem.HasInitialData = false;
			OpenView(View.CreateLobby);
		}

		public void OpenLobbyView()
		{
			if (BeamContext.Lobby == null)
			{
				return;
			}

			BeamContext.Lobby.OnLoadingFinished -= OnLobbyUpdated;
			BeamContext.Lobby.OnLoadingFinished += OnLobbyUpdated;

			LobbyPlayerSystem.RegisterLobbyPlayers(BeamContext.Lobby.Value.players);

			OpenView(View.InsideLobby);
		}

		public virtual async void OnLobbyUpdated()
		{
			if (BeamContext.Lobby.ChangeData.Event == PlayerLobby.LobbyEvent.None)
			{
				return;
			}

			switch (BeamContext.Lobby.ChangeData.Event)
			{
				case PlayerLobby.LobbyEvent.LobbyDisbanded:
					ShowInformWindow("Lobby was disbanded", OpenMainView);
					break;
				case PlayerLobby.LobbyEvent.PlayerJoined:
				case PlayerLobby.LobbyEvent.PlayerLeft:
				case PlayerLobby.LobbyEvent.DataChanged:
					LobbyPlayerSystem.RegisterLobbyPlayers(BeamContext.Lobby.Value.players);
					await ViewGroup.Enrich();
					break;
				case PlayerLobby.LobbyEvent.PlayerKicked:
					if (BeamContext.Lobby.ChangeData.Data == BeamContext.PlayerId.ToString())
					{
						ShowInformWindow("You have been kicked", OpenMainView);
					}
					else
					{
						LobbyPlayerSystem.RegisterLobbyPlayers(BeamContext.Lobby.Value.players);
						await ViewGroup.Enrich();
					}
					break;
				case PlayerLobby.LobbyEvent.HostPlayerChanged:
					if (BeamContext.Lobby.ChangeData.Data == BeamContext.PlayerId.ToString())
					{
						ShowInformWindow("You have been promoted to lobby host", null);
					}
					LobbyPlayerSystem.RegisterLobbyPlayers(BeamContext.Lobby.Value.players);
					await ViewGroup.Enrich();
					break;
				case PlayerLobby.LobbyEvent.LobbyCreated:
				case PlayerLobby.LobbyEvent.None:
					break;
			}
		}

		#region IOverlayController

		public void HideOverlay()
		{
			OverlaysController.HideOverlay();
		}

		public void ShowOverlayedLabel(string label)
		{
			OverlaysController.ShowLabel(label);
		}

		public void ShowErrorWindow(string message)
		{
			OverlaysController.ShowError(message);
		}

		public void ShowConfirmWindow(string message, Action confirmAction)
		{
			OverlaysController.ShowConfirm(message, confirmAction);
		}

		public void ShowInformWindow(string message, Action confirmAction)
		{
			OverlaysController.ShowInform(message, confirmAction);
		}

		#endregion

		#region CreateLobbyView callbacks

		public virtual void CreateLobbyRequestSent()
		{
			ShowOverlayedLabel("Creating lobby...");
		}

		public virtual void CreateLobbyRequestReceived()
		{
			HideOverlay();

			if (BeamContext.Lobby.Value != null)
			{
				CreateLobbyPlayerSystem.ResetData();
				OpenLobbyView();
			}
		}

		#endregion

		#region JoinLobbyView callbacks

		public virtual void JoinLobbyRequestSent()
		{
			ShowOverlayedLabel("Joining lobby...");
		}

		public virtual void JoinLobbyRequestReceived()
		{
			HideOverlay();

			if (BeamContext.Lobby.Value != null)
			{
				OpenLobbyView();
			}
		}

		public virtual void GetLobbiesRequestSent()
		{
			ShowOverlayedLabel("Getting lobbies...");
		}

		public virtual void GetLobbiesRequestReceived()
		{
			HideOverlay();
		}

		#endregion

		#region InsideLobbyView callbacks

		public virtual void StartMatchRequestSent()
		{
			if (BeamContext.Lobby != null)
			{
				BeamContext.Lobby.OnUpdated -= OnLobbyUpdated;
			}

			ShowOverlayedLabel("Starting match...");
		}

		public virtual void StartMatchResponseReceived()
		{
			HideOverlay();
			OnMatchStarted?.Invoke();
		}

		public virtual void AdminLeaveLobbyRequestSent()
		{
			async void ConfirmAction()
			{
				if (BeamContext.Lobby != null)
				{
					BeamContext.Lobby.OnUpdated -= OnLobbyUpdated;
				}

				try
				{
					ShowOverlayedLabel("Leaving lobby...");
					await LobbyPlayerSystem.LeaveLobby();
					LobbyLeft();
				}
				catch (Exception e)
				{
					if (e is PlatformRequesterException pre)
					{
						ShowErrorWindow(pre.Error.error);
					}
				}
			}

			ShowConfirmWindow("After leaving lobby it will be closed because You are an admin. Are You sure?",
							  ConfirmAction);
		}

		public virtual void PlayerLeaveLobbyRequestSent()
		{
			if (BeamContext.Lobby != null)
			{
				BeamContext.Lobby.OnUpdated -= OnLobbyUpdated;
			}

			ShowOverlayedLabel("Leaving lobby...");
		}

		public virtual void LobbyLeft()
		{
			OpenJoinLobbyView();
			HideOverlay();
		}

		public virtual void KickPlayerClicked()
		{
			if (LobbyPlayerSystem.CurrentlySelectedPlayerIndex == null)
			{
				return;
			}

			async void ConfirmAction()
			{
				try
				{
					ShowOverlayedLabel("Kicking player...");
					await LobbyPlayerSystem.KickPlayer();
					HideOverlay();
				}
				catch (Exception e)
				{
					if (e is PlatformRequesterException pre)
					{
						ShowErrorWindow(pre.Error.error);
					}
				}
			}

			ShowConfirmWindow("Are You sure You want to kick this player?", ConfirmAction);
		}

		public virtual void PassLeadershipClicked()
		{
			if (LobbyPlayerSystem.CurrentlySelectedPlayerIndex == null)
			{
				return;
			}

			async void ConfirmAction()
			{
				try
				{
					ShowOverlayedLabel("Passing leadership...");
					await LobbyPlayerSystem.PassLeadership();
					LobbyPlayerSystem.CurrentlySelectedPlayerIndex = null;
					HideOverlay();
				}
				catch (Exception e)
				{
					ShowErrorWindow(e.Message);
				}
			}

			ShowConfirmWindow("Are You sure You want to pass the leadership?", ConfirmAction);
		}

		public virtual void SettingsButtonClicked()
		{
			if (!LobbyPlayerSystem.IsPlayerAdmin)
			{
				return;
			}

			void ConfirmAction(string lobbyName, string description, string host)
			{
				LobbyPlayerSystem.UpdateLobby(lobbyName, description, host);
			}

			OverlaysController.ShowLobbySettings(LobbyPlayerSystem.Name, LobbyPlayerSystem.Description, ConfirmAction,
												 BeamContext.Lobby.Passcode);
		}

		#endregion

		public virtual async void RebuildRequested()
		{
			await ViewGroup.Enrich();
		}
	}
}
