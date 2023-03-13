using System.Collections.Generic;
using UnityEngine;

namespace Beamable.UI.Sdf
{
	public static class SdfMaterialManager
	{
		private static readonly int BackgroundTexturePropID = Shader.PropertyToID("_SecondaryTexture");

		private static readonly Dictionary<SdfMaterialData, Material> _materials =
			new Dictionary<SdfMaterialData, Material>();

		public static Material GetMaterial(Material baseMaterial, Texture secondaryTexture,
			SdfImage.SdfMode imageMode, SdfShadowMode shadowMode, SdfBackgroundMode backgroundMode, bool isBackgroundTexMain)
		{

			var baseMaterialID = baseMaterial.GetInstanceID();
			var data = new SdfMaterialData()
			{
				baseMaterialID = baseMaterialID,
				secondaryTextureID = secondaryTexture != null ? secondaryTexture.GetInstanceID() : baseMaterialID,
				imageMode = imageMode,
				shadowMode = shadowMode,
				backgroundMode = backgroundMode,
				isBackgroundTexMain = isBackgroundTexMain
			};

			if (!_materials.TryGetValue(data, out var material) || material == null)
			{
				material = new Material(baseMaterial);
				_materials[data] = material;
				material.SetTexture(BackgroundTexturePropID, secondaryTexture);
				ApplySdfMode(imageMode, material);
				ApplyShadowMode(shadowMode, material);
				ApplyBackgroundMode(backgroundMode, material);
				ApplyMainTextureSource(isBackgroundTexMain, material);
			}

			return material;
			// TODO: cleaning unused materials
		}

		public static void ApplySdfMode(SdfImage.SdfMode mode, Material material)
		{
			const string modeDefault = "_MODE_DEFAULT";
			const string modeRect = "_MODE_RECT";
			switch (mode)
			{
				case SdfImage.SdfMode.Default:
					material.EnableKeyword(modeDefault);
					material.DisableKeyword(modeRect);
					break;
				case SdfImage.SdfMode.RectOnly:
					material.DisableKeyword(modeDefault);
					material.EnableKeyword(modeRect);
					break;
			}
		}

		public static void ApplyShadowMode(SdfShadowMode mode, Material material)
		{
			const string shadowmodeDefault = "_SHADOWMODE_DEFAULT";
			const string shadowmodeInner = "_SHADOWMODE_INNER";
			switch (mode)
			{
				case SdfShadowMode.Default:
					material.EnableKeyword(shadowmodeDefault);
					material.DisableKeyword(shadowmodeInner);
					break;
				case SdfShadowMode.Inner:
					material.DisableKeyword(shadowmodeDefault);
					material.EnableKeyword(shadowmodeInner);
					break;
			}
		}

		public static void ApplyBackgroundMode(SdfBackgroundMode mode, Material material)
		{
			const string bgmodeDefault = "_BGMODE_DEFAULT";
			const string bgmodeOutline = "_BGMODE_OUTLINE";
			const string bgmodeFull = "_BGMODE_FULL";
			switch (mode)
			{
				case SdfBackgroundMode.Default:
					material.EnableKeyword(bgmodeDefault);
					material.DisableKeyword(bgmodeOutline);
					material.DisableKeyword(bgmodeFull);
					break;
				case SdfBackgroundMode.Outline:
					material.DisableKeyword(bgmodeDefault);
					material.EnableKeyword(bgmodeOutline);
					material.DisableKeyword(bgmodeFull);
					break;
				case SdfBackgroundMode.Full:
					material.DisableKeyword(bgmodeDefault);
					material.DisableKeyword(bgmodeOutline);
					material.EnableKeyword(bgmodeFull);
					break;
			}
		}

		public static void ApplyMainTextureSource(bool isBackgroundTexMain, Material material)
		{
			const string keyword = "_BACKGROUND_TEX_AS_MAIN";
			const string keywordNeg = "_BACKGROUND_TEX_AS_MAIN_NEG";

			if (isBackgroundTexMain)
			{
				material.EnableKeyword(keyword);
				material.DisableKeyword(keywordNeg);
			}
			else
			{
				material.DisableKeyword(keyword);
				material.EnableKeyword(keywordNeg);
			}
		}
	}
}
