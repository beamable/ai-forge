using System;

namespace Beamable.UI.Tweening
{
	public abstract class GenericTween<T> : BaseTween
	{
		private T _startValue;
		private T _endValue;
		private readonly Action<T> _updateAction;
		public event Action CompleteEvent;

		public GenericTween(Action<T> updateAction) : this(0f, default, default, updateAction) { }

		public GenericTween(float duration, T startValue, T endValue, Action<T> updateAction) : base(duration)
		{
			StartValue = startValue;
			EndValue = endValue;
			_updateAction = updateAction;
		}

		public T StartValue
		{
			get => _startValue;
			set
			{
				if (IsRunning)
				{
					throw new Exception(
						"Can not change the StartValue of running tween. Stop the tween before changing the value.");
				}

				_startValue = value;
			}
		}

		public T EndValue
		{
			get => _endValue;
			set
			{
				if (IsRunning)
				{
					throw new Exception(
						"Can not change the EndValue of running tween. Stop the tween before changing the value.");
				}

				_endValue = value;
			}
		}

		protected override bool Update(float t)
		{
			_updateAction?.Invoke(Lerp(StartValue, EndValue, t));
			return true;
		}

		protected abstract T Lerp(T from, T to, float t);

		protected override void OnComplete()
		{
			CompleteEvent?.Invoke();
		}
	}
}
