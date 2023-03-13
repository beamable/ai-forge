using Beamable.Editor.Content;
using NUnit.Framework;

namespace Beamable.Editor.Tests.Content
{
	public class AreTagsEqualTests
	{
		[Test]
		public void LocalHasOneLessTag()
		{
			var serverTags = new string[] { "one", "two" };
			var localTags = new string[] { "one" };
			var tagsAreNotEqual = ContentIO.AreTagsEqual(localTags, serverTags);
			Assert.AreEqual(false, tagsAreNotEqual);
		}
		[Test]
		public void ServerHasOneLessTag()
		{
			var serverTags = new string[] { "one" };
			var localTags = new string[] { "one", "two" };
			var tagsAreNotEqual = ContentIO.AreTagsEqual(localTags, serverTags);
			Assert.AreEqual(false, tagsAreNotEqual);
		}
		[Test]
		public void LocalHasNoTags()
		{
			var serverTags = new string[] { "one" };
			var localTags = new string[] { };
			var tagsAreNotEqual = ContentIO.AreTagsEqual(localTags, serverTags);
			Assert.AreEqual(false, tagsAreNotEqual);
		}
		[Test]
		public void ServerHasNoTags()
		{
			var serverTags = new string[] { };
			var localTags = new string[] { "one" };
			var tagsAreNotEqual = ContentIO.AreTagsEqual(localTags, serverTags);
			Assert.AreEqual(false, tagsAreNotEqual);
		}
		[Test]
		public void AllTagsAreTheSame()
		{
			var serverTags = new string[] { "one", "two" };
			var localTags = new string[] { "one", "two" };
			var tagsAreEqual = ContentIO.AreTagsEqual(localTags, serverTags);
			Assert.IsTrue(tagsAreEqual);
		}

		[Test]
		public void AllTagsAreTheSameUnsorted()
		{
			var serverTags = new string[] { "four", "three", "one", "two" };
			var localTags = new string[] { "one", "two", "three", "four" };
			var tagsAreEqual = ContentIO.AreTagsEqual(localTags, serverTags);
			Assert.IsTrue(tagsAreEqual);
		}

	}
}
