using Beamable.Common;
using Beamable.Common.Dependencies;
using System;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor
{
	public static class DependencyExtensions
	{
		/// <summary>
		/// Add a singleton to the <see cref="IDependencyBuilder"/> that will be automatically serialized to the Unity Session State
		/// when the resultant <see cref="IDependencyProviderScope"/> is destroyed. The next time that the singleton is
		/// instantiated, it will use the given <see cref="factory"/> function, and then apply the serialized data from Session State.
		/// </summary>
		/// <param name="builder">A <see cref="IDependencyBuilder"/> to register the service to</param>
		/// <param name="factory">A method that produces a <see cref="T"/> service. <b> DO NOT JUST ASK THE DI TO CREATE THE INSTANCE </b>, because then an infinite loop will occur.</param>
		/// <typeparam name="T">The type of service to register</typeparam>
		/// <returns>The same <see cref="IDependencyBuilder"/> that was given</returns>
		public static IDependencyBuilder LoadSingleton<T>(this IDependencyBuilder builder, Func<IDependencyProvider, T> factory)
		{
			builder.AddSingleton<T>(provider =>
			{
				var wrapper = provider.GetService<SingletonStorageWrapper<T>>();
				wrapper.Storage.Apply(wrapper.Service);
				return wrapper.Service;
			});
			builder.AddSingleton(provider => new SingletonStorageWrapper<T>(provider.GetService<ServiceStorage>(), factory(provider)));
			return builder;
		}
	}

	public class SingletonStorageWrapper<T> : IBeamableDisposable
	{
		public ServiceStorage Storage { get; }
		public T Service { get; }

		public SingletonStorageWrapper(ServiceStorage storage, T service)
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

	public class ServiceStorage
	{
		private readonly BeamEditorContext _context;
		private const string DEFAULT = "__doesn't exist";
		private const string SINGLETON = "singleton";

		public ServiceStorage(BeamEditorContext context)
		{
			_context = context;
		}

		private string GetKey<T>(string name)
		{
			return $"p{_context.PlayerCode}_s{typeof(T).Name}_n{name}";
		}

		/// <summary>
		/// Same as <see cref="Save{T}(string, T)"/>, however, the name is derived from the type name, thus acting as a singleton.
		/// </summary>
		public void Save<T>(T service) => Save(SINGLETON, service);

		/// <summary>
		/// Serialize the given service to SessionState.
		/// </summary>
		/// <param name="name">A unique name for the service. Names can be re-used across contexts.</param>
		/// <param name="service">The service to serialize.</param>
		/// <typeparam name="T">The type of service</typeparam>
		public void Save<T>(string name, T service)
		{
			if (service is ISerializationCallbackReceiver receiver)
			{
				receiver.OnBeforeSerialize();
			}

			if (service is IServiceStorable storable)
			{
				storable.OnBeforeSaveState();
			}
			var json = JsonUtility.ToJson(service);
			SessionState.SetString(GetKey<T>(name), json);
		}

		/// <summary>
		/// Same as <see cref="Apply{T}(string, T)"/>, however, the name is derived from the type name, thus acting as a singleton.
		/// </summary>
		public void Apply<T>(T service) => Apply(SINGLETON, service);

		/// <summary>
		/// Deserialize data for the given name, and apply it to the given service. This will modify the given service.
		/// </summary>
		/// <param name="name">A unique name for the service. Names can be re-used across contexts.</param>
		/// <param name="service">The service to apply.</param>
		/// <typeparam name="T">The type of service</typeparam>
		public void Apply<T>(string name, T service)
		{
			var json = SessionState.GetString(GetKey<T>(name), DEFAULT);
			if (!string.Equals(DEFAULT, json))
			{
				JsonUtility.FromJsonOverwrite(json, service);
				if (service is ISerializationCallbackReceiver receiver)
				{
					receiver.OnAfterDeserialize();
				}
			}

			if (service is IServiceStorable storable)
			{
				storable.OnAfterLoadState();
			}
		}
	}
}
