using UnityEngine;

namespace Beamable.UI.Scripts
{
	public class CloseButtonBehaviour : MonoBehaviour
	{
		public MenuManagementBehaviour MenuManager;

		public void Close()
		{
			MenuManager.CloseAll();
		}
	}
}
