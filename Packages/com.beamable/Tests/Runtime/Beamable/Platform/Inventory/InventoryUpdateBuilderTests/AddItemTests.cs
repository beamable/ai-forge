using Beamable.Common.Api.Inventory;
using NUnit.Framework;
using System.Collections.Generic;

namespace Beamable.Platform.Tests.Inventory.InventoryUpdateBuilderTests
{
	public class AddItemTests
	{
		[Test]
		public void AddOneItem()
		{
			var updateBuilder = new InventoryUpdateBuilder();

			var props = new Dictionary<string, string> { { "key", "value" } };
			var contentId = "contentId";
			updateBuilder.AddItem(contentId, props);

			Assert.AreEqual(1, updateBuilder.newItems.Count);

			Assert.AreEqual(props, updateBuilder.newItems[0].properties);
			Assert.AreEqual(contentId, updateBuilder.newItems[0].contentId);
		}
	}
}
