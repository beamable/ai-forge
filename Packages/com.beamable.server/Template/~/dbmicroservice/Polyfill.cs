
using System;
namespace Beamable.Server
{
	[AttributeUsage(AttributeTargets.Class)]
	public class MicroserviceAttribute : Attribute
	{
		public string MicroserviceName { get; }
		public string SourcePath { get; }

		public MicroserviceAttribute(string microserviceName, string sourcePath = "polyfill")
		{
			MicroserviceName = microserviceName;
			SourcePath = sourcePath;
		}
	}
}
