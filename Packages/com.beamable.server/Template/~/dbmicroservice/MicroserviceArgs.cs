using System;

namespace Beamable.Server
{
	public interface IMicroserviceArgs
	{
		string CustomerID { get; }
		string ProjectName { get; }
		string Host { get; }
		string Secret { get; }
	}

	public class MicroserviceArgs : IMicroserviceArgs
	{
		public string CustomerID { get; set; }
		public string ProjectName { get; set; }
		public string Secret { get; set; }
		public string Host { get; set; }
	}

	public static class MicroserviceArgsExtensions
	{
		public static IMicroserviceArgs Copy(this IMicroserviceArgs args)
		{
			return new MicroserviceArgs
			{
				CustomerID = args.CustomerID,
				ProjectName = args.ProjectName,
				Secret = args.Secret,
				Host = args.Host
			};
		}
	}

	public class EnviornmentArgs : IMicroserviceArgs
	{
		public string CustomerID => Environment.GetEnvironmentVariable("CID");
		public string ProjectName => Environment.GetEnvironmentVariable("PID");
		public string Host => Environment.GetEnvironmentVariable("HOST");
		public string Secret => Environment.GetEnvironmentVariable("SECRET");
	}
}
