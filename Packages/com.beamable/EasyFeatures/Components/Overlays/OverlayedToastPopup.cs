using System.Collections;
using TMPro;
using UnityEngine;

namespace Beamable.EasyFeatures.Components
{
	public class OverlayedToastPopup : MonoBehaviour, IOverlayComponent
	{
		public TextMeshProUGUI Content;

		public void Show(string message, float durationSeconds = 3f)
		{
			Content.text = message;
			gameObject.SetActive(true);
			StartCoroutine(HideAfterDelay(durationSeconds));
		}

		public void Hide()
		{
			gameObject.SetActive(false);
		}

		private IEnumerator HideAfterDelay(float delay)
		{
			yield return new WaitForSeconds(delay);
			Hide();
		}
	}
}
