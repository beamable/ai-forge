using Beamable.Theme.Palettes;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Theme.Appliers
{
	[System.Serializable]
	public class LayoutStyleApplier : StyleApplier<LayoutGroup>
	{
		public LayoutBinding LayoutBinding;
		public override void Apply(ThemeObject theme, LayoutGroup component)
		{
			var layoutStyle = theme.GetPaletteStyle(LayoutBinding);
			component.padding.bottom = layoutStyle.Padding.bottom;
			component.padding.top = layoutStyle.Padding.top;
			component.padding.right = layoutStyle.Padding.right;
			component.padding.left = layoutStyle.Padding.left;

			LayoutRebuilder.MarkLayoutForRebuild(component.GetComponent<RectTransform>());
		}
	}
}
