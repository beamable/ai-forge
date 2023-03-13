using System;

namespace Beamable.Common.Dependencies
{
	/// <summary>
	/// Any service that is in a <see cref="IDependencyProvider"/> can have this interface.
	/// When the <see cref="IDependencyProvider"/> is disposed via the <see cref="IDependencyProviderScope.Dispose"/> method,
	/// this service's <see cref="IBeamableDisposable.OnDispose"/> method will trigger.
	/// </summary>
	public interface IBeamableDisposable
	{
		/// <summary>
		/// Used as a way to allow services attached to a <see cref="IDependencyProvider"/> to clean up their state when
		/// the <see cref="IDependencyProviderScope.Dispose"/> method is called
		/// </summary>
		/// <returns>A promise that represents when the service's clean up is complete. This promise will hang the entire disposal process of the <see cref="IDependencyProviderScope"/></returns>
		Promise OnDispose();
	}

	/// <summary>
	/// Describes how to create a service instance
	/// </summary>
	public class ServiceDescriptor
	{
		public Type Interface, Implementation;
		public Func<IDependencyProvider, object> Factory;
	}

}
