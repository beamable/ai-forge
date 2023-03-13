using UnityEditor;
using UnityEngine;

namespace Beamable.Editor
{
	public class EditorGUIRectController
	{
		public Rect rect;

		public EditorGUIRectController(Rect rect)
		{
			this.rect = rect;
		}

		public Rect ReserveWidth(float width)
		{
			var result = new Rect(rect.x, rect.y, width, rect.height);
			rect.x += width;
			rect.width -= width;
			return result;
		}

		public Rect ReserveHeight(float height)
		{
			var result = new Rect(rect.x, rect.y, rect.width, height);
			rect.y += height;
			rect.height -= height;
			return result;
		}

		public Rect ReserveWidthByFraction(float fraction)
		{
			return ReserveWidth(fraction * rect.width);
		}

		public Rect ReserveHeightByFraction(float fraction)
		{
			return ReserveHeight(fraction * rect.height);
		}

		public Rect ReserveWidthFromRight(float width)
		{
			var result = new Rect(rect.xMax - width, rect.y, width, rect.height);
			rect.width -= width;
			return result;
		}

		public Rect ReserveHeightFromBottom(float height)
		{
			var result = new Rect(rect.x, rect.yMax - height, rect.width, height);
			rect.height -= height;
			return result;
		}

		public Rect ReserveLabelRect()
		{
			return ReserveWidth(EditorGUIUtility.labelWidth);
		}

		public Rect ReserveSingleLine()
		{
			return ReserveHeight(EditorGUIUtility.singleLineHeight);
		}

		public void MoveIndent(int amount)
		{
			var indent = 15 * amount;
			rect.x += indent;
			rect.width -= indent;
		}
	}
}
