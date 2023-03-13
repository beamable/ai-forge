using Beamable.Common;
using NUnit.Framework;

namespace Beamable.Tests.Runtime.Environment.PackageVersionTests
{
	public class FromSemanticVersionStringTests
	{

		[Test]
		public void CanParse_Prod()
		{
			var version = PackageVersion.FromSemanticVersionString("1.17.1");
			Assert.AreEqual(1, version.Major);
			Assert.AreEqual(17, version.Minor);
			Assert.AreEqual(1, version.Patch);
			Assert.AreEqual(0, version.RC);
			Assert.AreEqual(0, version.NightlyTime);
			Assert.AreEqual(false, version.IsNightly);
			Assert.AreEqual(false, version.IsReleaseCandidate);
			Assert.AreEqual(false, version.IsExperimental);
			Assert.AreEqual(false, version.IsPreview);
		}

		[Test]
		public void CanParse_Prod_WithMajor()
		{
			var version = PackageVersion.FromSemanticVersionString("1.17.1");
			Assert.AreEqual(1, version.Major);
			Assert.AreEqual(17, version.Minor);
			Assert.AreEqual(1, version.Patch);
			Assert.AreEqual(0, version.RC);
			Assert.AreEqual(0, version.NightlyTime);
			Assert.AreEqual(false, version.IsNightly);
			Assert.AreEqual(false, version.IsReleaseCandidate);
			Assert.AreEqual(false, version.IsExperimental);
			Assert.AreEqual(false, version.IsPreview);
		}

		[Test]
		public void CanParse_Prod_WithZeroMinor()
		{
			var version = PackageVersion.FromSemanticVersionString("1.0.0");
			Assert.AreEqual(1, version.Major);
			Assert.AreEqual(0, version.Minor);
			Assert.AreEqual(0, version.Patch);
			Assert.AreEqual(0, version.RC);
			Assert.AreEqual(0, version.NightlyTime);
			Assert.AreEqual(false, version.IsNightly);
			Assert.AreEqual(false, version.IsReleaseCandidate);
			Assert.AreEqual(false, version.IsExperimental);
			Assert.AreEqual(false, version.IsPreview);
		}

		[Test]
		public void CanParse_RC()
		{
			var version = PackageVersion.FromSemanticVersionString("1.16.0-PREVIEW.RC3");
			Assert.AreEqual(1, version.Major);
			Assert.AreEqual(16, version.Minor);
			Assert.AreEqual(0, version.Patch);
			Assert.AreEqual(3, version.RC);
			Assert.AreEqual(0, version.NightlyTime);
			Assert.AreEqual(false, version.IsNightly);
			Assert.AreEqual(true, version.IsReleaseCandidate);
			Assert.AreEqual(false, version.IsExperimental);
			Assert.AreEqual(true, version.IsPreview);
		}

		[Test]
		public void CanParse_RC2()
		{
			var version = PackageVersion.FromSemanticVersionString("1.16.3-PREVIEW.RC19");
			Assert.AreEqual(1, version.Major);
			Assert.AreEqual(16, version.Minor);
			Assert.AreEqual(3, version.Patch);
			Assert.AreEqual(19, version.RC);
			Assert.AreEqual(0, version.NightlyTime);
			Assert.AreEqual(false, version.IsNightly);
			Assert.AreEqual(true, version.IsReleaseCandidate);
			Assert.AreEqual(false, version.IsExperimental);
			Assert.AreEqual(true, version.IsPreview);
		}

		[Test]
		public void CanParse_Nightly()
		{
			var version = PackageVersion.FromSemanticVersionString("1.0.0-PREVIEW.NIGHTLY-202101081800");
			Assert.AreEqual(1, version.Major);
			Assert.AreEqual(0, version.Minor);
			Assert.AreEqual(0, version.Patch);
			Assert.AreEqual(0, version.RC);
			Assert.AreEqual(202101081800, version.NightlyTime);
			Assert.AreEqual(true, version.IsNightly);
			Assert.AreEqual(false, version.IsReleaseCandidate);
			Assert.AreEqual(false, version.IsExperimental);
			Assert.AreEqual(true, version.IsPreview);
		}

		[Test]
		public void CanParse_Exp_Implicit_0Major()
		{
			var version = PackageVersion.FromSemanticVersionString("0.1.2");
			Assert.AreEqual(0, version.Major);
			Assert.AreEqual(1, version.Minor);
			Assert.AreEqual(2, version.Patch);
			Assert.AreEqual(0, version.RC);
			Assert.AreEqual(0, version.NightlyTime);
			Assert.AreEqual(false, version.IsNightly);
			Assert.AreEqual(false, version.IsReleaseCandidate);
			Assert.AreEqual(true, version.IsExperimental);
			Assert.AreEqual(false, version.IsPreview);
		}

		[Test]
		public void CanParse_Exp_Explicit()
		{
			var version = PackageVersion.FromSemanticVersionString("1.1.2-exp.12");
			Assert.AreEqual(1, version.Major);
			Assert.AreEqual(1, version.Minor);
			Assert.AreEqual(2, version.Patch);
			Assert.AreEqual(0, version.RC);
			Assert.AreEqual(0, version.NightlyTime);
			Assert.AreEqual(false, version.IsNightly);
			Assert.AreEqual(false, version.IsReleaseCandidate);
			Assert.AreEqual(false, version.IsPreview);
			Assert.AreEqual(true, version.IsExperimental);

		}

		[Test]
		public void CanParse_Preview()
		{
			var version = PackageVersion.FromSemanticVersionString("1.1.2-pre");
			Assert.AreEqual(1, version.Major);
			Assert.AreEqual(1, version.Minor);
			Assert.AreEqual(2, version.Patch);
			Assert.AreEqual(0, version.RC);
			Assert.AreEqual(0, version.NightlyTime);
			Assert.AreEqual(false, version.IsNightly);
			Assert.AreEqual(false, version.IsReleaseCandidate);
			Assert.AreEqual(false, version.IsExperimental);
			Assert.AreEqual(true, version.IsPreview);
		}


		[Test]
		public void CanParse_Preview_WithSuffix()
		{
			var version = PackageVersion.FromSemanticVersionString("1.1.2-pre.32");
			Assert.AreEqual(1, version.Major);
			Assert.AreEqual(1, version.Minor);
			Assert.AreEqual(2, version.Patch);
			Assert.AreEqual(0, version.RC);
			Assert.AreEqual(0, version.NightlyTime);
			Assert.AreEqual(false, version.IsNightly);
			Assert.AreEqual(false, version.IsReleaseCandidate);
			Assert.AreEqual(false, version.IsExperimental);
			Assert.AreEqual(true, version.IsPreview);
		}
	}
}
