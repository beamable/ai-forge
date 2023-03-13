using UnityEngine;

namespace Beamable.UI.Sdf
{
	public static class RectUtility
	{
		public static Rect Extrude(this Rect rect, float value)
		{
			var full = new Vector2(value, value);
			var half = full * .5f;
			return new Rect(rect.position - half, rect.size + full);
		}

		public static Rect Scale(this Rect rect, float scale)
		{
			var delta = scale - 1f;
			var full = new Vector2(rect.size.x * delta, rect.size.y * delta);
			var half = full * .5f;
			return new Rect(rect.position - half, rect.size + full);
		}

		public static Rect Extrude(this Rect rect, Vector2 value)
		{
			var half = value * .5f;
			return new Rect(rect.position - half, rect.size + value);
		}

		public static Rect Scale(this Rect rect, Vector2 scale)
		{
			var delta = scale - Vector2.one;
			var full = new Vector2(rect.size.x * delta.x, rect.size.y * delta.y);
			var half = full * .5f;
			return new Rect(rect.position - half, rect.size + full);
		}

		public static Rect Map(this Rect rect, Rect map)
		{
			return Rect.MinMaxRect(
				Mathf.Lerp(map.xMin, map.xMax, rect.xMin),
				Mathf.Lerp(map.yMin, map.yMax, rect.yMin),
				Mathf.Lerp(map.xMin, map.xMax, rect.xMax),
				Mathf.Lerp(map.yMin, map.yMax, rect.yMax));
		}

		public static Vector2 GetBottomLeft(this Rect rect) => new Vector2(rect.xMin, rect.yMin);
		public static Vector2 GetBottomRight(this Rect rect) => new Vector2(rect.xMax, rect.yMin);
		public static Vector2 GetTopLeft(this Rect rect) => new Vector2(rect.xMin, rect.yMax);
		public static Vector2 GetTopRight(this Rect rect) => new Vector2(rect.xMax, rect.yMax);
	}
}
