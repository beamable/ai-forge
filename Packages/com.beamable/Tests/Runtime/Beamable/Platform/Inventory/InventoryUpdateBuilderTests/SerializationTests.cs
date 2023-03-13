using Beamable.Common.Api.Inventory;
using NUnit.Framework;
using System.Collections.Generic;

namespace Beamable.Platform.Tests.Inventory.InventoryUpdateBuilderTests
{
	public class SerializationTests
	{
		[Test]
		public void TransactionStored()
		{
			var builder = new InventoryUpdateBuilder();
			var trans = "abc";

			var json = InventoryUpdateBuilderSerializer.ToNetworkJson(builder, trans);
			var deserialized = InventoryUpdateBuilderSerializer.FromNetworkJson(json);

			Assert.AreEqual(trans, deserialized.Item2);
		}

		[Test]
		public void ApplyVip_True()
		{
			var builder = new InventoryUpdateBuilder();
			builder.applyVipBonus = true;

			var json = InventoryUpdateBuilderSerializer.ToNetworkJson(builder);
			var deserialized = InventoryUpdateBuilderSerializer.FromNetworkJson(json);

			Assert.AreEqual(true, deserialized.Item1.applyVipBonus);
		}

		[Test]
		public void ApplyVip_False()
		{
			var builder = new InventoryUpdateBuilder();
			builder.applyVipBonus = false;

			var json = InventoryUpdateBuilderSerializer.ToNetworkJson(builder);
			var deserialized = InventoryUpdateBuilderSerializer.FromNetworkJson(json);

			Assert.AreEqual(false, deserialized.Item1.applyVipBonus);
		}

		[Test]
		public void ApplyVip_Unset()
		{
			var builder = new InventoryUpdateBuilder();

			var json = InventoryUpdateBuilderSerializer.ToNetworkJson(builder);
			var deserialized = InventoryUpdateBuilderSerializer.FromNetworkJson(json);

			Assert.AreEqual(null, deserialized.Item1.applyVipBonus);
		}


		[Test]
		public void Currencies_Unset()
		{
			var builder = new InventoryUpdateBuilder();

			var json = InventoryUpdateBuilderSerializer.ToNetworkJson(builder);
			var deserialized = InventoryUpdateBuilderSerializer.FromNetworkJson(json);

			Assert.AreEqual(0, deserialized.Item1.currencies.Count);
		}

		[Test]
		public void Currencies_WithGems()
		{
			var builder = new InventoryUpdateBuilder();
			builder = builder.CurrencyChange("currency.gems", 3);

			var json = InventoryUpdateBuilderSerializer.ToNetworkJson(builder);
			var deserialized = InventoryUpdateBuilderSerializer.FromNetworkJson(json);

			Assert.AreEqual(3, deserialized.Item1.currencies["currency.gems"]);
		}

		[Test]
		public void CurrenciesProperties_Unset()
		{
			var builder = new InventoryUpdateBuilder();

			var json = InventoryUpdateBuilderSerializer.ToNetworkJson(builder);
			var deserialized = InventoryUpdateBuilderSerializer.FromNetworkJson(json);

			Assert.AreEqual(0, deserialized.Item1.currencyProperties.Count);
		}

		[Test]
		public void CurrenciesProperties_WithStuff()
		{
			var builder = new InventoryUpdateBuilder();
			builder.SetCurrencyProperties("currency.gems", new List<CurrencyProperty>
			{
				new CurrencyProperty
				{
					name = "a",
					value = "b"
				}
			});
			var json = InventoryUpdateBuilderSerializer.ToNetworkJson(builder);
			var deserialized = InventoryUpdateBuilderSerializer.FromNetworkJson(json);

			Assert.AreEqual(1, deserialized.Item1.currencyProperties["currency.gems"].Count);
			Assert.AreEqual("a", deserialized.Item1.currencyProperties["currency.gems"][0].name);
			Assert.AreEqual("b", deserialized.Item1.currencyProperties["currency.gems"][0].value);
		}

		[Test]
		public void NewItems_Unset()
		{
			var builder = new InventoryUpdateBuilder();

			var json = InventoryUpdateBuilderSerializer.ToNetworkJson(builder);
			var deserialized = InventoryUpdateBuilderSerializer.FromNetworkJson(json);

			Assert.AreEqual(0, deserialized.Item1.newItems.Count);
		}

		[Test]
		public void NewItems_WithStuff()
		{
			var builder = new InventoryUpdateBuilder();
			builder = builder.AddItem("item.tuna", new Dictionary<string, string> { ["a"] = "b" });

			var json = InventoryUpdateBuilderSerializer.ToNetworkJson(builder);
			var deserialized = InventoryUpdateBuilderSerializer.FromNetworkJson(json);

			Assert.AreEqual(1, deserialized.Item1.newItems.Count);
			Assert.AreEqual("item.tuna", deserialized.Item1.newItems[0].contentId);
			Assert.AreEqual("b", deserialized.Item1.newItems[0].properties["a"]);
		}

		[Test]
		public void NewItems_WithStuff_RequestIdUnset()
		{
			var builder = new InventoryUpdateBuilder();
			builder = builder.AddItem("item.tuna", new Dictionary<string, string> { ["a"] = "b" });

			var startReqId = builder.newItems[0].requestId;

			var json = InventoryUpdateBuilderSerializer.ToNetworkJson(builder);
			var deserialized = InventoryUpdateBuilderSerializer.FromNetworkJson(json);

			Assert.AreEqual(1, deserialized.Item1.newItems.Count);
			Assert.AreEqual("item.tuna", deserialized.Item1.newItems[0].contentId);
			Assert.AreEqual("b", deserialized.Item1.newItems[0].properties["a"]);
			Assert.IsNotNull(deserialized.Item1.newItems[0].requestId);
			Assert.AreEqual(startReqId, deserialized.Item1.newItems[0].requestId);
		}

		[Test]
		public void NewItems_WithStuff_RequestIdSet()
		{
			var builder = new InventoryUpdateBuilder();
			builder = builder.AddItem("item.tuna", new Dictionary<string, string> { ["a"] = "b" });
			builder.newItems[0].requestId = "abc";
			var startReqId = builder.newItems[0].requestId;

			var json = InventoryUpdateBuilderSerializer.ToNetworkJson(builder);
			var deserialized = InventoryUpdateBuilderSerializer.FromNetworkJson(json);

			Assert.AreEqual(1, deserialized.Item1.newItems.Count);
			Assert.AreEqual("item.tuna", deserialized.Item1.newItems[0].contentId);
			Assert.AreEqual("b", deserialized.Item1.newItems[0].properties["a"]);
			Assert.IsNotNull(deserialized.Item1.newItems[0].requestId);
			Assert.AreEqual(startReqId, deserialized.Item1.newItems[0].requestId);

		}


		[Test]
		public void DeleteItems_Unset()
		{
			var builder = new InventoryUpdateBuilder();

			var json = InventoryUpdateBuilderSerializer.ToNetworkJson(builder);
			var deserialized = InventoryUpdateBuilderSerializer.FromNetworkJson(json);

			Assert.AreEqual(0, deserialized.Item1.deleteItems.Count);
		}

		[Test]
		public void DeleteItems_WithStuff()
		{
			var builder = new InventoryUpdateBuilder();
			builder = builder.DeleteItem("item.tuna", 3);

			var json = InventoryUpdateBuilderSerializer.ToNetworkJson(builder);
			var deserialized = InventoryUpdateBuilderSerializer.FromNetworkJson(json);

			Assert.AreEqual(1, deserialized.Item1.deleteItems.Count);
			Assert.AreEqual("item.tuna", deserialized.Item1.deleteItems[0].contentId);
			Assert.AreEqual(3, deserialized.Item1.deleteItems[0].itemId);
		}


		[Test]
		public void ChangeItems_Unset()
		{
			var builder = new InventoryUpdateBuilder();

			var json = InventoryUpdateBuilderSerializer.ToNetworkJson(builder);
			var deserialized = InventoryUpdateBuilderSerializer.FromNetworkJson(json);

			Assert.AreEqual(0, deserialized.Item1.updateItems.Count);
		}

		[Test]
		public void ChangeItems_WithStuff()
		{
			var builder = new InventoryUpdateBuilder();
			builder = builder.UpdateItem("item.tuna", 3, new Dictionary<string, string> { ["a"] = "b" });

			var json = InventoryUpdateBuilderSerializer.ToNetworkJson(builder);
			var deserialized = InventoryUpdateBuilderSerializer.FromNetworkJson(json);

			Assert.AreEqual(1, deserialized.Item1.updateItems.Count);
			Assert.AreEqual("item.tuna", deserialized.Item1.updateItems[0].contentId);
			Assert.AreEqual(3, deserialized.Item1.updateItems[0].itemId);
			Assert.AreEqual("b", deserialized.Item1.updateItems[0].properties["a"]);
		}

	}
}
