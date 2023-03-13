using Beamable.Common;
using Beamable.Coroutines;
using System;
using System.Collections;
using UnityEngine;

namespace Beamable.UI.Scripts
{
	public class BeamableAnimationException : Exception
	{
		public BeamableAnimationException(string message) : base(message) { }

		public static readonly BeamableAnimationException CutShort = new BeamableAnimationException("Monobehaviour was destroyed");
	}

	public static class BeamableAnimationUtil
	{
		const float C4 = (float)((2 * Math.PI) / 3);

		public static Promise<Unit> RunAnimation(this MonoBehaviour root, Action<float> handler, float duration = .2f, int steps = 10, Func<float, float> easing = null)
		{
			var promise = new Promise<Unit>();
			promise.Error(ex =>
			{
				if (!(ex is BeamableAnimationException))
				{
					throw ex;
				}
			});

			if (!root || root == null)
			{
				promise.CompleteError(BeamableAnimationException.CutShort);
				return promise; //short circuit.
			}

			Coroutine routine = null; // XXX: allow the closure to capture the routine reference.
			routine = root.StartCoroutine(Animate((animationCompletionPercentage, i) =>
			{
				handler(i);
				if (animationCompletionPercentage >= 1)
				{
					promise.CompleteSuccess(PromiseBase.Unit);
				}
				else if (promise.IsCompleted)
				{
					root.StopCoroutine(routine);
				}
			}, duration, steps, easing));

			return promise;
		}

		public static IEnumerator Animate(Action<float, float> handler, float duration = .2f, int steps = 10, Func<float, float> easing = null)
		{
			var startTime = Time.realtimeSinceStartup;
			var endTime = startTime + duration;
			var timeStep = duration / steps;
			easing = easing ?? LinearEasing;

			handler(0, easing(0));
			while (Time.realtimeSinceStartup < endTime)
			{
				var ratio = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / duration);
				var easedRatio = easing(ratio);

				handler(ratio, easedRatio);
				yield return Yielders.Seconds(timeStep);
			}

			handler(1, easing(1));
		}

		public static float LinearEasing(float i)
		{
			return i;
		}

		public static float EaseInCubic(float i)
		{
			return i * i * i;
		}


		public static Func<float, float> GenerateElasticFunction(float amp = 10f, float p = 10, float offset = -.75f)
		{
			// https://easings.net/#easeOutElastic
			var func = new Func<float, float>(i =>
			{
				return i == 0
				? 0
				: i == 1
				   ? 1
				   : Mathf.Pow(2, -p * i) * Mathf.Sin((i * amp - offset) * C4) + 1f;
			});

			return func;
		}
	}
}
