// unset

using System;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Common.Dependencies
{
	/// <summary>
	/// The <see cref="IDependencyProvider"/> is a collection of services, built from a <see cref="IDependencyBuilder"/>.
	/// Use the <see cref="GetService{T}"/> method to get services.
	/// </summary>
	public interface IDependencyProvider : IServiceProvider
	{
		/// <summary>
		/// Returns true if the given service is registered in the provider.
		/// If you make a call to <see cref="GetService{T}"/> for a service that doesn't exist, you'll get an exception.
		///
		/// </summary>
		/// <param name="t">the type of service to check for</param>
		/// <returns>True if the service can be got, False otherwise.</returns>
		bool CanBuildService(Type t);

		/// <summary>
		/// Finds an instance of a service given the type parameter.
		/// All services are lazily initialized, so its possible that if is the first time the type of
		/// service has been requested, this call could allocate a new instance of the service.
		///
		/// If the service wasn't registered, you'll get an exception. Use <see cref="CanBuildService{T}"/> to check if the service can be got.
		/// </summary>
		/// <typeparam name="T">The type of service to get</typeparam>
		/// <returns>An instance of <typeparamref name="T"/></returns>
		T GetService<T>();

		/// <summary>
		/// Returns true if the given service is registered in the provider.
		/// If you make a call to <see cref="GetService{T}"/> for a service that doesn't exist, you'll get an exception.
		///
		/// </summary>
		/// <typeparam name="T">The type of service to get</typeparam>
		/// <returns>True if the service can be got, False otherwise.</returns>
		bool CanBuildService<T>();

		/// <summary>
		/// Create a new <see cref="IDependencyProviderScope"/>, using the current instance as a parent.
		/// Any service that was registered as a Scoped service will be re-created in the new provider.
		/// Any service that was registered as a Singleton will use the instance from the parent provider.
		/// If the parent provider has <see cref="IDependencyProviderScope.Dispose"/> called, then the child provider
		/// will also be disposed.
		/// </summary>
		/// <param name="configure">Optionally, you can pass a configuration function that registers new services specific to the child provider.</param>
		/// <returns>A new <see cref="IDependencyProviderScope"/></returns>
		IDependencyProviderScope Fork(Action<IDependencyBuilder> configure = null);
	}

	/// <summary>
	/// The <see cref="IDependencyProviderScope"/> is a <see cref="IDependencyProvider"/>
	/// But has more access methods and lifecycle controls.
	/// </summary>
	public interface IDependencyProviderScope : IDependencyProvider
	{

		/// <summary>
		/// Give some type, try to find the service <see cref="DependencyLifetime"/> for the type.
		/// If the service isn't registered, then the method will return false.
		/// </summary>
		/// <param name="lifetime"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		bool TryGetServiceLifetime<T>(out DependencyLifetime lifetime);

		/// <summary>
		/// Disposing a <see cref="IDependencyProviderScope"/> will call <see cref="IBeamableDisposable.OnDispose"/>
		/// on all inner services that implement <see cref="IBeamableDisposable"/>.
		/// Also, after the dispose method has been called, any call to <see cref="IDependencyProvider.GetService"/> will fail.
		/// After this method, the <see cref="IsDisposed"/> will be true.
		/// However, if you re-hydrate a scope using the <see cref="Hydrate"/> method, then the scope is no longer in a disposed state.
		/// </summary>
		/// <returns>A promise capturing when the disposal will complete.</returns>
		Promise Dispose();

		/// <summary>
		/// Using the <see cref="IDependencyProvider.Fork"/> method allows you to create hierarchical <see cref="IDependencyProviderScope"/>.
		/// If this scope has a parent, this will point to it, or be null.
		/// </summary>
		IDependencyProviderScope Parent { get; }

		/// <summary>
		/// Using the <see cref="IDependencyProvider.Fork"/> method allows you to create hierarchical <see cref="IDependencyProviderScope"/>.
		/// If this scope has any children, this enumeration shows them, or it will be empty.
		/// </summary>
		IEnumerable<IDependencyProviderScope> Children { get; }

		/// <summary>
		/// It is possible to "un-dispose" a scope. Hydrating a scope from another scope copies all of the services
		/// from the other scope, and uses the other scope's service descriptors to create new services.
		/// Any children will be re-hydrated and their original configuration methods will be re-fire.
		/// After this method, the <see cref="IsDisposed"/> will be false.
		///
		/// This method can be useful to re-use an instance of a <see cref="IDependencyProviderScope"/> that had been disposed.
		/// </summary>
		/// <param name="serviceScope">The scope to get life from.</param>
		void Hydrate(IDependencyProviderScope serviceScope);

		/// <summary>
		/// By default, returns false.
		/// Returns true after <see cref="Dispose"/> completed.
		/// If <see cref="Hydrate"/> is run, then this returns false again.
		/// </summary>
		bool IsDisposed { get; }

		/// <summary>
		/// By default, returns true.
		/// Returns false after <see cref="Dispose"/> is started.
		/// </summary>
		bool IsActive { get; }

		/// <summary>
		/// An enumerable set of the <see cref="ServiceDescriptor"/>s that are attached to this provider as transient
		/// </summary>
		IEnumerable<ServiceDescriptor> TransientServices { get; }

		/// <summary>
		/// An enumerable set of the <see cref="ServiceDescriptor"/>s that are attached to this provider as scoped
		/// </summary>
		IEnumerable<ServiceDescriptor> ScopedServices { get; }

		/// <summary>
		/// An enumerable set of the <see cref="ServiceDescriptor"/>s that are attached to this provider as singleton
		/// </summary>
		IEnumerable<ServiceDescriptor> SingletonServices { get; }

		/// <summary>
		/// If <see cref="IDependencyProvider.Fork"/> had been called on this instance, then there would be children.
		/// If you want to disassociate the parent/child relationship, use this function.
		/// </summary>
		/// <param name="child"></param>
		void RemoveChild(IDependencyProviderScope child);
	}

	public class DependencyProvider : IDependencyProviderScope
	{
		private Dictionary<Type, ServiceDescriptor> Transients { get; set; }
		private Dictionary<Type, ServiceDescriptor> Scoped { get; set; }
		private Dictionary<Type, ServiceDescriptor> Singletons { get; set; }

		private Dictionary<Type, object> SingletonCache { get; set; } = new Dictionary<Type, object>();
		private Dictionary<Type, object> ScopeCache { get; set; } = new Dictionary<Type, object>();

		private bool _destroyed;
		private bool _isDestroying;

		public bool IsDisposed => _destroyed;
		public bool IsActive => !_isDestroying && !_destroyed;

		public IEnumerable<ServiceDescriptor> TransientServices => Transients.Values;
		public IEnumerable<ServiceDescriptor> ScopedServices => Scoped.Values;
		public IEnumerable<ServiceDescriptor> SingletonServices => Singletons.Values;

		public IDependencyProviderScope Parent { get; private set; }
		private HashSet<IDependencyProviderScope> _children = new HashSet<IDependencyProviderScope>();

		private Dictionary<IDependencyProviderScope, Action<IDependencyBuilder>> _childToConfigurator =
			new Dictionary<IDependencyProviderScope, Action<IDependencyBuilder>>();
		public IEnumerable<IDependencyProviderScope> Children => _children;

		public Guid Id = Guid.NewGuid();

		private BuildOptions _options;

		public DependencyProvider(DependencyBuilder builder, BuildOptions options = null)
		{
			if (options == null)
			{
				options = new BuildOptions();
			}

			_options = options;
			Transients = new Dictionary<Type, ServiceDescriptor>();
			foreach (var desc in builder.TransientServices)
			{
				Transients.Add(desc.Interface, desc);
			}

			Scoped = new Dictionary<Type, ServiceDescriptor>();
			foreach (var desc in builder.ScopedServices)
			{
				Scoped.Add(desc.Interface, desc);
			}

			Singletons = new Dictionary<Type, ServiceDescriptor>();
			foreach (var desc in builder.SingletonServices)
			{
				Singletons.Add(desc.Interface, desc);
			}
		}


		public bool TryGetServiceLifetime<T>(out DependencyLifetime lifetime)
		{

			lifetime = DependencyLifetime.Unknown;
			if (Transients.TryGetValue(typeof(T), out _))
			{
				lifetime = DependencyLifetime.Transient;
				return true;
			}
			if (Scoped.TryGetValue(typeof(T), out _))
			{
				lifetime = DependencyLifetime.Scoped;
				return true;
			}
			if (Singletons.TryGetValue(typeof(T), out _))
			{
				lifetime = DependencyLifetime.Singleton;
				return true;
			}

			if (Parent == null)
			{
				return false;
			}
			else
			{
				return Parent.TryGetServiceLifetime<T>(out lifetime);
			}
		}

		public T GetService<T>()
		{
			return (T)GetService(typeof(T));
		}

		public bool CanBuildService<T>()
		{
			return CanBuildService(typeof(T));
		}

		public bool CanBuildService(Type t)
		{
			if (_destroyed) throw new Exception("Provider scope has been destroyed and can no longer be accessed.");

			return Transients.ContainsKey(t) || Scoped.ContainsKey(t) || Singletons.ContainsKey(t) || (Parent?.CanBuildService(t) ?? false);
		}

		public object GetService(Type t)
		{
			if (_destroyed) throw new Exception("Provider scope has been destroyed and can no longer be accessed.");

			if (t == typeof(IDependencyProvider)) return this;
			if (t == typeof(IDependencyProviderScope)) return this;

			if (Transients.TryGetValue(t, out var descriptor))
			{
				var service = descriptor.Factory(this);
				return service;
			}

			if (Scoped.TryGetValue(t, out descriptor))
			{
				if (ScopeCache.TryGetValue(t, out var instance))
				{
					return instance;
				}

				return ScopeCache[t] = descriptor.Factory(this);
			}


			if (Singletons.TryGetValue(t, out descriptor))
			{
				if (SingletonCache.TryGetValue(t, out var instance))
				{
					return instance;
				}

				return SingletonCache[t] = descriptor.Factory(this);
			}

			if (Parent != null)
			{
				return Parent.GetService(t);
			}


			throw new Exception($"Service not found {t.Name}");
		}

		List<Promise<Unit>> disposalPromises = new List<Promise<Unit>>();
		List<Promise<Unit>> childRemovalPromises = new List<Promise<Unit>>();

		// ReSharper disable Unity.PerformanceAnalysis
		public async Promise Dispose()
		{
			if (_isDestroying || _destroyed) return; // don't dispose twice!
			_isDestroying = true;

			disposalPromises.Clear();
			childRemovalPromises.Clear();


			lock (_children)
			{
				var childrenClone = new List<IDependencyProviderScope>(_children);
				foreach (var child in childrenClone)
				{
					if (child != null)
					{
						var removePromise = child.Dispose();
						if (removePromise != null)
						{
							childRemovalPromises.Add(removePromise);
						}
					}
				}
			}

			await Promise.Sequence(childRemovalPromises);

			void DisposeServices(IEnumerable<object> services)
			{
				var clonedList = new List<object>(services);
				foreach (var service in clonedList)
				{
					if (service == null) continue;
					if (service is IBeamableDisposable disposable)
					{
						var promise = disposable.OnDispose();
						if (promise != null)
						{
							disposalPromises.Add(promise);
						}
					}
				}
			}

			void ClearServices(Dictionary<Type, ServiceDescriptor> descriptors)
			{
				foreach (var kvp in descriptors)
				{
					// clear factories...
					kvp.Value.Factory = null;
					kvp.Value.Implementation = null;
					kvp.Value.Interface = null;
				}
				descriptors.Clear();
			}



			DisposeServices(SingletonCache.Values.Distinct());
			DisposeServices(ScopeCache.Values.Distinct());

			await Promise.Sequence(disposalPromises);

			SingletonCache.Clear();
			ScopeCache.Clear();

			if (!_options.allowHydration)
			{
				// remove from parent.
				Parent?.RemoveChild(this);

				ClearServices(Singletons);
				ClearServices(Transients);
				ClearServices(Scoped);
				Singletons = null;
				Transients = null;
				Scoped = null;

				SingletonCache = null;
				ScopeCache = null;
			}

			_destroyed = true;
		}

		public void Hydrate(IDependencyProviderScope other)
		{
			if (!_options.allowHydration) throw new InvalidOperationException("Cannot rehydrate scope.");

			_destroyed = other.IsDisposed;
			_isDestroying = false;
			Transients = new Dictionary<Type, ServiceDescriptor>();
			foreach (var desc in other.TransientServices)
			{
				Transients.Add(desc.Interface, desc);
			}
			Scoped = new Dictionary<Type, ServiceDescriptor>();
			foreach (var desc in other.ScopedServices)
			{
				Scoped.Add(desc.Interface, desc);
			}
			Singletons = new Dictionary<Type, ServiceDescriptor>();
			foreach (var desc in other.SingletonServices)
			{
				Singletons.Add(desc.Interface, desc);
			}
			SingletonCache.Clear();
			ScopeCache.Clear();

			lock (_children)
			{
				var oldChildren = new HashSet<IDependencyProviderScope>(_children);
				var newChildren = new HashSet<IDependencyProviderScope>();
				foreach (var child in oldChildren)
				{
					var configurator = _childToConfigurator[child];
					var newChild = Fork(configurator);
					newChildren.Add(newChild);
					child.Hydrate(newChild);
				}

				foreach (var child in newChildren)
				{
					_children.Remove(child);
				}
			}
		}

		public void RemoveChild(IDependencyProviderScope child)
		{
			lock (_children)
			{
				_childToConfigurator[child] = null;
				_childToConfigurator.Remove(child);
				_children.Remove(child);
			}
		}

		void AddDescriptors(List<ServiceDescriptor> target,
			Dictionary<Type, ServiceDescriptor> source,
			Func<IDependencyProvider, ServiceDescriptor, object> factory)
		{
			foreach (var kvp in source)
			{
				target.Add(new ServiceDescriptor
				{
					Implementation = kvp.Value.Implementation,
					Interface = kvp.Value.Interface,
					Factory = p => factory(p, kvp.Value)
				});
			}
		}

		public IDependencyProviderScope Fork(Action<IDependencyBuilder> configure = null)
		{
			var builder = new DependencyBuilder();
			// populate all of the existing services we have in this scope.

			// transients are stupid, and I should probably delete them.
			AddDescriptors(builder.TransientServices, Transients, (nextProvider, desc) => desc.Factory(nextProvider));

			// all scoped descriptors
			AddDescriptors(builder.ScopedServices, Scoped, (nextProvider, desc) => desc.Factory(nextProvider));
			// scopes services build brand new instances per provider

			// singletons use their parent singleton cache.
			AddDescriptors(builder.SingletonServices, Scoped, (_, desc) => GetService(desc.Interface));

			configure?.Invoke(builder);

			var provider = new DependencyProvider(builder, _options) { Parent = this };
			lock (_children)
			{
				_children.Add(provider);
				_childToConfigurator[provider] = configure;
			}

			return provider;
		}
	}

}
