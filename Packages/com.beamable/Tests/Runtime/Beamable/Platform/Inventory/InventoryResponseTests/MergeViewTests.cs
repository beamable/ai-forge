using Beamable.Common.Api.Inventory;
using NUnit.Framework;
using System.Collections.Generic;

namespace Beamable.Platform.Tests.Inventory.InventoryResponseTests
{
	public class MergeViewTests
	{
		[Test]
		public void EmptyView_PlusEmptyResponse_YieldsEmptyView()
		{
			var res = new InventoryResponse
			{
				scope = "items"
			};
			var view = new InventoryView();
			res.MergeView(view);

			Assert.AreEqual(0, view.currencies.Count);
			Assert.AreEqual(0, view.items.Count);
		}

		[Test]
		public void AddNewCurrencyToEmptyView()
		{
			var res = new InventoryResponse
			{
				scope = "currency",
				currencies = new List<Currency> { new Currency { amount = 1, id = "currency.gems" } }
			};
			var view = new InventoryView();
			res.MergeView(view);

			Assert.AreEqual(1, view.currencies.Count);
			Assert.AreEqual(0, view.items.Count);
		}

		[Test]
		public void AddNewCurrencyToNonEmptyView()
		{
			var res = new InventoryResponse
			{
				scope = "currency.gems",
				currencies = new List<Currency> { new Currency { amount = 1, id = "currency.gems" } }
			};
			var view = new InventoryView
			{
				currencies = new Dictionary<string, long>
			{
			   {"currency.tunas", 1}
			}
			};
			res.MergeView(view);

			Assert.AreEqual(2, view.currencies.Count);
			Assert.AreEqual(true, view.currencies.ContainsKey("currency.gems"));
			Assert.AreEqual(true, view.currencies.ContainsKey("currency.tunas"));

			Assert.AreEqual(0, view.items.Count);
		}

		[Test]
		public void UpdateCurrency()
		{
			var res = new InventoryResponse
			{
				scope = "currency.gems",
				currencies = new List<Currency> { new Currency { amount = 3, id = "currency.gems" } }
			};
			var view = new InventoryView
			{
				currencies = new Dictionary<string, long>
			{
			   {"currency.gems", 2}
			}
			};
			res.MergeView(view);

			Assert.AreEqual(1, view.currencies.Count);
			Assert.AreEqual(true, view.currencies.ContainsKey("currency.gems"));
			Assert.AreEqual(3, view.currencies["currency.gems"]);
			Assert.AreEqual(0, view.items.Count);
		}

		[Test]
		public void UpdateCurrency_AndAddOne_AndDisregardAnother()
		{
			var res = new InventoryResponse
			{
				scope = "currency.gems,currency.tunas",
				currencies = new List<Currency> { new Currency { amount = 3, id = "currency.gems" }, new Currency { amount = 4, id = "currency.tunas" } }
			};
			var view = new InventoryView
			{
				currencies = new Dictionary<string, long>
			{
			   {"currency.gems", 2},
			   {"currency.tunas", 2},
			   {"currency.ignoreme", 2},
			}
			};
			res.MergeView(view);

			Assert.AreEqual(3, view.currencies.Count);
			Assert.AreEqual(true, view.currencies.ContainsKey("currency.gems"));
			Assert.AreEqual(true, view.currencies.ContainsKey("currency.tunas"));
			Assert.AreEqual(true, view.currencies.ContainsKey("currency.ignoreme"));
			Assert.AreEqual(3, view.currencies["currency.gems"]);
			Assert.AreEqual(4, view.currencies["currency.tunas"]);
			Assert.AreEqual(2, view.currencies["currency.ignoreme"]);
			Assert.AreEqual(0, view.items.Count);
		}

		[Test]
		public void DeleteFromView()
		{
			var res = new InventoryResponse
			{
				scope = "currency",
				// empty currency set...
			};
			var view = new InventoryView
			{
				currencies = new Dictionary<string, long>
			{
			   {"currency.gems", 1}
			}
			};
			res.MergeView(view);

			Assert.AreEqual(0, view.currencies.Count);
			Assert.AreEqual(0, view.items.Count);
		}

		[Test]
		public void DeleteUnrelatedFromView()
		{
			var res = new InventoryResponse
			{
				scope = "currency.blah",
				// empty currency set...
			};
			var view = new InventoryView
			{
				currencies = new Dictionary<string, long>
			{
			   {"currency.gems", 1}
			}
			};
			res.MergeView(view);

			Assert.AreEqual(1, view.currencies.Count);
			Assert.AreEqual(true, view.currencies.ContainsKey("currency.gems"));
			Assert.AreEqual(0, view.items.Count);
		}

		[Test]
		public void DeleteUnrelatedSibling()
		{
			var res = new InventoryResponse
			{
				scope = "currency.gems.b",
				// empty currency set...
			};
			var view = new InventoryView
			{
				currencies = new Dictionary<string, long>
			{
			   {"currency.gems.a", 1}
			}
			};
			res.MergeView(view);

			Assert.AreEqual(1, view.currencies.Count);
			Assert.AreEqual(0, view.items.Count);
		}

		[Test]
		public void Pause()
		{
			// if the response comes back with a parent scope, items.sword vs items.weapons.sword
			var res = new InventoryResponse
			{
				scope = "currency",
				// empty currency set...
			};
			var view = new InventoryView
			{
				currencies = new Dictionary<string, long>
			{
			   {"currency.gems.a", 1}
			}
			};
			res.MergeView(view);

			Assert.AreEqual(0, view.currencies.Count);
			Assert.AreEqual(0, view.items.Count);
		}

		[Test]
		public void DeleteParentButNotChild()
		{
			// if the response comes back with a parent scope, items.sword vs items.weapons.sword
			var res = new InventoryResponse
			{
				scope = "currency.gems.a",
				// empty currency set...
			};
			var view = new InventoryView
			{
				currencies = new Dictionary<string, long>
			{
			   {"currency.gems.a", 1},
			   {"currency.gems", 1},
			}
			};
			res.MergeView(view);

			Assert.AreEqual(1, view.currencies.Count);
			Assert.AreEqual(true, view.currencies.ContainsKey("currency.gems"));
			Assert.AreEqual(0, view.items.Count);
		}

		[Test]
		public void DeleteUnrelatedFromView_SimilarNames()
		{
			var res = new InventoryResponse
			{
				scope = "currency.gems2",
				// empty currency set...
			};
			var view = new InventoryView
			{
				currencies = new Dictionary<string, long>
			{
			   {"currency.gems", 1}
			}
			};
			res.MergeView(view);

			Assert.AreEqual(1, view.currencies.Count);
			Assert.AreEqual(0, view.items.Count);
		}
	}
}
