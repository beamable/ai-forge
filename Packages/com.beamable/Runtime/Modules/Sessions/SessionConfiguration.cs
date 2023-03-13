using Beamable.Api.Sessions;
using UnityEngine;

namespace Beamable.Sessions
{
	public class SessionConfiguration : ModuleConfigurationObject
	{
		public static SessionConfiguration Instance => Get<SessionConfiguration>();

		[Tooltip("If you need to track custom parameters per user session, create an instance of the Session Parameter Provider, and link it here.")]
		public SessionParameterProvider CustomParameterProvider;

		public SessionDeviceOptions DeviceOptions;
	}

}
