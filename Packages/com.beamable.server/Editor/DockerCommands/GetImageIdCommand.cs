using System;

namespace Beamable.Server.Editor.DockerCommands
{
	public class GetImageDetailsCommand : DockerCommandReturnable<ImageDetails>
	{
		public string ImageName { get; }
		private bool WasBuildLocally { get; }

		private const char SEPARATOR = ',';
		private const char ID_SEPARATOR = ':';

		public GetImageDetailsCommand(IDescriptor descriptor)
		{
			ImageName = descriptor.ImageName;
			WasBuildLocally = BuildImageCommand.WasEverBuildLocally(descriptor);
		}
		public override string GetCommandString()
		{
			var formatString = "--format={{.Id}}" + SEPARATOR + "{{.Architecture}}" + SEPARATOR + "{{.Os}}"; // string interpolation with {{}} is more confusing than string concat.
			return $"{DockerCmd} inspect {formatString} {ImageName}";
		}

		protected override void Resolve()
		{
			var results = new ImageDetails();
			results.imageExists = WasBuildLocally && StandardOutBuffer?.Length > 0;
			if (results.imageExists)
			{
				// parse the id and the arch. There are two parts, separated
				var parts = StandardOutBuffer.Split(SEPARATOR);
				if (parts.Length != 3) throw new MicroserviceImageInfoException($"failed to parse. Incorrect number of base components.", StandardErrorBuffer);

				results.cpuArch = parts[1]; // the arch will be in the form of, "amd64", which we can take as is.
				var longId = parts[0]; // the id will be in the form of sha256:5f5739675ebc83134ab93353a14aeb2bed6283d93f7944f094dfb3168ff8ed42
				results.os = parts[2]; // the os will be in the form of "linux:, which we can take as is.

				// we need to strip out the sha256 image id part, but it may not always be sha256...
				// and shorten it to the short "5f5739675ebc" variant.
				var idParts = longId.Split(ID_SEPARATOR);
				if (idParts?.Length != 2) throw new MicroserviceImageInfoException($"failed to parse. Incorrect number of id components.", StandardErrorBuffer);
				results.imageId = idParts[1].Substring(0, 12); // take the first 12 digits.
			}

			// there is no built image, we shouldn't log an error, we should just know that empty string means "not built".
			Promise.CompleteSuccess(results);
		}
	}

	public class ImageDetails
	{
		public bool imageExists;
		public string imageId;
		public string cpuArch;
		public string os;

		public string Platform => $"{os}/{cpuArch}";
	}

	public class MicroserviceImageInfoException : Exception
	{
		public MicroserviceImageInfoException(string msg, string standardOut) : base($"{msg}. stdout=[{standardOut}]") { }
	}
}
