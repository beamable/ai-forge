namespace Beamable.Server.Editor.DockerCommands
{
	public class DockerLoginCommand : DockerCommandReturnable<bool>
	{
		private readonly string _userName;
		private readonly string _password;

		private bool _result;

		public DockerLoginCommand(string userName, string password)
		{
			_userName = userName;
			_password = password;
		}
		public override string GetCommandString()
		{
			return $"{DockerCmd} login --username {_userName} --password {_password}";
		}

		protected override void HandleStandardOut(string data) => HandleMessage(data);
		protected override void HandleStandardErr(string data) => HandleMessage(data);
		void HandleMessage(string data) => _result = data?.Contains("Login Succeeded") ?? false;

		protected override void Resolve()
		{
			Promise.CompleteSuccess(_result);
		}
	}
}
