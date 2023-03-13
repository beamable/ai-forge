using Beamable.Theme.Palettes;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Theme.Appliers
{
	[Serializable]
	public class TextStyleApplier : StyleApplier<TextMeshProUGUI>
	{
		public TextBinding TextBinding;
		public ColorBinding ColorBinding;

		public override void Apply(ThemeObject theme, TextMeshProUGUI component)
		{
			var textStyle = theme.GetPaletteStyle(TextBinding);
			var colorStyle = theme.GetPaletteStyle(ColorBinding);
			var fontStyle = theme.GetPaletteStyle(textStyle.Font);

			component.font = fontStyle?.FontAsset;
			component.fontSize = textStyle.FontSize;
			component.characterSpacing = textStyle.CharacterSpacing;
			component.lineSpacing = textStyle.LineSpacing;

			if (textStyle.TextMaterial == null && component.font != null)
			{
				component.fontMaterial = component.font.material;
			}
			else
			{
				component.fontMaterial = textStyle.TextMaterial;
			}

			component.fontStyle = FontStyles.Normal;
			if (textStyle.FontStyles != null && textStyle.FontStyles.Length > 0)
			{
				foreach (var style in textStyle.FontStyles)
				{
					component.fontStyle |= style;
				}
			}

			component.color = textStyle.BlendMode.Blend(colorStyle.Color, textStyle.TintColor);
			component.SetAllDirty();

			if (Application.isPlaying)
			{
				var parentLayout = component?.GetComponentInParent<LayoutGroup>();
				if (parentLayout != null)
				{
					LayoutRebuilder.ForceRebuildLayoutImmediate(parentLayout.GetComponent<RectTransform>());
				}
			}
		}
	}
}
