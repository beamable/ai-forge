namespace Beamable.Server.Editor.DockerCommands
{
	public class DockerPortResult
	{
		public bool ContainerExists;
		public string LocalFullAddress;
		public string LocalPort;
	}

	public class DockerPortCommand : DockerCommandReturnable<DockerPortResult>
	{
		private readonly IDescriptor _descriptor;
		private readonly int _containerPort;

		public DockerPortCommand(IDescriptor descriptor, int containerPort)
		{
			_descriptor = descriptor;
			_containerPort = containerPort;
		}

		public override string GetCommandString()
		{
			return $"{DockerCmd} port {_descriptor.ContainerName} {_containerPort}";
		}

		protected override void Resolve()
		{
			if (StandardErrorBuffer != null && StandardErrorBuffer.Contains("Error"))
			{
				Promise.CompleteSuccess(new DockerPortResult
				{
					ContainerExists = false
				});
			}
			else
			{
				var fullAddr = StandardOutBuffer.Trim().Split(':');

				var addr = fullAddr[0];
				var port = fullAddr[1];

				addr = addr == "0.0.0.0" ? "localhost" : addr;

				Promise.CompleteSuccess(new DockerPortResult
				{
					ContainerExists = true,
					LocalFullAddress = $"{addr}:{port}",
					LocalPort = port
				});
			}

		}
	}
}
