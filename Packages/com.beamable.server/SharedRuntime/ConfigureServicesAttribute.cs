using System;

namespace Beamable.Server
{
	/// <summary>
	/// This attribute allows a Microservice to configure custom dependencies when the service starts up.
	/// To use this, you must have a public, static, void, method in a Microservice that takes exactly one
	/// argument of a <see cref="IServiceBuilder"/> instance.
	///
	/// <code>
	/// [ConfigureServices]
	/// public static void Configure(IServiceBuilder builder) {
	///    // register custom services
	/// }
	/// </code>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class ConfigureServicesAttribute : Attribute
	{
		public int ExecutionOrder;

		public ConfigureServicesAttribute(int executionOrder = 0)
		{
			ExecutionOrder = executionOrder;
		}

	}
}
