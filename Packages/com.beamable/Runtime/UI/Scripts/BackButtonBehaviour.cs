using UnityEngine;

namespace Beamable.UI.Scripts
{
	public class BackButtonBehaviour : MonoBehaviour
	{
		public MenuManagementBehaviour MenuManager;

		public void GoBack()
		{
			MenuManager.GoBack();
		}
	}
}
