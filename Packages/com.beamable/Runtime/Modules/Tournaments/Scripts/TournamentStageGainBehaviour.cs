using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Tournaments
{
	[Serializable]
	public struct TournamentStageGainDefinition
	{
		public int StageGain;
		public TournamentStageGainBehaviour Prefab;
	}

	public class TournamentStageGainBehaviour : MonoBehaviour
	{
		public List<Image> ChevronImages;
		public Material GreyMaterial;

		public void SetEffect(bool useGrey)
		{
			// no-op. No longer needed.
		}
	}
}
