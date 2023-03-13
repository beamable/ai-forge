using Beamable.Common.Api;
using NUnit.Framework;
using System;

namespace Beamable.Editor.Tests
{
	public class AliasHelperTests
	{
		[TestCase("213")]
		[TestCase("0325185324")]
		[TestCase("1325185324")]
		[TestCase("0")]
		[TestCase("1")]
		[TestCase("")]
		public void ValidCid(string cid)
		{
			AliasHelper.ValidateCid(cid);
		}

		[TestCase("abc")]
		[TestCase("a42350123")]
		[TestCase("-123")]
		public void InvalidCid(string cid)
		{
			Assert.IsFalse(AliasHelper.IsCid(cid));

			Assert.Throws<ArgumentException>(() =>
			{
				AliasHelper.ValidateCid(cid);
			});
		}

		[TestCase("abc")]
		[TestCase("a124")]
		[TestCase("tuna-fish")]
		[TestCase("a01a")]
		[TestCase("a-b-c")]
		[TestCase("")]
		public void ValidAlias(string alias)
		{
			Assert.IsFalse(AliasHelper.IsCid(alias));
			AliasHelper.ValidateAlias(alias);
		}

		[TestCase("0123")]
		[TestCase("123")]
		[TestCase("t- test")]
		[TestCase("t e s t")]
		[TestCase("0 1 2 3")]
		[TestCase("   test")]
		[TestCase("   test-abc")]
		[TestCase("   012-abc")]
		[TestCase("   012-345")]
		[TestCase("555test")]
		[TestCase("555test-444-kek")]
		public void InvalidAlias(string alias)
		{
			Assert.Throws<ArgumentException>(() =>
			{
				AliasHelper.ValidateAlias(alias);
			});
		}
	}
}
