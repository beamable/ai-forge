using Beamable.Editor.Content;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Editor.Tests.Beamable.Content
{
	public class ContentManifestTests
	{
		private ContentManifestReference _content1;

		[SetUp]
		public void Init()
		{
			_content1 = new ContentManifestReference()
			{
				id = "content1",
				visibility = "public",
				checksum = "checksum",
				tags = new string[] { }
			};
		}

		[Test]
		public void DiffingSupportsAddition()
		{
			var a = new Manifest(new List<ContentManifestReference>() { _content1 });
			var b = new Manifest(new List<ContentManifestReference>());

			var diffSet = Manifest.FindDifferences(a, b);

			Assert.AreEqual(0, diffSet.Deletions.Count());
			Assert.AreEqual(0, diffSet.Modifications.Count());
			Assert.AreEqual(1, diffSet.Additions.Count());
			Assert.AreEqual(_content1, diffSet.Additions.First());
		}

		[Test]
		public void DiffingSupportsDeletion()
		{
			var a = new Manifest(new List<ContentManifestReference>());
			var b = new Manifest(new List<ContentManifestReference>() { _content1 });

			var diffSet = Manifest.FindDifferences(a, b);

			Assert.AreEqual(1, diffSet.Deletions.Count());
			Assert.AreEqual(0, diffSet.Modifications.Count());
			Assert.AreEqual(0, diffSet.Additions.Count());
			Assert.AreEqual(_content1, diffSet.Deletions.First());
		}

		[Test]
		public void DiffingSupportsModification()
		{
			var modifiedContent1 = new ContentManifestReference()
			{
				id = _content1.id,
				visibility = _content1.visibility,
				checksum = _content1 + "different",
				tags = new string[] { }
			};

			var a = new Manifest(new List<ContentManifestReference>() { modifiedContent1 });
			var b = new Manifest(new List<ContentManifestReference>() { _content1 });

			var diffSet = Manifest.FindDifferences(a, b);

			Assert.AreEqual(0, diffSet.Deletions.Count());
			Assert.AreEqual(1, diffSet.Modifications.Count());
			Assert.AreEqual(0, diffSet.Additions.Count());
			Assert.AreEqual(modifiedContent1, diffSet.Modifications.First());
		}

		[Test]
		public void DiffingSupportsNoOp()
		{
			// the purpose here is to have a different instance of the object, but representing the same doodad
			var unModifiedContent = new ContentManifestReference()
			{
				id = _content1.id,
				visibility = _content1.visibility,
				checksum = _content1.checksum,
				tags = new string[] { }
			};

			var a = new Manifest(new List<ContentManifestReference>() { unModifiedContent });
			var b = new Manifest(new List<ContentManifestReference>() { _content1 });

			var diffSet = Manifest.FindDifferences(a, b);

			Assert.AreEqual(0, diffSet.Deletions.Count());
			Assert.AreEqual(0, diffSet.Modifications.Count());
			Assert.AreEqual(0, diffSet.Additions.Count());
		}
	}
}
