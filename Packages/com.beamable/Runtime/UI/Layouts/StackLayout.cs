using UnityEngine;
using UnityEngine.UI;

namespace Beamable.UI.Layouts
{
	public enum StackLayoutDirection
	{
		HORIZONTAL, VERTICAL
	}

	public class StackLayout : LayoutGroup
	{
		public StackLayoutDirection Direction;

		public override void CalculateLayoutInputVertical()
		{
		}

		public override void SetLayoutHorizontal()
		{
			if (Direction != StackLayoutDirection.HORIZONTAL)
			{
				return;
			}

			var childCount = rectChildren.Count;
			var totalWidth = rectTransform.rect.size.x;

			var widthPerChild = totalWidth / (float)childCount;
			var widthRatio = widthPerChild / totalWidth;

			for (var i = 0; i < rectChildren.Count; i++)
			{
				var child = rectChildren[i];
				child.pivot = new Vector2(0, .5f);
				child.anchoredPosition = Vector2.zero;
				child.anchorMin = new Vector2(i * widthRatio, 0);
				child.anchorMax = new Vector2((i + 1) * widthRatio, 1);
				child.offsetMin = Vector2.zero;
				child.offsetMax = Vector2.zero;
			}

		}

		public override void SetLayoutVertical()
		{
			if (Direction != StackLayoutDirection.VERTICAL)
			{
				return;
			}

			var childCount = rectChildren.Count;
			var totalHeight = rectTransform.rect.size.y;

			var heightPerComponent = totalHeight / (float)childCount;
			var heightRatio = heightPerComponent / totalHeight;

			for (var i = 0; i < rectChildren.Count; i++)
			{
				var child = rectChildren[i];
				child.pivot = Vector2.zero;
				child.anchoredPosition = Vector2.zero;
				child.anchorMin = new Vector2(0, i * heightRatio);
				child.anchorMax = new Vector2(1, (i + 1) * heightRatio);
				child.offsetMin = Vector2.zero;
				child.offsetMax = Vector2.zero;
			}
		}
	}
}
