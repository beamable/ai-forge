using Beamable.Api.Payments;
using Beamable.Common.Inventory;
using Beamable.UI.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Shop.Defaults
{
	public class ObtainSummaryListCurrencyRenderer : MonoBehaviour
	{
		public RawImage Icon;
		public TextMeshProUGUI Quantity;
		public TextMeshProUGUI Name;

		public async void RenderObtainCurrency(ObtainCurrency data)
		{
			Name.text = data.symbol.Split('.')[1];
			Quantity.text = data.amount.ToString();

			var currency = await new CurrencyRef { Id = data.symbol }.Resolve();
			Icon.texture = await currency.icon.LoadTexture();
		}
	}
}
