using Beamable.Common.Dependencies;
using Beamable.Server;

namespace Beamable
{
	[BeamContextSystem]
	public static class BeamContextExtensions
	{
		[RegisterBeamableDependencies]
		public static void RegisterServices(IDependencyBuilder builder)
		{
			builder.AddScoped<MicroserviceClients>();
		}

		public static MicroserviceClients Microservices(this BeamContext ctx) =>
			ctx.ServiceProvider.GetService<MicroserviceClients>();
	}

}
