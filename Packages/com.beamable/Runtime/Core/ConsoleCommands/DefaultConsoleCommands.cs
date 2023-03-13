using Beamable.Common.Api;
using Beamable.Common.Dependencies;
using UnityEngine.Scripting;

namespace Beamable.ConsoleCommands
{
	[BeamableConsoleCommandProvider]
	public class DefaultConsoleCommands
	{
		private readonly IDependencyProvider _provider;
		private BeamableConsole Console => _provider.GetService<BeamableConsole>();

		[Preserve]
		public DefaultConsoleCommands(IDependencyProvider provider)
		{
			_provider = provider;
		}

		[BeamableConsoleCommand("DataPath", "Print the data path for where Beamable caches data", "DataPath")]
		public string PrintDataPath(params string[] args)
		{
			var fs = _provider.GetService<IBeamableFilesystemAccessor>();
			return fs.GetPersistentDataPathWithoutTrailingSlash();
		}


		[BeamableConsoleCommand("ECHO", "Repeat message to console.", "ECHO <message>")]
		private string Echo(params string[] args)
		{
			return string.Join(" ", args);
		}

		[BeamableConsoleCommand(nameof(Where), "Find where a specific console command was registered from, if it was registered with a BeamableConsoleCommand attribute", "WHERE <command>")]
		private string Where(params string[] args)
		{
			if (args.Length == 0)
			{
				return Console.Help(nameof(Where));
			}

			return Console.Origin(args[0]);
		}

	}
}
