using Beamable.Api.Payments;
using Beamable.Common.Inventory;
using Beamable.UI.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Shop.Defaults
{
	public class ObtainSummaryListItemRenderer : MonoBehaviour
	{
		public RawImage Icon;
		public TextMeshProUGUI Name;

		public async void RenderObtainItem(ObtainItem data)
		{
			Name.text = data.contentId.Split('.')[1];

			var item = await new ItemRef { Id = data.contentId }.Resolve();
			Icon.texture = await item.icon.LoadTexture();
		}
	}
}
