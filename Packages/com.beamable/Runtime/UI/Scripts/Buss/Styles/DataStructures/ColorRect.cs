using System;
using UnityEngine;

namespace Beamable.UI.Sdf
{
	[Serializable]
	public struct ColorRect
	{
		public Color BottomLeftColor;
		public Color BottomRightColor;
		public Color TopLeftColor;
		public Color TopRightColor;

#if UNITY_EDITOR
#pragma warning disable CS0414
        [SerializeField] private int _drawerMode;
#pragma warning restore CS0414
#endif

		public ColorRect(Color bottomLeftColor, Color bottomRightColor, Color topLeftColor, Color topRightColor)
		{
			BottomLeftColor = bottomLeftColor;
			BottomRightColor = bottomRightColor;
			TopLeftColor = topLeftColor;
			TopRightColor = topRightColor;
#if UNITY_EDITOR
            _drawerMode = 0;
#endif
		}

		public ColorRect(Color color = default) : this(color, color, color, color)
		{
#if UNITY_EDITOR
            _drawerMode = 1;
#endif
		}

		public ColorRect TopEdgeRect => new ColorRect(TopLeftColor, TopRightColor, TopLeftColor, TopRightColor);
		public ColorRect BottomEdgeRect => new ColorRect(BottomLeftColor, BottomRightColor, BottomLeftColor, BottomRightColor);
		public ColorRect LeftEdgeRect => new ColorRect(BottomLeftColor, BottomLeftColor, TopLeftColor, TopLeftColor);
		public ColorRect RightEdgeRect => new ColorRect(BottomRightColor, BottomRightColor, TopRightColor, TopRightColor);

		public static ColorRect Lerp(ColorRect a, ColorRect b, float value)
		{
			return new ColorRect(
				Color.Lerp(a.BottomLeftColor, b.BottomLeftColor, value),
				Color.Lerp(a.BottomRightColor, b.BottomRightColor, value),
				Color.Lerp(a.TopLeftColor, b.TopLeftColor, value),
				Color.Lerp(a.TopRightColor, b.TopRightColor, value)
			);
		}

#if UNITY_EDITOR
		public static class EditorHelper
		{
			public static ColorRect WithDrawerMode(ColorRect rect, int value)
			{
				rect._drawerMode = value;
				return rect;
			}

			public static int GetDrawerMode(ColorRect rect)
			{
				return rect._drawerMode;
			}
		}
#endif
	}
}
