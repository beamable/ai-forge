using UnityEngine;

namespace Beamable.Theme.Palettes
{

	[System.Serializable]
	public class TransformStyle : PaletteStyle
	{
		public Vector2 PositionOffset;
		public Vector2 Scale = Vector2.one;
	}

	[System.Serializable]
	public class TransformPalette : Palette<TransformStyle>
	{
		public override TransformStyle DefaultValue()
		{
			return new TransformStyle
			{
				Name = "default",
				Enabled = true,
				PositionOffset = Vector2.zero,
			};
		}
	}

	[System.Serializable]
	public class TransformBinding : TransformPalette.PaletteBinding { }
}
