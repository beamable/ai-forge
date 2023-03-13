using System;
using System.Collections.Generic;
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
	public class LabeledDropdownVisualElement : BeamableVisualElement
	{
		public new class UxmlFactory : UxmlFactory<LabeledDropdownVisualElement, UxmlTraits>
		{
		}

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			readonly UxmlStringAttributeDescription _label = new UxmlStringAttributeDescription
			{ name = "label", defaultValue = "Label" };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				if (ve is LabeledDropdownVisualElement component)
				{
					component.Label = _label.GetValueFromBag(bag, cc);
				}
			}
		}

		private Label _label;
		private List<string> _labels;
		private Action<int> _onOptionSelected;

		public DropdownVisualElement Dropdown { get; private set; }

		private string Label { get; set; }

		public LabeledDropdownVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(LabeledDropdownVisualElement)}/{nameof(LabeledDropdownVisualElement)}")
		{

		}

		public override void Refresh()
		{
			base.Refresh();

			_label = Root.Q<Label>("label");
			_label.text = Label;

			Dropdown = Root.Q<DropdownVisualElement>("dropdown");
			Dropdown.Setup(_labels, _onOptionSelected);
			Dropdown.Refresh();
		}

		public void Setup(List<string> labels, Action<int> onOptionSelected)
		{
			_labels = labels;
			_onOptionSelected = onOptionSelected;
		}

		public void Set(int id)
		{
			Dropdown.Set(id);
		}

		public void OverrideLabelWidth(float width)
		{
			_label?.style.SetWidth(width);
		}
	}
}
