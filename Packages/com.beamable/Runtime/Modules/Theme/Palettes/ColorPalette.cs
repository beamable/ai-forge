using UnityEngine;

namespace Beamable.Theme.Palettes
{
	[System.Serializable]
	public class ColorStyle : PaletteStyle
	{
		public Color Color;
	}

	[System.Serializable]
	public class ColorPalette : Palette<ColorStyle>
	{
		public override ColorStyle DefaultValue()
		{
			return new ColorStyle
			{
				Name = "default",
				Enabled = true,
				Color = Color.white
			};
		}
	}

	[System.Serializable]
	public class ColorBinding : ColorPalette.PaletteBinding { }
}
