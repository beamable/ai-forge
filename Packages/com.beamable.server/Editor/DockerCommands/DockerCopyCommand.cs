namespace Beamable.Server.Editor.DockerCommands
{
	public class DockerCopyCommand : DockerCommandReturnable<bool>
	{
		private readonly IDescriptor _descriptor;
		private readonly string _containerPath;
		private readonly string _host;
		private readonly CopyType _type;

		public enum CopyType
		{
			CONTAINER_TO_HOST,
			HOST_TO_CONTAINER
		}

		public DockerCopyCommand(IDescriptor descriptor, string containerPath, string host, CopyType direction = CopyType.CONTAINER_TO_HOST)
		{
			_descriptor = descriptor;
			_containerPath = containerPath;
			_host = host;
			_type = direction;
			WriteCommandToUnity = true;
			WriteLogToUnity = true;
		}

		private string GetCopyStr()
		{
			var containerPart = $"{_descriptor.ContainerName}:{_containerPath}";
			switch (_type)
			{
				case CopyType.CONTAINER_TO_HOST: return $"{containerPart} {_host}";
				case CopyType.HOST_TO_CONTAINER: return $"{_host} {containerPart}";
				default: return "";
			}
		}

		public override string GetCommandString()
		{
			return $"{DockerCmd} cp {GetCopyStr()}";
		}

		protected override void Resolve()
		{
			Promise.CompleteSuccess(true);
		}
	}
}
