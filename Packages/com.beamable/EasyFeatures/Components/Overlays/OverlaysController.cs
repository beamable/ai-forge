using System;
using UnityEngine;

namespace Beamable.EasyFeatures.Components
{
	public class OverlaysController : MonoBehaviour
	{
		[Header("Components")]
		public GameObject Mask;

		[Header("Generic elements")]
		public OverlayedLabel Label;

		public OverlayedModalWindow ModalWindow;
		public OverlayedLabelWithButton LabelWithButton;
		public OverlayedToastPopup ToastPopup;

		private IOverlayComponent _currentObject;

		public void ShowLabel(string label)
		{
			Show(Label, () => { Label.Show(label); });
		}

		public void ShowLabelWithButton(string label, string buttonLabel, Action onClick)
		{
			Show(LabelWithButton, () =>
			{
				LabelWithButton.Show(label, buttonLabel, () =>
{
	HideOverlay();
	onClick?.Invoke();
});
			});
		}

		public void ShowError(string message)
		{
			Show(ModalWindow, () => { ModalWindow.Show(message, HideOverlay, HideOverlay); });
		}

		public void ShowInform(string message, Action confirmAction)
		{
			Show(ModalWindow, () =>
			{
				ModalWindow.Show(message, () =>
				{
					HideOverlay();
					confirmAction?.Invoke();
				}, HideOverlay);
			});
		}

		public void ShowConfirm(string message, Action confirmAction)
		{
			Show(ModalWindow, () =>
			{
				ModalWindow.Show(message, () =>
				{
					HideOverlay();
					confirmAction?.Invoke();
				}, HideOverlay, OverlayedModalWindow.Mode.Confirm);
			});
		}

		public void HideOverlay()
		{
			Mask.SetActive(false);
			_currentObject?.Hide();
			_currentObject = null;
		}

		protected void Show(IOverlayComponent activeComponent, Action action)
		{
			_currentObject?.Hide();
			Mask.SetActive(true);
			action?.Invoke();
			_currentObject = activeComponent;
		}

		public void ShowToast(string message, float duration = 3f)
		{
			ToastPopup.Show(message, duration);
		}
	}
}
