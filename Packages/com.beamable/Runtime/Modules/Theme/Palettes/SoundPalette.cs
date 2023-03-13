using UnityEngine;

namespace Beamable.Theme.Palettes
{
	[System.Serializable]
	public class SoundStyle : PaletteStyle
	{
		public AudioClip AudioClip;
		public float Volume = 1;
	}

	[System.Serializable]
	public class SoundPalette : Palette<SoundStyle>
	{
		public override SoundStyle DefaultValue()
		{
			return new SoundStyle
			{
				Name = "default",
				Enabled = true,
				Volume = 1
			};
		}
	}


	[System.Serializable]
	public class SoundBinding : SoundPalette.PaletteBinding { }

}
