using Beamable;
using Beamable.Common;
using Beamable.Common.Dependencies;
using Beamable.Common.Leaderboards;
using Beamable.EasyFeatures;
using Beamable.Modules.Leaderboards;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Beamable.EasyFeatures.BasicLeaderboard
{
	[BeamContextSystem]
	public class BasicLeaderboardFeatureControl : MonoBehaviour, IBeamableFeatureControl
	{
		[RegisterBeamableDependencies(Order = Constants.SYSTEM_DEPENDENCY_ORDER)]
		public static void RegisterDefaultViewDeps(IDependencyBuilder builder)
		{
			builder.SetupUnderlyingSystemSingleton<BasicLeaderboardPlayerSystem,
				BasicLeaderboardView.ILeaderboardDeps>();
		}

		public const string TEST_DATA_LEADERBOARD_ID = "leaderboards.magical_test_data";

		[Header("Feature Control"), SerializeField]
		private bool _runOnEnable = true;

		public BeamableViewGroup LeaderboardViewGroup;

		[Header("Fast-Path Configuration")]
		public List<LeaderboardRef> LeaderboardRefs;

		public int EntriesAmount;
		public bool TestMode;

		public bool RunOnEnable { get => _runOnEnable; set => _runOnEnable = value; }
		public IEnumerable<BeamableViewGroup> ManagedViewGroups { get => new[] { LeaderboardViewGroup }; set => LeaderboardViewGroup = value.FirstOrDefault(); }

		public virtual void OnEnable()
		{
			// Ask the view group to update it's managed views.
			LeaderboardViewGroup.RebuildManagedViews();

			if (!RunOnEnable) return;
			Run();
		}

		public virtual async void Run()
		{
			// Ensures the player contexts this view is configured to use are ready (frictionless login flow completed). 
			await LeaderboardViewGroup.RebuildPlayerContexts(LeaderboardViewGroup.AllPlayerCodes);

			// Gets the view and puts it into a loading state.
			var basicLeaderboardView = LeaderboardViewGroup.ManagedViews.OfType<BasicLeaderboardView>().First();
			basicLeaderboardView.EnterLoadingState();

			// If we are in test mode, we regenerate a bunch of fake entries and tell the system to focus on a leaderboard with those fake entries.
			if (TestMode)
			{
				// Fake some latency.
				await Promise.Success.WaitForSeconds(1);

				// Generate a bunch of fake entries.
				var testUserEntry = LeaderboardsModelHelper.GenerateCurrentUserRankEntryTestData("_aliasStatObject.StatKey", "100");
				var testEntries = LeaderboardsModelHelper.GenerateLeaderboardsTestData(0, EntriesAmount, testUserEntry, "_aliasStatObject.StatKey", "100");

				// Register the entires as a leaderboard with the BasicLeaderboardPlayerSystem.
				var ctx = LeaderboardViewGroup.AllPlayerContexts[0];
				var leaderboardPlayerSystem = ctx.ServiceProvider.GetService<BasicLeaderboardPlayerSystem>();
				leaderboardPlayerSystem.RegisterLeaderboardEntries(TEST_DATA_LEADERBOARD_ID, testEntries, testUserEntry);

				// Sets it as the system's focused leaderboard (the focused leaderboard is the one that is returned by the short-hand properties of the system).
				leaderboardPlayerSystem.FocusedLeaderboardId = TEST_DATA_LEADERBOARD_ID;

				// Tells the ViewGroup to call it's views enrich methods.
				await LeaderboardViewGroup.EnrichWithPlayerCodes();
			}
			// Otherwise, we go talk to the back-end and update the BasicLeaderboardPlayerSystem's data as we get the response.
			else
			{
				await RefreshViewWithLeaderboard(LeaderboardRefs[0]);
			}
		}

		/// <summary>
		/// Asks the <see cref="BasicLeaderboardPlayerSystem"/> to fetch leaderboard data for the given leaderboard and then tells the ViewGroup to enrich its views again.
		/// </summary>
		public virtual async Promise RefreshViewWithLeaderboard(LeaderboardRef leaderboardRef)
		{
			var ctx = LeaderboardViewGroup.AllPlayerContexts[0];
			var leaderboardPlayerSystem = ctx.ServiceProvider.GetService<BasicLeaderboardPlayerSystem>();
			await leaderboardPlayerSystem.FetchLeaderboardData(leaderboardRef, 0, EntriesAmount);
			await LeaderboardViewGroup.EnrichWithPlayerCodes();
		}

		/// <summary>
		/// Utility to change what the Leaderboard's Back button does when it's clicked.
		/// </summary>
		/// <param name="onBackButtonClicked">The new action the back button should have.</param>
		public virtual void ReconfigureBackButton(UnityAction onBackButtonClicked)
		{
			var basicLeaderboardView = LeaderboardViewGroup.ManagedViews.OfType<BasicLeaderboardView>().First();
			basicLeaderboardView.BackButtonAction.RemoveAllListeners();
			basicLeaderboardView.BackButtonAction.AddListener(onBackButtonClicked);
		}
	}
}
