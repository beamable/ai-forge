using Beamable.Api.Payments;
using Beamable.UI.Scripts;
using UnityEngine;

namespace Beamable.Shop
{
	public class ShopRewardRenderer : MenuBase
	{
		public ObtainRenderer ObtainRenderer;
		public GameObject Frame;

		public PlayerListingView Listing;

		void Start()
		{
			Frame.SetActive(false);
		}

		public override void OnOpened()
		{
			Frame.SetActive(true);

			ObtainRenderer.RenderObtain(Listing);
		}
	}
}
