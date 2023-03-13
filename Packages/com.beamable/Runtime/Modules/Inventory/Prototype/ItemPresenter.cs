using Beamable.Modules.Inventory.Prototypes;
using Beamable.UI.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Beamable.Modules.Inventory
{
	public class ItemPresenter : MonoBehaviour
	{
#pragma warning disable CS0649
		[SerializeField] private Image _icon;
		[SerializeField] private LanguageLocalizationElement _name;
		[SerializeField] private TextMeshProUGUI _amount;
#pragma warning restore CS0649
		public async void Setup(AssetReferenceSprite assetReferenceSprite, string name, int amount)
		{
			_name.LocalizationKey = name;
			_amount.text = amount.ToString();

			_amount.gameObject.SetActive(amount > 0);

			// TODO: think about some caching service and preload assets before instantiation object.
			// It's not a good idea to load sprite each time.
			_icon.sprite = await assetReferenceSprite.LoadSprite();
		}
	}
}
