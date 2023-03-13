using System;

namespace Beamable.EasyFeatures.Components
{
	public interface IOverlayController
	{
		void HideOverlay();
		void ShowOverlayedLabel(string label);
		void ShowErrorWindow(string message);
		void ShowConfirmWindow(string message, Action confirmAction);
		void ShowInformWindow(string message, Action confirmAction);
	}
}
