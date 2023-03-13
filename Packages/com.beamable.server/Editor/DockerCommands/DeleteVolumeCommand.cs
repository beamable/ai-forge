using System.Collections.Generic;
using System.Linq;

namespace Beamable.Server.Editor.DockerCommands
{
	public class DeleteVolumeCommand : DockerCommandReturnable<Dictionary<string, bool>>
	{
		public string[] Volumes { get; }

		public Dictionary<string, bool> Results;

		public DeleteVolumeCommand(params string[] volumes)
		{
			Volumes = volumes;
			Results = Volumes.ToDictionary(v => v, v => false);
		}

		public DeleteVolumeCommand(StorageObjectDescriptor storage) : this(storage.DataVolume, storage.FilesVolume)
		{

		}

		public override string GetCommandString()
		{
			return $"{DockerCmd} volume rm {string.Join(" ", Volumes)}";
		}

		protected override void HandleStandardErr(string data) => HandleMessage(data);
		protected override void HandleStandardOut(string data) => HandleMessage(data);

		private void HandleMessage(string msg)
		{
			if (msg == null) return;
			msg = msg.Trim();

			if (Results.ContainsKey(msg)) Results[msg] = true;
		}

		protected override void Resolve()
		{
			Promise.CompleteSuccess(Results);
		}
	}
}
