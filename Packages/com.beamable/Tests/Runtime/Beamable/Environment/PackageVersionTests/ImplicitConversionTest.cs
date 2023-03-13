using Beamable.Common;
using NUnit.Framework;

namespace Beamable.Tests.Runtime.Environment.PackageVersionTests
{
	public class ImplicitConversionTest
	{
		[Test]
		public void StringToVersion()
		{
			var versionStr = "1.0.1";
			var version = PackageVersion.FromSemanticVersionString("1.0.2");
			Assert.AreEqual(false, versionStr < version);
			Assert.AreEqual(false, version > versionStr);
		}
	}
}
