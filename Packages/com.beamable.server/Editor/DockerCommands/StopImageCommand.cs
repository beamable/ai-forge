using Beamable.Common;

namespace Beamable.Server.Editor.DockerCommands
{
	public class StopImageReturnableCommand : DockerCommandReturnable<bool>
	{
		public string ContainerName { get; set; }

		public StopImageReturnableCommand(IDescriptor descriptor, bool skipDockerCheck = false) : this(
			descriptor.ContainerName, skipDockerCheck)
		{

		}

		public StopImageReturnableCommand(string containerName, bool skipDockerCheck = false)
		{
			ContainerName = containerName;
			UnityLogLabel = "STOP";
			WriteCommandToUnity = false;
			WriteLogToUnity = false;
			_skipDockerCheck = skipDockerCheck;
		}

		public override string GetCommandString()
		{
			var command = $"{DockerCmd} stop {ContainerName}";
			return command;
		}

		protected override void Resolve()
		{
			Promise?.CompleteSuccess(true);
		}
	}
	public class StopImageCommand : StopImageReturnableCommand
	{
		public StopImageCommand(IDescriptor descriptor, bool skipDockerCheck = false) : base(descriptor, skipDockerCheck)
		{
		}
		public StopImageCommand(string containerName, bool skipDockerCheck = false) : base(containerName, skipDockerCheck)
		{
		}

		public new Promise<bool> Start()
		{
			return StartAsync();
		}
	}
}
