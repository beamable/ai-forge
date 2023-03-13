using System;

namespace Beamable.Common
{
	public static partial class Constants
	{
		public static partial class Features
		{
			public static partial class Docker
			{
				[Obsolete]
				public const string CPU_LINUX_ARM_64 = "linux/arm64";
				public const string CPU_LINUX_AMD_64 = "linux/amd64";

				public static readonly string[] CPU_SUPPORTED = new string[] { CPU_LINUX_AMD_64 };
			}
		}
	}
}
