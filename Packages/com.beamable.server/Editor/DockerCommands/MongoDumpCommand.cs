namespace Beamable.Server.Editor.DockerCommands
{
	public class MongoDumpCommand : DockerCommandReturnable<bool>
	{
		private readonly StorageObjectDescriptor _storage;

		public MongoDumpCommand(StorageObjectDescriptor storage)
		{
			_storage = storage;
			WriteCommandToUnity = true;
			WriteLogToUnity = true;
		}

		public override string GetCommandString()
		{
			var config = MicroserviceConfiguration.Instance.GetStorageEntry(_storage.Name);
			return $"{DockerCmd} exec {_storage.ContainerName} mongodump --out=/beamable -u {config.LocalInitUser} -p {config.LocalInitPass}";
		}

		protected override void Resolve()
		{
			Promise.CompleteSuccess(true);
		}
	}
}
