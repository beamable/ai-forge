using Beamable.Common;
using NUnit.Framework;

namespace Beamable.Tests.Runtime.Environment.PackageVersionTests
{
	public class ToStringTests
	{
		[Test]
		public void SelfSimilar_Prod()
		{
			var str = "0.15.3";
			var version = PackageVersion.FromSemanticVersionString(str);
			Assert.AreEqual(str, version.ToString());
		}

		[Test]
		public void SelfSimilar_RC()
		{
			var str = "0.15.3-PREVIEW.RC4";
			var version = PackageVersion.FromSemanticVersionString(str);
			Assert.AreEqual(str, version.ToString());
		}

		[Test]
		public void SelfSimilar_Nightly()
		{
			var str = "0.15.3-PREVIEW.NIGHTLY-1234567";
			var version = PackageVersion.FromSemanticVersionString(str);
			Assert.AreEqual(str, version.ToString());
		}

		[Test]
		public void ProdString()
		{
			var version = new PackageVersion(0, 15, 3);
			var str = version.ToString();
			Assert.AreEqual("0.15.3", str);
		}

		[Test]
		public void ProdStringWithNoPatch()
		{
			var version = new PackageVersion(1, 15, 0);
			var str = version.ToString();
			Assert.AreEqual("1.15.0", str);
		}

		[Test]
		public void RCString()
		{
			var version = new PackageVersion(0, 15, 3, rc: 3, isPreview: true);
			var str = version.ToString();
			Assert.AreEqual("0.15.3-PREVIEW.RC3", str);
		}

		[Test]
		public void NightlyString()
		{
			var version = new PackageVersion(0, 15, 3, nightlyTime: 12345677, isPreview: true);
			var str = version.ToString();
			Assert.AreEqual("0.15.3-PREVIEW.NIGHTLY-12345677", str);
		}
	}
}
