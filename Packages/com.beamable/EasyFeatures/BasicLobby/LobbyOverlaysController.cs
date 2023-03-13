using Beamable.EasyFeatures.Components;
using System;
using UnityEngine;

namespace Beamable.EasyFeatures.BasicLobby
{
	public class LobbyOverlaysController : OverlaysController
	{
		[Header("Lobby elements")]
		public OverlayedLobbySettingsWindow SettingsWindow;

		public void ShowLobbySettings(string name, string description, Action<string, string, string> confirmAction, string password = "")
		{
			Show(SettingsWindow, () =>
			{
				SettingsWindow.Show(name, description, (newName, newDescription, newHost) =>
				{
					HideOverlay();
					confirmAction?.Invoke(newName, newDescription, newHost);
				}, HideOverlay, password);
			});
		}
	}
}
