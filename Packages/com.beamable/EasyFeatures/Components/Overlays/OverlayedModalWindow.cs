using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.EasyFeatures.Components
{
	public class OverlayedModalWindow : MonoBehaviour, IOverlayComponent
	{
		public enum Mode
		{
			Default,
			Confirm,
		}

		[Header("Components")]
		public TextMeshProUGUI Content;
		public Button CancelButton;
		public Button ConfirmButton;

		public void Show(string content, Action confirmAction, Action closeAction, Mode mode = Mode.Default)
		{
			Content.text = content;

			CancelButton.onClick.ReplaceOrAddListener(closeAction.Invoke);
			ConfirmButton.onClick.ReplaceOrAddListener(confirmAction.Invoke);

			if (mode == Mode.Default)
			{
				CancelButton.gameObject.SetActive(false);
			}

			gameObject.SetActive(true);
		}

		public void Hide()
		{
			gameObject.SetActive(false);
			CancelButton.onClick.RemoveAllListeners();
			ConfirmButton.onClick.RemoveAllListeners();
		}
	}
}
