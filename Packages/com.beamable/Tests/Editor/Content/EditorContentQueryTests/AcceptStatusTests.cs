using Beamable.Editor.Content;
using Beamable.Editor.Content.Models;
using NUnit.Framework;

namespace Beamable.Editor.Tests.Content.EditorContentQueryTests
{
	public class AcceptStatusTests
	{
		[Test]
		public void Single()
		{
			var query = new EditorContentQuery
			{
				StatusConstraint = ContentModificationStatus.SERVER_ONLY,
				HasStatusConstraint = true
			};

			Assert.IsTrue(query.AcceptStatus(ContentModificationStatus.SERVER_ONLY));
			Assert.IsFalse(query.AcceptStatus(ContentModificationStatus.LOCAL_ONLY));
			Assert.IsFalse(query.AcceptStatus(ContentModificationStatus.MODIFIED));
			Assert.IsFalse(query.AcceptStatus(ContentModificationStatus.NOT_AVAILABLE_ANYWHERE));
			Assert.IsFalse(query.AcceptStatus(ContentModificationStatus.NOT_MODIFIED));
		}

		[Test]
		public void Multiple()
		{
			var query = new EditorContentQuery
			{
				StatusConstraint = ContentModificationStatus.SERVER_ONLY | ContentModificationStatus.MODIFIED,
				HasStatusConstraint = true
			};

			Assert.IsTrue(query.AcceptStatus(ContentModificationStatus.SERVER_ONLY));
			Assert.IsFalse(query.AcceptStatus(ContentModificationStatus.LOCAL_ONLY));
			Assert.IsTrue(query.AcceptStatus(ContentModificationStatus.MODIFIED));
			Assert.IsFalse(query.AcceptStatus(ContentModificationStatus.NOT_AVAILABLE_ANYWHERE));
			Assert.IsFalse(query.AcceptStatus(ContentModificationStatus.NOT_MODIFIED));
		}
	}
}
