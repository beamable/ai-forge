using System;

namespace Beamable.Server
{
	/// <summary>
	/// The <see cref="InitializeServicesAttribute"/> allows a Microservice to run custom
	/// logic after the root scope has been built, but before the service is allowed to accept
	/// any HTTP traffic. To use this, there must be a public static void method in a Microservice class that
	/// takes exactly one argument of the <see cref="IServiceInitializer"/> type.
	///
	/// The return type may also be <see cref="Promise"/>, or async <see cref="Promise"/>
	///
	/// <code>
	/// [InitializeServices]
	/// public static void Initialize(IServiceInitializer provider) {
	///    // run custom logic
	/// }
	/// </code>
	/// <para>
	/// This method only runs once per service, and the given service scope is the root scope.
	/// Every request to the service will generate a sub scope.
	/// </para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class InitializeServicesAttribute : Attribute
	{
		public int ExecutionOrder;

		public InitializeServicesAttribute(int executionOrder = 0)
		{
			ExecutionOrder = executionOrder;
		}
	}
}
