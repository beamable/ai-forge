using Beamable;
using Beamable.Api.Connectivity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OfflineObject : MonoBehaviour
{
	public UIBehaviour Component;

	private IConnectivityService Connectivity => _beamContext.ServiceProvider.GetService<IConnectivityService>();
	private BeamContext _beamContext;

	private async void Start()
	{
		_beamContext = BeamContext.InParent(this);
		await _beamContext.OnReady;
		Connectivity.OnConnectivityChanged += ToggleOfflineMode;
		ObtainSupportedComponent();
		if (!Connectivity.HasConnectivity)
		{
			ToggleOfflineMode(false);
		}
	}

	private void ToggleOfflineMode(bool offlineStatus)
	{
		if (Component == null)
		{
			return;
		}

		switch (Component)
		{
			case Selectable selectable:
				selectable.interactable = offlineStatus;
				break;
			default:
				Debug.LogWarning("No Offline Functionality selected for GameObject: " + gameObject.name +
								 ". Consider removing this component if not planned for use.");
				break;
		}
	}

	private void ObtainSupportedComponent()
	{
		switch (Component)
		{
			case Button b:
				Component = gameObject.GetComponent<Button>();
				break;
			case TMP_InputField t:
				Component = gameObject.GetComponent<TMP_InputField>();
				break;
		}
	}

	public void OnDestroy()
	{
		if (_beamContext != null && !_beamContext.IsStopped)
		{
			Connectivity.OnConnectivityChanged -= ToggleOfflineMode;
		}

		Destroy(this);
	}
}
