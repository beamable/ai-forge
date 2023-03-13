using Beamable.Common;
using NUnit.Framework;

namespace Beamable.Tests.Runtime.Environment.PackageVersionTests
{
	public class RangeOperatorTests
	{
		[Test]
		public void AreEqual()
		{
			var a = PackageVersion.FromSemanticVersionString("1.3.2");
			var b = PackageVersion.FromSemanticVersionString("1.3.2");

			Assert.AreEqual(true, a == b);
		}

		[Test]
		public void AreNotEqual()
		{
			var a = PackageVersion.FromSemanticVersionString("1.0.0");
			var b = PackageVersion.FromSemanticVersionString("1.3.0");

			Assert.AreEqual(true, a != b);
		}

		[Test]
		public void MajorMoreThan()
		{
			var a = PackageVersion.FromSemanticVersionString("1.0.0");
			var b = PackageVersion.FromSemanticVersionString("2.0.0");

			Assert.AreEqual(false, a > b);
			Assert.AreEqual(true, b > a);
			Assert.AreEqual(false, a >= b);
			Assert.AreEqual(true, b >= a);
		}

		[Test]
		public void MinorMoreThan()
		{
			var a = PackageVersion.FromSemanticVersionString("2.4.0");
			var b = PackageVersion.FromSemanticVersionString("2.18.0");

			Assert.AreEqual(false, a > b);
			Assert.AreEqual(true, b > a);
			Assert.AreEqual(false, a >= b);
			Assert.AreEqual(true, b >= a);
		}

		[Test]
		public void PatchMoreThan()
		{
			var a = PackageVersion.FromSemanticVersionString("2.4.2");
			var b = PackageVersion.FromSemanticVersionString("2.4.53");

			Assert.AreEqual(false, a > b);
			Assert.AreEqual(true, b > a);
			Assert.AreEqual(false, a >= b);
			Assert.AreEqual(true, b >= a);
		}

		[Test]
		public void MinorMoreThan_StoppedByMajor()
		{
			var a = PackageVersion.FromSemanticVersionString("2.4.0");
			var b = PackageVersion.FromSemanticVersionString("3.1.0");

			Assert.AreEqual(false, a > b);
			Assert.AreEqual(true, b > a);
			Assert.AreEqual(false, a >= b);
			Assert.AreEqual(true, b >= a);
		}

		[Test]
		public void PatchMoreThan_StoppedByMajor()
		{
			var a = PackageVersion.FromSemanticVersionString("2.1.4");
			var b = PackageVersion.FromSemanticVersionString("3.1.2");
			Assert.AreEqual(false, a > b);
			Assert.AreEqual(true, b > a);
			Assert.AreEqual(false, a >= b);
			Assert.AreEqual(true, b >= a);
		}

		[Test]
		public void PatchMoreThan_StoppedByMinor()
		{
			var a = PackageVersion.FromSemanticVersionString("2.1.4");
			var b = PackageVersion.FromSemanticVersionString("2.3.2");

			Assert.AreEqual(false, a > b);
			Assert.AreEqual(true, b > a);
			Assert.AreEqual(false, a >= b);
			Assert.AreEqual(true, b >= a);
		}

		[Test]
		public void MajorLessThan()
		{
			var a = PackageVersion.FromSemanticVersionString("1.0.0");
			var b = PackageVersion.FromSemanticVersionString("2.0.0");

			Assert.AreEqual(true, a < b);
			Assert.AreEqual(false, b < a);
			Assert.AreEqual(true, a <= b);
			Assert.AreEqual(false, b <= a);
		}

		[Test]
		public void MinorLessThan()
		{
			var a = PackageVersion.FromSemanticVersionString("2.4.0");
			var b = PackageVersion.FromSemanticVersionString("2.18.0");

			Assert.AreEqual(true, a < b);
			Assert.AreEqual(false, b < a);
			Assert.AreEqual(true, a <= b);
			Assert.AreEqual(false, b <= a);
		}

		[Test]
		public void PatchLessThan()
		{
			var a = PackageVersion.FromSemanticVersionString("2.4.2");
			var b = PackageVersion.FromSemanticVersionString("2.4.53");

			Assert.AreEqual(true, a < b);
			Assert.AreEqual(false, b < a);
			Assert.AreEqual(true, a <= b);
			Assert.AreEqual(false, b <= a);
		}

		[Test]
		public void MinorLessThan_StoppedByMajor()
		{
			var a = PackageVersion.FromSemanticVersionString("2.4.0");
			var b = PackageVersion.FromSemanticVersionString("3.1.0");

			Assert.AreEqual(true, a < b);
			Assert.AreEqual(false, b < a);
			Assert.AreEqual(true, a <= b);
			Assert.AreEqual(false, b <= a);
		}

		[Test]
		public void PatchLessThan_StoppedByMajor()
		{
			var a = PackageVersion.FromSemanticVersionString("2.1.4");
			var b = PackageVersion.FromSemanticVersionString("3.1.2");

			Assert.AreEqual(true, a < b);
			Assert.AreEqual(false, b < a);
			Assert.AreEqual(true, a <= b);
			Assert.AreEqual(false, b <= a);
		}

		[Test]
		public void PatchLessThan_StoppedByMinor()
		{
			var a = PackageVersion.FromSemanticVersionString("2.1.4");
			var b = PackageVersion.FromSemanticVersionString("2.3.2");

			Assert.AreEqual(true, a < b);
			Assert.AreEqual(false, b < a);
			Assert.AreEqual(true, a <= b);
			Assert.AreEqual(false, b <= a);
		}

		[Test]
		public void GreaterThanOrEqualTo_Equal()
		{
			var a = PackageVersion.FromSemanticVersionString("2.1.4");
			var b = PackageVersion.FromSemanticVersionString("2.1.4");

			Assert.AreEqual(true, a >= b);
			Assert.AreEqual(true, b >= a);
		}

		[Test]
		public void LessThanOrEqualTo_Equal()
		{
			var a = PackageVersion.FromSemanticVersionString("2.1.4");
			var b = PackageVersion.FromSemanticVersionString("2.1.4");

			Assert.AreEqual(true, a <= b);
			Assert.AreEqual(true, b <= a);
		}

		[Test]
		public void Same_AreNotGreater()
		{
			var a = PackageVersion.FromSemanticVersionString("2.4.1");
			var b = PackageVersion.FromSemanticVersionString("2.4.1");

			Assert.AreEqual(false, a > b);
			Assert.AreEqual(false, b > a);
		}

		[Test]
		public void Same_AreNotLesser()
		{
			var a = PackageVersion.FromSemanticVersionString("2.4.1");
			var b = PackageVersion.FromSemanticVersionString("2.4.1");

			Assert.AreEqual(false, a < b);
			Assert.AreEqual(false, b < a);
		}
	}
}
