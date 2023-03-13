using Beamable.UI.Buss;
using EasyFeatures.Components;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.EasyFeatures.Components
{
	public class MultiToggleOption : MonoBehaviour
	{
		[SerializeField] private Toggle _toggle;
		[SerializeField] private TextMeshProUGUI _label;

		public BussElement MainBussElement;
		public BussElement LabelBussElement;

		private Action _onClickAction;

		public void Setup(string option, Action onClickAction, ToggleGroup group, bool selected)
		{
			_onClickAction = onClickAction;
			_label.text = option;
			_toggle.group = group;
			_toggle.onValueChanged.AddListener(ToggleClicked);
			_toggle.SetIsOnWithoutNotify(selected);

			MainBussElement.UpdateClasses(new List<string> { "toggle", "option" });
			LabelBussElement.UpdateClasses(new List<string> { "toggle", "option", "text" });

			SetSelected(selected);
		}

		private void ToggleClicked(bool value)
		{
			if (value)
			{
				_onClickAction.Invoke();
			}

			SetSelected(value);
		}

		private void SetSelected(bool selected)
		{
			MainBussElement.SetSelected(selected);
			LabelBussElement.SetSelected(selected);
		}
	}
}
