using Beamable.Common;
using UnityEngine;

using static Beamable.Common.Constants.Features.Docker;

namespace Beamable.Server.Editor.DockerCommands
{
	public class CheckImageReturnableCommand : DockerCommandReturnable<bool>
	{
		public string ContainerName { get; }
		public bool IsRunning { get; protected set; }

		public CheckImageReturnableCommand(IDescriptor descriptor)
		   : this(descriptor.ContainerName)
		{

		}

		public CheckImageReturnableCommand(string containerName)
		{
			ContainerName = containerName;
		}

		public override string GetCommandString()
		{
			var command = $"{DockerCmd} ps -f \"name={ContainerName}\"";
			return command;
		}

		protected override void HandleStandardOut(string data)
		{
			base.HandleStandardOut(data);

			// 7c7e95c20caf        tunafish            "dotnet tunafish.dll"   7 hours ago         Up 7 hours          0.0.0.0:56798->80/tcp   tunafishcontainer

			// TODO: We could use a better text matching system, but for now...
			// TODO: Support other languages
			if (data != null && data.Contains($" {ContainerName}") && data.Contains(" Up "))
			{
				IsRunning = true;
			}
		}

		protected override void Resolve()
		{
			Promise.CompleteSuccess(IsRunning);
		}
	}

	public class CheckImageCommand : CheckImageReturnableCommand
	{
		public CheckImageCommand(MicroserviceDescriptor descriptor) : base(descriptor)
		{
		}

		public new Promise<bool> Start()
		{
			return StartAsync();
		}
	}
}
