using Beamable.Theme.Palettes;
using Beamable.UI.Scripts;
using System;

namespace Beamable.Theme.Appliers
{
	[Serializable]
	public class TransformStyleApplier : StyleApplier<TransformOffsetBehaviour>
	{
		public TransformBinding Transform;
		public override void Apply(ThemeObject theme, TransformOffsetBehaviour component)
		{
			var transformStyle = theme.GetPaletteStyle(Transform);
			if (transformStyle == null) return;

			component.Offset = transformStyle.PositionOffset;
			component.Scale = transformStyle.Scale;
			component.ApplyOffset();
		}
	}
}
