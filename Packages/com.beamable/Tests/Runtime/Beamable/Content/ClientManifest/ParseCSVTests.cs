using NUnit.Framework;

namespace Beamable.Tests.Content.ClientManifest
{
	public class ParseCSVTests
	{
		[Test]
		public void HandleEmptyManifest()
		{
			var manifest = Common.Content.ClientManifest.ParseCSV("");
			Assert.NotNull(manifest);
			Assert.AreEqual(0, manifest.entries.Count);
		}

		[Test]
		public void HandleNullManifest()
		{
			var manifest = Common.Content.ClientManifest.ParseCSV(null);
			Assert.NotNull(manifest);
			Assert.AreEqual(0, manifest.entries.Count);
		}

		[Test]
		public void Handle_NoTags()
		{
			var manifest = Common.Content.ClientManifest.ParseCSV("a0,b0,c0,d0\na1,b1,c1,d1");
			Assert.NotNull(manifest);
			Assert.AreEqual(2, manifest.entries.Count);

			Assert.AreEqual("a0", manifest.entries[0].type);
			Assert.AreEqual("b0", manifest.entries[0].contentId);
			Assert.AreEqual("c0", manifest.entries[0].version);
			Assert.AreEqual("d0", manifest.entries[0].uri);
			Assert.AreEqual(0, manifest.entries[0].tags.Length);

			Assert.AreEqual("a1", manifest.entries[1].type);
			Assert.AreEqual("b1", manifest.entries[1].contentId);
			Assert.AreEqual("c1", manifest.entries[1].version);
			Assert.AreEqual("d1", manifest.entries[1].uri);
			Assert.AreEqual(0, manifest.entries[1].tags.Length);
		}

		[Test]
		public void Handle_WithTags()
		{
			var manifest = Common.Content.ClientManifest.ParseCSV("a0,b0,c0,d0,e1;ee1\na1,b1,c1,d1,e2");
			Assert.NotNull(manifest);
			Assert.AreEqual(2, manifest.entries.Count);

			Assert.AreEqual("a0", manifest.entries[0].type);
			Assert.AreEqual("b0", manifest.entries[0].contentId);
			Assert.AreEqual("c0", manifest.entries[0].version);
			Assert.AreEqual("d0", manifest.entries[0].uri);
			Assert.AreEqual(2, manifest.entries[0].tags.Length);

			Assert.AreEqual("a1", manifest.entries[1].type);
			Assert.AreEqual("b1", manifest.entries[1].contentId);
			Assert.AreEqual("c1", manifest.entries[1].version);
			Assert.AreEqual("d1", manifest.entries[1].uri);
			Assert.AreEqual(1, manifest.entries[1].tags.Length);
		}
		[Test]
		public void Handle_WithTagMix()
		{
			var manifest = Common.Content.ClientManifest.ParseCSV("a0,b0,c0,d0,\na1,b1,c1,d1,e2");
			Assert.NotNull(manifest);
			Assert.AreEqual(2, manifest.entries.Count);

			Assert.AreEqual("a0", manifest.entries[0].type);
			Assert.AreEqual("b0", manifest.entries[0].contentId);
			Assert.AreEqual("c0", manifest.entries[0].version);
			Assert.AreEqual("d0", manifest.entries[0].uri);
			Assert.AreEqual(0, manifest.entries[0].tags.Length);

			Assert.AreEqual("a1", manifest.entries[1].type);
			Assert.AreEqual("b1", manifest.entries[1].contentId);
			Assert.AreEqual("c1", manifest.entries[1].version);
			Assert.AreEqual("d1", manifest.entries[1].uri);
			Assert.AreEqual(1, manifest.entries[1].tags.Length);
		}

		[Test]
		public void Handle_WithDoubleQuotes()
		{
			var manifest = Common.Content.ClientManifest.ParseCSV("a0,\"b0,da\",c0,d0,\na1,b1,c1,d1,\"e2;ads\nda\"");
			Assert.NotNull(manifest);
			Assert.AreEqual(2, manifest.entries.Count);

			Assert.AreEqual("a0", manifest.entries[0].type);
			Assert.AreEqual("b0,da", manifest.entries[0].contentId);
			Assert.AreEqual("c0", manifest.entries[0].version);
			Assert.AreEqual("d0", manifest.entries[0].uri);
			Assert.AreEqual(0, manifest.entries[0].tags.Length);

			Assert.AreEqual("a1", manifest.entries[1].type);
			Assert.AreEqual("b1", manifest.entries[1].contentId);
			Assert.AreEqual("c1", manifest.entries[1].version);
			Assert.AreEqual("d1", manifest.entries[1].uri);
			Assert.AreEqual(2, manifest.entries[1].tags.Length);
		}
	}
}
