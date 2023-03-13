using Beamable.Common.Dependencies;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
#endif

namespace Beamable.Service
{
	public static class ServiceManager
	{
		private static readonly Dictionary<Type, IServiceResolver> Resolvers = new Dictionary<Type, IServiceResolver>();


#if UNITY_EDITOR
		private static bool _testingServices;
		private const bool RegisterEditorResolversByDefault = true;
		private static bool _registerEditorResolvers = RegisterEditorResolversByDefault;

		private static IDependencyProvider _legacyProvider;

		// public static IDependencyProvider LegacyDependencyProvider =>
		// 	_legacyProvider ?? (_legacyProvider = new LegacyProvider());

#if UNITY_2019_1_OR_NEWER
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
		static void Init()
		{
			ClearResolvers();
		}

		public class TestScope : IDisposable
		{
			internal static readonly TestScope Instance = new TestScope();

			private TestScope()
			{
			}

			public void Dispose()
			{
				_testingServices = false;
			}
		}

		static ServiceManager()
		{
			EditorApplication.playModeStateChanged += OnPlayModeChanged;
		}
#endif

		// This is an optimization for FindResolver<T> that stores resolvers inside a static generic class.  This allows
		// us to quickly look up the instance by type without digging into the dictionary and avoids any casting.
		private static class ResolverHolder<T>
			where T : class
		{
			public static IServiceResolver<T> Resolver;
		}



		public static void RuntimeOnlyDontDestroyOnLoad(GameObject go)
		{
#if !UNITY_EDITOR
			GameObject.DontDestroyOnLoad(go);
#endif
		}

		public static void DisableEditorResolvers()
		{
#if UNITY_EDITOR
			_registerEditorResolvers = false;
			ServicesLogger.Log("Disabling use of editor resolvers.");
#endif
		}

		public static void Provide<T>(IServiceResolver<T> resolver, bool overrideExisting = true)
			where T : class
		{
			var tType = typeof(T);
			if (!overrideExisting && Resolvers.ContainsKey(tType))
			{
				return;
			}
			Resolvers[tType] = resolver;
			ResolverHolder<T>.Resolver = resolver; // Optimization for FindResolver<T>, see notes below.
			ServicesLogger.LogFormat("Registered {0} to provide service {1}.", resolver.GetType().Name, typeof(T).Name);
		}

		public static void ProvideWithDefaultContainer<T>(T service, bool overrideExisting = true)
			where T : class
		{
			Provide(new ServiceContainer<T>(service), overrideExisting);
		}

		public static void Provide<T>(IDependencyProvider provider, bool overrideExisting = true)
			where T : class
		{
			Provide(new ServiceContainer<T>(provider.GetService<T>()));
		}

		public static void Remove<T>(IServiceResolver<T> resolver = null)
			where T : class
		{
			// Null resolver must be called with generic, and allows removal of whatever is currently registered.  We grab
			// A reference solely for the logging below.
			if (resolver == null)
			{
				resolver = ResolverHolder<T>.Resolver;
			}
			else if (resolver != ResolverHolder<T>.Resolver)
			{
				ServicesLogger.LogFormat("Service {0} not registered, ignore remove request.", resolver.GetType().Name);
				return;
			}

			Resolvers.Remove(typeof(T));
			ResolverHolder<T>.Resolver = null;
			ServicesLogger.LogFormat("Removed {0} from providing service {1}.", (resolver == null ? "null" : resolver.GetType().Name), typeof(T).Name);
		}

		// Returns true if the next call to Resolve<T>() will return a valid instance of the required service.  Note, this
		// could mean that the service will be lazily created by that call, so it should be used for cases where it's OK
		// for the service not to exist and also OK for the service to be created on the fly if necessary.  For code like
		// cleanup where you only want the service to be returned if it was already created, use Exists() instead.
		public static bool CanResolve<T>()
			where T : class
		{
			var resolver = FindResolver<T>();
			return resolver != null && resolver.CanResolve();
		}

		// Returns true if the service has already been created and thus a valid instance will be returned from the next
		// call to Resolve<T>().  This differs from CanResolve<T> in that it will return false even if the call to
		// Resolve<T>() would succeed by lazily creating the service object.  Use this method when doing clean-up
		// operations with the required service if it already exists, but don't want to create it if not.
		public static bool Exists<T>()
			where T : class
		{
			var resolver = FindResolver<T>();
			return resolver != null && resolver.Exists();
		}

		// [Obsolete("The service manager will be replaced by the BeamContext.Default.ServiceProvider")]
		public static T Resolve<T>()
			where T : class
		{
#if UNITY_EDITOR
			if (!Application.isPlaying && !_testingServices)
				Debug.LogError("ServiceManager Resolve while not playing!");
#endif
			var resolver = FindResolver<T>();
			if (resolver != null)
			{
				return resolver.Resolve();
			}
			throw new InvalidOperationException("No service found of type " + typeof(T).Name);
		}

		public static T Resolve<T>(IDependencyProvider provider) where T : class
		{
			return provider.GetService<T>();
		}

		public static T ResolveWithProvider<T>(IDependencyProvider provider)
			where T : class
		{
			try
			{
				return provider.GetService<T>();
			}
			catch
			{
				throw new InvalidOperationException("No service found of type " + typeof(T).Name);
			}
		}



		/// <summary>
		/// Convenience function for resolving a service only if it is available
		/// to be resolved.
		/// </summary>
		/// <typeparam name="T">Service type, which may be an interface.</typeparam>
		/// <returns>Resolved service, or null if unresolvable.</returns>
		public static T ResolveIfAvailable<T>() where T : class
		{
			return CanResolve<T>() ? Resolve<T>() : null;
		}

		public static void OnTeardown()
		{
			ServicesLogger.Log("Tearing down all service resolvers.");

			// We create a new list because it is possible that teardown handlers will remove
			// some resolvers.
			var tornDownResolvers = new List<IServiceResolver>(Resolvers.Values);
			for (int i = 0; i < tornDownResolvers.Count; i++)
			{
				tornDownResolvers[i].OnTeardown();
			}
		}

#if UNITY_EDITOR
		// Use in tests that are testing code that would normally be run in play mode
		public static TestScope AllowInTests()
		{
			_testingServices = true;
			return TestScope.Instance;
		}
#endif

		// For debugging purposes only.
		public static void LogResolvers(StringBuilder builder = null)
		{
			foreach (var resolverPair in Resolvers)
			{
				var type = resolverPair.Key;
				var resolver = resolverPair.Value;
				builder?.Append(resolver.GetType().Name).Append(" -> ").Append(type.Name).Append("\n");
				ServicesLogger.LogFormat("Resolver {0} provides service {1}.", resolver.GetType().Name, type.Name);
			}
		}

		public static void PlaceUnderServiceRoot(GameObject serviceObject)
		{
			// would be nice to cache this, but then clearing the pointer becomes difficult..
			var root = GameObject.Find("/Services");
			if (root == null)
			{
				root = new GameObject("Services");
			}
			RuntimeOnlyDontDestroyOnLoad(root);
			serviceObject.transform.SetParent(root.transform);
		}

		private static IServiceResolver<T> FindResolver<T>()
									where T : class
		{
			var resolver = ResolverHolder<T>.Resolver;
			if (resolver != null)
			{
				return resolver;
			}

#if UNITY_EDITOR
			if (_registerEditorResolvers)
			{
				return RegisterEditorResolver<T>();
			}
#endif

			return null;
		}

#if UNITY_EDITOR

		private static IServiceResolver<T> RegisterEditorResolver<T>()
			where T : class
		{
			var serviceType = typeof(T);
			var att = serviceType.GetCustomAttributes(typeof(EditorServiceResolverAttribute), true);
			if (att.Length > 0)
			{
				var editorAttr = att[0] as EditorServiceResolverAttribute;

				var resolver = Activator.CreateInstance(editorAttr.resolverType) as IServiceResolver<T>;

				// Note that dynamically registered resolvers always supply false for the overrideExisting flag.  This
				// is to ensure that no dynamic editor-only resolver ever overrides a real resolver for any reason.
				// It is desired for the inverse to be true -- that a non-editor resolver overrides an editor resolver.
				Provide(resolver, false);

				return resolver;
			}
			return null;
		}

		private static void OnPlayModeChanged(PlayModeStateChange state)
		{
			// Allow editor resolvers again when entering edit mode
			// Entering play mode already resets this state
			if (state != PlayModeStateChange.EnteredEditMode) return;

			ClearResolvers();
		}

		private static void ClearResolvers()
		{
			_registerEditorResolvers = RegisterEditorResolversByDefault;

			foreach (var pair in Resolvers)
			{
				var holderType = typeof(ResolverHolder<>).MakeGenericType(pair.Key);
				var field = holderType.GetField("resolver", BindingFlags.Static | BindingFlags.Public);
				field?.SetValue(null, null);
			}
			Resolvers.Clear();
		}

#endif
	}
}
