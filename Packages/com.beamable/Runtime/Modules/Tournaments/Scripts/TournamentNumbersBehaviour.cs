using TMPro;
using UnityEngine;

namespace Beamable.Tournaments
{
	public class TournamentNumbersBehaviour : MonoBehaviour
	{
		public TextMeshProUGUI Text;

		public void Set(int number)
		{
			if (gameObject == null) return;

			var isActive = number > 0;
			gameObject.SetActive(isActive);
			Text.text = "" + number;
		}
	}

}
