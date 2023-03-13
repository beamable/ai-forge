using Beamable.UnityEngineClone.UI.Extensions;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Beamable.UI.Scripts
{
	public class BeamableButton : UIBehaviour
	{
		public Button Button;
		public TextMeshProUGUI Text;
		public Gradient Gradient;
		public EventSoundBehaviour SoundBehaviour;
		public bool RequiresConnectivity;
		private IBeamableAPI _engineInstance;


		protected override async void Start()
		{
			base.Start();
			_engineInstance = await API.Instance;
			_engineInstance.ConnectivityService.OnConnectivityChanged += toggleButton;
		}

		public void toggleButton(bool offlineStatus)
		{
			if (RequiresConnectivity && Button != null)
			{
				Button.interactable = offlineStatus;
			}
		}
	}
}
