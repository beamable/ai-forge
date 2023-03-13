using Beamable.Editor.Content;
using Beamable.Editor.Content.Models;
using NUnit.Framework;

namespace Beamable.Editor.Tests.Content.EditorContentQueryTests
{
	public class ToStringTests
	{
		[Test]
		public void SerializeValid()
		{
			var query = new EditorContentQuery
			{ ValidationConstraint = ContentValidationStatus.VALID, HasValidationConstraint = true };
			var str = query.ToString();

			Assert.AreEqual("valid:y", str);
		}

		[Test]
		public void Serialize_StatusMany()
		{
			var query = new EditorContentQuery
			{
				StatusConstraint = ContentModificationStatus.MODIFIED | ContentModificationStatus.SERVER_ONLY,
				HasStatusConstraint = true
			};
			var str = query.ToString();

			Assert.AreEqual("status:server modified", str);
		}

		[Test]
		public void Serialize_StatusSingle()
		{
			var query = new EditorContentQuery
			{
				StatusConstraint = ContentModificationStatus.MODIFIED,
				HasStatusConstraint = true
			};
			var str = query.ToString();

			Assert.AreEqual("status:modified", str);
		}

		[Test]
		public void Serialize_Empty()
		{
			var query = new EditorContentQuery();

			var str = query.ToString();

			Assert.AreEqual("", str);
		}

		[Test]
		public void Serialize_StatusSingle_NotModified()
		{
			var query = new EditorContentQuery
			{
				StatusConstraint = ContentModificationStatus.NOT_MODIFIED,
				HasStatusConstraint = true
			};
			var str = query.ToString();

			Assert.AreEqual("status:sync", str);
		}

		[Test]
		public void Serialize_Existing_Status()
		{

			var existing = "status:local";
			var query = new EditorContentQuery
			{
				HasStatusConstraint = true,
				StatusConstraint = ContentModificationStatus.LOCAL_ONLY | ContentModificationStatus.MODIFIED
			};
			var str = query.ToString(existing);
			Assert.AreEqual("status:local modified", str);
		}

		[Test]
		public void Serialize_Existing_Status_WithId()
		{

			var existing = "status:local, foobar";
			var query = new EditorContentQuery
			{
				HasStatusConstraint = true,
				StatusConstraint = ContentModificationStatus.LOCAL_ONLY | ContentModificationStatus.MODIFIED
			};
			var str = query.ToString(existing);
			Assert.AreEqual("status:local modified, foobar", str);
		}
	}
}
