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
	public class BeamableCheckboxVisualElement : BeamableVisualElement
	{
		public new class UxmlFactory : UxmlFactory<BeamableCheckboxVisualElement, UxmlTraits>
		{
		}

		// TODO: remove after implementing composite validation rules
		public Action OnValueChangedNotifier;
		public event Action<bool> OnValueChanged;

		public bool Value
		{
			get => _value;
			set
			{
				SetWithoutNotify(value);
				OnValueChanged?.Invoke(value);
				OnValueChangedNotifier?.Invoke();
			}
		}

		public Button Button => _button;

		private bool _value;

		private VisualElement _onNotifier;
		private Button _button;

		public BeamableCheckboxVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(BeamableCheckboxVisualElement)}/{nameof(BeamableCheckboxVisualElement)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();
			_onNotifier = Root.Q<VisualElement>("onNotifier");
			_button = Root.Q<Button>("checkboxButton");
			_button.clickable.clicked += () => Value = !_value; ;
			UpdateLook();
		}

		public void SetWithoutNotify(bool value)
		{
			_value = value;
			UpdateLook();
		}

		void UpdateLook()
		{
			_onNotifier.visible = Value;
		}
	}
}
