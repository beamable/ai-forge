using Beamable.Common.Dependencies;
using System;

namespace Beamable.Server
{
	public delegate T ServiceFactory<out T>(IServiceProvider provider);

	public interface IServiceBuilder
	{
		/// <summary>
		/// Access the <see cref="IDependencyBuilder"/> for the root scope.
		/// </summary>
		IDependencyBuilder Builder { get; }

		/// <summary>
		/// Adds an instance of a service that respects the <typeparamref name="TService"/> contract as a per-request instance.
		/// This means that data here is completely isolated and exists per-request. One thing to keep in mind:
		///   - If 2 Transient services, A and B, spawned for Request-1 ask for a Scoped service, C, both A and B will share C's instance.  
		/// </summary>
		void AddTransient<T>(ServiceFactory<T> factory);

		/// <summary>
		/// Adds an instance of a service that respects the <typeparamref name="TService"/> contract as a singleton (shared data across all requests being processed).
		/// Some things to keep in mind since the data in this service is shared between all requests being serviced:
		///  - If it's not read-only, you need to guarantee thread safety yourself.
		///  - If it is read-only, you can access fields here without any fear.
		/// </summary>
		void AddSingleton<T>(ServiceFactory<T> factory);


		/// <summary>
		/// Adds an instance of a service that respects the <typeparamref name="TService"/> contract as a per-request singleton.
		/// This means that data in Scoped services are shared between all Transient services being used by a request.
		/// </summary>
		void AddScoped<T>(ServiceFactory<T> factory);

		/// <summary>
		/// Adds an instance of a service that respects the <typeparamref name="TService"/> contract as a per-request instance.
		/// This means that data here is completely isolated and exists per-request. One thing to keep in mind:
		///   - If 2 Transient services, A and B, spawned for Request-1 ask for a Scoped service, C, both A and B will share C's instance.  
		/// </summary>
		void AddTransient<T>();

		/// <summary>
		/// Adds an instance of a service that respects the <typeparamref name="TService"/> contract as a singleton (shared data across all requests being processed).
		/// Some things to keep in mind since the data in this service is shared between all requests being serviced:
		///  - If it's not read-only, you need to guarantee thread safety yourself.
		///  - If it is read-only, you can access fields here without any fear.
		/// </summary>
		[Obsolete("There's currently an issue with this implementation. It'll be fixed in the future.\n" +
				  "For now, manually create an instance of T and use the AddSingleton<T>(ServiceFactory<T>) overload to " +
				  "register a factory method that returns your manually created instance.")]
		void AddSingleton<T>();

		/// <summary>
		/// Adds an instance of a service that respects the <typeparamref name="TService"/> contract as a per-request singleton.
		/// This means that data in Scoped services are shared between all Transient services being used by a request.
		/// </summary>
		void AddScoped<T>();

		void AddTransient<TService, TImplementation>()
		   where TService : class
		   where TImplementation : class, TService;

		[Obsolete("There's currently an issue with this implementation. It'll be fixed in the future.\n" +
				  "For now, manually create an instance of TImplementation and use the AddSingleton<T>(ServiceFactory<T>) overload to " +
				  "register a factory method that returns your manually created instance.")]
		void AddSingleton<TService, TImplementation>()
		   where TService : class
		   where TImplementation : class, TService;

		void AddScoped<TService, TImplementation>()
		   where TService : class
		   where TImplementation : class, TService;
	}
}
