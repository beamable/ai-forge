using Beamable.Common.Content;
using Beamable.Editor.Content;
using Beamable.Editor.Content.Models;
using NUnit.Framework;
using System.Collections.Generic;

namespace Beamable.Editor.Tests.Content.UI.Models.ContentDataModelTests
{
	public class ContentTypeTreeViewItemsTests
	{
		[Test]
		public void FlatList()
		{
			var contentIO = new ContentIO(null);
			var model = new ContentDataModel(contentIO);

			model.SetContentTypes(new List<ContentTypePair>
		 {
			new ContentTypePair { Name = "tuna" } ,
			new ContentTypePair { Name = "fish" } ,
			new ContentTypePair { Name = "cans" } ,
		 });

			var viewItems = model.ContentTypeTreeViewItems();

			Assert.AreEqual(3, viewItems.Count);
			Assert.AreEqual(0, viewItems[0].depth);
			Assert.AreEqual(0, viewItems[1].depth);
			Assert.AreEqual(0, viewItems[2].depth);
		}

		[Test]
		public void OneDeepLast()
		{
			var contentIO = new ContentIO(null);
			var model = new ContentDataModel(contentIO);

			model.SetContentTypes(new List<ContentTypePair>
		 {
			new ContentTypePair { Name = "tuna" } ,
			new ContentTypePair { Name = "tuna.fish" } ,
			new ContentTypePair { Name = "fish" } ,
			new ContentTypePair { Name = "cans" } ,
		 });

			var viewItems = model.ContentTypeTreeViewItems();


			Assert.AreEqual(4, viewItems.Count);
			Assert.AreEqual(0, viewItems[0].depth);
			Assert.AreEqual("cans", viewItems[0].displayName);

			Assert.AreEqual(0, viewItems[1].depth);
			Assert.AreEqual("fish", viewItems[1].displayName);

			Assert.AreEqual(0, viewItems[2].depth);
			Assert.AreEqual("tuna", viewItems[2].displayName);

			Assert.AreEqual(1, viewItems[3].depth);
			Assert.AreEqual("fish", viewItems[3].displayName);
		}

		[Test]
		public void OneDeepFirst()
		{
			var contentIO = new ContentIO(null);
			var model = new ContentDataModel(contentIO);

			model.SetContentTypes(new List<ContentTypePair>
		 {
			new ContentTypePair { Name = "tuna" } ,
			new ContentTypePair { Name = "cans.fish" } ,
			new ContentTypePair { Name = "fish" } ,
			new ContentTypePair { Name = "cans" } ,
		 });

			var viewItems = model.ContentTypeTreeViewItems();


			Assert.AreEqual(4, viewItems.Count);
			Assert.AreEqual(0, viewItems[0].depth);
			Assert.AreEqual("cans", viewItems[0].displayName);

			Assert.AreEqual(1, viewItems[1].depth);
			Assert.AreEqual("fish", viewItems[1].displayName);

			Assert.AreEqual(0, viewItems[2].depth);
			Assert.AreEqual("fish", viewItems[2].displayName);

			Assert.AreEqual(0, viewItems[3].depth);
			Assert.AreEqual("tuna", viewItems[3].displayName);
		}

		[Test]
		public void OneDeepMid()
		{
			var contentIO = new ContentIO(null);
			var model = new ContentDataModel(contentIO);

			model.SetContentTypes(new List<ContentTypePair>
		 {
			new ContentTypePair { Name = "tuna" } ,
			new ContentTypePair { Name = "fish.iceberg" } ,
			new ContentTypePair { Name = "fish" } ,
			new ContentTypePair { Name = "cans" } ,
		 });

			var viewItems = model.ContentTypeTreeViewItems();


			Assert.AreEqual(4, viewItems.Count);
			Assert.AreEqual(0, viewItems[0].depth);
			Assert.AreEqual("cans", viewItems[0].displayName);

			Assert.AreEqual(0, viewItems[1].depth);
			Assert.AreEqual("fish", viewItems[1].displayName);

			Assert.AreEqual(1, viewItems[2].depth);
			Assert.AreEqual("iceberg", viewItems[2].displayName);

			Assert.AreEqual(0, viewItems[3].depth);
			Assert.AreEqual("tuna", viewItems[3].displayName);
		}

		[Test]
		public void TwoGroups()
		{
			var contentIO = new ContentIO(null);
			var model = new ContentDataModel(contentIO);

			model.SetContentTypes(new List<ContentTypePair>
		 {
			new ContentTypePair { Name = "tuna" } ,
			new ContentTypePair { Name = "fish.iceberg" } ,
			new ContentTypePair { Name = "fish" } ,
			new ContentTypePair { Name = "tuna.land" } ,
			new ContentTypePair { Name = "cans" } ,
		 });

			var viewItems = model.ContentTypeTreeViewItems();


			Assert.AreEqual(5, viewItems.Count);
			Assert.AreEqual(0, viewItems[0].depth);
			Assert.AreEqual("cans", viewItems[0].displayName);

			Assert.AreEqual(0, viewItems[1].depth);
			Assert.AreEqual("fish", viewItems[1].displayName);

			Assert.AreEqual(1, viewItems[2].depth);
			Assert.AreEqual("iceberg", viewItems[2].displayName);

			Assert.AreEqual(0, viewItems[3].depth);
			Assert.AreEqual("tuna", viewItems[3].displayName);

			Assert.AreEqual(1, viewItems[4].depth);
			Assert.AreEqual("land", viewItems[4].displayName);
		}

		[Test]
		public void TwoGroupsWithManyEntries()
		{
			var contentIO = new ContentIO(null);
			var model = new ContentDataModel(contentIO);

			model.SetContentTypes(new List<ContentTypePair>
		 {
			new ContentTypePair { Name = "tuna" } ,
			new ContentTypePair { Name = "fish.iceberg" } ,
			new ContentTypePair { Name = "fish.iceberg2" } ,
			new ContentTypePair { Name = "fish" } ,
			new ContentTypePair { Name = "tuna.land" } ,
			new ContentTypePair { Name = "cans" } ,
		 });

			var viewItems = model.ContentTypeTreeViewItems();


			Assert.AreEqual(6, viewItems.Count);
			Assert.AreEqual(0, viewItems[0].depth);
			Assert.AreEqual("cans", viewItems[0].displayName);

			Assert.AreEqual(0, viewItems[1].depth);
			Assert.AreEqual("fish", viewItems[1].displayName);

			Assert.AreEqual(1, viewItems[2].depth);
			Assert.AreEqual("iceberg", viewItems[2].displayName);

			Assert.AreEqual(1, viewItems[3].depth);
			Assert.AreEqual("iceberg2", viewItems[3].displayName);

			Assert.AreEqual(0, viewItems[4].depth);
			Assert.AreEqual("tuna", viewItems[4].displayName);

			Assert.AreEqual(1, viewItems[5].depth);
			Assert.AreEqual("land", viewItems[5].displayName);
		}
	}
}
