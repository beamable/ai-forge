using Beamable.Common.Spew;
using System.Diagnostics;

namespace Beamable.Service
{
	[SpewLogger]
	public static class ServicesLogger
	{
		[Conditional("SPEW_ALL"), Conditional("SPEW_SERVICES")]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional("SPEW_ALL"), Conditional("SPEW_SERVICES")]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}
}
