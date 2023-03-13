using Beamable.Coroutines;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Beamable.UI.Layouts
{
	[System.Serializable]
	public class MediaQueryEvent : UnityEvent<MediaSourceBehaviour, bool> { }

	public class MediaQueryBehaviour : MonoBehaviour
	{
		public MediaSourceBehaviour Source;
		public MediaQueryEvent OnChanged;
		public UnityEvent OnAspectChanged;

		private MediaSourceBehaviour _lastSource;
		private int _approxAspect;

		private void Awake()
		{
			_lastSource = Source;
		}

		private void OnEnable()
		{
			if (Source != null)
			{
				Source.Subscribe(OnMediaQueryChange);
				OnMediaQueryChange(Source, Source.Calculate());
			}

		}

		private void OnDisable()
		{
			Source?.Unsubscribe(OnMediaQueryChange);
		}

		private void Update()
		{

			var nextApproxAspect = (int)(100 * (Screen.width / (float)Screen.height));

			if (nextApproxAspect != _approxAspect)
			{
				_approxAspect = nextApproxAspect;
				StartCoroutine(RunAspectUpdateLater());
			}

			if (Source != _lastSource)
			{
				_lastSource?.Unsubscribe(OnMediaQueryChange);
				_lastSource = Source;
				Source?.Subscribe(OnMediaQueryChange);
			}


		}

		protected virtual void OnMediaQueryChange(MediaSourceBehaviour source, bool output)
		{
			OnChanged?.Invoke(source, output);
			StartCoroutine(RunAspectUpdateLater());
		}

		protected IEnumerator RunAspectUpdateLater()
		{
			yield return Yielders.EndOfFrame;
			OnAspectChanged?.Invoke();

		}
	}
}
