using UnityEngine;

namespace Beamable.Service
{
	// This service resolver is for creating services that are themselves a MonoBehavior which are created on the
	// fly by attaching to a pre-existing GameObject.  This can mean that the resolver hangs around after the app
	// quits (eg. in editor mode stopping playback without calling ServiceManager.Teardown), but it will get clobbered
	// the next time the app starts up and will correctly return false for CanResolve() and Exists().
	//
	// Register services with this container via:
	//
	//   ServiceManager.Provide(MonoBehaviourServiceContainer<MyServiceType>().CreateComponent(myGameObject);
	//
	public class MonoBehaviourServiceContainer<T> : IServiceResolver<T>
	   where T : MonoBehaviour
	{
		private T service;

		public static MonoBehaviourServiceContainer<T> CreateComponent(GameObject gameObject)
		{
			var service = gameObject.AddComponent<T>();
			return new MonoBehaviourServiceContainer<T>(service);
		}

		private MonoBehaviourServiceContainer(T service)
		{
			this.service = service;
		}

		public void OnTeardown()
		{
			if (service != null)
			{
				GameObject.Destroy(service);
				service = null;
			}
			ServiceManager.Remove(this);
		}

		public bool CanResolve()
		{
			return service != null;
		}

		public bool Exists()
		{
			return service != null && !ApplicationLifetime.isQuitting;
		}

		public T Resolve()
		{
			if (ApplicationLifetime.isQuitting)
			{
				Debug.LogError($"Application is quiting, but something is still resolving service {typeof(T).Name}, which is going to return null. Use check existence with ServiceManager before resolving on cleanup code");
			}
			return service;
		}
	}
}
