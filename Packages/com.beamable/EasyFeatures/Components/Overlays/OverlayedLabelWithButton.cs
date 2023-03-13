using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.EasyFeatures.Components
{
	public class OverlayedLabelWithButton : MonoBehaviour, IOverlayComponent
	{
		[Header("Components")]
		public TextMeshProUGUI Label;
		public TextMeshProUGUI ButtonLabel;

		public Button Button;

		public void Show(string label, string buttonLabel, Action onClick)
		{
			Label.text = label;
			ButtonLabel.text = buttonLabel;
			Button.onClick.ReplaceOrAddListener(() =>
			{
				onClick?.Invoke();
			});
			gameObject.SetActive(true);
		}

		public void Hide()
		{
			gameObject.SetActive(false);
			Label.text = string.Empty;
			ButtonLabel.text = string.Empty;
			Button.onClick.RemoveAllListeners();
		}
	}
}
