using Beamable.Common.Content;
using Beamable.UI.Buss;
using Beamable.UI.Sdf;
using System;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace Beamable.UI.Sdf
{
	[ExecuteAlways]
	public class SdfImage : Image
	{

		private BussStyle _style;
		public BussStyle Style
		{
			get => _style;
			set
			{
				_style = value;
				ApplyStyle();
				SetVerticesDirty();
				SetMaterialDirty();
			}
		}

		public ImageType imageType;
		public NineSliceSource nineSliceSource;
		public SdfMode mode;
		public ColorRect colorRect = new ColorRect(Color.white);
		public float threshold;
		public float rounding;
		public Sprite secondaryTexture;
		public SdfBackgroundMode backgroundMode;
		public float meshFrame;
		public float outlineWidth;
		public ColorRect outlineColor;
		public ColorRect shadowColor;
		public Vector2 shadowOffset;
		public float shadowThreshold;
		public float shadowSoftness;
		public SdfShadowMode shadowMode;
		public bool isBackgroundTexMain;

		public Sprite SDFSprite
		{
			get => isBackgroundTexMain ? secondaryTexture : sprite;
			set
			{
				if (isBackgroundTexMain)
				{
					secondaryTexture = value;
				}
				else
				{
					sprite = value;
				}
			}
		}
		public Sprite BackgroundSprite
		{
			get => isBackgroundTexMain ? sprite : secondaryTexture;
			set
			{
				if (isBackgroundTexMain)
				{
					sprite = value;
				}
				else
				{
					secondaryTexture = value;
				}
			}
		}

		public Sprite NineSliceSourceSprite
		{
			get
			{
				switch (nineSliceSource)
				{
					case NineSliceSource.Background: return BackgroundSprite;
					case NineSliceSource.Sdf: return SDFSprite;
					case NineSliceSource.BackgroundFirst: return BackgroundSprite == null ? SDFSprite : BackgroundSprite;
					case NineSliceSource.SdfFirst: return SDFSprite == null ? BackgroundSprite : SDFSprite;
					default: return null;
				}
			}
		}

		public bool IsNineSliceFromBackgroundTexture
		{
			get
			{
				switch (nineSliceSource)
				{
					case NineSliceSource.Background: return true;
					case NineSliceSource.Sdf: return false;
					case NineSliceSource.BackgroundFirst: return BackgroundSprite != null;
					case NineSliceSource.SdfFirst: return SDFSprite == null;
					default: return false;
				}
			}
		}

		public override Material material
		{
			get
			{
				return SdfMaterialManager.GetMaterial(base.material,
					secondaryTexture == null ? null : secondaryTexture.texture,
					mode, shadowMode, backgroundMode, isBackgroundTexMain);
			}
			set => base.material = value;
		}

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();
            if (canvas != null) {
                canvas.additionalShaderChannels = (AdditionalCanvasShaderChannels) int.MaxValue;
            }
        }
#endif

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			if (SDFSprite == null)
			{
				mode = SdfMode.RectOnly;
			}
			if (imageType == ImageType.Sliced && NineSliceSourceSprite != null && NineSliceSourceSprite.border.sqrMagnitude > 0f)
			{
				GenerateSlicedMesh(vh);
			}
			else
			{
				GenerateSpriteMesh(vh);
			}
		}

		private void GenerateSpriteMesh(VertexHelper vh)
		{
			vh.Clear();
			var rt = rectTransform;
			var spriteRect = GetNormalizedSpriteRect(SDFSprite);
			var bgRect = GetNormalizedSpriteRect(BackgroundSprite);
			var position = new Rect(
				-rt.rect.size * rt.pivot,
				rt.rect.size);
			ImageMeshUtility.AddRect(this, vh, position, spriteRect, bgRect, new Rect(Vector2.zero, Vector2.one), rt.rect.size);
			ImageMeshUtility.AddFrame(this, vh, position, spriteRect, rt.rect.size, meshFrame);
		}

		private void GenerateSlicedMesh(VertexHelper vh)
		{
			vh.Clear();

			var rt = rectTransform;
			var size = rt.rect.size;

			var slicedSprite = NineSliceSourceSprite;

			float ppu = GetPixelsPerUnit(slicedSprite);

			ImageMeshUtility.Calculate9SliceValue(slicedSprite, size, rectTransform.pivot, ppu,
												  out var positions, out var uvs, out var coords);

			var bgRect = GetNormalizedSpriteRect(secondaryTexture);
			bool isBackgroundSliced = IsNineSliceFromBackgroundTexture;

			for (int xi = 0; xi < 3; xi++)
			{
				for (int yi = 0; yi < 3; yi++)
				{
					var posMin = new Vector2(positions[xi].x, positions[yi].y);
					var posSize = new Vector2(positions[xi + 1].x, positions[yi + 1].y) - posMin;
					var positionRect = new Rect(posMin, posSize);
					var uvMin = new Vector2(uvs[xi].x, uvs[yi].y);
					var uvSize = new Vector2(uvs[xi + 1].x, uvs[yi + 1].y) - uvMin;
					var uvRect = new Rect(uvMin, uvSize);
					var coordsRect = Rect.MinMaxRect(coords[xi].x, coords[yi].y,
													 coords[xi + 1].x, coords[yi + 1].y);
					var localBgRect = coordsRect.Map(bgRect);

					if (isBackgroundSliced)
					{
						(uvRect, localBgRect) = (localBgRect, uvRect);
					}

					ImageMeshUtility.AddRect(this, vh, positionRect, uvRect, localBgRect, coordsRect, size);
				}
			}

			ImageMeshUtility.AddFrame(this, vh,
									  new Rect(positions[0], positions[3] - positions[0]),
									  new Rect(uvs[0], uvs[3]),
									  size, meshFrame);
		}

		private Rect GetNormalizedSpriteRect(Sprite sprite)
		{
			if (sprite == null) return new Rect(Vector2.zero, Vector2.one);
			var spriteRect = sprite.rect;
			spriteRect.x /= sprite.texture.width;
			spriteRect.width /= sprite.texture.width;
			spriteRect.y /= sprite.texture.height;
			spriteRect.height /= sprite.texture.height;
			return spriteRect;
		}

		private void ApplyStyle()
		{
			if (_style == null) return;

			var size = rectTransform.rect.size;
			var minSize = Mathf.Min(size.x, size.y);

			isBackgroundTexMain = BussStyle.MainTextureSource.Get(Style).Enum ==
								  MainTextureBussProperty.Options.BackgroundSprite;
			imageType = BussStyle.ImageType.Get(Style).Enum;
#if UNITY_2019_1_OR_NEWER
			pixelsPerUnitMultiplier = BussStyle.PixelsPerUnitMultiplier.Get(Style).FloatValue;
#endif
			nineSliceSource = BussStyle.NineSliceSource.Get(Style).Enum;

			// color
			colorRect = BussStyle.BackgroundColor.Get(Style).ColorRect;
			BackgroundSprite = BussStyle.BackgroundImage.Get(Style).SpriteValue;
			backgroundMode = BussStyle.BackgroundMode.Get(Style).Enum;

			// outline
			outlineWidth = BussStyle.BorderWidth.Get(Style).FloatValue;
			outlineColor = BussStyle.BorderColor.Get(Style).ColorRect;

			// shape
			mode = BussStyle.SdfMode.Get(Style).Enum;
			rounding = BussStyle.RoundCorners.Get(Style).GetFloatValue(minSize);
			threshold = BussStyle.Threshold.Get(Style).FloatValue;
			SDFSprite = BussStyle.SdfImage.Get(Style).SpriteValue;

			switch (BussStyle.BorderMode.Get(Style).Enum)
			{
				case BorderMode.Outside:
					break;
				case BorderMode.Inside:
					threshold -= outlineWidth;
					break;
			}

			// shadow
			shadowColor = BussStyle.ShadowColor.Get(Style).ColorRect;
			shadowOffset = BussStyle.ShadowOffset.Get(Style).Vector2Value;
			shadowThreshold = BussStyle.ShadowThreshold.Get(Style).FloatValue;
			shadowSoftness = BussStyle.ShadowSoftness.Get(Style).FloatValue;
			shadowMode = BussStyle.ShadowMode.Get(Style).Enum;

			meshFrame = Mathf.Max(0f,
				threshold +
				Mathf.Abs(shadowThreshold)
				+ outlineWidth
				+ Mathf.Max(
					Mathf.Abs(shadowOffset.x),
					Mathf.Abs(shadowOffset.y)));
		}

		private float GetPixelsPerUnit(Sprite slicedSprite)
		{
#if UNITY_2019_1_OR_NEWER
			var ppu = slicedSprite.pixelsPerUnit * pixelsPerUnitMultiplier;
#else
			var ppu = slicedSprite.pixelsPerUnit;
#endif
			return ppu;
		}

		public enum ImageType
		{
			Simple,
			Sliced
		}

		public enum NineSliceSource
		{
			SdfFirst,
			BackgroundFirst,
			Sdf,
			Background
		}

		public enum SdfMode
		{
			Default,
			RectOnly
		}

		public enum BorderMode
		{
			Outside,
			Inside
		}
	}
}
