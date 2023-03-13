using System;
using UnityEngine;

namespace Beamable.UI.Sdf
{
	[Serializable]
	public struct Quad2D
	{
		public Vector2 bottomLeft, bottomRight, topLeft, topRight;

		public Quad2D(Vector2 bl, Vector2 br, Vector2 tl, Vector2 tr)
		{
			bottomLeft = bl;
			bottomRight = br;
			topLeft = tl;
			topRight = tr;
		}

		public static Quad2D MinMaxQuad(float xMin, float yMin, float xMax, float yMax)
		{
			return new Quad2D(new Vector2(xMin, yMin), new Vector2(xMax, yMin), new Vector2(xMin, yMax), new Vector2(xMax, yMax));
		}

		public static implicit operator Quad2D(Rect rect) => MinMaxQuad(rect.xMin, rect.yMin, rect.xMax, rect.yMax);
	}
}
