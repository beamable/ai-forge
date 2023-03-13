namespace Beamable.Server.Editor.DockerCommands
{
	public class CheckDockerCommand : DockerCommandReturnable<bool>
	{
		public override bool DockerRequired => false;

		public override string GetCommandString()
		{
			ClearDockerInstallFlag();
			var command = $"{DockerCmd} --version";
			return command;
		}

		protected override void Resolve()
		{
			var isInstalled = _exitCode == 0;
			DockerNotInstalled = !isInstalled;
			Promise.CompleteSuccess(isInstalled);
		}
	}
}
