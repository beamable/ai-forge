using Beamable.EasyFeatures;
using Beamable.EasyFeatures.BasicLeaderboard;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EasyFeaturesIntegrationExamples.BasicIntegration
{
	public class BasicIntegrationBoot : MonoBehaviour
	{
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
}
