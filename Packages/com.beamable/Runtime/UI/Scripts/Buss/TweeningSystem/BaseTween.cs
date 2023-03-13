using System;
using UnityEngine;

namespace Beamable.UI.Tweening
{
	public abstract class BaseTween
	{
		public float Duration { get; private set; }
		public Easing Easing { get; private set; } = Easing.Linear;
		public bool IsRunning { get; private set; } = false;
		public float StartTime { get; private set; } = -1f;

		private float Time => UnityEngine.Time.time;

		public BaseTween(float duration)
		{
			Duration = duration;
		}

		public void Run()
		{
			if (!Application.isPlaying) return;
			if (!IsRunning)
			{
				TweenManager.Instance.RegisterTween(this);
			}

			StartTime = Time;
			IsRunning = true;
		}

		public void Stop()
		{
			if (IsRunning)
			{
				TweenManager.Instance.UnregisterTween(this);
				try
				{
					OnStopped();
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}

			IsRunning = false;
		}

		public bool Tick()
		{
			var normalizedTime = (Time - StartTime) / Duration;
			var isValid = false;

			try
			{
				isValid |= Update(EasingFunction.Ease(Easing, normalizedTime));
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			if (!isValid)
			{
				return false;
			}
			else if (normalizedTime >= 1f)
			{
				try
				{
					OnComplete();
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}

				return false;
			}

			return true;
		}

		protected abstract bool Update(float t);

		protected virtual void OnComplete() { }
		protected virtual void OnStopped() { }

		public BaseTween SetDuration(float duration)
		{
			if (IsRunning)
			{
				throw new Exception(
					"Can not change the Duration of running tween. Stop the tween before changing the value.");
			}

			Duration = duration;
			return this;
		}

		public BaseTween SetEasing(Easing easing)
		{
			if (IsRunning)
			{
				throw new Exception(
					"Can not change the Easing of running tween. Stop the tween before changing the value.");
			}

			Easing = easing;
			return this;
		}
	}
}
