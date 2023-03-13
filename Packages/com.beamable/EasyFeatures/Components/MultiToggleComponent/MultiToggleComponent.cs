using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.EasyFeatures.Components
{
	public class MultiToggleComponent : MonoBehaviour
	{
		[SerializeField] private MultiToggleOption _optionPrefab;
		[SerializeField] private ToggleGroup _toggleGroup;

		private readonly List<MultiToggleOption> _spawnedOptions = new List<MultiToggleOption>();

		public void Setup(List<string> options, Action<int> onOptionSelected, int selectedOptionId)
		{
			Clear();

			for (int i = 0; i < options.Count; i++)
			{
				int index = i;
				MultiToggleOption option = Instantiate(_optionPrefab, transform, false);
				option.Setup(options[index], () => { onOptionSelected.Invoke(index); }, _toggleGroup,
							 index == selectedOptionId);

				_spawnedOptions.Add(option);
			}
		}

		private void Clear()
		{
			foreach (var option in _spawnedOptions)
			{
				Destroy(option.gameObject);
			}

			_spawnedOptions.Clear();
		}
	}
}
