using Beamable.Server.Editor;
using Beamable.Server.Editor.ManagerClient;
using System.Threading.Tasks;

namespace Beamable.Editor.UI.Model
{
	public interface IBeamableMicroservice : IBeamableService
	{
		MicroserviceDescriptor ServiceDescriptor { get; }
		MicroserviceBuilder ServiceBuilder { get; }
		ServiceStatus RemoteStatus { get; }
		MicroserviceConfigurationEntry Config { get; }

		Task Build();
		Task BuildAndStart();
		Task BuildAndRestart();
		void EnrichWithRemoteReference(ServiceReference remoteReference);
		void EnrichWithStatus(ServiceStatus status);
	}
}
