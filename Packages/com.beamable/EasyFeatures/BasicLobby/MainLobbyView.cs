using UnityEngine;

namespace Beamable.EasyFeatures.BasicLobby
{
	public class MainLobbyView : MonoBehaviour, ISyncBeamableView
	{
		[Header("View Configuration")]
		public int EnrichOrder;

		public bool IsVisible
		{
			get => gameObject.activeSelf;
			set => gameObject.SetActive(value);
		}

		public int GetEnrichOrder() => EnrichOrder;

		public virtual void EnrichWithContext(BeamContextGroup managedPlayers)
		{
		}
	}
}
