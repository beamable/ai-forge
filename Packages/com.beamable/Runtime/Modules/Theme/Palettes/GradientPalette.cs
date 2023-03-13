using Beamable.UnityEngineClone.UI.Extensions;
using UnityEngine;
using GradientMode = Beamable.UnityEngineClone.UI.Extensions.GradientMode;

namespace Beamable.Theme.Palettes
{
	[System.Serializable]
	public class GradientPalette : Palette<GradientStyle>
	{
		public override GradientStyle DefaultValue()
		{
			return new GradientStyle
			{
				Name = "default",
				Enabled = true,
				Start = new ColorBinding(),
				Finish = new ColorBinding()
			};
		}
	}

	[System.Serializable]
	public class GradientStyle : PaletteStyle
	{
		public GradientMode Mode = GradientMode.Global;
		public GradientDir Direction = GradientDir.Vertical;
		public bool OverrideAllColor = false;
		public ColorBinding Start;
		public ColorBinding Finish;
		public StyleBlendMode StartBlendMode = StyleBlendMode.Multiply;
		public StyleBlendMode FinishBlendMode = StyleBlendMode.Multiply;
		public Color StartTint = Color.white;
		public Color FinishTint = Color.white;

		public override PaletteStyle Clone()
		{
			return new GradientStyle
			{
				Name = Name,
				Enabled = Enabled,
				Mode = Mode,
				Direction = Direction,
				OverrideAllColor = OverrideAllColor,
				Start = Start.Clone(),
				Finish = Finish.Clone(),
				StartBlendMode = StartBlendMode,
				FinishBlendMode = FinishBlendMode,
				StartTint = StartTint,
				FinishTint = FinishTint
			};
		}
	}

	[System.Serializable]
	public class GradientBinding : GradientPalette.PaletteBinding { }

}
