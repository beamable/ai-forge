using UnityEngine;

namespace Beamable.Theme.Palettes
{


	[System.Serializable]
	public class WindowStyle : PaletteStyle
	{
		public ImageBinding HeaderImage;
		public ColorBinding HeaderColor;
		public GradientBinding HeaderGradient;
		public float HeaderHeight;
		public Vector2 Pivot, AnchoredPosition, AnchorMax, AnchorMin, OffsetMax, OffsetMin, SizeDelta;
		public Vector3 AnchoredPosition3D;

		public override PaletteStyle Clone()
		{
			var clone = base.Clone() as WindowStyle;
			clone.HeaderImage = HeaderImage.Clone();
			clone.HeaderColor = HeaderColor.Clone();
			clone.HeaderGradient = HeaderGradient.Clone();
			return clone;
		}
	}

	[System.Serializable]
	public class WindowPalette : Palette<WindowStyle>
	{
		public override WindowStyle DefaultValue()
		{
			return new WindowStyle
			{
				Name = "default",
				Enabled = true
			};
		}
	}


	[System.Serializable]
	public class WindowBinding : WindowPalette.PaletteBinding { }
}
