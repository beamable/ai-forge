using System.Collections.Generic;
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
	public class LabeledSpritePickerVisualElement : BeamableVisualElement
	{
		public new class UxmlFactory : UxmlFactory<LabeledSpritePickerVisualElement, UxmlTraits> { }

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
				if (ve is LabeledSpritePickerVisualElement component)
				{
					component.Label = _label.GetValueFromBag(bag, cc);
				}
			}
		}

		private SpritePickerVisualElement _spritePicker;
		private Label _label;

		public string Label { get; set; }

		public Sprite SelectedSprite => _spritePicker.SelectedSprite;

		public LabeledSpritePickerVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(LabeledSpritePickerVisualElement)}/{nameof(LabeledSpritePickerVisualElement)}")
		{ }

		public override void Refresh()
		{
			base.Refresh();

			_label = Root.Q<Label>("label");
			_label.text = Label;

			_spritePicker = Root.Q<SpritePickerVisualElement>("imageField");
			_spritePicker.Refresh();
		}
	}
}
