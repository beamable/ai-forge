using Beamable.Theme.Palettes;
using Beamable.UI.Scripts;

namespace Beamable.Theme.Appliers
{
	[System.Serializable]
	public class WindowStyleApplier : StyleApplier<BeamableWindow>
	{
		public WindowBinding Window;

		private ImageStyleApplier _imageApplier = new ImageStyleApplier();
		private GradientStyleApplier _gradientApplier = new GradientStyleApplier();

		public override void Apply(ThemeObject theme, BeamableWindow component)
		{
			var style = theme.GetPaletteStyle(Window);
			if (style == null) return;

			if (component.HeaderImage != null)
			{
				_imageApplier.ImageBinding = style.HeaderImage;
				_imageApplier.ColorBinding = style.HeaderColor;
				_imageApplier.Apply(theme, component.HeaderImage);
			}

			if (component.HeaderGradient != null)
			{
				_gradientApplier.GradientBinding = style.HeaderGradient;
				_gradientApplier.Apply(theme, component.HeaderGradient);
			}

			if (component.HeaderElement != null)
			{
				component.HeaderElement.preferredHeight = style.HeaderHeight;
			}

			if (component.WindowTransform != null)
			{
				component.WindowTransform.pivot = style.Pivot;
				component.WindowTransform.anchoredPosition = style.AnchoredPosition;
				component.WindowTransform.anchorMax = style.AnchorMax;
				component.WindowTransform.anchorMin = style.AnchorMin;
				component.WindowTransform.sizeDelta = style.SizeDelta;
				component.WindowTransform.anchoredPosition3D = style.AnchoredPosition3D;
				component.WindowTransform.offsetMax = style.OffsetMax;
				component.WindowTransform.offsetMin = style.OffsetMin;
				component.WindowTransform.ForceUpdateRectTransforms();
			}
		}
	}
}
