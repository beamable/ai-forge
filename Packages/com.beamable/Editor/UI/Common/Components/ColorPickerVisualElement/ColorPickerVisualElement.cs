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
	public class ColorPickerVisualElement : BeamableVisualElement
	{
		public new class UxmlFactory : UxmlFactory<ColorPickerVisualElement, UxmlTraits> { }

		public ColorPickerVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(ColorPickerVisualElement)}/{nameof(ColorPickerVisualElement)}")
		{ }

		public Color SelectedColor { get; private set; }

		public override void Refresh()
		{
			base.Refresh();

			VisualElement main = Root.Q<VisualElement>("mainVisualElement");

			ColorField colorField = new ColorField();
			colorField.name = "colorField";
			colorField.RegisterValueChangedCallback(OnColorChanged);
			main.Add(colorField);
		}

		private void OnColorChanged(ChangeEvent<Color> evt)
		{
			SelectedColor = evt.newValue;
		}
	}
}
