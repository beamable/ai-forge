using TMPro;
using UnityEngine;

namespace Beamable.Modules.Inventory.Prototypes
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class LanguageLocalizationElement : MonoBehaviour
	{
#pragma warning disable CS0649
		[SerializeField] private string _localizationKey;
#pragma warning restore CS0649

		private TextMeshProUGUI _textComponent;

		public string LocalizationKey
		{
			get => _localizationKey;
			set
			{
				_localizationKey = value;
				Translate();
			}
		}

		private void Translate()
		{
			if (_textComponent == null)
			{
				_textComponent = GetComponent<TextMeshProUGUI>();
			}

			if (_textComponent != null)
			{
				_textComponent.text = LanguageLocalizationManager.GetTranslation(_localizationKey);
			}
		}

		private void Awake()
		{
			if (!string.IsNullOrEmpty(LocalizationKey))
			{
				_textComponent = GetComponent<TextMeshProUGUI>();
				Translate();
			}
		}
	}
}
