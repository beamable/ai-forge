using System;
using UnityEngine.Scripting;

namespace Beamable.ConsoleCommands
{
	[AttributeUsage(AttributeTargets.Method)]
	public class BeamableConsoleCommandAttribute : PreserveAttribute
	{
		public string[] Names { get; }
		public string Description { get; }
		public string Usage { get; }

		public BeamableConsoleCommandAttribute(string name, string description, string usage)
		{
			Names = new[] { name.ToUpperInvariant() };
			Description = description;
			Usage = usage;
		}

		public BeamableConsoleCommandAttribute(string[] names, string description, string usage)
		{
			Names = Array.ConvertAll(names, x => x.ToUpperInvariant());
			Description = description;
			Usage = usage;
		}
	}
}
