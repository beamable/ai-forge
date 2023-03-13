using Beamable.Editor.UI.Model;
using Beamable.Server.Editor.DockerCommands;
using System.Linq;
using UnityEditor;
using static Beamable.Common.Constants.Features.Services;

namespace Beamable.Server.Editor
{
	[InitializeOnLoadAttribute]
	public class MicroservicePrefixSetter
	{
		// register an event handler when the class is initialized
		static MicroservicePrefixSetter()
		{
			EditorApplication.playModeStateChanged += LogPlayModeState;
		}

		private static async void LogPlayModeState(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
			{
				return;
			}

			if (DockerCommand.DockerNotInstalled) return;

			try
			{
				var microserviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
				foreach (var service in microserviceRegistry.Descriptors.ToList())
				{
					var command = new CheckImageCommand(service)
					{
						WriteLogToUnity = false
					};
					var isRunning = await command.Start();
					if (isRunning)
					{
						MicroserviceIndividualization.UseServicePrefix(service.Name);
					}
					else
					{
						if (state == PlayModeStateChange.EnteredPlayMode)
						{
							MicroserviceLogHelper.HandleLog(service, LogLevel.INFO, USING_REMOTE_SERVICE_MESSAGE,
								MicroserviceConfiguration.Instance.LogWarningLabelColor, true, "remote_icon");
						}

						MicroserviceIndividualization.ClearServicePrefix(service.Name);
					}
				}
			}
			catch (DockerNotInstalledException)
			{
				// purposefully do nothing... If docker isn't installed; do nothing...
			}
		}
	}
}
