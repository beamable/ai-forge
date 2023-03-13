using System;

namespace Beamable.Server.Editor.DockerCommands
{
	public class SnykTestCommand : DockerCommandReturnable<SnykTestCommand.Result>
	{
		private readonly MicroserviceDescriptor _descriptor;

		private Result _result = new Result();

		public SnykTestCommand(MicroserviceDescriptor descriptor)
		{
			UnityLogLabel = "SNYK";
			_descriptor = descriptor;
		}

		public override string GetCommandString()
		{
			return $"{DockerCmd} scan --accept-license {_descriptor.ImageName}";
		}

		protected override void HandleStandardOut(string data)
		{
			CheckLoginRequirement(data);
			_result.Output += data + Environment.NewLine;
			base.HandleStandardOut(data);
		}

		protected override void HandleStandardErr(string data)
		{
			CheckLoginRequirement(data);
			base.HandleStandardErr(data);
		}

		void CheckLoginRequirement(string data)
		{
			if (data?.Contains("failed to get DockerScanID") ?? false)
			{
				_result.RequiresLogin = true;
			}
		}

		protected override void Resolve()
		{
			Promise.CompleteSuccess(_result);
		}

		public class Result
		{
			public bool RequiresLogin { get; set; }
			public string Output { get; set; }
		}
	}
}
