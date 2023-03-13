using Beamable;
using UnityEngine;
using UnityEngine.UI;

public class OfflineNotificationBehaviour : MonoBehaviour
{
	public GameObject notification;
	public GameObject message;
	public GameObject wifiLostIcon;
	public LayoutElement offlineObjectLayout;
	private IBeamableAPI _engineInstance;

	private async void Start()
	{
		_engineInstance = await Beamable.API.Instance;
		_engineInstance.ConnectivityService.OnConnectivityChanged += toggleOfflineNotification;
		if (!_engineInstance.ConnectivityService.HasConnectivity)
		{
			toggleOfflineNotification(false);
		}
	}

	public void toggleOfflineNotification(bool OfflineStatus)
	{
		if (!OfflineStatus)
		{
			offlineObjectLayout.preferredHeight = 90;
		}
		else
		{
			offlineObjectLayout.preferredHeight = 0;
		}
		notification.SetActive(!OfflineStatus);
		message.SetActive(!OfflineStatus);
		wifiLostIcon.SetActive(!OfflineStatus);
	}
}
