using Beamable.Common.Content;
using Beamable.Common.Inventory;
using Beamable.Editor.Content;
using Beamable.Editor.Content.Models;
using NUnit.Framework;

namespace Beamable.Editor.Tests.Content.EditorContentQueryTests
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
		public void CanParseStandardRules()
		{
			var q = EditorContentQuery.Parse("id:foo, t:items, tag:a");
			Assert.AreEqual("foo", q.IdContainsConstraint);
			Assert.AreEqual(true, q.TagConstraints.Contains("a"));
			Assert.AreEqual(true, q.TypeConstraints.Contains(typeof(ItemContent)));
		}

		[Test]
		public void Status_SupportsOr_Modified_or_Local()
		{
			var q = EditorContentQuery.Parse("status:modified local");
			Assert.AreEqual(true, q.HasStatusConstraint);
			Assert.AreEqual(ContentModificationStatus.MODIFIED | ContentModificationStatus.LOCAL_ONLY, q.StatusConstraint);
		}

		[Test]
		public void Parse_Equals_ToString()
		{
			var str = "v";
			var query = EditorContentQuery.Parse(str);

			var str2 = query.ToString(str);

			Assert.AreEqual(str, str2);
		}

		[Test]
		public void Parse_Equals_ToString2()
		{
			var str = "tag:a, status:";
			var query = EditorContentQuery.Parse(str);

			var str2 = query.ToString(str);

			Assert.AreEqual(str, str2);
		}

		[Test]
		public void Parse_Equals_ToString3()
		{
			var str = "tag:a, status:modified";
			var query = EditorContentQuery.Parse(str);

			var str2 = query.ToString(str);

			Assert.AreEqual(str, str2);
		}

		[Test]
		public void Parse_Equals_ToString4()
		{
			var str = "tag:a,status";
			var query = EditorContentQuery.Parse(str);

			var str2 = query.ToString(str);

			Assert.AreEqual(str, str2);
		}

		[Test]
		public void Parse_Equals_ToString5()
		{
			var str = "tag:a, ";
			var query = EditorContentQuery.Parse(str);

			var str2 = query.ToString(str);

			Assert.AreEqual(str, str2);
		}

		[Test]
		public void Parse_Equals_ToString6()
		{
			var str = "tag:a, sta";
			var query = EditorContentQuery.Parse(str);

			var str2 = query.ToString(str);

			Assert.AreEqual(str, str2);
		}
	}
}
