using Beamable;
using Beamable.EasyFeatures;
using Beamable.EasyFeatures.BasicLeaderboard;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace EasyFeaturesIntegrationExamples.FeatureCompositionIntegration
{
	/// <summary>
	/// This is a simple example of how to leverage our prefabs in scene-based UIs that have 3D assets loaded based on information already loaded by the basic EasyFeature system.
	/// It's easy to set this up to get an instance of a character model instead of a cube, for example, and to play an animation on it instead of simply rotating it.  
	/// </summary>
	public class Player3DAssetLeaderboardView : MonoBehaviour, ISyncBeamableView
	{
		public GameObject PlayerAsset;

		public float RotateSpeedIfAboveThreshold;
		public long RankThresholdForRotation;

		public Coroutine RotatingCoroutine;

		public bool IsVisible
		{
			get => gameObject.activeSelf;
			set => gameObject.SetActive(value);
		}
		public int GetEnrichOrder() => int.MaxValue;

		public void EnrichWithContext(BeamContextGroup managedPlayers)
		{
			// Gets the current BeamContext feeding data to this view.
			var currentContext = managedPlayers.GetSinglePlayerContext();

			// Gets the service that is feeding data into the BasicLeaderboardView
			var leaderboardViewDeps = currentContext.ServiceProvider.GetService<BasicLeaderboardView.ILeaderboardDeps>();

			// If that service has no ranks, do nothing.
			if (leaderboardViewDeps.Ranks.Count == 0)
				return;

			// Otherwise, get the player rank 
			var playerRank = leaderboardViewDeps.PlayerRank;

			// Stop rotating if we were already rotating.
			if (RotatingCoroutine != null)
				StopCoroutine(RotatingCoroutine);

			// Start rotating the cube if the player rank is higher than the given threshold.
			var speed = playerRank <= RankThresholdForRotation ? RotateSpeedIfAboveThreshold : 0;
			RotatingCoroutine = StartCoroutine(Rotate(PlayerAsset, speed));


			// This is just a helper coroutine that rotates an arbitrary GO.
			IEnumerator Rotate(GameObject toRotate, float rotateSpeed)
			{
				while (true)
				{
					toRotate.transform.localEulerAngles += Vector3.up * rotateSpeed;
					yield return null;
				}
			}
		}
	}
}
