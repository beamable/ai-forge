using System;
using UnityEngine;

namespace Beamable.Service
{
	// This service resolver is for creating services that are themselves a MonoBehavior which is placed into a scene
	// at design time.  The desire is that when that scene is loaded this resolver register itself, and when the
	// GameObject it is tied to is destroyed for any reason, the resolver unregister itself.  To use this resolver,
	// first define a concrete resolver class:
	//
	//   public class MyService : SceneLoadedServiceResolver<MyService>
	//
	public class SceneLoadedServiceResolver<TResolvedAs> : MonoBehaviour, IServiceResolver<TResolvedAs>
	   where TResolvedAs : class
	{
		private TResolvedAs service;

		protected virtual void Awake()
		{
			service = this as TResolvedAs;
			ServiceManager.Provide(this);
		}

		protected virtual void OnDestroy()
		{
			ServiceManager.Remove(this);
		}

		public void OnTeardown()
		{
			// Our lifetime is tied to the underlying GameObject, not dictated by ServiceManager.  If the underlying
			// GameObject is marked as DontDestroyOnLoad, it's possibly this service will live across multiple lifecycles
			// of the ServiceManager (eg, see UICanvasCamera).
			if (service is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		public bool CanResolve()
		{
			return service != null;
		}

		public bool Exists()
		{
			return service != null && !ApplicationLifetime.isQuitting;
		}

		public TResolvedAs Resolve()
		{
			if (ApplicationLifetime.isQuitting)
			{
				Debug.LogError(string.Format(
				   "Application is quiting, but something is still resolving service {0}, which is going to return null. Use check existence with ServiceManager before resolving on cleanup code",
				   typeof(TResolvedAs).Name
				));
			}
			return service;
		}
	}
}
