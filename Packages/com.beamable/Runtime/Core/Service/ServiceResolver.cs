using System;

namespace Beamable.Service
{
	/// <summary>
	/// This type defines functionality implemented by a collections of resolvers.
	///
	/// It contains only those methods that can
	/// be used generically across resolvers without knowing the type they resolve.  Pragmatically any implementor should
	/// always inherit the generic typed interface below.
	/// 
	/// </summary>
	public interface IServiceResolver
	{
		void OnTeardown();
	}

	public interface IServiceResolver<T> : IServiceResolver
	   where T : class
	{
		// Returns true if Resolve will return a valid object, even if lazily constructed.
		bool CanResolve();

		// Returns true only if Resolve will return an existing object without doing any creation work.
		bool Exists();

		T Resolve();
	}


	public class ServiceContainer<T> : IServiceResolver<T>
	   where T : class
	{
		protected T Instance;

		public ServiceContainer(T instance)
		{
			Instance = instance;
		}

		public bool CanResolve()
		{
			return Instance != null;
		}

		public bool Exists()
		{
			return Instance != null;
		}

		public T Resolve()
		{
			return Instance;
		}

		public virtual void OnTeardown()
		{
			var disposable = Instance as IDisposable;
			disposable?.Dispose();
			Instance = null;
			ServiceManager.Remove(this);
		}
	}
}
