using System;
using System.Collections.Generic;

namespace Beamable.Common.Dependencies
{

	public enum DependencyLifetime
	{
		Unknown,
		Transient,
		Scoped,
		Singleton
	}

	/// <summary>
	/// The <see cref="IDependencyBuilder"/> is part of the Beamable dependency injection system.
	/// It is used to describe a set of services that <i>will</i> exist, but the builder never actually creates any service instance.
	///
	/// You use a <see cref="IDependencyBuilder"/> to assemble a description of your service layout, and
	/// then use the <see cref="IDependencyBuilder.Build"/> method to construct a <see cref="IDependencyProvider"/>.
	///
	/// Before you finalize the builder, add services to the builder.
	/// </summary>
	public interface IDependencyBuilder
	{

		/// <summary>
		/// Add a transient service to the <see cref="IDependencyBuilder"/>.
		/// A transient service will be re-instantiated everytime it is requested from <see cref="IDependencyProvider.GetService"/>.
		/// </summary>
		/// <param name="factory">A function that takes a <see cref="IDependencyProvider"/> and produces an instance of <typeparamref name="TInterface"/></param>
		/// <typeparam name="TInterface">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <typeparam name="TImpl">The concrete type of the instance that will be returned from <see cref="IDependencyProvider.GetService"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddTransient<TInterface, TImpl>(Func<IDependencyProvider, TInterface> factory) where TImpl : TInterface;

		/// <summary>
		/// Add a transient service to the <see cref="IDependencyBuilder"/>.
		/// A transient service will be re-instantiated everytime it is requested from <see cref="IDependencyProvider.GetService"/>.
		/// </summary>
		/// <param name="factory">A function that produces an instance of <typeparamref name="TInterface"/></param>
		/// <typeparam name="TInterface">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <typeparam name="TImpl">The concrete type of the instance that will be returned from <see cref="IDependencyProvider.GetService"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddTransient<TInterface, TImpl>(Func<TInterface> factory) where TImpl : TInterface;

		/// <summary>
		/// Add a transient service to the <see cref="IDependencyBuilder"/>.
		/// A transient service will be re-instantiated everytime it is requested from <see cref="IDependencyProvider.GetService"/>.
		///
		/// <para>
		/// The instance will automatically be created by using the first available constructor of the <typeparamref name="TImpl"/> type,
		/// and providing parameters for the constructor from the <see cref="IDependencyProvider"/> itself
		/// </para>
		/// </summary>
		/// <typeparam name="TInterface">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <typeparam name="TImpl">The concrete type of the instance that will be returned from <see cref="IDependencyProvider.GetService"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddTransient<TInterface, TImpl>() where TImpl : TInterface;

		/// <summary>
		/// Add a transient service to the <see cref="IDependencyBuilder"/>.
		/// A transient service will be re-instantiated everytime it is requested from <see cref="IDependencyProvider.GetService"/>.
		/// </summary>
		/// <param name="factory">A function that takes a <see cref="IDependencyProvider"/> and produces an instance of <typeparamref name="TInterface"/></param>
		/// <typeparam name="T">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddTransient<T>(Func<IDependencyProvider, T> factory);

		/// <summary>
		/// Add a transient service to the <see cref="IDependencyBuilder"/>.
		/// A transient service will be re-instantiated everytime it is requested from <see cref="IDependencyProvider.GetService"/>.
		/// </summary>
		/// <param name="factory">A function that produces an instance of <typeparamref name="TInterface"/></param>
		/// <typeparam name="T">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddTransient<T>(Func<T> factory);

		/// <summary>
		/// Add a transient service to the <see cref="IDependencyBuilder"/>.
		/// A transient service will be re-instantiated everytime it is requested from <see cref="IDependencyProvider.GetService"/>.
		/// <para>
		/// The instance will automatically be created by using the first available constructor of the <typeparamref name="TImpl"/> type,
		/// and providing parameters for the constructor from the <see cref="IDependencyProvider"/> itself
		/// </para>
		/// </summary>
		/// <typeparam name="T">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddTransient<T>();

		/// <summary>
		/// Add a scoped service to the <see cref="IDependencyBuilder"/>.
		/// <para>
		/// A scoped service will be instantiated once, and the same instance will be requested from <see cref="IDependencyProvider.GetService"/>.
		/// However, if you use <see cref="Clone"/>, then all calls to <see cref="IDependencyProvider.GetService"/> on the resultant <see cref="IDependencyProvider"/>
		/// will generate a new scope instance.
		/// </para>
		/// </summary>
		/// <param name="factory">A function that takes a <see cref="IDependencyProvider"/> and produces an instance of <typeparamref name="TInterface"/></param>
		/// <typeparam name="TInterface">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <typeparam name="TImpl">The concrete type of the instance that will be returned from <see cref="IDependencyProvider.GetService"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddScoped<TInterface, TImpl>(Func<IDependencyProvider, TInterface> factory) where TImpl : TInterface;

		/// <summary>
		/// Add a transient service to the <see cref="IDependencyBuilder"/>.
		/// <para>
		/// A scoped service will be instantiated once, and the same instance will be requested from <see cref="IDependencyProvider.GetService"/>.
		/// However, if you use <see cref="Clone"/>, then all calls to <see cref="IDependencyProvider.GetService"/> on the resultant <see cref="IDependencyProvider"/>
		/// will generate a new scope instance.
		/// </para>
		/// </summary>
		/// <param name="factory">A function that produces an instance of <typeparamref name="TInterface"/></param>
		/// <typeparam name="TInterface">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <typeparam name="TImpl">The concrete type of the instance that will be returned from <see cref="IDependencyProvider.GetService"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddScoped<TInterface, TImpl>(Func<TInterface> factory) where TImpl : TInterface;

		/// <summary>
		/// Add a transient service to the <see cref="IDependencyBuilder"/>.
		/// <para>
		/// A scoped service will be instantiated once, and the same instance will be requested from <see cref="IDependencyProvider.GetService"/>.
		/// However, if you use <see cref="Clone"/>, then all calls to <see cref="IDependencyProvider.GetService"/> on the resultant <see cref="IDependencyProvider"/>
		/// will generate a new scope instance.
		/// </para>
		/// </summary>
		/// <param name="service">a value of <typeparamref name="TInterface"/> that will be used as the scoped instance</param>
		/// <typeparam name="TInterface">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <typeparam name="TImpl">The concrete type of the instance that will be returned from <see cref="IDependencyProvider.GetService"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddScoped<TInterface, TImpl>(TInterface service) where TImpl : TInterface;

		/// <summary>
		/// Add a transient service to the <see cref="IDependencyBuilder"/>.
		/// <para>
		/// A scoped service will be instantiated once, and the same instance will be requested from <see cref="IDependencyProvider.GetService"/>.
		/// However, if you use <see cref="Clone"/>, then all calls to <see cref="IDependencyProvider.GetService"/> on the resultant <see cref="IDependencyProvider"/>
		/// will generate a new scope instance.
		/// </para>
		/// <para>
		/// The instance will automatically be created by using the first available constructor of the <typeparamref name="TImpl"/> type,
		/// and providing parameters for the constructor from the <see cref="IDependencyProvider"/> itself
		/// </para>
		/// </summary>
		/// <typeparam name="TInterface">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <typeparam name="TImpl">The concrete type of the instance that will be returned from <see cref="IDependencyProvider.GetService"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddScoped<TInterface, TImpl>() where TImpl : TInterface;

		/// <summary>
		/// Add a transient service to the <see cref="IDependencyBuilder"/>.
		/// <para>
		/// A scoped service will be instantiated once, and the same instance will be requested from <see cref="IDependencyProvider.GetService"/>.
		/// However, if you use <see cref="Clone"/>, then all calls to <see cref="IDependencyProvider.GetService"/> on the resultant <see cref="IDependencyProvider"/>
		/// will generate a new scope instance.
		/// </para>
		/// </summary>
		/// <param name="factory">A function that takes a <see cref="IDependencyProvider"/> and produces an instance of <typeparamref name="TInterface"/></param>
		/// <typeparam name="T">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddScoped<T>(Func<IDependencyProvider, T> factory);

		/// <summary>
		/// Add a transient service to the <see cref="IDependencyBuilder"/>.
		/// <para>
		/// A scoped service will be instantiated once, and the same instance will be requested from <see cref="IDependencyProvider.GetService"/>.
		/// However, if you use <see cref="Clone"/>, then all calls to <see cref="IDependencyProvider.GetService"/> on the resultant <see cref="IDependencyProvider"/>
		/// will generate a new scope instance.
		/// </para>
		/// </summary>
		/// <param name="factory">A function that produces an instance of <typeparamref name="TInterface"/></param>
		/// <typeparam name="T">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddScoped<T>(Func<T> factory);

		/// <summary>
		/// Add a transient service to the <see cref="IDependencyBuilder"/>.
		/// <para>
		/// A scoped service will be instantiated once, and the same instance will be requested from <see cref="IDependencyProvider.GetService"/>.
		/// However, if you use <see cref="Clone"/>, then all calls to <see cref="IDependencyProvider.GetService"/> on the resultant <see cref="IDependencyProvider"/>
		/// will generate a new scope instance.
		/// </para>
		/// </summary>
		/// <param name="service">a value of <typeparamref name="T"/> that will be used as the scoped instance</param>
		/// <typeparam name="T">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddScoped<T>(T service);

		/// <summary>
		/// Add a transient service to the <see cref="IDependencyBuilder"/>.
		/// <para>
		/// A scoped service will be instantiated once, and the same instance will be requested from <see cref="IDependencyProvider.GetService"/>.
		/// However, if you use <see cref="Clone"/>, then all calls to <see cref="IDependencyProvider.GetService"/> on the resultant <see cref="IDependencyProvider"/>
		/// will generate a new scope instance.
		/// </para>
		/// <para>
		/// The instance will automatically be created by using the first available constructor of the <typeparamref name="TImpl"/> type,
		/// and providing parameters for the constructor from the <see cref="IDependencyProvider"/> itself
		/// </para>
		/// </summary>
		/// <typeparam name="T">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddScoped<T>();

		/// <summary>
		/// Add a singleton service to the <see cref="IDependencyBuilder"/>.
		/// <para>
		/// A singleton service will be instantiated once, and the same instance will be requested from <see cref="IDependencyProvider.GetService"/>.
		/// Even if you use <see cref="Clone"/>, the one instance will be returned for all children providers.
		/// </para>
		/// </summary>
		/// <param name="factory">A function that takes a <see cref="IDependencyProvider"/> and produces an instance of <typeparamref name="TInterface"/></param>
		/// <typeparam name="TInterface">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <typeparam name="TImpl">The concrete type of the instance that will be returned from <see cref="IDependencyProvider.GetService"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddSingleton<TInterface, TImpl>(Func<IDependencyProvider, TInterface> factory) where TImpl : TInterface;

		/// <summary>
		/// Add a singleton service to the <see cref="IDependencyBuilder"/>.
		/// <para>
		/// A singleton service will be instantiated once, and the same instance will be requested from <see cref="IDependencyProvider.GetService"/>.
		/// Even if you use <see cref="Clone"/>, the one instance will be returned for all children providers.
		/// </para>
		/// </summary>
		/// <param name="factory">A function that produces an instance of <typeparamref name="TInterface"/></param>
		/// <typeparam name="TInterface">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <typeparam name="TImpl">The concrete type of the instance that will be returned from <see cref="IDependencyProvider.GetService"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddSingleton<TInterface, TImpl>(Func<TInterface> factory) where TImpl : TInterface;

		/// <summary>
		/// Add a singleton service to the <see cref="IDependencyBuilder"/>.
		/// <para>
		/// A singleton service will be instantiated once, and the same instance will be requested from <see cref="IDependencyProvider.GetService"/>.
		/// Even if you use <see cref="Clone"/>, the one instance will be returned for all children providers.
		/// </para>
		/// </summary>
		/// <param name="service">A value instance of <typeparamref name="TInterface"/></param>
		/// <typeparam name="TInterface">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <typeparam name="TImpl">The concrete type of the instance that will be returned from <see cref="IDependencyProvider.GetService"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddSingleton<TInterface, TImpl>(TInterface service) where TImpl : TInterface;

		/// <summary>
		/// Add a singleton service to the <see cref="IDependencyBuilder"/>.
		/// <para>
		/// A singleton service will be instantiated once, and the same instance will be requested from <see cref="IDependencyProvider.GetService"/>.
		/// Even if you use <see cref="Clone"/>, the one instance will be returned for all children providers.
		/// </para>
		/// <para>
		/// The instance will automatically be created by using the first available constructor of the <typeparamref name="TImpl"/> type,
		/// and providing parameters for the constructor from the <see cref="IDependencyProvider"/> itself
		/// </para>
		/// </summary>
		/// <typeparam name="TInterface">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <typeparam name="TImpl">The concrete type of the instance that will be returned from <see cref="IDependencyProvider.GetService"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddSingleton<TInterface, TImpl>() where TImpl : TInterface;

		/// <summary>
		/// Add a singleton service to the <see cref="IDependencyBuilder"/>.
		/// <para>
		/// A singleton service will be instantiated once, and the same instance will be requested from <see cref="IDependencyProvider.GetService"/>.
		/// Even if you use <see cref="Clone"/>, the one instance will be returned for all children providers.
		/// </para>
		/// </summary>
		/// <param name="factory">A function that takes a <see cref="IDependencyProvider"/> and produces an instance of <typeparamref name="T"/></param>
		/// <typeparam name="T">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddSingleton<T>(Func<IDependencyProvider, T> factory);

		/// <summary>
		/// Add a singleton service to the <see cref="IDependencyBuilder"/>.
		/// <para>
		/// A singleton service will be instantiated once, and the same instance will be requested from <see cref="IDependencyProvider.GetService"/>.
		/// Even if you use <see cref="Clone"/>, the one instance will be returned for all children providers.
		/// </para>
		/// </summary>
		/// <param name="factory">A function that produces an instance of <typeparamref name="T"/></param>
		/// <typeparam name="T">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddSingleton<T>(Func<T> factory);

		/// <summary>
		/// Add a singleton service to the <see cref="IDependencyBuilder"/>. This overload allows for you to add batches of concrete service implementations
		/// that are tied to their concrete implementation dynamically (without knowing them at compilation-time).
		/// </summary>
		/// <param name="registeredType">The service is added tied to this type. Once the services are built, use this type to retrieve it.</param>
		/// <param name="factory">A function  that produces an instance of <paramref name="registeredType"/>. This type must inherit from <typeparamref name="T"/>.</param>
		/// <typeparam name="T">A parent type of <paramref name="registeredType"/>.</typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddSingleton<T>(Type registeredType, Func<T> factory);

		/// <summary>
		/// Add a singleton service to the <see cref="IDependencyBuilder"/>.
		/// <para>
		/// A singleton service will be instantiated once, and the same instance will be requested from <see cref="IDependencyProvider.GetService"/>.
		/// Even if you use <see cref="Clone"/>, the one instance will be returned for all children providers.
		/// </para>
		/// </summary>
		/// <param name="service">A value instance of <typeparamref name="TInterface"/></param>
		/// <typeparam name="T">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddSingleton<T>(T service);

		/// <summary>
		/// Add a singleton service to the <see cref="IDependencyBuilder"/>.
		/// <para>
		/// A singleton service will be instantiated once, and the same instance will be requested from <see cref="IDependencyProvider.GetService"/>.
		/// Even if you use <see cref="Clone"/>, the one instance will be returned for all children providers.
		/// </para>
		/// <para>
		/// The instance will automatically be created by using the first available constructor of the <typeparamref name="TImpl"/> type,
		/// and providing parameters for the constructor from the <see cref="IDependencyProvider"/> itself
		/// </para>
		/// </summary>
		/// <param name="t">The type of the service that is registered</param>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddSingleton(Type t);

		/// <summary>
		/// Add a singleton service to the <see cref="IDependencyBuilder"/>.
		/// <para>
		/// A singleton service will be instantiated once, and the same instance will be requested from <see cref="IDependencyProvider.GetService"/>.
		/// Even if you use <see cref="Clone"/>, the one instance will be returned for all children providers.
		/// </para>
		/// <para>
		/// The instance will automatically be created by using the first available constructor of the <typeparamref name="TImpl"/> type,
		/// and providing parameters for the constructor from the <see cref="IDependencyProvider"/> itself
		/// </para>
		/// </summary>
		/// <typeparam name="T">The type that is actually registered in the <see cref="IDependencyBuilder"/>. There can only be one registration for this type per <see cref="IDependencyBuilder"/></typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder AddSingleton<T>();

		/// <summary>
		/// Replace a singleton service already registered in the <see cref="IDependencyBuilder"/>
		/// </summary>
		/// <typeparam name="TExisting">The existing type that's implementation will be replaced</typeparam>
		/// <typeparam name="TNew">The new implementation type</typeparam>
		/// <param name="autoCreate">True by default. When true, if there was no existing service for <see cref="TExisting"/>, then the service will be registered. When false, an exception will be thrown. </param>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder ReplaceSingleton<TExisting, TNew>(bool autoCreate = true) where TNew : TExisting;

		/// <summary>
		/// Replace a singleton service already registered in the <see cref="IDependencyBuilder"/>
		/// </summary>
		/// <typeparam name="TExisting">The existing type that's implementation will be replaced</typeparam>
		/// <typeparam name="TNew">The new implementation type</typeparam>
		/// <param name="newService">The instance of <see cref="TNew"/> that will be used </param>
		/// <param name="autoCreate">True by default. When true, if there was no existing service for <see cref="TExisting"/>, then the service will be registered. When false, an exception will be thrown. </param>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder ReplaceSingleton<TExisting, TNew>(TNew newService, bool autoCreate = true) where TNew : TExisting;

		/// <summary>
		/// Replace a singleton service already registered in the <see cref="IDependencyBuilder"/>
		/// </summary>
		/// <typeparam name="TExisting">The existing type that's implementation will be replaced</typeparam>
		/// <typeparam name="TNew">The new implementation type</typeparam>
		/// <param name="factory">A method that will be invoked to create the instance</param>
		/// <param name="autoCreate">True by default. When true, if there was no existing service for <see cref="TExisting"/>, then the service will be registered. When false, an exception will be thrown. </param>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder ReplaceSingleton<TExisting, TNew>(Func<TNew> factory, bool autoCreate = true) where TNew : TExisting;

		/// <summary>
		/// Replace a singleton service already registered in the <see cref="IDependencyBuilder"/>
		/// </summary>
		/// <typeparam name="TExisting">The existing type that's implementation will be replaced</typeparam>
		/// <typeparam name="TNew">The new implementation type</typeparam>
		/// <param name="factory">A method that will be invoked to create the instance</param>
		/// <param name="autoCreate">True by default. When true, if there was no existing service for <see cref="TExisting"/>, then the service will be registered. When false, an exception will be thrown. </param>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder ReplaceSingleton<TExisting, TNew>(Func<IDependencyProvider, TNew> factory, bool autoCreate = true) where TNew : TExisting;

		/// <summary>
		/// After you have registered all your services, use this method to finalize the builder and produce a
		/// <see cref="IDependencyProvider"/> that can be used to get services.
		/// The actual return value will be a <see cref="IDependencyProviderScope"/> which has lifecycle controls for the <see cref="IDependencyProvider"/>
		/// </summary>
		/// <returns>A <see cref="IDependencyProviderScope"/> instance</returns>
		IDependencyProviderScope Build(BuildOptions options = null);

		/// <summary>
		/// Removes a service of some type that was added with any of the AddTransient, AddScoped, or AddSingleton calls.
		/// It doesn't matter how the type was registered.
		/// If no service was added, then this method will throw an exception. Use <see cref="RemoveIfExists{T}"/> to avoid the exception.
		/// </summary>
		/// <typeparam name="T">The type of service to remove</typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder Remove<T>();

		/// <summary>
		/// Removes a service of some type that was added with any of the AddTransient, AddScoped, or AddSingleton calls.
		/// It doesn't matter how the type was registered.
		/// If no service was added, then this method is a no-op.
		/// </summary>
		/// <typeparam name="T">The type of service to remove</typeparam>
		/// <returns>The same instance of <see cref="IDependencyBuilder"/> so that you can chain methods together.</returns>
		IDependencyBuilder RemoveIfExists<T>();

		/// <summary>
		/// Returns true is the given service has been registered.
		/// </summary>
		/// <typeparam name="T">The type of service to check for</typeparam>
		/// <returns>True if the service is registered in the builder, False otherwise</returns>
		bool Has<T>();

		/// <summary>
		/// Clones the <see cref="IDependencyBuilder"/>. The resulting builder will have of the same
		/// registrations that the current instance has. However, if you add any services to the original instance, or the
		/// returned instance, the addition won't affect the service registration state of the builder.
		/// </summary>
		/// <returns>A new <see cref="IDependencyBuilder"/></returns>
		IDependencyBuilder Clone();
	}

	/// <summary>
	/// Configuration that controls how a <see cref="IDependencyProvider"/> will be constructed
	/// </summary>
	public class BuildOptions
	{
		/// <summary>
		/// When building a provider, if this is false, then any calls to <see cref="IDependencyProviderScope.Hydrate"/> won't be allowed.
		/// </summary>
		public bool allowHydration = true;
	}

	/// <summary>
	/// The default implementation of <see cref="IDependencyBuilder"/>.
	/// It is not recommended that you ever use this class directly. You should only ever reference the interface.
	/// </summary>
	public class DependencyBuilder : IDependencyBuilder
	{
		public List<ServiceDescriptor> TransientServices { get; protected set; } = new List<ServiceDescriptor>();
		public List<ServiceDescriptor> ScopedServices { get; protected set; } = new List<ServiceDescriptor>();
		public List<ServiceDescriptor> SingletonServices { get; protected set; } = new List<ServiceDescriptor>();



		public IDependencyBuilder AddTransient<TInterface, TImpl>(Func<IDependencyProvider, TInterface> factory) where TImpl : TInterface
		{
			TransientServices.Add(new ServiceDescriptor
			{
				Interface = typeof(TInterface),
				Implementation = typeof(TImpl),
				Factory = (provider) => factory(provider)
			});
			return this;
		}

		public IDependencyBuilder AddTransient<TInterface, TImpl>(Func<TInterface> factory) where TImpl : TInterface =>
			AddTransient<TInterface, TImpl>(_ => factory());

		public IDependencyBuilder AddTransient<TInterface, TImpl>() where TImpl : TInterface =>
			AddTransient<TInterface, TImpl>(factory => Instantiate<TImpl>(factory));

		public IDependencyBuilder AddTransient<T>(Func<IDependencyProvider, T> factory) => AddTransient<T, T>(factory);

		public IDependencyBuilder AddTransient<T>(Func<T> factory) => AddTransient<T, T>(factory);

		public IDependencyBuilder AddTransient<T>() => AddTransient<T, T>();

		public IDependencyBuilder AddScoped<TInterface, TImpl>(Func<IDependencyProvider, TInterface> factory) where TImpl : TInterface
		{
			ScopedServices.Add(new ServiceDescriptor
			{
				Interface = typeof(TInterface),
				Implementation = typeof(TImpl),
				Factory = (provider) => factory(provider)
			});
			return this;
		}

		public IDependencyBuilder AddScoped<TInterface, TImpl>(Func<TInterface> factory) where TImpl : TInterface =>
			AddScoped<TInterface, TImpl>(_ => factory());

		public IDependencyBuilder AddScoped<TInterface, TImpl>(TInterface service) where TImpl : TInterface =>
			AddScoped<TInterface, TImpl>(_ => service);

		public IDependencyBuilder AddScoped<TInterface, TImpl>() where TImpl : TInterface =>
			AddScoped<TInterface, TImpl>(factory => Instantiate<TImpl>(factory));

		public IDependencyBuilder AddScoped<T>(Func<IDependencyProvider, T> factory) => AddScoped<T, T>(factory);

		public IDependencyBuilder AddScoped<T>(Func<T> factory) => AddScoped<T, T>(factory);

		public IDependencyBuilder AddScoped<T>(T service) => AddScoped<T, T>(service);

		public IDependencyBuilder AddScoped<T>() => AddScoped<T, T>();


		public IDependencyBuilder AddSingleton<TInterface, TImpl>(Func<IDependencyProvider, TInterface> factory) where TImpl : TInterface
		{
			SingletonServices.Add(new ServiceDescriptor
			{
				Interface = typeof(TInterface),
				Implementation = typeof(TImpl),
				Factory = (provider) => factory(provider)
			});
			return this;
		}

		public IDependencyBuilder AddSingleton(Type type)
		{
			SingletonServices.Add(new ServiceDescriptor
			{
				Interface = type,
				Implementation = type,
				Factory = provider => Instantiate(type, provider)
			});
			return this;
		}

		public IDependencyBuilder AddSingleton<T>(Type registeringType, Func<T> factory)
		{
			System.Diagnostics.Debug.Assert(typeof(T).IsAssignableFrom(registeringType), $"RegisteringType [{registeringType.Name}] does not implement/inherit from [{typeof(T).Name}]!");
			SingletonServices.Add(new ServiceDescriptor
			{
				Interface = registeringType,
				Implementation = registeringType,
				Factory = (provider) => factory()
			});
			return this;
		}

		public IDependencyBuilder AddSingleton<TInterface, TImpl>(Func<TInterface> factory) where TImpl : TInterface =>
			AddSingleton<TInterface, TImpl>(_ => factory());

		public IDependencyBuilder AddSingleton<TInterface, TImpl>(TInterface service) where TImpl : TInterface =>
			AddSingleton<TInterface, TImpl>(_ => service);

		public IDependencyBuilder AddSingleton<TInterface, TImpl>() where TImpl : TInterface =>
			AddSingleton<TInterface, TImpl>(factory => Instantiate<TImpl>(factory));

		public IDependencyBuilder AddSingleton<T>(Func<IDependencyProvider, T> factory) => AddSingleton<T, T>(factory);

		public IDependencyBuilder AddSingleton<T>(Func<T> factory) => AddSingleton<T, T>(factory);

		public IDependencyBuilder AddSingleton<T>(T service) => AddSingleton<T, T>(service);

		public IDependencyBuilder AddSingleton<T>() => AddSingleton<T, T>();

		public IDependencyBuilder ReplaceSingleton<TExisting, TNew>(bool autoCreate = true)
			where TNew : TExisting =>
			ReplaceSingleton<TExisting, TNew>(Instantiate<TNew>, autoCreate);

		public IDependencyBuilder ReplaceSingleton<TExisting, TNew>(TNew newService, bool autoCreate = true)
			where TNew : TExisting =>
			ReplaceSingleton<TExisting, TNew>(() => newService, autoCreate);

		public IDependencyBuilder ReplaceSingleton<TExisting, TNew>(Func<TNew> factory, bool autoCreate = true)
			where TNew : TExisting =>
			ReplaceSingleton<TExisting, TNew>(_ => factory(), autoCreate);

		public IDependencyBuilder ReplaceSingleton<TExisting, TNew>(Func<IDependencyProvider, TNew> factory, bool autoCreate = true) where TNew : TExisting
		{
			if (autoCreate)
			{
				RemoveIfExists<TExisting>();
			}
			else
			{
				Remove<TExisting>();
			}
			SingletonServices.Add(new ServiceDescriptor
			{
				Interface = typeof(TExisting),
				Implementation = typeof(TNew),
				Factory = provider => factory(provider)
			});
			return this;
		}

		/// <inheritdoc cref="Instantiate"/>
		public static TImpl Instantiate<TImpl>(IDependencyProvider provider)
		{
			return (TImpl)Instantiate(typeof(TImpl), provider);
		}

		/// <summary>
		/// Create an instance of the given type by using the dependencies available in the given <see cref="IDependencyProvider"/>
		/// </summary>
		/// <param name="type"></param>
		/// <param name="provider"></param>
		/// <returns>
		/// </returns>
		/// <exception cref="Exception"></exception>
		public static object Instantiate(Type type, IDependencyProvider provider)
		{
			// Gets all constructors
			var constructors = type.GetConstructors();

			if (constructors.Length == 0)
			{
				throw new Exception(
					$"Cannot create {type.Name} via automatic reflection with Dependency Injection. No constructors exist for the type. Likely, the file has been code-stripped from the assembly.");
			}

			// TODO: XXX: This only works for the largest constructor (the one with the most dependencies); really it should scan for the constructor it can match with the most dependencies
			// Currently, we just get the constructor with the most parameters
			var bestConstructor = constructors[0];
			var bestLength = bestConstructor.GetParameters().Length;
			for (var i = 1; i < constructors.Length; i++)
			{
				var currConstructor = constructors[i];
				var currLength = currConstructor.GetParameters().Length;
				if (currLength > bestLength)
				{
					bestConstructor = currConstructor;
					bestLength = currLength;
				}
			}

			var cons = bestConstructor;
			if (cons == null)
				throw new Exception(
					$"Cannot create {type.Name} via automatic reflection with Dependency Injection. There isn't a single constructor found.");
			var parameters = cons.GetParameters();
			var values = new object[parameters.Length];
			for (var i = 0; i < parameters.Length; i++)
			{
				values[i] = provider.GetService(parameters[i].ParameterType);
			}

			var instance = cons?.Invoke(values);
			return instance;
		}

		public IDependencyProviderScope Build(BuildOptions options = null)
		{
			return new DependencyProvider(this, options);
		}

		public IDependencyBuilder RemoveIfExists<T>() => Has<T>() ? Remove<T>() : this;

		public IDependencyBuilder Remove<T>()
		{
			if (TryGetTransient(typeof(T), out var transient))
			{
				TransientServices.Remove(transient);
				return this;
			}

			if (TryGetScoped(typeof(T), out var scoped))
			{
				ScopedServices.Remove(scoped);
				return this;
			}

			if (TryGetSingleton(typeof(T), out var singleton))
			{
				SingletonServices.Remove(singleton);
				return this;
			}

			throw new Exception($"Service does not exist, so cannot be removed. type=[{typeof(T)}]");
		}

		public bool Has<T>()
		{
			return TryGetTransient(typeof(T), out _) || TryGetScoped(typeof(T), out _) || TryGetSingleton(typeof(T), out _);
		}

		public bool TryGetTransient(Type type, out ServiceDescriptor descriptor)
		{
			foreach (var serviceDescriptor in TransientServices)
			{
				if (serviceDescriptor.Interface == type)
				{
					descriptor = serviceDescriptor;
					return true;
				}
			}

			descriptor = default(ServiceDescriptor);
			return false;
		}
		public bool TryGetScoped(Type type, out ServiceDescriptor descriptor)
		{
			foreach (var serviceDescriptor in ScopedServices)
			{
				if (serviceDescriptor.Interface == type)
				{
					descriptor = serviceDescriptor;
					return true;
				}
			}

			descriptor = default(ServiceDescriptor);
			return false;
		}

		public bool TryGetSingleton(Type type, out ServiceDescriptor descriptor)
		{
			foreach (var serviceDescriptor in SingletonServices)
			{
				if (serviceDescriptor.Interface == type)
				{
					descriptor = serviceDescriptor;
					return true;
				}
			}

			descriptor = default(ServiceDescriptor);
			return false;
		}


		public IDependencyBuilder Clone()
		{
			return new DependencyBuilder
			{
				ScopedServices = new List<ServiceDescriptor>(ScopedServices),
				TransientServices = new List<ServiceDescriptor>(TransientServices),
				SingletonServices = new List<ServiceDescriptor>(SingletonServices)
			};
		}
	}

}
