using System;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Content.Components
{
	public class FilterRowVisualElement : ContentManagerComponent
	{
		private Toggle _checkbox;
		public string FilterName { set; get; }
		public event Action<bool> OnValueChanged;

		public FilterRowVisualElement() : base(nameof(FilterRowVisualElement))
		{

		}

		public override void Refresh()
		{
			base.Refresh();
			var title = Root.Q<Label>("tagName");
			_checkbox = Root.Q<Toggle>(name: "filterCheckbox");
			_checkbox.RegisterValueChangedCallback(evt => OnValueChanged?.Invoke(evt.newValue));

			title.text = FilterName;
		}

		public void SetValue(bool value)
		{
			_checkbox.SetValueWithoutNotify(value);
		}
	}


}

