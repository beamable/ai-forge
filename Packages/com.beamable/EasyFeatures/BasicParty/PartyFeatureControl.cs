using Beamable.Common.Dependencies;
using Beamable.EasyFeatures.Components;
using Beamable.Experimental.Api.Parties;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.EasyFeatures.BasicParty
{
	[BeamContextSystem]
	public class PartyFeatureControl : MonoBehaviour, IBeamableFeatureControl
	{
		private enum View
		{
			Party,
			Create,
			Join,
			Invite,
		}

		public BeamableViewGroup PartyViewGroup;
		public OverlaysController OverlaysController;

		protected BeamContext Context;
		protected BasicPartyPlayerSystem PartyPlayerSystem;

		private IBeamableView _currentView;
		private readonly Dictionary<View, IBeamableView> views = new Dictionary<View, IBeamableView>();

		public IEnumerable<BeamableViewGroup> ManagedViewGroups
		{
			get;
		}

		[RegisterBeamableDependencies]
		public static void RegisterDefaultViewDeps(IDependencyBuilder builder)
		{
			builder.SetupUnderlyingSystemSingleton<BasicPartyPlayerSystem, CreatePartyView.IDependencies>();
		}

		[SerializeField]
		private bool _runOnEnable = true;

		public bool RunOnEnable
		{
			get => _runOnEnable;
			set => _runOnEnable = value;
		}

		public void OnEnable()
		{
			PartyViewGroup.RebuildManagedViews();

			if (!_runOnEnable)
			{
				return;
			}

			Run();
		}

		public async void Run()
		{
			await PartyViewGroup.RebuildPlayerContexts(PartyViewGroup.AllPlayerCodes);

			Context = PartyViewGroup.AllPlayerContexts[0];
			await Context.OnReady;

			Context.Party.OnPlayerInvited -= OnPlayerInvitedToParty;
			Context.Party.OnPlayerInvited += OnPlayerInvitedToParty;
			PartyPlayerSystem = Context.ServiceProvider.GetService<BasicPartyPlayerSystem>();

			foreach (var view in PartyViewGroup.ManagedViews)
			{
				views.Add(TypeToViewEnum(view.GetType()), view);
			}

			OpenView(View.Create);
		}

		private async void OnPlayerInvitedToParty(PartyInviteNotification inviteNotification)
		{
			if (_currentView == views[View.Join])
			{
				await PartyViewGroup.Enrich();
			}

			OverlaysController.ShowConfirm($"{inviteNotification.invitingPlayerId} invited you to a party. Would you like to join?", () => AcceptPartyInvite(inviteNotification.partyId));
		}

		private async void AcceptPartyInvite(string partyId)
		{
			await Context.Party.Join(partyId);
			OpenPartyView();
		}

		private View TypeToViewEnum(Type type)
		{
			if (type == typeof(CreatePartyView))
			{
				return View.Create;
			}

			if (type == typeof(InvitePlayersView))
			{
				return View.Invite;
			}

			if (type == typeof(BasicPartyView))
			{
				return View.Party;
			}

			if (type == typeof(JoinPartyView))
			{
				return View.Join;
			}

			throw new ArgumentException("View enum does not support provided type.");
		}

		public void OpenPartyView()
		{
			if (!Context.Party.IsInParty)
			{
				return;
			}

			OpenView(View.Party);
		}

		// when party data is provided the view turns to settings
		public void OpenCreatePartyView()
		{
			OpenView(View.Create);
		}

		public void OpenInviteView()
		{
			OpenView(View.Invite);
		}

		public void OpenJoinView()
		{
			OpenView(View.Join);
		}

		private async void OpenView(View view)
		{
			if (_currentView != null)
			{
				_currentView.IsVisible = false;
			}

			_currentView = views[view];
			_currentView.IsVisible = true;

			await PartyViewGroup.Enrich();
		}

		private void OnDestroy()
		{
			Context.Party.OnPlayerInvited -= OnPlayerInvitedToParty;
		}
	}
}
