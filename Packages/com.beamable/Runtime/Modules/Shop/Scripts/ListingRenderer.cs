using Beamable.Api.Payments;
using UnityEngine;

namespace Beamable.Shop
{
	public abstract class ListingRenderer : MonoBehaviour
	{
		public abstract void RenderListing(PlayerStoreView store, PlayerListingView listing);
	}
}
