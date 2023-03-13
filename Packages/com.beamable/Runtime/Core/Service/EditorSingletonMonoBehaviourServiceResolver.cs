using Beamable.Extensions;
using UnityEngine;

namespace Beamable.Service
{
	public class EditorSingletonMonoBehaviourServiceResolver<TResolvedAs, TCreatedAs> : IServiceResolver<TResolvedAs>
		where TResolvedAs : class
		where TCreatedAs : MonoBehaviour, TResolvedAs
	{
		private static TCreatedAs instance;

		public bool CanResolve()
		{
			return !ApplicationLifetime.isQuitting;
		}

		public bool Exists()
		{
			return (instance != null) && !ApplicationLifetime.isQuitting;
		}

		public TResolvedAs Resolve()
		{
			if (instance != null)
				return instance;

			if (ApplicationLifetime.isQuitting)
			{
				Debug.LogError(string.Format("Application is quiting, but something is still calling ServiceManager.Resolve<{0}>, which is going to return null. Use ServiceManager.Exists before ServiceManager.Resolve on cleanup code", typeof(TResolvedAs).ToString()));
				return null;
			}

			instance = Object.FindObjectOfType<TCreatedAs>();

			if (instance != null)
				return instance;

			var name = typeof(TCreatedAs).Name;
			var gameObject = new GameObject(name);
			//ServiceManager.DontDestroyOnLoad(gameObject);
			ServiceManager.PlaceUnderServiceRoot(gameObject);

			instance = gameObject.AddComponent<TCreatedAs>(OnCreate);
			AfterCreate(instance);
			return instance;
		}

		public void OnTeardown()
		{
			if (ApplicationLifetime.isQuitting)
			{
				return;
			}

			if (instance != null)
			{
				Object.Destroy(instance.gameObject);
				instance = null;
			}
		}

		protected virtual void OnCreate(TCreatedAs createdAs)
		{
		}

		protected virtual void AfterCreate(TCreatedAs createdAs)
		{
		}
	}

	public class EditorSingletonMonoBehaviourServiceResolver<T> : EditorSingletonMonoBehaviourServiceResolver<T, T>
		where T : MonoBehaviour
	{
	}
}
