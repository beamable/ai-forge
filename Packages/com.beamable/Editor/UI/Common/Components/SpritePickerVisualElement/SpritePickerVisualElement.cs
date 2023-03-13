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
	public class SpritePickerVisualElement : BeamableVisualElement
	{
		public new class UxmlFactory : UxmlFactory<SpritePickerVisualElement, UxmlTraits> { }

		public SpritePickerVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(SpritePickerVisualElement)}/{nameof(SpritePickerVisualElement)}")
		{ }

		public Sprite SelectedSprite { get; private set; }
		public override void Refresh()
		{
			base.Refresh();
			VisualElement main = Root.Q<VisualElement>("mainVisualElement");
			ObjectField imageField = new ObjectField();
			imageField.objectType = typeof(Sprite);
			imageField.name = "imageField";
			imageField.RegisterValueChangedCallback(SpriteChanged);
			main.Add(imageField);
		}

		private void SpriteChanged(ChangeEvent<Object> evt)
		{
			Sprite sprite = evt.newValue as Sprite;
			SelectedSprite = sprite;
		}
	}
}
