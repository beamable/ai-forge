using Beamable.Common.Dependencies;
using System;

namespace Beamable.Server
{
	/// <summary>
	/// When creating a IServiceInitializer remember to inject a <see cref="RequestContext"/> using it's parameterless constructor.
	/// This is used to notify users they must call <see cref="Microservice.AssumeUser"/> before attempting to use certain user-specific routes. 
	/// </summary>
	public interface IServiceInitializer : IServiceProvider
	{
		/// <summary>
		/// Gets a service registered during <see cref="BeamableMicroService.InitServices"/>.
		/// If you intend to cache data on the service, you must use a singleton service for that.
		/// This method can be used to explicitly guard against mistakenly getting a non-singleton service for cache-ing purposes.
		/// <para/>
		/// Functionally, there is no difference between this and <see cref="GetService{TService}"/>. There's just a guard baked in that you can use to declare that a specific service is being used as a Cache
		/// and, therefore, must have been added via <see cref="IServiceBuilder.AddSingleton{T}(Beamable.Server.ServiceFactory{T})"/> or <see cref="IServiceBuilder.AddSingleton{T}()"/>.
		/// </summary>
		TService GetServiceAsCache<TService>()
			where TService : class;

		/// <summary>
		/// Gets a service registered during <see cref="BeamableMicroService.InitServices"/>.
		/// This assumes nothing about what you are using the service for and only guards against attempting to get a non-existent service. 
		/// </summary>
		TService GetService<TService>()
			where TService : class;

		/// <summary>
		/// Access the <see cref="IDependencyProvider"/> for the root scope of the service.
		/// </summary>
		IDependencyProvider Provider { get; }
	}
}
