using UnityEngine;

namespace Beamable.Theme.Palettes
{
	[System.Serializable]
	public class LayoutStyle : PaletteStyle
	{
		public RectOffset Padding;
	}


	[System.Serializable]
	public class LayoutPalette : Palette<LayoutStyle>
	{
		public override LayoutStyle DefaultValue()
		{
			return new LayoutStyle
			{
				Name = "default",
				Enabled = true,
				Padding = new RectOffset()
			};
		}
	}

	[System.Serializable]
	public class LayoutBinding : LayoutPalette.PaletteBinding { }
}
