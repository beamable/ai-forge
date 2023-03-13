using Beamable.Theme.Palettes;
using UnityEngine.UI;

namespace Beamable.Theme.Appliers
{
	[System.Serializable]
	public class ImageStyleApplier : StyleApplier<Image>
	{
		public ColorBinding ColorBinding;
		public ImageBinding ImageBinding;

		public override void Apply(ThemeObject theme, Image component)
		{
			var colorStyle = theme.GetPaletteStyle(ColorBinding);
			var imageStyle = theme.GetPaletteStyle(ImageBinding);

			if (!ImageBinding.Exists())
			{
				component.enabled = false;
				return;
			}

			component.enabled = true;
			component.material = imageStyle.Material;
			component.sprite = imageStyle.Sprite;
			component.type = imageStyle.ImageType;
			component.color = imageStyle.BlendMode.Blend(colorStyle.Color, imageStyle.TintColor);
		}
	}
}
