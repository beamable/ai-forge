using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Theme.Objects
{
	[System.Serializable]
	public class LayoutStyleObject : StyleObject<LayoutGroup>
	{
		public int X = 0;
		public RectOffset Padding;

		protected override void Apply(LayoutGroup target)
		{
			target.padding.bottom = Padding.bottom;
			target.padding.left = Padding.left;
			target.padding.right = Padding.right;
			target.padding.top = Padding.top;
		}
	}
}
