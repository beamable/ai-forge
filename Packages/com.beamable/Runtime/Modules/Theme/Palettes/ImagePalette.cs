using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Theme.Palettes
{

	[System.Serializable]
	public class ImageStyle : PaletteStyle
	{
		public Material Material;
		public Sprite Sprite;
		public Image.Type ImageType;
		public Color TintColor = Color.white;
		public StyleBlendMode BlendMode = StyleBlendMode.Multiply;
	}
	[System.Serializable]
	public class ImagePalette : Palette<ImageStyle>
	{
		public override ImageStyle DefaultValue()
		{
			return new ImageStyle
			{
				Name = "default",
				Enabled = true
			};
		}
	}
	[System.Serializable]
	public class ImageBinding : ImagePalette.PaletteBinding { }

}
