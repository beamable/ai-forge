using Beamable.Common;
using System;

namespace Beamable.Server.Editor.DockerCommands
{
	public class SaveImageCommand : DockerCommandReturnable<Unit>
	{
		public MicroserviceDescriptor Descriptor { get; }
		public string ImageId { get; }
		public string OutputPath { get; }


		public SaveImageCommand(MicroserviceDescriptor descriptor, string imageId, string outputPath)
		{
			Descriptor = descriptor;
			ImageId = imageId;
			OutputPath = outputPath;
		}
		public override string GetCommandString()
		{
			return $"{DockerCmd} image save -o {OutputPath} {ImageId}";
		}

		protected override void Resolve()
		{
			if (string.IsNullOrEmpty(StandardErrorBuffer))
			{
				Promise.CompleteSuccess(PromiseBase.Unit);
			}
			else
			{
				Promise.CompleteError(new Exception($"Failed to save image. id=[{ImageId}] error=[{StandardErrorBuffer}]"));
			}
		}

	}
}
