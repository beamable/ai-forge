using Beamable.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Coroutines
{
	/// <summary>
	/// The <see cref="CoroutineService"/> allows any system to start a Unity coroutine, even if that system is not
	/// a MonoBehaviour or associated with a GameObject.
	/// Beamable runs many coroutines per player instance to manage multiple networking based resources. All coroutines are routed
	/// through the CoroutineService.
	/// The CoroutineService is a MonoBehaviour itself, and all coroutines are associated to the related GameObject.
	/// Use the <see cref="StartNew"/> method to start a coroutine.
	/// </summary>
	[EditorServiceResolver(typeof(EditorSingletonMonoBehaviourServiceResolver<CoroutineService>))]
	public class CoroutineService : MonoBehaviour
	{
		private Dictionary<string, List<IEnumerator>> coroutines = new Dictionary<string, List<IEnumerator>>();
		private event Action _everySecond;

		/// <summary>
		/// Start a new Coroutine.
		/// The Coroutine will be attached to the GameObject that the <see cref="CoroutineService"/> is connected to.
		/// </summary>
		/// <param name="context">A context is a semantic grouping of Coroutines. Many Coroutines
		/// can be running at the same time, so the context helps organize them.
		/// You can use the <see cref="StopAll"/> method to stop all Coroutines of a given context.
		/// </param>
		/// <param name="enumerator">
		/// The Coroutine function
		/// </param>
		/// <returns>A Unity Coroutine</returns>
		public virtual Coroutine StartNew(string context, IEnumerator enumerator)
		{
			List<IEnumerator> contextCoroutines = null;
			if (!coroutines.TryGetValue(context, out contextCoroutines))
			{
				contextCoroutines = new List<IEnumerator>();
				coroutines[context] = contextCoroutines;
			}

			contextCoroutines.Add(enumerator);
			return StartCoroutine(RunCoroutine(contextCoroutines, enumerator));
		}

		/// <summary>
		/// Stop all Coroutines for a given context.
		/// You can start Coroutines with the <see cref="StartNew"/> method.
		/// </summary>
		/// <param name="context">A context is a semantic grouping of Coroutines. Many Coroutines
		/// can be running at the same time, so the context helps organize them.</param>
		public void StopAll(string context)
		{
			List<IEnumerator> contextCoroutines = null;
			if (coroutines.TryGetValue(context, out contextCoroutines))
			{
				coroutines.Remove(context);
				for (int i = 0; i < contextCoroutines.Count; i++)
				{
					StopCoroutine(contextCoroutines[i]);
				}
			}
		}

		/// <summary>
		/// A utility function that triggers every game second.
		/// The callback in this function will happen in a Coroutine context.
		/// </summary>
		public event Action EverySecond
		{
			add
			{
				if (_everySecond == null)
				{
					StartNew("everySecond", FireEverySecond());
				}
				_everySecond += value;
			}

			remove
			{
				_everySecond -= value;
				if (_everySecond == null)
				{
					StopAll("everySecond");
				}
			}
		}

		private IEnumerator RunCoroutine(List<IEnumerator> coroutines, IEnumerator enumerator)
		{
			yield return enumerator;
			coroutines.Remove(enumerator);
		}

		private IEnumerator FireEverySecond()
		{
			while (true)
			{
				yield return Yielders.Seconds(1.0f);
				_everySecond?.Invoke();
			}
		}
	}
}
