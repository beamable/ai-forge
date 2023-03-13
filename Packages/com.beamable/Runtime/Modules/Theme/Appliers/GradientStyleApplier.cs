using Beamable.Theme.Palettes;
using Beamable.UnityEngineClone.UI.Extensions;

namespace Beamable.Theme.Appliers
{
	[System.Serializable]
	public class GradientStyleApplier : StyleApplier<Gradient>
	{
		public GradientBinding GradientBinding;
		public override void Apply(ThemeObject theme, Gradient component)
		{
			if (!GradientBinding.Exists())
			{
				component.enabled = false;
				return;
			}


			var gradientStyle = theme.GetPaletteStyle(GradientBinding);
			component.enabled = true;
			component.GradientDir = gradientStyle.Direction;
			component.GradientMode = gradientStyle.Mode;
			component.OverwriteAllColor = gradientStyle.OverrideAllColor;
			component.Vertex1 = gradientStyle.StartBlendMode.Blend(
			   theme.GetPaletteStyle(gradientStyle.Start).Color,
			   gradientStyle.StartTint);
			component.Vertex2 = gradientStyle.FinishBlendMode.Blend(
			   theme.GetPaletteStyle(gradientStyle.Finish).Color,
			   gradientStyle.FinishTint);
		}
	}
}
