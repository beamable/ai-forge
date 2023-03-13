using Beamable.InputManagerIntegration;
using UnityEngine;

namespace Beamable.Console
{
#if BEAMABLE_DEVELOPER
	[CreateAssetMenu]
#endif
	public class ConsoleConfiguration : ModuleConfigurationObject
	{
		public static ConsoleConfiguration Instance => Get<ConsoleConfiguration>();

#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
		public InputActionArg ToggleAction = new InputActionArg
		{
			KeyCode = KeyCode.BackQuote
		};
#elif ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
      public InputActionArg ToggleAction;
#endif

		[Tooltip("When true, anyone will be able to open the admin console, regardless of access role. Make sure to uncheck this box for production builds")]
		public bool ForceEnabled;
	}
}
