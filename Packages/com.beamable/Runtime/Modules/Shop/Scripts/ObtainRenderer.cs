using Beamable.Api.Payments;
using UnityEngine;

namespace Beamable.Shop
{
	public abstract class ObtainRenderer : MonoBehaviour
	{
		public abstract void RenderObtain(PlayerListingView data);
	}
}
