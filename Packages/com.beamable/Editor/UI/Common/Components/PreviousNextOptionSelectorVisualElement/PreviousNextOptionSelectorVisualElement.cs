using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants;

namespace Beamable.Editor.UI.Components
{
	public class PreviousNextOptionSelectorVisualElement : BeamableVisualElement
	{
		public new class UxmlFactory : UxmlFactory<PreviousNextOptionSelectorVisualElement, UxmlTraits>
		{
		}

		public PreviousNextOptionSelectorVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(PreviousNextOptionSelectorVisualElement)}/{nameof(PreviousNextOptionSelectorVisualElement)}")
		{
		}

		private VisualElement _previousBtn;
		private VisualElement _nextBtn;

		private Dictionary<int, string> _options = new Dictionary<int, string>();
		private int _currentOption;
		private Label _label;
		private Action _onOptionChanged;

		public KeyValuePair<int, string> CurrentOption { get; private set; }

		public override void Refresh()
		{
			base.Refresh();

			_previousBtn = Root.Q<VisualElement>("previousBtn");
			_previousBtn.RegisterCallback<MouseDownEvent>(OnPreviousClicked);

			_nextBtn = Root.Q<VisualElement>("nextBtn");
			_nextBtn.RegisterCallback<MouseDownEvent>(OnNextClicked);

			_label = Root.Q<Label>("currentOptionLabel");
			SetCurrentOption(_currentOption);
		}

		public void Setup(Dictionary<int, string> options, int currentOption, Action onOptionChanged)
		{
			_options = options;
			_onOptionChanged = onOptionChanged;
			SetCurrentOption(currentOption);
		}

		public void SetCurrentOption(int currentOption)
		{
			if (_options.Count == 0)
			{
				return;
			}

			_currentOption = currentOption;
			CurrentOption = _options.ElementAt(_currentOption);
			if (_label != null) _label.text = CurrentOption.Value;
			_onOptionChanged?.Invoke();
		}

		private void OnPreviousClicked(MouseDownEvent evt)
		{
			int possibleOption = Mathf.Clamp(_currentOption - 1, 0, _options.Count - 1);
			SetCurrentOption(possibleOption);
		}

		private void OnNextClicked(MouseDownEvent evt)
		{
			int possibleOption = Mathf.Clamp(_currentOption + 1, 0, _options.Count - 1);
			SetCurrentOption(possibleOption);
		}
	}
}
