using System;
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
	public class DayToggleVisualElement : BeamableVisualElement
	{
		public new class UxmlFactory : UxmlFactory<DayToggleVisualElement, UxmlTraits>
		{
		}

		public Action OnValueChanged;

		private VisualElement _button;
		private Label _label;
		private string _labelValue;
		private bool _active;

		public bool Selected { get; private set; }
		public string Value { get; private set; }

		public DayToggleVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(DayToggleVisualElement)}/{nameof(DayToggleVisualElement)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();
			_button = Root.Q<VisualElement>("button");
			_label = Root.Q<Label>("buttonLabel");
			_label.text = _labelValue;

			_button.RegisterCallback<MouseDownEvent>(OnClick);

			Render();
		}

		private void Render()
		{
			if (!_active)
			{
				_button?.AddToClassList("inactive");
				return;
			}

			_button?.RemoveFromClassList("inactive");

			_button?.EnableInClassList("checked", Selected);
			_button?.EnableInClassList("unchecked", !Selected);
		}

		private void OnClick(MouseDownEvent evt)
		{
			if (!_active)
			{
				return;
			}

			Selected = !Selected;
			OnValueChanged?.Invoke();
			Render();
		}

		public void Setup(string label, string option)
		{
			_labelValue = label;
			Value = option;
			_active = true;
		}

		public void Set(bool value)
		{
			Selected = value;
			Render();
		}

		public void SetInactive()
		{
			_active = false;
			Render();
		}
	}
}
