namespace Beamable.Server
{
	public static class MicroserviceBootstrapper
	{
		public static void Start<TMicroService>() where TMicroService : MicroService, new()
		{
			var beamableService = new BeamableMicroService();
			var args = new EnviornmentArgs();

			var factory = new ServiceFactory<TMicroService>(() => new TMicroService());
			beamableService.Start(factory, args);
		}
	}
}
