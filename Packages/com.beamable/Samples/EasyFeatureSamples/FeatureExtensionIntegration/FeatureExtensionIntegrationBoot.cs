using Beamable.Api.Leaderboard;
using Beamable.Common.Api;
using Beamable.Common.Dependencies;
using Beamable.EasyFeatures.BasicLeaderboard;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EasyFeaturesIntegrationExamples.FeatureExtensionIntegration
{

	/// <summary>
	/// This class explains how you can extend, via inheritance, the behaviour of EasyFeature prefabs.
	/// Please, take a look at the prefab variant here as well to understand how to extend the EasyFeature's visuals as well.
	/// </summary>
	[BeamContextSystem]
	public class FeatureExtensionIntegrationBoot : MonoBehaviour
	{
		/// <summary>
		/// This function can be used to modify Beamable's Dependency Injection <see cref="Beamable.Beam"/> and <see cref="Beamable.BeamContext"/>.
		/// </summary>
		[RegisterBeamableDependencies]
		public static void SetupBeamableDependencies(IDependencyBuilder builder)
		{
			// Beamable EasyFeature views and systems get their dependencies via BeamContext.
			// This means you can swap the implementation of an underlying system and other views/systems will start talking to yours by default.
			// This line effectively says:
			builder.SetupUnderlyingSystemSingleton<

				// Setup an instance of this type inside a BeamContext and return it whenever...
				SearchableLeaderboardPlayerSystem,

				// ...someone asks that BeamContext for any of these types.
				BasicLeaderboardPlayerSystem,
				BasicLeaderboardView.ILeaderboardDeps>();
		}

		public GameObject LeaderboardPrefab;
		private BasicLeaderboardFeatureControl _leaderboardInstance;

		public void OpenLeaderboardEasyFeature()
		{
			// Instantiate and cache the instance
			_leaderboardInstance = Instantiate(LeaderboardPrefab).GetComponent<BasicLeaderboardFeatureControl>();

			// Enable it so that the View Group and other scripts run their OnEnable methods.
			_leaderboardInstance.gameObject.SetActive(true);

			// Use the utility functions in FeatureControl to make changes that integrate the feature into your game's control flow.
			_leaderboardInstance.ReconfigureBackButton(() => Destroy(_leaderboardInstance.gameObject));

			// Tell the feature to start up with whatever "Fast-Path configuration is set up in the Feature Control Script".
			_leaderboardInstance.Run();
		}
	}


	/// <summary>
	/// This is an extended version of the <see cref="BasicLeaderboardPlayerSystem"/>.
	/// It adds the ability to filter the list of entries by whatever filter is set in <see cref="CurrentAliasFilter"/>.
	///
	/// Take a look at the FeatureExtension Leaderboard prefab-variant to see how this filter gets set.
	/// </summary>
	public class SearchableLeaderboardPlayerSystem : BasicLeaderboardPlayerSystem
	{
		public string CurrentAliasFilter = "";

		public SearchableLeaderboardPlayerSystem(LeaderboardService leaderboardService, IUserContext ctx) :
			base(leaderboardService, ctx)
		{ }

		public override IEnumerable<BasicLeaderboardView.BasicLeaderboardViewEntry> Entries =>
			base.Entries.Where(e => string.IsNullOrEmpty(CurrentAliasFilter) || e.Alias.Contains(CurrentAliasFilter));
	}
}
