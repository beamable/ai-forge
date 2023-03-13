using System;
using UnityEngine;

namespace Beamable.Extensions
{
	// This is a helper for temporarily disabling a game object and ensuring
	// the previous active state is reinstated, in RAII fashion. Used in conjunction
	// with AddComponent below, it can be used to add components and configure
	// their data before Awake/Start is called. For example:
	//
	// var go = new GameObject();
	// go.AddComponent<FPSDisplay>(fps => fps.size = 10);
	public class GameObjectDeactivateSection : IDisposable
	{
		private GameObject _go;
		private bool _oldState;

		public GameObjectDeactivateSection(GameObject go)
		{
			_go = go;
			_oldState = go.activeSelf;
			_go.SetActive(false);
		}

		public void Dispose()
		{
			_go.SetActive(_oldState);
		}
	}

	public static class GameObjectExtensions
	{
		public static IDisposable Deactivate(this GameObject obj)
		{
			return new GameObjectDeactivateSection(obj);
		}

		public static T AddComponent<T>(this GameObject gameObject, Action<T> action) where T : Component
		{
			using (gameObject.Deactivate())
			{
				T component = gameObject.AddComponent<T>();
				action?.Invoke(component);
				return component;
			}
		}
	}
}
