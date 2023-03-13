using UnityEngine;
using UnityEngine.Serialization;

namespace Beamable.UI.Scripts
{
	/// <summary>
	/// Just a "toggler" between two game objects. Mostly used to display Loading Indicators while waiting for a callback.
	/// </summary>
	public class GameObjectToggler : MonoBehaviour
	{
		public GameObject PrimaryRepresentation;
		public GameObject SecondaryRepresentation;

		private void Awake()
		{
			Toggle(true);
		}

		public void Toggle(bool toBackUpRepresentation)
		{
			SecondaryRepresentation.SetActive(toBackUpRepresentation);
			PrimaryRepresentation.SetActive(!toBackUpRepresentation);
		}
	}
}
