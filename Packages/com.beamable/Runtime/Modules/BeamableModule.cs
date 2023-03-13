using Beamable.InputManagerIntegration;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Beamable
{
	public class BeamableModule : MonoBehaviour
	{
		void OnEnable()
		{
			if (EventSystem.current == null && Application.isPlaying)
			{
				BeamableInput.AddInputSystem();
			}
		}
	}
}
