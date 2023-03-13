using Beamable.UI.Scripts;
using UnityEngine;

namespace Beamable.Announcements
{
	public class AnnouncementBehavior : MonoBehaviour
	{
		public MenuManagementBehaviour MenuManager;

		public void Toggle(bool announcementDesiredState)
		{
			if (!announcementDesiredState && MenuManager.IsOpen)
			{
				MenuManager.CloseAll();
			}
			else if (announcementDesiredState && !MenuManager.IsOpen)
			{
				MenuManager.Show<AnnouncementMainMenu>();
			}
		}
	}
}
