using Beamable.Common.Api;
using Beamable.Common.Assistant;
using System;

namespace Beamable.Common.Dependencies
{
	public static class DependencyExtensions
	{
		/// <summary>
		/// Registers a scoped service, T, but with storage capabilities.
		/// When the service is loaded for the time, it will be applied its old state.
		/// If the T service implements <see cref="IServiceStorable"/>, then its callback methods will be triggered.
		/// If the T service implements <see cref="IStorageHandler{T}"/>, then it will be sent an instance of the serializer
		/// so that it can save itself whenever.
		/// </summary>
		/// <param name="builder"></param>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TStorageLayer"></typeparam>
		/// <returns></returns>
		public static IDependencyBuilder AddScopedStorage<T, TStorageLayer>(this IDependencyBuilder builder)
			where TStorageLayer : IStorageLayer
		{
			builder.AddScoped<T>(provider =>
			{
				var wrapper = provider.GetService<StorageWrapper<T>>();
				wrapper.Storage.Apply(wrapper.Service);

				return wrapper.Service;
			});
			builder.AddScoped<StorageWrapper<T>>(
				provider =>
				{
					var instance = DependencyBuilder.Instantiate<T>(provider);
					var storage = provider.GetService<ScopedServiceStorage<TStorageLayer>>();
					var wrapper = new StorageWrapper<T>(storage, instance);
					if (instance is IStorageHandler<T> handler)
					{
						var handle = new StorageHandle<T>(wrapper);
						handler.ReceiveStorageHandle(handle);
					}

					return wrapper;
				});

			if (!builder.Has<ScopedServiceStorage<TStorageLayer>>())
			{
				builder.AddScoped<ScopedServiceStorage<TStorageLayer>>();
			}

			return builder;
		}
	}

	public interface IStorageHandler<T>
	{
		void ReceiveStorageHandle(StorageHandle<T> handle);
	}

	public class StorageHandle<T>
	{
		private readonly StorageWrapper<T> _wrapper;

		public StorageHandle(StorageWrapper<T> wrapper)
		{
			_wrapper = wrapper;
		}

		/// <summary>
		/// manually save the content so that when this service is reloaded next time, it will have the latest data.
		/// </summary>
		public void Save()
		{
			_wrapper.Storage.Save(_wrapper.Service);
		}

		/// <summary>
		/// manually load the previously saved state onto the service. Use this to restore or unload data changes. 
		/// </summary>
		public void Load()
		{
			_wrapper.Storage.Apply(_wrapper.Service);
		}
	}

	public class StorageWrapper<T> : IBeamableDisposable
	{
		public IServiceStorage Storage { get; }
		public T Service { get; }

		public StorageWrapper(IServiceStorage storage, T service)
		{
			Storage = storage;
			Service = service;
		}

		public Promise OnDispose()
		{
			Storage.Save(Service);
			return Promise.Success;
		}
	}

	public interface IServiceStorable
	{
		void OnBeforeSaveState();
		void OnAfterLoadState();
	}

	public interface IServiceStorage
	{
		void Save<T>(T service);
		void Apply<T>(T service);
	}

	public interface IStorageLayer
	{
		void Save<T>(string key, T content);
		void Apply<T>(string key, T instance);
	}

	public class ScopedServiceStorage<TStorageLayer> : ServiceStorage<TStorageLayer>
		where TStorageLayer : IStorageLayer
	{
		private readonly IUserContext _context;
		private readonly IDependencyNameProvider _depName;
		private readonly IDependencyScopeNameProvider _scopeName;
		private readonly IDependencyProvider _provider;

		public ScopedServiceStorage(IUserContext context, IDependencyNameProvider depName, IDependencyScopeNameProvider scopeName, TStorageLayer storageLayer) : base(storageLayer)
		{
			_context = context;
			_depName = depName;
			_scopeName = scopeName;
		}
		protected override string GetKey<T>()
		{
			return $"scoped_{typeof(T).Name}_ctx_{_context.UserId}_provider_{_depName.DependencyProviderName}_scope_{_scopeName.DependencyScopeName}";
		}
	}

	public class SingletonServiceStorage<TStorageLayer> : ServiceStorage<TStorageLayer>
		where TStorageLayer : IStorageLayer
	{
		private readonly IUserContext _context;
		private readonly IDependencyNameProvider _scopeName;

		public SingletonServiceStorage(IUserContext context, IDependencyNameProvider scopeName, TStorageLayer storageLayer) : base(storageLayer)
		{
			_context = context;
			_scopeName = scopeName;
		}
		protected override string GetKey<T>()
		{
			return $"singleton_{typeof(T).Name}_ctx_{_context.UserId}_provider_{_scopeName.DependencyProviderName}";
		}
	}

	public abstract class ServiceStorage<TStorageLayer> : IServiceStorage
		where TStorageLayer : IStorageLayer
	{
		private readonly TStorageLayer _storageLayer;

		protected ServiceStorage(TStorageLayer storageLayer)
		{
			_storageLayer = storageLayer;
		}

		protected abstract string GetKey<T>();

		public void Save<T>(T service)
		{
			if (service is IServiceStorable storable)
			{
				storable.OnBeforeSaveState();
			}
			_storageLayer.Save(GetKey<T>(), service);
		}

		public void Apply<T>(T service)
		{
			_storageLayer.Apply<T>(GetKey<T>(), service);
			if (service is IServiceStorable storable)
			{
				storable.OnAfterLoadState();
			}
		}
	}
}
