using Beamable.Common;
using Beamable.Server.Editor.CodeGen;
using UnityEngine;

namespace Beamable.Server.Editor.DockerCommands
{
	public class PullImageCommand : DockerCommandReturnable<bool>
	{
		private readonly string _imageAndTag;
		private readonly CPUArchitectureContext _cpu;

		public static PullImageCommand PullBeamService(CPUArchitectureContext cpu) =>
			new PullImageCommand($"{DockerfileGenerator.BASE_IMAGE}:{DockerfileGenerator.BASE_TAG}", cpu);

		public PullImageCommand(string imageAndTag, CPUArchitectureContext cpu = CPUArchitectureContext.DEFAULT)
		{
			_imageAndTag = imageAndTag;
			_cpu = cpu;
			WriteCommandToUnity = false;
			WriteLogToUnity = false;
		}

		public override string GetCommandString()
		{
			var platform = MicroserviceConfiguration.Instance.GetCPUArchitecture(_cpu);

			var platformStr = "";
#if !BEAMABLE_DISABLE_AMD_MICROSERVICE_BUILDS
			platformStr = $"--platform {platform}";
#endif

			return $"{DockerCmd} pull {platformStr} {_imageAndTag}";
		}

		protected override void Resolve()
		{
			Promise.CompleteSuccess(_exitCode == 0);
		}
	}
}
