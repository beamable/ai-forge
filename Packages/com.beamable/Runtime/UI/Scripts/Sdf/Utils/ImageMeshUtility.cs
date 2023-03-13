using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace Beamable.UI.Sdf
{
	public static class ImageMeshUtility
	{
		private static readonly Vector2[] PositionValues = new Vector2[4];
		private static readonly Vector2[] UVValues = new Vector2[4];
		private static readonly Vector2[] CoordValues = new Vector2[4];

		public static void Calculate9SliceValue(Sprite sprite,
															   Vector2 size,
															   Vector2 pivot,
															   float pixelsPerUnit,
															   out Vector2[] positions,
															   out Vector2[] uvs,
															   out Vector2[] coords)
		{
			var startPosition = -size * pivot;
			var endPosition = startPosition + size;
			var borders = sprite.border / pixelsPerUnit;

			SetFourValues(PositionValues,
						  startPosition,
						  startPosition + new Vector2(borders.x, borders.y),
						  endPosition - new Vector2(borders.z, borders.w),
						  endPosition);

			var outer = DataUtility.GetOuterUV(sprite);
			var inner = DataUtility.GetInnerUV(sprite);

			SetFourValues(UVValues,
						  new Vector2(outer.x, outer.y),
						  new Vector2(inner.x, inner.y),
						  new Vector2(inner.z, inner.w),
						  new Vector2(outer.z, outer.w));

			SetFourValues(CoordValues,
						  Vector2.zero,
						  new Vector2(borders.x / size.x, borders.y / size.y),
						  Vector2.one - new Vector2(borders.z / size.x, borders.w / size.y),
						  Vector2.one);
			positions = PositionValues;
			uvs = UVValues;
			coords = CoordValues;
		}

		private static void SetFourValues<T>(T[] array, T v0, T v1, T v2, T v3)
		{
			array[0] = v0;
			array[1] = v1;
			array[2] = v2;
			array[3] = v3;
		}

		public static void AddFrame(SdfImage image, VertexHelper vh, Rect position, Rect uv, Vector2 size, float meshFrame)
		{
			if (meshFrame < .01f) return;
			var doubledFrame = meshFrame * 2f;
			// GrownPosition and GrownUV are outer rects of the frame.
			var grownPosition = new Rect(
				position.x - meshFrame, position.y - meshFrame,
				position.size.x + doubledFrame, position.size.y + doubledFrame);
			var grownUV = new Rect(
				uv.xMin - meshFrame / size.x,
				uv.yMin - meshFrame / size.y,
				uv.width * grownPosition.width / position.width,
				uv.height * grownPosition.height / position.height);
			var ratio = new Vector2(meshFrame, meshFrame) / size;
			var coords = new Rect(0f, 0f, 1f, 1f);
			var grownCoords = new Rect(-ratio, 2f * ratio + Vector2.one);

			// A, B, C and D are left, right, bottom and top parts of the frame.

			var posA = new Quad2D(grownPosition.GetBottomLeft(), position.GetBottomLeft(), grownPosition.GetTopLeft(), position.GetTopLeft());
			var posB = new Quad2D(position.GetBottomRight(), grownPosition.GetBottomRight(), position.GetTopRight(), grownPosition.GetTopRight());
			var posC = new Quad2D(grownPosition.GetBottomLeft(), grownPosition.GetBottomRight(), position.GetBottomLeft(), position.GetBottomRight());
			var posD = new Quad2D(position.GetTopLeft(), position.GetTopRight(), grownPosition.GetTopLeft(), grownPosition.GetTopRight());

			var uvA = new Quad2D(grownUV.GetBottomLeft(), uv.GetBottomLeft(), grownUV.GetTopLeft(), uv.GetTopLeft());
			var uvB = new Quad2D(uv.GetBottomRight(), grownUV.GetBottomRight(), uv.GetTopRight(), grownUV.GetTopRight());
			var uvC = new Quad2D(grownUV.GetBottomLeft(), grownUV.GetBottomRight(), uv.GetBottomLeft(), uv.GetBottomRight());
			var uvD = new Quad2D(uv.GetTopLeft(), uv.GetTopRight(), grownUV.GetTopLeft(), grownUV.GetTopRight());

			var coordsA = new Quad2D(grownCoords.GetBottomLeft(), coords.GetBottomLeft(), grownCoords.GetTopLeft(), coords.GetTopLeft());
			var coordsB = new Quad2D(coords.GetBottomRight(), grownCoords.GetBottomRight(), coords.GetTopRight(), grownCoords.GetTopRight());
			var coordsC = new Quad2D(grownCoords.GetBottomLeft(), grownCoords.GetBottomRight(), coords.GetBottomLeft(), coords.GetBottomRight());
			var coordsD = new Quad2D(coords.GetTopLeft(), coords.GetTopRight(), grownCoords.GetTopLeft(), grownCoords.GetTopRight());

			var bgRect = new Rect(0f, 0f, 0f, 0f);

			AddRect(image, vh, posA, uvA, bgRect, coordsA, size,
			 image.colorRect.LeftEdgeRect, image.outlineColor.LeftEdgeRect, image.shadowColor.LeftEdgeRect);
			AddRect(image, vh, posB, uvB, bgRect, coordsB, size,
			 image.colorRect.RightEdgeRect, image.outlineColor.RightEdgeRect, image.shadowColor.RightEdgeRect);
			AddRect(image, vh, posC, uvC, bgRect, coordsC, size,
			 image.colorRect.BottomEdgeRect, image.outlineColor.BottomEdgeRect, image.shadowColor.BottomEdgeRect);
			AddRect(image, vh, posD, uvD, bgRect, coordsD, size,
			 image.colorRect.TopEdgeRect, image.outlineColor.TopEdgeRect, image.shadowColor.TopEdgeRect);
		}

		public static void AddRect(SdfImage image, VertexHelper vh, Quad2D position, Quad2D spriteRect, Quad2D bgRect, Quad2D coordsRect, Vector2 size)
		{
			AddRect(image, vh, position, spriteRect, bgRect, coordsRect, size, image.colorRect, image.outlineColor, image.shadowColor);
		}

		public static void AddRect(SdfImage image, VertexHelper vh, Quad2D position, Quad2D spriteRect, Quad2D bgRect, Quad2D coordsRect,
								   Vector2 size, ColorRect colorRect, ColorRect outlineColor, ColorRect shadowColor)
		{
			vh.AddRect(
				position,
				spriteRect,
				bgRect,
				coordsRect,
				colorRect,
				size,
				image.threshold,
				image.rounding,
				image.outlineWidth, outlineColor,
				shadowColor, image.shadowThreshold, image.shadowOffset, image.shadowSoftness
			);
		}
	}
}
