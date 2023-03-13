using UnityEngine;

namespace Beamable.Service
{
	// This ServiceResolver is for services that need to be a MonoBehaviour but want the ServiceManager to own lifetime.
	// This means the service resolver will create a GameObject to attach the MonoBehaviour based service to on the fly,
	// and destroy said GameObject when the service is torn down.  To use this, define the service as a MonoBehaviour
	// and then create a resolver thus:
	//
	// class MyService : MonoBehaviour { }
	// ...
	// ServiceManager.Provide(new LazyMonoBehaviourServiceResolver<MyService>());
	//
	public class LazyMonoBehaviourServiceResolver<T> : IServiceResolver<T>
		where T : MonoBehaviour
	{
		private T service;

		public bool CanResolve()
		{
			return !ApplicationLifetime.isQuitting;
		}

		public bool Exists()
		{
			return !ApplicationLifetime.isQuitting && (service != null);
		}

		public T Resolve()
		{
			if (ApplicationLifetime.isQuitting)
			{
				Debug.LogError(string.Format(
					"Application is quiting, but something is still resolving service {0}, which is going to return null. Use check existence with ServiceManager before resolving on cleanup code",
					typeof(T).Name));
			}
			else if (service == null)
			{
				ServicesLogger.LogFormat("Creating GameObject for service {0}.", typeof(T).Name);
				var gameObject = new GameObject(typeof(T).Name);
				ServiceManager.PlaceUnderServiceRoot(gameObject);
				service = gameObject.AddComponent<T>();
			}

			return service;
		}

		public void OnTeardown()
		{
			if (service != null)
			{
				ServicesLogger.LogFormat("Destroying GameObject for service {0}.", typeof(T).Name);
				GameObject.Destroy(service.gameObject);
				service = null;
			}
			ServiceManager.Remove(this);
		}
	}
}
