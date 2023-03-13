using Beamable.Common.Api;
using Beamable.Common.Api.Inventory;
using Beamable.Serialization.SmallerJSON;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.TestTools;

namespace Beamable.Platform.Tests.Inventory.InventoryServiceTests
{
	public class GetItemTests : InventoryServiceTestBase
	{
		[UnityTest]
		public IEnumerator GetItemGroupSubset()
		{
			// mock out a piece of content.
			var contentName = "test";
			_content.Provide(new InventoryTestItem { name = "junk", Foo = 1 }.SetContentName("junk"));
			_content.Provide(new InventoryTestItem { name = contentName, Foo = 123 }.SetContentName(contentName));
			_content.Provide(new InventoryTestItem { name = "rando", Foo = 2 }.SetContentName("rando"));

			// Mock out a network request that get an item. This semi defines the web API itself.
			_requester
			   .MockRequest<InventoryResponse>(Method.POST)
			   .WithURIPrefix("/object/inventory")
			   .WithBodyMatch<ArrayDict>(sent =>
			   {
				   var expected = new ArrayDict() { { "scopes", new[] { "items.inventoryTestItem.test" } } };

				   var matchKeys = expected.Keys.SequenceEqual(sent.Keys);
				   var matchValuesLength = expected.Values.Count == sent.Values.Count;
				   var matchValues = ((string[])expected.Values.ElementAt(0))[0] == ((string[])sent.Values.ElementAt(0))[0];

				   return matchKeys && matchValuesLength && matchValues;
			   })
			   .WithResponse(new InventoryResponse
			   {
				   currencies = new List<Currency>(),
				   scope = "items.inventoryTestItem.test",
				   items = new List<ItemGroup>
				  {
				  new ItemGroup
				  {
					 id = "items.inventoryTestItem.test",
					 items = new List<Item>
					 {
						new Item
						{
						   id = "1",
						   properties = new List<ItemProperty>
						   {
							  new ItemProperty
							  {
								 name = "foo",
								 value = "bar1"
							  }
						   }
						}
					 }
				  },
				  new ItemGroup
				  {
					 id ="items.inventoryTestItem.junk",
					 items = new List<Item>()
				  },
				  new ItemGroup
				  {
					 id="items.inventoryTestItem.rando",
					 items = new List<Item>
					 {
						new Item
						{
						   id="1",
						   properties = new List<ItemProperty>()
						}
					 }
				  }
				  }
			   });


			// test our sdk code, and verify that the response is what we expect.
			yield return _service.GetItems<InventoryTestItem>(new InventoryTestItemRef($"items.inventoryTestItem.{contentName}")).Then(view =>
			{
				Assert.AreEqual(1, view.Count);
				Assert.AreEqual(contentName, view[0].ItemContent.name);
				Assert.AreEqual("bar1", view[0].Properties["foo"]);
				Assert.AreEqual(123, view[0].ItemContent.Foo);

			}).AsYield();
		}

		[UnityTest]
		public IEnumerator GetManyItems()
		{
			// mock out a piece of content.
			var contentName = "test";
			var content = new InventoryTestItem { Foo = 123 };
			content.SetContentName(contentName);
			_content.Provide(content);

			// Mock out a network request that get an item. This semi defines the web API itself.
			_requester
			   .MockRequest<InventoryResponse>(Method.POST)
			   .WithURIPrefix("/object/inventory")
			   .WithBodyMatch<ArrayDict>(sent =>
			   {
				   var expected = new ArrayDict() { { "scopes", new[] { "items.inventoryTestItem" } } };

				   var matchKeys = expected.Keys.SequenceEqual(sent.Keys);
				   var matchValuesLength = expected.Values.Count == sent.Values.Count;
				   var matchValues = ((string[])expected.Values.ElementAt(0))[0] == ((string[])sent.Values.ElementAt(0))[0];

				   return matchKeys && matchValuesLength && matchValues;
			   })
			   .WithResponse(new InventoryResponse
			   {
				   currencies = new List<Currency>(),
				   scope = "items.inventoryTestItem.test",
				   items = new List<ItemGroup>
				  {
				  new ItemGroup
				  {
					 id = "items.inventoryTestItem.test",
					 items = new List<Item>
					 {
						new Item
						{
						   id = "1",
						   properties = new List<ItemProperty>
						   {
							  new ItemProperty
							  {
								 name = "foo",
								 value = "bar1"
							  }
						   }
						},
						new Item
						{
						   id = "2",
						   properties = new List<ItemProperty>
						   {
							  new ItemProperty
							  {
								 name="foo",
								 value="bar2"
							  }
						   }
						}
					 }
				  }
				  }
			   });


			// test our sdk code, and verify that the response is what we expect.
			yield return _service.GetItems<InventoryTestItem>().Then(view =>
			{
				Assert.AreEqual(2, view.Count);
				Assert.AreEqual(contentName, view[0].ItemContent.name);
				Assert.AreEqual("bar1", view[0].Properties["foo"]);
				Assert.AreEqual(123, view[0].ItemContent.Foo);

				Assert.AreEqual(contentName, view[1].ItemContent.name);
				Assert.AreEqual("bar2", view[1].Properties["foo"]);
				Assert.AreEqual(123, view[1].ItemContent.Foo);
			}).AsYield();
		}

		[UnityTest]
		public IEnumerator GetAnItemThatExists()
		{
			// mock out a piece of content.
			var contentName = "test";
			var content = new InventoryTestItem { Foo = 123 };
			content.SetContentName(contentName);
			_content.Provide(content);

			// Mock out a network request that get an item. This semi defines the web API itself.
			_requester
			   .MockRequest<InventoryResponse>(Method.POST)
			   .WithURIPrefix("/object/inventory")
			   .WithResponse(new InventoryResponse
			   {
				   currencies = new List<Currency>(),
				   scope = "items.inventoryTestItem.test",
				   items = new List<ItemGroup>
				  {
				  new ItemGroup
				  {
					 id = "items.inventoryTestItem.test",
					 items = new List<Item>
					 {
						new Item
						{
						   id = "1",
						   properties = new List<ItemProperty>
						   {
							  new ItemProperty
							  {
								 name = "foo",
								 value = "bar"
							  }
						   }
						}
					 }
				  }
				  }
			   });


			// test our sdk code, and verify that the response is what we expect.
			yield return _service.GetItems<InventoryTestItem>().Then(view =>
			{
				Assert.AreEqual(1, view.Count);
				Assert.AreEqual(contentName, view[0].ItemContent.name);
				Assert.AreEqual("bar", view[0].Properties["foo"]);
				Assert.AreEqual(123, view[0].ItemContent.Foo);
			}).AsYield();

		}
	}
}
