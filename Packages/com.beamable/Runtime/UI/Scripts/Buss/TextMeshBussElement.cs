using TMPro;
using UnityEngine;

namespace Beamable.UI.Buss
{
	[ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(TextMeshProUGUI))]
	public class TextMeshBussElement : BussElement
	{
		private TextMeshProUGUI _text;
		private bool _hasText;
		private bool _hasTMPEssentials;

		public override string TypeName => "text";

		public override void ApplyStyle()
		{
			if (!_hasText)
			{
				_text = GetComponent<TextMeshProUGUI>();
				_hasText = true;
			}

			if (Style == null) return;

			// BASE
			_text.font = BussStyle.Font.Get(Style).FontAsset;
			_text.fontSize = BussStyle.FontSize.Get(Style).FloatValue;
			_text.color = BussStyle.FontColor.Get(Style).Color;

			// Alignment
			_text.alignment = BussStyle.TextAlignment.Get(Style).Enum;

			if (!_hasTMPEssentials && null != Resources.Load<TMP_Settings>("TMP Settings"))
				_hasTMPEssentials = true;
			else
				return;

			if (_text.fontSharedMaterial == null) return;

			// OUTLINE
			float borderWidth = BussStyle.BorderWidth.Get(Style).FloatValue;

			if (borderWidth > 0)
				_text.fontMaterial.EnableKeyword(ShaderUtilities.Keyword_Outline);
			else
				_text.fontMaterial.DisableKeyword(ShaderUtilities.Keyword_Underlay);

			_text.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, borderWidth);
			_text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor,
										BussStyle.BorderColor.Get(Style).ColorRect.TopLeftColor);
			_text.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor,
										BussStyle.ShadowColor.Get(Style).ColorRect.TopLeftColor);

			// SHADOW
			Vector2 shadowOffset = BussStyle.ShadowOffset.Get(Style).Vector2Value;

			if (shadowOffset != Vector2.zero)
				_text.fontMaterial.EnableKeyword(ShaderUtilities.Keyword_Underlay);
			else
				_text.fontMaterial.DisableKeyword(ShaderUtilities.Keyword_Underlay);

			_text.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, shadowOffset.x);
			_text.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, shadowOffset.y);
			_text.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlaySoftness,
										BussStyle.ShadowSoftness.Get(Style).FloatValue);
			_text.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayDilate,
										BussStyle.ShadowThreshold.Get(Style).FloatValue);
		}
	}
}
