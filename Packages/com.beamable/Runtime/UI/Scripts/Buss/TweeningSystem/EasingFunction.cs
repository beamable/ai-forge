using System;
using UnityEngine;

namespace Beamable.UI.Tweening
{
	public static class EasingFunction
	{
		public delegate float EasingFunctionDelegate(float t);

		public static float Linear(float t) => t;
		// Quad
		public static float InQuad(float t) => t * t;
		public static float OutQuad(float t) => 1 - (1 - t) * (1 - t);
		public static float InOutQuad(float t) => Mathf.Lerp(InQuad(t), OutQuad(t), t);
		// TODO: add more easing functions

		public static EasingFunctionDelegate GetFunction(Easing easing)
		{
			switch (easing)
			{
				case Easing.Linear:
					return Linear;
				case Easing.InQuad:
					return InQuad;
				case Easing.OutQuad:
					return OutQuad;
				case Easing.InOutQuad:
					return InOutQuad;
				default:
					throw new ArgumentOutOfRangeException(nameof(easing), easing, null);
			}
		}

		public static float Ease(Easing easing, float t) => GetFunction(easing)(Mathf.Clamp01(t));
		public static float EaseUnclamped(Easing easing, float t) => GetFunction(easing)(t);
	}

	public enum Easing
	{
		Linear,
		InQuad,
		OutQuad,
		InOutQuad
	}
}
