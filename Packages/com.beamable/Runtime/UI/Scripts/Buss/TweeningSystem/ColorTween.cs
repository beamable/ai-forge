using System;
using UnityEngine;

namespace Beamable.UI.Tweening
{
	public class ColorTween : GenericTween<Color>
	{
		public ColorTween(float duration, Color startValue, Color endValue, Action<Color> updateAction) : base(duration, startValue, endValue, updateAction) { }
		protected override Color Lerp(Color @from, Color to, float t)
		{
			return Color.LerpUnclamped(from, to, t);
		}
	}
}
