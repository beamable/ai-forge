using System;
using UnityEngine;

namespace Beamable.UI.Tweening
{
	public class FloatTween : GenericTween<float>
	{
		public FloatTween(Action<float> updateAction) : base(updateAction) { }

		public FloatTween(float duration, float startValue, float endValue, Action<float> updateAction) : base(
			duration, startValue, endValue, updateAction)
		{ }

		protected override float Lerp(float @from, float to, float t)
		{
			return Mathf.Lerp(from, to, t);
		}
	}
}
