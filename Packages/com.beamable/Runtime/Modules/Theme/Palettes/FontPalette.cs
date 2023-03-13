using TMPro;

namespace Beamable.Theme.Palettes
{
	[System.Serializable]
	public class FontStyle : PaletteStyle
	{
		public TMP_FontAsset FontAsset;
	}
	[System.Serializable]
	public class FontPalette : Palette<FontStyle>
	{
		public override FontStyle DefaultValue()
		{
			return new FontStyle
			{
				Name = "default",
				Enabled = true,
				FontAsset = null
			};
		}
	}


	[System.Serializable]
	public class FontBinding : FontPalette.PaletteBinding { }

}
