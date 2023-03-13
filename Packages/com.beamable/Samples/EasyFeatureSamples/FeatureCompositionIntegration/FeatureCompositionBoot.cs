using Beamable.Common.Dependencies;
using Beamable.EasyFeatures.BasicLeaderboard;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EasyFeaturesIntegrationExamples.FeatureCompositionIntegration
{
	public class FeatureCompositionBoot : MonoBehaviour
	{
		public void OpenLeaderboardScene()
		{
			var loadSceneAsync = SceneManager.LoadSceneAsync("FeatureCompositionIntegrationScene");
			loadSceneAsync.completed += _ =>
			{
				// Gets the feature control for the BasicLeaderboard EasyFeature in the loaded scene.
				var leaderboardFeatureControl = FindObjectOfType<BasicLeaderboardFeatureControl>();

				// Gets the Player3D Asset Leaderboard View in the scene and add it to the Leaderboard prefab's BeamableViewGroup.
				var playerAssetLeaderboardView = FindObjectOfType<Player3DAssetLeaderboardView>();
				leaderboardFeatureControl.LeaderboardViewGroup.ManagedViews.Add(playerAssetLeaderboardView);

				// Show the leaderboard
				leaderboardFeatureControl.gameObject.SetActive(true);

				// Setup the back button
				leaderboardFeatureControl.ReconfigureBackButton(() => SceneManager.LoadScene("FeatureCompositionSample"));
				leaderboardFeatureControl.Run();
			};
		}
	}
}
