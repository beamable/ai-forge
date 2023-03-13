using System;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Modules.Inventory.Prototypes
{
	[RequireComponent(typeof(Button))]
	public class GenericTextButton : MonoBehaviour
	{
		private Button _button;
		private Action _onClick;

		private void Awake()
		{
			_button = GetComponent<Button>();
		}

		private void OnDestroy()
		{
			if (_button != null)
			{
				_button.onClick.RemoveAllListeners();
			}
		}

		public void Setup(Action onClick)
		{
			_onClick = onClick;

			if (_button != null)
			{
				_button.onClick.AddListener(() => { _onClick?.Invoke(); });
			}
		}
	}
}
