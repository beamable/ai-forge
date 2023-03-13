using Beamable.Editor.Environment;
using Beamable.Editor.Microservice.UI;
using UnityEditor;

namespace Beamable.Server.Editor
{
	[InitializeOnLoad]
	public class PackageAvailability
	{
		static PackageAvailability()
		{

#if !BEAMABLE_LEGACY_MSW
			BeamablePackages.ProvideServerWindow(MicroserviceWindow.Init);
#else
         BeamablePackages.ProvideServerWindow(DebugWindow.Init);
#endif
		}
	}
}
