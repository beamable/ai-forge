using UnityEngine;
using UnityEngine.UI;
#if !UNITY_2019_1_OR_NEWER
using UnityEngine.Experimental.UI;
#endif

namespace Beamable.UI.Sdf
{
	public static class SDFUtils
	{
		/// <summary>
		/// Packs SDFImage parameters in vertex data and adds quad to the vertex helper;
		/// </summary>
		public static void AddRect(this VertexHelper vh,
								   Quad2D position,
								   Quad2D uvs,
								   Quad2D backgroundUvs,
								   Quad2D coords,
								   ColorRect vertexColor,
								   Vector2 size,
								   float threshold,
								   float rounding,
								   float outlineWidth,
								   ColorRect outlineColor,
								   ColorRect shadowColor,
								   float shadowThreshold,
								   Vector2 shadowOffset,
								   float shadowSoftness)
		{
			var normal = new Vector3(
				threshold,
				size.x,
				size.y);

			shadowThreshold = (shadowThreshold / 100f) + .5f;

			var startVertexIndex = vh.currentVertCount;
			vh.AddVert(
				new Vector3(position.bottomLeft.x, position.bottomLeft.y, rounding),
				ClipColorAlpha(vertexColor.BottomLeftColor),
				uvs.bottomLeft,
				backgroundUvs.bottomLeft,
				new Vector2(outlineWidth,
							PackRGBToFloat(outlineColor.BottomLeftColor)),
				coords.bottomLeft,
				normal,
				new Vector4(shadowSoftness, PackVector2ToFloat(shadowOffset.x, shadowOffset.y),
							PackVector3ToFloat(outlineColor.BottomLeftColor.a, shadowColor.BottomLeftColor.a,
											   shadowThreshold),
							PackRGBToFloat(shadowColor.BottomLeftColor)));
			vh.AddVert(
				new Vector3(position.bottomRight.x, position.bottomRight.y, rounding),
				ClipColorAlpha(vertexColor.BottomRightColor),
				uvs.bottomRight,
				backgroundUvs.bottomRight,
				new Vector2(outlineWidth,
							PackRGBToFloat(outlineColor.BottomRightColor)),
				coords.bottomRight,
				normal,
				new Vector4(shadowSoftness, PackVector2ToFloat(shadowOffset.x, shadowOffset.y),
							PackVector3ToFloat(outlineColor.BottomRightColor.a, shadowColor.BottomRightColor.a,
											   shadowThreshold),
							PackRGBToFloat(shadowColor.BottomRightColor)));
			vh.AddVert(
				new Vector3(position.topRight.x, position.topRight.y, rounding),
				ClipColorAlpha(vertexColor.TopRightColor),
				uvs.topRight,
				backgroundUvs.topRight,
				new Vector2(outlineWidth,
							PackRGBToFloat(outlineColor.TopRightColor)),
				coords.topRight,
				normal,
				new Vector4(shadowSoftness, PackVector2ToFloat(shadowOffset.x, shadowOffset.y),
							PackVector3ToFloat(outlineColor.TopRightColor.a, shadowColor.TopRightColor.a,
											   shadowThreshold),
							PackRGBToFloat(shadowColor.TopRightColor)));
			vh.AddVert(
				new Vector3(position.topLeft.x, position.topLeft.y, rounding),
				ClipColorAlpha(vertexColor.TopLeftColor),
				uvs.topLeft,
				backgroundUvs.topLeft,
				new Vector2(outlineWidth,
							PackRGBToFloat(outlineColor.TopLeftColor)),
				coords.topLeft,
				normal,
				new Vector4(shadowSoftness, PackVector2ToFloat(shadowOffset.x, shadowOffset.y),
							PackVector3ToFloat(outlineColor.TopLeftColor.a, shadowColor.TopLeftColor.a,
											   shadowThreshold),
							PackRGBToFloat(shadowColor.TopLeftColor)));

			vh.AddTriangle(startVertexIndex, startVertexIndex + 3, startVertexIndex + 2);
			vh.AddTriangle(startVertexIndex, startVertexIndex + 2, startVertexIndex + 1);
		}

		/// <summary>
		/// Adds quad to the vertex helper.
		/// </summary>
		private static void AddRect(this VertexHelper vh,
									Quad2D position,
									float z,
									Quad2D uvs,
									Quad2D coords,
									ColorRect vertexColor,
									Quad2D uvs1,
									Vector2 uv2,
									Vector3 normal,
									Vector4 tangent)
		{
			var startVertexIndex = vh.currentVertCount;
			vh.AddVert(
				new Vector3(position.bottomLeft.x, position.bottomLeft.y, z),
				ClipColorAlpha(vertexColor.BottomLeftColor),
				uvs.bottomLeft,
				uvs1.bottomLeft,
				uv2,
				coords.bottomLeft,
				normal,
				tangent);
			vh.AddVert(
				new Vector3(position.bottomRight.x, position.bottomRight.y, z),
				ClipColorAlpha(vertexColor.BottomRightColor),
				uvs.bottomRight,
				uvs1.bottomRight,
				uv2,
				coords.bottomRight,
				normal,
				tangent);
			vh.AddVert(
				new Vector3(position.topRight.x, position.topRight.y, z),
				ClipColorAlpha(vertexColor.TopRightColor),
				uvs.topRight,
				uvs1.topRight,
				uv2,
				coords.topRight,
				normal,
				tangent);
			vh.AddVert(
				new Vector3(position.topLeft.x, position.topLeft.y, z),
				ClipColorAlpha(vertexColor.TopLeftColor),
				uvs.topLeft,
				uvs1.topLeft,
				uv2,
				coords.topLeft,
				normal,
				tangent);
			vh.AddTriangle(startVertexIndex, startVertexIndex + 3, startVertexIndex + 2);
			vh.AddTriangle(startVertexIndex, startVertexIndex + 2, startVertexIndex + 1);
		}

		private static float PackVector3ToFloat(float x, float y, float z)
		{
			return PackVector3ToFloat(new Vector3(x, y, z));
		}

		private static float PackVector3ToFloat(this Vector3 vector)
		{
			return Vector3.Dot(Vector3Int.RoundToInt(vector * 255), new Vector3(65536, 256, 1));
		}

		private static float PackRGBToFloat(this Color color) => PackVector3ToFloat(color.r, color.g, color.b);

		private static float PackVector2ToFloat(float x, float y)
		{
			var max = Mathf.Max(Mathf.Abs(x) * 2f, Mathf.Abs(y) * 2f, 1);
			return PackVector3ToFloat(x / max + .5f, y / max + .5f, max / 255);
		}

		private static Color32 ClipColorAlpha(Color32 color32)
		{
			if (color32.a == 0)
			{ // hack to avoid object disappear when alpha is equal to zero
				color32.a = 1;
			}

			return color32;
		}
	}
}
