using TMPro;
using UnityEngine;

namespace Beamable.EasyFeatures.Components
{
	public class OverlayedLabel : MonoBehaviour, IOverlayComponent
	{
		[Header("Components")]
		public TextMeshProUGUI Label;

		public void Show(string message)
		{
			Label.text = message;
			gameObject.SetActive(true);
		}

		public void Hide()
		{
			gameObject.SetActive(false);
			Label.text = string.Empty;
		}
	}
}
