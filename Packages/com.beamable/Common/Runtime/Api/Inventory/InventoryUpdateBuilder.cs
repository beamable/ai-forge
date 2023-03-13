using Beamable.Common.Content;
using Beamable.Common.Inventory;
using Beamable.Common.Pooling;
using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Common.Api.Inventory
{
	/// <summary>
	/// This type defines the %Inventory feature's create request.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature-overview">Inventory</a> feature documentation
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class ItemCreateRequest
	{
		public string contentId;
		public SerializableDictionaryStringToString properties;
		public string requestId = Guid.NewGuid().ToString();
	}

	/// <summary>
	/// This type defines the %Inventory feature's delete request.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature-overview">Inventory</a> feature documentation
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class ItemDeleteRequest
	{
		public string contentId;
		public long itemId;
	}

	/// <summary>
	/// This type defines the %Inventory feature's update request.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature-overview">Inventory</a> feature documentation
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class ItemUpdateRequest
	{
		public string contentId;
		public long itemId;
		public SerializableDictionaryStringToString properties;
	}

	/// <summary>
	/// This type defines the %Inventory feature's updates.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature-overview">Inventory</a> feature documentation
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class InventoryUpdateBuilder : ISerializationCallbackReceiver
	{
		public readonly SerializableDictionaryStringToLong currencies;
		public readonly SerializedDictionaryStringToCurrencyPropertyList currencyProperties;
		public readonly List<ItemCreateRequest> newItems;
		public readonly List<ItemDeleteRequest> deleteItems;
		public readonly List<ItemUpdateRequest> updateItems;
		public bool? applyVipBonus;

		private enum SerializableNullableBool { NULL, TRUE, FALSE }
		[SerializeField]
		private SerializableNullableBool _serializedApplyVipBonus;

		[SerializeField]
		private SerializableDictionaryStringToLong _serializedCurrencies = new SerializableDictionaryStringToLong();
		[SerializeField]
		private SerializedDictionaryStringToCurrencyPropertyList _serializedCurrencyProperties = new SerializedDictionaryStringToCurrencyPropertyList();
		[SerializeField]
		private List<ItemCreateRequest> _serializedNewItems = new List<ItemCreateRequest>();
		[SerializeField]
		private List<ItemDeleteRequest> _serializedDeleteItems = new List<ItemDeleteRequest>();
		[SerializeField]
		private List<ItemUpdateRequest> _serializedUpdateItems = new List<ItemUpdateRequest>();

		/// <summary>
		/// Checks if the <see cref="InventoryUpdateBuilder"/> has any inventory updates.
		/// True if there are no updates, false otherwise.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return currencies.Count == 0 &&
					   currencyProperties.Count == 0 &&
					   newItems.Count == 0 &&
					   deleteItems.Count == 0 &&
					   updateItems.Count == 0;
			}
		}

		public InventoryUpdateBuilder()
		{
			currencies = new SerializableDictionaryStringToLong();
			currencyProperties = new SerializedDictionaryStringToCurrencyPropertyList();
			newItems = new List<ItemCreateRequest>();
			deleteItems = new List<ItemDeleteRequest>();
			updateItems = new List<ItemUpdateRequest>();
		}

		public InventoryUpdateBuilder(
			Dictionary<string, long> currencies,
			Dictionary<string, List<CurrencyProperty>> currencyProperties,
			List<ItemCreateRequest> newItems,
			List<ItemDeleteRequest> deleteItems,
			List<ItemUpdateRequest> updateItems
			) : this()
		{
			this.currencies = new SerializableDictionaryStringToLong(currencies);
			this.currencyProperties = new SerializedDictionaryStringToCurrencyPropertyList(currencyProperties);
			this.newItems = newItems;
			this.deleteItems = deleteItems;
			this.updateItems = updateItems;
		}

		public InventoryUpdateBuilder(InventoryUpdateBuilder clone)
		{
			this.currencies = new SerializableDictionaryStringToLong(clone.currencies);
			this.currencyProperties = new SerializedDictionaryStringToCurrencyPropertyList(clone.currencyProperties);
			this.newItems = new List<ItemCreateRequest>(clone.newItems);
			this.deleteItems = new List<ItemDeleteRequest>(clone.deleteItems);
			this.updateItems = new List<ItemUpdateRequest>(clone.updateItems);
			this.applyVipBonus = clone.applyVipBonus;
		}

		/// <summary>
		/// Mutate the <see cref="InventoryUpdateBuilder"/>'s <see cref="applyVipBonus"/> field.
		/// When the vip bonus is enabled, any currencies configured with the <see cref="CurrencyChange"/> method
		/// will have vip bonus multipliers included in the reward.
		/// </summary>
		/// <param name="apply">true to have currencies apply vip bonus, false otherwise</param>
		/// <returns>The mutated <see cref="InventoryUpdateBuilder"/></returns>
		public InventoryUpdateBuilder ApplyVipBonus(bool apply)
		{
			applyVipBonus = apply;

			return this;
		}

		/// <summary>
		/// Add or remove currency from the player's inventory.
		/// Multiple calls to this method for the same currency will combine into one currency update.
		/// <para> For example, if you changed up by 5, and then changed up again by 5, the final result would be 10 </para>
		/// </summary>
		/// <param name="contentId">The content ID for a currency value</param>
		/// <param name="amount">The amount to change the given currency. Positive numbers add currency, and negative numbers subtract currency.</param>
		/// <returns>The mutated <see cref="InventoryUpdateBuilder"/></returns>
		public InventoryUpdateBuilder CurrencyChange(string contentId, long amount)
		{
			if (currencies.TryGetValue(contentId, out var currentValue))
			{
				currencies[contentId] = currentValue + amount;
			}
			else
			{
				currencies.Add(contentId, amount);
			}

			return this;
		}

		/// <summary>
		/// Set the <see cref="CurrencyProperty"/> values for a currency.
		/// This will overwrite the previous currency properties.
		/// </summary>
		/// <param name="contentId">The content ID for a currency value</param>
		/// <param name="properties">A list of <see cref="CurrencyProperty"/> values</param>
		/// <returns>The mutated <see cref="InventoryUpdateBuilder"/></returns>
		public InventoryUpdateBuilder SetCurrencyProperties(string contentId, List<CurrencyProperty> properties)
		{
			currencyProperties[contentId] = new CurrencyPropertyList(properties);

			return this;
		}

		/// <summary>
		/// Add an item instance to the inventory.
		/// </summary>
		/// <param name="contentId">The content ID for an item type</param>
		/// <param name="properties">A set of instance level item properties</param>
		/// <param name="requestId">An ID that symbolizes the addition of the item. By default, this will be set to a random GUID. </param>
		/// <returns>The mutated <see cref="InventoryUpdateBuilder"/></returns>
		public InventoryUpdateBuilder AddItem(string contentId, Dictionary<string, string> properties = null, string requestId = null)
		{
			newItems.Add(new ItemCreateRequest
			{
				contentId = contentId,
				properties = new SerializableDictionaryStringToString(properties),
				requestId = requestId ?? Guid.NewGuid().ToString()
			});

			return this;
		}

		/// <summary>
		/// Add an item instance to the inventory.
		/// </summary>
		/// <param name="itemRef">An <see cref="ItemRef"/> for the item type</param>
		/// <param name="properties">A set of instance level item properties</param>
		/// <returns>The mutated <see cref="InventoryUpdateBuilder"/></returns>
		public InventoryUpdateBuilder AddItem(ItemRef itemRef, Dictionary<string, string> properties = null)
			=> AddItem(itemRef.Id, properties);

		/// <summary>
		/// Remove a specific item instance from the inventory
		/// </summary>
		/// <param name="contentId">The content ID for an item type</param>
		/// <param name="itemId">The item instance ID</param>
		/// <returns>The mutated <see cref="InventoryUpdateBuilder"/></returns>
		public InventoryUpdateBuilder DeleteItem(string contentId, long itemId)
		{
			deleteItems.Add(new ItemDeleteRequest
			{
				contentId = contentId,
				itemId = itemId
			});

			return this;
		}

		/// <summary>
		/// Remove a specific item instance from the inventory
		/// </summary>
		/// <param name="itemId">The item instance ID</param>
		/// <typeparam name="TContent">The type of item to remove</typeparam>
		/// <returns>The mutated <see cref="InventoryUpdateBuilder"/></returns>
		public InventoryUpdateBuilder DeleteItem<TContent>(long itemId) where TContent : ItemContent, new()
		{
			var contentId = ContentTypeReflectionCache.Instance.TypeToName(typeof(TContent));
			return DeleteItem(contentId, itemId);
		}

		/// <summary>
		/// Remove a specific item instance from the inventory
		/// </summary>
		/// <param name="item">The <see cref="InventoryObject{TContent}"/> to remove from the inventory</param>
		/// <typeparam name="TContent">The type of item to remove</typeparam>
		/// <returns>The mutated <see cref="InventoryUpdateBuilder"/></returns>
		public InventoryUpdateBuilder DeleteItem<TContent>(InventoryObject<TContent> item)
			where TContent : ItemContent, new()
		{
			return DeleteItem(item.ItemContent.Id, item.Id);
		}

		/// <summary>
		/// Update the instance properties of a specific item
		/// </summary>
		/// <param name="contentId">The content ID for an item type</param>
		/// <param name="itemId">The item instance ID</param>
		/// <param name="properties">
		/// The new instance properties for the item. This will overwrite the existing properties.
		/// </param>
		/// <returns>The mutated <see cref="InventoryUpdateBuilder"/></returns>
		public InventoryUpdateBuilder UpdateItem(string contentId, long itemId, Dictionary<string, string> properties)
		{
			updateItems.Add(new ItemUpdateRequest
			{
				contentId = contentId,
				itemId = itemId,
				properties = new SerializableDictionaryStringToString(properties)
			});

			return this;
		}

		/// <summary>
		/// Update the instance properties of a specific item
		/// </summary>
		/// <param name="itemId">The item instance ID</param>
		/// <param name="properties">
		/// The new instance properties for the item. This will overwrite the existing properties.
		/// </param>
		/// <typeparam name="TContent">The type of item to remove</typeparam>
		/// <returns>The mutated <see cref="InventoryUpdateBuilder"/></returns>
		public InventoryUpdateBuilder UpdateItem<TContent>(long itemId, Dictionary<string, string> properties)
			where TContent : ItemContent, new()
		{
			var contentId = ContentTypeReflectionCache.Instance.TypeToName(typeof(TContent));
			return UpdateItem(contentId, itemId, properties);
		}

		/// <summary>
		/// Update the instance properties of a specific item
		/// </summary>
		/// <param name="item">The <see cref="InventoryObject{TContent}"/> to remove from the inventory</param>
		/// <typeparam name="TContent">The type of item to remove</typeparam>
		/// <returns>The mutated <see cref="InventoryUpdateBuilder"/></returns>
		public InventoryUpdateBuilder UpdateItem<TContent>(InventoryObject<TContent> item)
			where TContent : ItemContent, new()
		{
			return UpdateItem(item.ItemContent.Id, item.Id, item.Properties);
		}

		/// <summary>
		/// Get a set of inventory scopes that the updater will affect.
		/// </summary>
		/// <returns>A set of scopes that will be changed based on the changes described in the builder</returns>
		public HashSet<string> BuildScopes()
		{
			var scopes = new HashSet<string>();
			foreach (var item in newItems)
			{
				scopes.Add(item.contentId);
			}

			foreach (var item in updateItems)
			{
				scopes.Add(item.contentId);
			}

			foreach (var item in deleteItems)
			{
				scopes.Add(item.contentId);
			}

			foreach (var curr in currencies)
			{
				scopes.Add(curr.Key);
			}

			foreach (var curr in currencyProperties)
			{
				scopes.Add(curr.Key);
			}
			return scopes;
		}

		public InventoryUpdateBuilder Concat(InventoryUpdateBuilder builder)
		{
			var next = new InventoryUpdateBuilder(this);
			if (builder.applyVipBonus.HasValue)
			{
				next.applyVipBonus = builder.applyVipBonus;
			}

			foreach (var kvp in builder.currencyProperties)
			{
				next.currencyProperties[kvp.Key] = kvp.Value;
			}

			foreach (var curr in builder.currencies)
			{
				next.CurrencyChange(curr.Key, curr.Value);
			}

			next.newItems.AddRange(builder.newItems);
			next.updateItems.AddRange(builder.updateItems);
			next.deleteItems.AddRange(builder.deleteItems);
			return next;
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			_serializedApplyVipBonus = applyVipBonus == null
				? SerializableNullableBool.NULL
				: (applyVipBonus == true ? SerializableNullableBool.TRUE : SerializableNullableBool.FALSE);

			_serializedCurrencies = currencies;
			_serializedCurrencyProperties = currencyProperties;
			_serializedDeleteItems = deleteItems;
			_serializedNewItems = newItems;
			_serializedUpdateItems = updateItems;
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			switch (_serializedApplyVipBonus)
			{
				case SerializableNullableBool.NULL:
					applyVipBonus = null;
					break;
				case SerializableNullableBool.TRUE:
					applyVipBonus = true;
					break;
				case SerializableNullableBool.FALSE:
					applyVipBonus = false;
					break;
			}

			currencies.Clear();
			foreach (var curr in _serializedCurrencies)
			{
				currencies.Add(curr.Key, curr.Value);
			}
			currencyProperties.Clear();
			foreach (var curr in _serializedCurrencyProperties)
			{
				currencyProperties.Add(curr.Key, curr.Value);
			}
			deleteItems.Clear();
			deleteItems.AddRange(_serializedDeleteItems);
			newItems.Clear();
			newItems.AddRange(_serializedNewItems);
			updateItems.Clear();
			updateItems.AddRange(_serializedUpdateItems);
		}
	}

	public static class InventoryUpdateBuilderSerializer
	{
		private const string TRANSACTION = "transaction";
		private const string APPLY_VIP_BONUS = "applyVipBonus";
		private const string CURRENCIES = "currencies";
		private const string CURRENCY_PROPERTIES = "currencyProperties";
		private const string CURRENCY_PROPERTY_NAME = "name";
		private const string CURRENCY_PROPERTY_VALUE = "value";
		private const string NEW_ITEMS = "newItems";
		private const string NEW_ITEM_CONTENT_ID = "contentId";
		private const string NEW_ITEM_PROPERTIES = "properties";
		private const string NEW_ITEM_PROPERTY_NAME = "name";
		private const string NEW_ITEM_PROPERTY_VALUE = "value";
		private const string NEW_ITEM_REQ_ID = "reqId";
		private const string DELETE_ITEMS = "deleteItems";
		private const string DELETE_ITEMS_CONTENT_ID = "contentId";
		private const string DELETE_ITEMS_ITEM_ID = "id";
		private const string UPDATE_ITEMS = "updateItems";
		private const string UPDATE_ITEMS_ITEM_ID = "id";
		private const string UPDATE_ITEMS_CONTENT_ID = "contentId";
		private const string UPDATE_ITEMS_PROPERTIES = "properties";




		public static (InventoryUpdateBuilder, string) FromNetworkJson(string json)
		{
			string transaction = null;
			bool? applyVipBonus = null;
			var currencies = new Dictionary<string, long>();
			var currencyProperties = new Dictionary<string, List<CurrencyProperty>>();
			var newItems = new List<ItemCreateRequest>();
			var deleteItems = new List<ItemDeleteRequest>();
			var updateItems = new List<ItemUpdateRequest>();

			var dict = Json.Deserialize(json) as ArrayDict;
			if (dict.TryGetValue(TRANSACTION, out var transactionObj) && transactionObj is string storedTransaction)
			{
				transaction = storedTransaction;
			}

			if (dict.TryGetValue(APPLY_VIP_BONUS, out var applyVipBonusObj) && applyVipBonusObj is bool storedApplyVipBonus)
			{
				applyVipBonus = storedApplyVipBonus;
			}

			if (dict.TryGetValue(CURRENCIES, out var currencyObj) && currencyObj is ArrayDict storedCurrencies)
			{
				currencies = storedCurrencies.ToDictionary(kvp => kvp.Key, kvp => (long)kvp.Value); ;
			}

			if (dict.TryGetValue(CURRENCY_PROPERTIES, out var currPropsObj) && currPropsObj is ArrayDict storedCurrProps)
			{
				currencyProperties = storedCurrProps.ToDictionary(
					kvp => kvp.Key,
					kvp =>
					{
						List<CurrencyProperty> props = null;
						if (kvp.Value is List<object> objs)
						{
							var subDict = objs.Cast<ArrayDict>();
							props = subDict.Select(x => new CurrencyProperty
							{
								name = x[CURRENCY_PROPERTY_NAME]?.ToString(),
								value = x[CURRENCY_PROPERTY_VALUE]?.ToString(),
							}).ToList();
						}
						return props ?? new List<CurrencyProperty>();
					});
			}

			if (dict.TryGetValue(NEW_ITEMS, out var newItemsObjs) && newItemsObjs is List<object> newItemsObjList)
			{
				var subDicts = newItemsObjList.Cast<ArrayDict>().ToList();
				newItems = subDicts.Select(x =>
				{
					var propsObjs = ((List<object>)x[NEW_ITEM_PROPERTIES]).Cast<ArrayDict>().ToList();
					var propsDict = propsObjs.ToDictionary(p => p[NEW_ITEM_PROPERTY_NAME]?.ToString(),
														   p => p[NEW_ITEM_PROPERTY_VALUE]?.ToString());
					return new ItemCreateRequest
					{
						contentId = x[NEW_ITEM_CONTENT_ID]?.ToString(),
						requestId = x[NEW_ITEM_REQ_ID]?.ToString(),
						properties = new SerializableDictionaryStringToString(propsDict)
					};
				}).ToList();
			}

			if (dict.TryGetValue(DELETE_ITEMS, out var deleteItemsObj) && deleteItemsObj is List<object> deleteItemsObjList)
			{
				var subDicts = deleteItemsObjList.Cast<ArrayDict>().ToList();
				deleteItems = subDicts.Select(x =>
				{
					return new ItemDeleteRequest()
					{
						contentId = x[DELETE_ITEMS_CONTENT_ID]?.ToString(),
						itemId = long.Parse(x[DELETE_ITEMS_ITEM_ID]?.ToString())
					};
				}).ToList();
			}

			if (dict.TryGetValue(UPDATE_ITEMS, out var updateItemsObj) && updateItemsObj is List<object> updateItemsObjList)
			{
				var subDicts = updateItemsObjList.Cast<ArrayDict>().ToList();
				updateItems = subDicts.Select(x =>
				{
					var propsObjs = ((List<object>)x[UPDATE_ITEMS_PROPERTIES]).Cast<ArrayDict>().ToList();
					var propsDict = propsObjs.ToDictionary(p => p[NEW_ITEM_PROPERTY_NAME]?.ToString(),
														   p => p[NEW_ITEM_PROPERTY_VALUE]?.ToString());
					return new ItemUpdateRequest()
					{
						contentId = x[UPDATE_ITEMS_CONTENT_ID]?.ToString(),
						itemId = long.Parse(x[UPDATE_ITEMS_ITEM_ID]?.ToString()),
						properties = new SerializableDictionaryStringToString(propsDict)
					};
				}).ToList();
			}

			var builder = new InventoryUpdateBuilder(currencies, currencyProperties, newItems, deleteItems, updateItems);
			builder.applyVipBonus = applyVipBonus;

			return (builder, transaction);
		}


		public static string ToUnityJson(InventoryUpdateBuilder builder) =>
			JsonUtility.ToJson(builder);

		public static InventoryUpdateBuilder FromUnityJson(string json) =>
			JsonUtility.FromJson<InventoryUpdateBuilder>(json);


		public static string ToNetworkJson(InventoryUpdateBuilder builder, string transaction = null)
		{
			using (var pooledBuilder = StringBuilderPool.StaticPool.Spawn())
			{
				var dict = new ArrayDict();
				if (!string.IsNullOrEmpty(transaction))
				{
					dict.Add(TRANSACTION, transaction);
				}

				if (builder.applyVipBonus.HasValue)
				{
					dict.Add(APPLY_VIP_BONUS, builder.applyVipBonus.Value);
				}

				if (builder.currencies != null && builder.currencies.Count > 0)
				{
					dict.Add(CURRENCIES, builder.currencies);
				}

				if (builder.currencyProperties != null && builder.currencyProperties.Count > 0)
				{
					var currencyDict = new ArrayDict();
					foreach (var kvp in builder.currencyProperties)
					{
						var newProperties = kvp.Value.Properties.Select(newProperty => new ArrayDict
						{
							{CURRENCY_PROPERTY_NAME, newProperty.name},
							{CURRENCY_PROPERTY_VALUE, newProperty.value}
						}).ToArray();
						currencyDict.Add(kvp.Key, newProperties);
					}

					dict.Add(CURRENCY_PROPERTIES, currencyDict);
				}

				if (builder.newItems != null && builder.newItems.Count > 0)
				{
					var newItems = builder.newItems.Select(newItem => new ArrayDict
					{
						{NEW_ITEM_REQ_ID, newItem.requestId},
						{NEW_ITEM_CONTENT_ID, newItem.contentId},
						{
							NEW_ITEM_PROPERTIES, newItem.properties
														?.Select(
															kvp => new ArrayDict {{NEW_ITEM_PROPERTY_NAME, kvp.Key}, {NEW_ITEM_PROPERTY_VALUE, kvp.Value}})
														.ToArray() ?? new object[] { }
						}
					}).ToArray();

					dict.Add(NEW_ITEMS, newItems);
				}

				if (builder.deleteItems != null && builder.deleteItems.Count > 0)
				{
					var deleteItems = builder.deleteItems.Select(deleteItem => new ArrayDict
					{
						{DELETE_ITEMS_CONTENT_ID, deleteItem.contentId},
						{DELETE_ITEMS_ITEM_ID, deleteItem.itemId}
					}).ToArray();

					dict.Add(DELETE_ITEMS, deleteItems);
				}

				if (builder.updateItems != null && builder.updateItems.Count > 0)
				{
					var updateItems = builder.updateItems.Select(updateItem => new ArrayDict
					{
						{"contentId", updateItem.contentId},
						{"id", updateItem.itemId},
						{
							"properties", updateItem.properties
													.Select(kvp => new ArrayDict
													{
														{"name", kvp.Key}, {"value", kvp.Value}
													}).ToArray()
						}
					}).ToArray();

					dict.Add(UPDATE_ITEMS, updateItems);
				}

				var json = Json.Serialize(dict, pooledBuilder.Builder);
				return json;
			}
		}
	}
}
