using System.Collections.Generic;
using UnityEngine;

namespace Beamable.UI.Tweening
{
	public class TweenManager : MonoBehaviour
	{
		public static bool Initialized { get; private set; }
		private static TweenManager _instance;

		private List<BaseTween> _tweens = new List<BaseTween>();
		private List<BaseTween> _toStop = new List<BaseTween>();

		public static TweenManager Instance
		{
			get
			{
				if (!Initialized)
				{
					var go = new GameObject("BeamableTweenManager");
					DontDestroyOnLoad(go);
					_instance = go.AddComponent<TweenManager>();
					Initialized = true;
				}

				return _instance;
			}
		}

		public void RegisterTween(BaseTween tween)
		{
			if (!_tweens.Contains(tween))
			{
				_tweens.Add(tween);
			}
		}

		public void UnregisterTween(BaseTween tween)
		{
			_tweens.Remove(tween);
		}

		private void Update()
		{
			foreach (var tween in _tweens)
			{
				if (!tween.Tick())
				{
					_toStop.Add(tween);
				}
			}

			foreach (var tween in _toStop)
			{
				tween.Stop();
			}
			_toStop.Clear();
		}
	}
}
