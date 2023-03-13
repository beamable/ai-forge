using Beamable.Common.Api.Inventory;
using Beamable.Pooling;
using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Server
{
	public class InventoryViewEx
	{
		[SerializeField] string _serializedData;

		public InventoryViewEx(InventoryView inventoryView)
		{
			using (var pooledBuilder = StringBuilderPool.StaticPool.Spawn())
			{
				var dict = new ArrayDict();

				if (inventoryView.currencies != null && inventoryView.currencies.Count > 0)
				{
					dict.Add(nameof(inventoryView.currencies), inventoryView.currencies);
				}

				if (inventoryView.currencyProperties != null && inventoryView.currencyProperties.Count > 0)
				{
					var currencyDict = new ArrayDict();
					foreach (var kvp in inventoryView.currencyProperties)
					{
						var newProperties = kvp.Value.Select(newProperty => new ArrayDict
						{
							{nameof(CurrencyProperty.name), newProperty.name},
							{nameof(CurrencyProperty.value), newProperty.value}
						}).ToArray();
						currencyDict.Add(kvp.Key, newProperties);
					}

					dict.Add(nameof(InventoryView.currencyProperties), currencyDict);
				}

				if (inventoryView.items != null && inventoryView.items.Count > 0)
				{
					var itemsDict = new ArrayDict();
					foreach (var kvp in inventoryView.items)
					{
						var singleItem = kvp.Value.Select(selectedItem => new ArrayDict
						{
							{nameof(ItemView.id), selectedItem.id},
							{nameof(ItemView.createdAt), selectedItem.createdAt},
							{nameof(ItemView.updatedAt), selectedItem.updatedAt},
							{
								nameof(ItemView.properties),
								selectedItem.properties
								            .Select(newProperty =>
									                    new ArrayDict
									                    {
										                    {
											                    nameof(newProperty.Key),
											                    newProperty.Key
										                    },
										                    {
											                    nameof(newProperty.Value),
											                    newProperty.Value
										                    }
									                    }).ToArray()
							}
						}).ToArray();

						itemsDict.Add(kvp.Key, singleItem);
					}

					dict.Add(nameof(InventoryView.items), itemsDict);
				}

				_serializedData = Json.Serialize(dict, pooledBuilder.Builder);
			}
		}

		public static InventoryView DeserializeToInventoryView(string json)
		{
			Dictionary<string, string> ExtractProperty(ArrayDict dc, string name)
			{
				if (dc.TryGetValue(name, out var currencyObj) &&
				    currencyObj is List<object> objs)
				{
					var subDict = objs.Cast<ArrayDict>();

					var nn = subDict.Select(x => new KeyValuePair<string, string>(
						                        x["Key"]?.ToString(),
						                        x["Value"]?.ToString())).ToDictionary(item => item.Key,
						item => item.Value);

					return nn;
				}

				return null;
			}

			InventoryView tmp = new InventoryView();

			var dict = Json.Deserialize(json) as ArrayDict;

			if (dict != null)
			{
				if (dict.TryGetValue(nameof(InventoryView.currencies), out var currencyObj) &&
				    currencyObj is ArrayDict storedCurrencies)
				{
					tmp.currencies = storedCurrencies.ToDictionary(kvp => kvp.Key, kvp => (long)kvp.Value);
					;
				}

				if (dict.TryGetValue(nameof(InventoryView.currencyProperties), out var currPropsObj) &&
				    currPropsObj is ArrayDict storedCurrProps)
				{
					tmp.currencyProperties = storedCurrProps.ToDictionary(
						kvp => kvp.Key,
						kvp =>
						{
							List<CurrencyProperty> props = null;
							if (kvp.Value is List<object> objs)
							{
								var subDict = objs.Cast<ArrayDict>();
								props = subDict.Select(x => new CurrencyProperty
								{
									name = x[nameof(CurrencyProperty.name)]?.ToString(),
									value = x[nameof(CurrencyProperty.value)]?.ToString(),
								}).ToList();
							}

							return props ?? new List<CurrencyProperty>();
						});
				}

				if (dict.TryGetValue(nameof(InventoryView.items), out var currItemsObj) &&
				    currItemsObj is ArrayDict storedItems)
				{
					tmp.items = storedItems.ToDictionary(
						kvp => kvp.Key,
						kvp =>
						{
							List<ItemView> props = null;
							if (kvp.Value is List<object> objs)
							{
								var subDict = objs.Cast<ArrayDict>();
								props = subDict.Select(x => new ItemView()
								{
									id = long.Parse(
										x[nameof(ItemView.id)]?.ToString() ?? string.Empty),
									createdAt =
										long.Parse(
											x[nameof(ItemView.createdAt)]?.ToString() ??
											string.Empty),
									updatedAt =
										long.Parse(
											x[nameof(ItemView.updatedAt)]?.ToString() ??
											string.Empty),
									properties =
										ExtractProperty(x, nameof(ItemView.properties))
								}).ToList();
							}

							return props ?? new List<ItemView>();
						});
				}
			}

			return tmp;
		}
	}
}
