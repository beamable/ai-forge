using Beamable.Api;
using Beamable.Editor;

namespace Beamable.Server.Editor.ManagerClient
{
	public static class BeamableExtensions
	{
		public static MicroserviceManager GetMicroserviceManager(this BeamEditorContext de)
		{
			return de.ServiceScope.GetService<MicroserviceManager>();
		}
	}
}
