using Beamable.Common.Content;
using Beamable.Common.Inventory;
using NUnit.Framework;

namespace Beamable.Editor.Tests.Beamable.Content.ContentQueryTests
{
	public class ParseTests
	{
		private ContentTypeReflectionCache contentTypeReflectionCache;
		[SetUp]
		public void Setup()
		{
			contentTypeReflectionCache = BeamEditor.GetReflectionSystem<ContentTypeReflectionCache>();
		}

		[Test]
		public void CanParseEmpty()
		{
			var q = ContentQuery.Parse("");
			Assert.AreEqual(null, q.TypeConstraints);
			Assert.AreEqual(null, q.IdContainsConstraint);
			Assert.AreEqual(null, q.TagConstraints);
		}

		[Test]
		public void CanParseAll()
		{
			var q = ContentQuery.Parse("t:items , tag: truck frank,  fooishbar");
			Assert.AreEqual(true, q.TypeConstraints.Contains(typeof(ItemContent)));
			Assert.AreEqual("fooishbar", q.IdContainsConstraint);
			Assert.AreEqual(true, q.TagConstraints.Contains("truck"));
			Assert.AreEqual(true, q.TagConstraints.Contains("frank"));
		}

		[Test]
		public void CanParseType()
		{
			var q = ContentQuery.Parse("t:items");
			Assert.AreEqual(true, q.TypeConstraints.Contains(typeof(ItemContent)));
			Assert.AreEqual(null, q.IdContainsConstraint);
			Assert.AreEqual(null, q.TagConstraints);
		}

		[Test]
		public void CanParseMultipleTypes()
		{
			var q = ContentQuery.Parse("t:items emails");
			Assert.AreEqual(true, q.TypeConstraints.Contains(typeof(ItemContent)));
			Assert.AreEqual(true, q.TypeConstraints.Contains(typeof(EmailContent)));
			Assert.AreEqual(null, q.IdContainsConstraint);
			Assert.AreEqual(null, q.TagConstraints);
		}

		[Test]
		public void CanParseMultipleTypes_InProgress()
		{
			var q = ContentQuery.Parse("t:items ");
			Assert.AreEqual(true, q.TypeConstraints.Contains(typeof(ItemContent)));

			var str = q.ToString("t:items ");
			Assert.AreEqual("t:items ", str);
		}

		[Test]
		public void ParseIdThenTag()
		{
			var orig = "welcome, tag:foo";
			var q = ContentQuery.Parse(orig);
			Assert.AreEqual(null, q.TypeConstraints);
			Assert.AreEqual("welcome", q.IdContainsConstraint);
			Assert.AreEqual(true, q.TagConstraints.Contains("foo"));

		}

		[Test]
		public void CanParseTags()
		{
			var q = ContentQuery.Parse("tag:foo bar");
			Assert.AreEqual(null, q.TypeConstraints);
			Assert.AreEqual(null, q.IdContainsConstraint);
			Assert.AreEqual(true, q.TagConstraints.Contains("foo"));
			Assert.AreEqual(true, q.TagConstraints.Contains("bar"));
		}

		[Test]
		public void CanParseId()
		{
			var q = ContentQuery.Parse("id:foobar");
			Assert.AreEqual(null, q.TypeConstraints);
			Assert.AreEqual("foobar", q.IdContainsConstraint);
			Assert.AreEqual(null, q.TagConstraints);
		}

		[Test]
		public void CanParseIdWithoutCouple()
		{
			var q = ContentQuery.Parse("foobar");
			Assert.AreEqual(null, q.TypeConstraints);
			Assert.AreEqual("foobar", q.IdContainsConstraint);
			Assert.AreEqual(null, q.TagConstraints);
		}
	}
}
