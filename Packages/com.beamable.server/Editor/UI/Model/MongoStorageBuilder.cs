using Beamable.Server.Editor;
using Beamable.Server.Editor.DockerCommands;
using System;
using System.Threading.Tasks;

namespace Beamable.Editor.UI.Model
{
	[Serializable]
	public class MongoStorageBuilder : ServiceBuilderBase
	{
		public void ForwardEventsTo(MongoStorageBuilder oldBuilder)
		{
			if (oldBuilder == null) return;
			OnIsRunningChanged += oldBuilder.OnIsRunningChanged;
		}

		protected override async Task<RunImageCommand> PrepareRunCommand()
		{
			await Task.Delay(0);
			return new RunStorageCommand((StorageObjectDescriptor)Descriptor);
		}
	}
}
