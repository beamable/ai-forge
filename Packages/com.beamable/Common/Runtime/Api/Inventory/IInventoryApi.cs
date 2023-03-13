using Beamable.Common.Content;
using Beamable.Common.Inventory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Common.Api.Inventory
{
	/// <summary>
	/// This type defines the %Client main entry point for the %Inventory feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature-overview">Inventory</a> feature documentation
	/// - See InventoryService script reference
	///
	/// </summary>
	public interface IInventoryApi : ISupportsGet<InventoryView>
	{
		/// <summary>
		/// Provides the VIP Bonus multipliers that are applicable for this player according to their tier.
		/// </summary>
		/// <returns></returns>
		Promise<GetMultipliersResponse> GetMultipliers();

		/// <summary>
		/// Players may sometimes receive additional currency as a result of qualifying for a VIP Tier
		/// This API previews what that amount of currency would be ahead of an update.
		/// </summary>
		/// <param name="currencyIdsToAmount"></param>
		/// <returns></returns>
		Promise<PreviewCurrencyGainResponse> PreviewCurrencyGain(Dictionary<string, long> currencyIdsToAmount);

		/// <summary>
		/// Sets the currency.
		/// If you need to make multiple inventory updates, use the <see cref="Update(System.Action{Beamable.Common.Api.Inventory.InventoryUpdateBuilder},string)"/> method.
		/// </summary>
		/// <param name="currencyId"></param>
		/// <param name="amount"></param>
		/// <param name="transaction"></param>
		/// <returns></returns>
		Promise<Unit> SetCurrency(string currencyId, long amount, string transaction = null);

		/// <summary>
		/// Sets the currency.
		/// If you need to make multiple inventory updates, use the <see cref="Update(System.Action{Beamable.Common.Api.Inventory.InventoryUpdateBuilder},string)"/> method.
		/// </summary>
		/// <param name="currency"></param>
		/// <param name="amount"></param>
		/// <param name="transaction"></param>
		/// <returns></returns>
		Promise<Unit> SetCurrency(CurrencyRef currency, long amount, string transaction = null);

		/// <summary>
		/// Adds the currency
		/// If you need to make multiple inventory updates, use the <see cref="Update(System.Action{Beamable.Common.Api.Inventory.InventoryUpdateBuilder},string)"/> method.
		/// </summary>
		/// <param name="currencyId"></param>
		/// <param name="amount"></param>
		/// <param name="transaction"></param>
		/// <returns></returns>
		Promise<Unit> AddCurrency(string currencyId, long amount, string transaction = null);

		/// <summary>
		/// Adds the currency
		/// If you need to make multiple inventory updates, use the <see cref="Update(System.Action{Beamable.Common.Api.Inventory.InventoryUpdateBuilder},string)"/> method.
		/// </summary>
		/// <param name="currency"></param>
		/// <param name="amount"></param>
		/// <param name="transaction"></param>
		/// <returns></returns>
		Promise<Unit> AddCurrency(CurrencyRef currency, long amount, string transaction = null);

		/// <summary>
		/// Set multiple currency values.
		/// If you need to make multiple inventory updates, use the <see cref="Update(System.Action{Beamable.Common.Api.Inventory.InventoryUpdateBuilder},string)"/> method.
		/// </summary>
		/// <param name="currencyIdsToAmount">A dictionary where the keys are content IDs of the currency, and the values are the new currency values for the player</param>
		/// <param name="transaction">An inventory transaction ID. Leave this argument empty.</param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call.</returns>
		Promise<Unit> SetCurrencies(Dictionary<string, long> currencyIdsToAmount, string transaction = null);

		/// <summary>
		/// Set multiple currency values.
		/// If you need to make multiple inventory updates, use the <see cref="Update(System.Action{Beamable.Common.Api.Inventory.InventoryUpdateBuilder},string)"/> method.
		/// </summary>
		/// <param name="currencyToAmount">A dictionary where the keys are <see cref="CurrencyRef"/>s, and the values are the new currency values for the player</param>
		/// <param name="transaction">An inventory transaction ID. Leave this argument empty.</param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call.</returns>
		Promise<Unit> SetCurrencies(Dictionary<CurrencyRef, long> currencyToAmount, string transaction = null);

		/// <summary>
		/// Add multiple currency values.
		/// If you need to make multiple inventory updates, use the <see cref="Update(System.Action{Beamable.Common.Api.Inventory.InventoryUpdateBuilder},string)"/> method.
		/// </summary>
		/// <param name="currencyIdsToAmount">A dictionary where the keys are content IDs of the currency, and the values are the new currency values for the player</param>
		/// <param name="transaction">An inventory transaction ID. Leave this argument empty.</param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call.</returns>
		Promise<Unit> AddCurrencies(Dictionary<string, long> currencyIdsToAmount, string transaction = null);

		/// <summary>
		/// Add multiple currency values.
		/// If you need to make multiple inventory updates, use the <see cref="Update(System.Action{Beamable.Common.Api.Inventory.InventoryUpdateBuilder},string)"/> method.
		/// </summary>
		/// <param name="currencyToAmount">A dictionary where the keys are <see cref="CurrencyRef"/>s, and the values are the new currency values for the player</param>
		/// <param name="transaction">An inventory transaction ID. Leave this argument empty.</param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call.</returns>
		Promise<Unit> AddCurrencies(Dictionary<CurrencyRef, long> currencyToAmount, string transaction = null);

		/// <summary>
		/// Get a set of currency values for the current player.
		/// </summary>
		/// <param name="currencyIds">the content IDs for the currencies that will be returned</param>
		/// <returns>
		/// A <see cref="Promise{T}"/> containing a dictionary where the keys are content IDs and the values are the player currency values.
		/// </returns>
		Promise<Dictionary<string, long>> GetCurrencies(string[] currencyIds);

		/// <summary>
		/// Get a set of currency values for the current player.
		/// </summary>
		/// <param name="currencyRefs"><see cref="CurrencyRef"/>s for the currencies that will be returned</param>
		/// <returns>
		/// A <see cref="Promise{T}"/> containing a dictionary where the keys are <see cref="CurrencyRef"/>s and the values are the player currency values.
		/// </returns>
		Promise<Dictionary<CurrencyRef, long>> GetCurrencies(CurrencyRef[] currencyRefs);

		/// <summary>
		/// Gets the currency.
		/// </summary>
		/// <param name="currencyId"></param>
		/// <returns></returns>
		Promise<long> GetCurrency(string currencyId);

		/// <summary>
		/// Gets the currency.
		/// </summary>
		/// <param name="currency">A <see cref="CurrencyRef"/></param>
		/// <returns></returns>
		Promise<long> GetCurrency(CurrencyRef currency);

		/// <summary>
		/// Set the <see cref="CurrencyProperty"/> values for a player's currency
		/// </summary>
		/// <param name="currencyId">The content id of the currency</param>
		/// <param name="properties">A list of <see cref="CurrencyProperty"/> values</param>
		/// <param name="transaction">An inventory transaction id.</param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call</returns>
		Promise<Unit> SetCurrencyProperties(string currencyId, List<CurrencyProperty> properties, string transaction = null);

		/// <summary>
		/// Set the <see cref="CurrencyProperty"/> values for a player's currency
		/// </summary>
		/// <param name="currency">A <see cref="CurrencyRef"/></param>
		/// <param name="properties">A list of <see cref="CurrencyProperty"/> values</param>
		/// <param name="transaction">An inventory transaction id</param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call</returns>
		Promise<Unit> SetCurrencyProperties(CurrencyRef currency, List<CurrencyProperty> properties, string transaction = null);

		/// <summary>
		/// Add an instance of the given <see cref="itemRef"/> to the player's inventory.
		/// If you need to make multiple inventory updates, use the <see cref="Update(System.Action{Beamable.Common.Api.Inventory.InventoryUpdateBuilder},string)"/> method.
		/// </summary>
		/// <param name="itemRef">A <see cref="ItemRef"/></param>
		/// <param name="properties">a set of instance properties for the new item</param>
		/// <param name="transaction">An inventory transaction id</param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call</returns>
		Promise<Unit> AddItem(ItemRef itemRef, Dictionary<string, string> properties = null, string transaction = null);

		/// <summary>
		/// Add an item instance of the given <see cref="contentId"/> to the player's inventory.
		/// If you need to make multiple inventory updates, use the <see cref="Update(System.Action{Beamable.Common.Api.Inventory.InventoryUpdateBuilder},string)"/> method.
		/// </summary>
		/// <param name="contentId">A content ID of the item type</param>
		/// <param name="properties">a set of instance properties for the new item</param>
		/// <param name="transaction">An inventory transaction ID. Leave this argument empty.</param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call</returns>
		Promise<Unit> AddItem(string contentId, Dictionary<string, string> properties = null, string transaction = null);

		/// <summary>
		/// Remove an item instance from the player's inventory.
		/// If you need to make multiple inventory updates, use the <see cref="Update(System.Action{Beamable.Common.Api.Inventory.InventoryUpdateBuilder},string)"/> method.
		/// </summary>
		/// <param name="contentId">The content ID of the type of item to remove</param>
		/// <param name="itemId">The runtime ID of the item to remove</param>
		/// <param name="transaction">an inventory transaction ID</param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call</returns>
		Promise<Unit> DeleteItem(string contentId, long itemId, string transaction = null);

		/// <summary>
		/// Update the instance level item properties of an item in the player's inventory.
		/// If you need to make multiple inventory updates, use the <see cref="Update(System.Action{Beamable.Common.Api.Inventory.InventoryUpdateBuilder},string)"/> method.
		/// </summary>
		/// <param name="itemRef">An <see cref="ItemRef"/> pointing to the content type of the item to update.</param>
		/// <param name="itemId">The runtime ID of the item to remove</param>
		/// <param name="properties">A new set of instance property values for the item. This will overwrite the existing properties.</param>
		/// <param name="transaction">an inventory transaction ID</param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call</returns>
		Promise<Unit> UpdateItem(ItemRef itemRef, long itemId, Dictionary<string, string> properties,
			string transaction = null);

		/// <summary>
		/// Update the instance level item properties of an item in the player's inventory.
		/// If you need to make multiple inventory updates, use the <see cref="Update(System.Action{Beamable.Common.Api.Inventory.InventoryUpdateBuilder},string)"/> method.
		/// </summary>
		/// <param name="contentId">A content ID pointing to the content type of the item to update.</param>
		/// <param name="itemId">The runtime ID of the item to remove</param>
		/// <param name="properties">A new set of instance property values for the item. This will overwrite the existing properties.</param>
		/// <param name="transaction">an inventory transaction ID</param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call</returns>
		Promise<Unit> UpdateItem(string contentId, long itemId, Dictionary<string, string> properties,
			string transaction = null);

		/// <summary>
		///	<inheritdoc cref="Update(InventoryUpdateBuilder,string)"/>
		/// </summary>
		/// <param name="action">A configurator for the <see cref="InventoryUpdateBuilder"/>. You should configure the
		/// builder with all of the inventory updates. If you already have an instance of the builder, use the
		/// <see cref="Update(InventoryUpdateBuilder,string)"/> method instead.</param>
		/// <param name="transaction">an inventory transaction ID</param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call</returns>
		Promise<Unit> Update(Action<InventoryUpdateBuilder> action, string transaction = null);

		/// <summary>
		/// Perform multiple updates to the player's inventory in one network call.
		/// The <see cref="InventoryUpdateBuilder"/> that you pass to this method will be converted into
		/// one update call to Beamable.
		/// </summary>
		/// <param name="builder">
		/// An <see cref="InventoryUpdateBuilder"/> containing all of the inventory updates.
		/// Use the <see cref="Update(System.Action{Beamable.Common.Api.Inventory.InventoryUpdateBuilder},string)"/> method
		/// to configure an <see cref="InventoryUpdateBuilder"/> instead.
		/// </param>
		/// <param name="transaction">an inventory transaction ID</param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call</returns>
		Promise<Unit> Update(InventoryUpdateBuilder builder, string transaction = null);

		/// <summary>
		/// Get every <see cref="InventoryObject{TContent}"/> that is of a specific item content type.
		/// </summary>
		/// <typeparam name="TContent">The type of content to retrieve. All children types will be included the result.</typeparam>
		/// <returns>A <see cref="Promise"/> containing the <see cref="InventoryObject{TContent}"/> that matches the given item type</returns>
		Promise<List<InventoryObject<TContent>>> GetItems<TContent>()
			where TContent : ItemContent, new();

		/// <summary>
		/// Get the  <see cref="InventoryObject{TContent}"/> that are of a specific item content type and match the given <see cref="itemReferences"/>
		/// </summary>
		/// <param name="itemReferences">
		/// Filter for only the items that match the given <see cref="ItemRef{TContent}"/> types.
		/// </param>
		/// <typeparam name="TContent">The type of content to retrieve. All children types will be included the result.</typeparam>
		/// <returns>A <see cref="Promise"/> containing the <see cref="InventoryObject{TContent}"/> that match the given item type</returns>
		Promise<List<InventoryObject<TContent>>> GetItems<TContent>(params ItemRef<TContent>[] itemReferences)
			where TContent : ItemContent, new();

	}

	/// <summary>
	/// This type defines the %content related to the %InventoryService.
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
	/// <typeparam name="TContent"></typeparam>
	[System.Serializable]
	public class InventoryObject<TContent> where TContent : ItemContent
	{
		/// <summary>
		/// The base piece of content that the inventory item derives from.
		/// </summary>
		public TContent ItemContent;

		/// <summary>
		/// The dynamic properties of the inventory item instance.
		/// </summary>
		public Dictionary<string, string> Properties;

		/// <summary>
		/// The id of the item within the inventory group.
		/// </summary>
		public long Id;

		/// <summary>
		/// The timestamp of when the item was added to the player inventory.
		/// </summary>
		public long CreatedAt;

		/// <summary>
		/// The timestamp of when the last modification to item occured.
		/// </summary>
		public long UpdatedAt;

		/// <summary>
		/// The code is a unique hashing code that combines the Content Id and the Item Id
		/// </summary>
		public int UniqueCode;
	}

	/// <summary>
	/// This type defines the %Inventory feature's get request.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[System.Serializable]
	public class GetInventoryResponse
	{
		public List<Currency> currencies;
	}

	/// <summary>
	/// This type defines the %Inventory feature's update request.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[System.Serializable]
	public class InventoryUpdateRequest
	{
		public string transaction; // will be set by api
		public Dictionary<string, long> currencies;
	}

	/// <summary>
	/// This type defines the response of fresh data loaded, related to the %InventoryService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class InventoryResponse
	{
		public string scope;
		public List<Currency> currencies = new List<Currency>();
		public List<ItemGroup> items = new List<ItemGroup>();

		private HashSet<string> _scopes;
		public HashSet<string> Scopes
		{
			get
			{
				if (_scopes == null)
				{
					if (!string.IsNullOrEmpty(scope))
						_scopes = new HashSet<string>(scope.Split(','));
					else
						_scopes = new HashSet<string>();
				}

				return _scopes;
			}
		}

		public HashSet<string> GetNotifyScopes(string[] givenScopes = null)
		{
			var notifyScopes = new HashSet<string>();
			notifyScopes.UnionWith(currencies.Select(currency => currency.id));
			notifyScopes.UnionWith(items.Select(item => item.id));
			notifyScopes.UnionWith(Scopes);
			notifyScopes.Add(""); // always notify the root scope
								  // TODO: if a scope is in notifySCopes, 'a.b.c', we should also make sure 'a.b', and 'a' are also in the set, so that item parent/child relationships are respected.
			if (givenScopes != null)
			{
				notifyScopes.UnionWith(givenScopes);
			}
			return ResolveAllScopes(notifyScopes);
		}

		private HashSet<string> ResolveAllScopes(IEnumerable<string> notifyScopes)
		{
			var resolved = new HashSet<string>();

			foreach (string notifyScope in notifyScopes)
			{
				var newScopes = ResolveScope(notifyScope);
				resolved.UnionWith(newScopes);
			}

			return resolved;
		}

		private HashSet<string> ResolveScope(string notifyScope)
		{
			var result = new HashSet<string>();
			string[] slicedScopes = notifyScope.Split('.');

			foreach (string slicedScope in slicedScopes)
			{
				if (result.Count == 0)
				{
					result.Add(slicedScope);
				}
				else
				{
					string newScope = string.Join(".", result.Last(), slicedScope);
					result.Add(newScope);
				}
			}

			return result;
		}

		private HashSet<string> ResolveMergeScopes(InventoryView view, string[] givenScopes = null)
		{
			var resolved = new HashSet<string>();
			var scopes = Scopes;

			var scopesLookup = new HashSet<string>();
			scopesLookup.UnionWith(scopes);

			// add the current scopes
			resolved.UnionWith(scopes);


			if (givenScopes != null)
			{
				resolved.UnionWith(givenScopes);
			}

			// the view may have data that is a child of a scope modified in the response.
			resolved.UnionWith(view.currencies.Keys.Where(currencyType => SetContainsPrefixOf(scopesLookup, currencyType)));
			resolved.UnionWith(view.items.Keys.Where(itemType => SetContainsPrefixOf(scopesLookup, itemType)));
			resolved.UnionWith(currencies.Select(currency => currency.id));
			resolved.UnionWith(items.Select(item => item.id));

			return resolved;
		}

		private bool SetContainsPrefixOf(HashSet<string> set, string element)
		{
			return set.Any(element.StartsWith);
		}

		public void MergeView(InventoryView view, string[] givenScopes = null)
		{
			var relevantScopes = ResolveMergeScopes(view, givenScopes);


			foreach (var contentId in view.currencies.Keys.ToList().Where(relevantScopes.Contains))
			{
				view.currencies.Remove(contentId);
			}

			foreach (var contentId in view.items.Keys.ToList().Where(relevantScopes.Contains))
			{
				view.items.Remove(contentId);
			}

			// handle entire item deletions. If an item's scope isn't in the view, its been deleted.



			foreach (var currency in currencies)
			{
				view.currencies[currency.id] = currency.amount;
				view.currencyProperties[currency.id] = currency.properties;
			}

			foreach (var itemGroup in items)
			{
				var itemViews = itemGroup.items.Select(item =>
				{
					ItemView itemView = new ItemView();
					itemView.id = long.Parse(item.id);
					var properties = new Dictionary<string, string>();
					if (item.properties != null)
					{
						foreach (var prop in item.properties)
						{
							if (properties.ContainsKey(prop.name))
							{
								BeamableLogger.LogWarning($"Inventory item has duplicate key. Overwriting existing key. item=[{itemGroup.id}] id=[{item.id}] key=[{prop.name}]");
							}
							properties[prop.name] = prop.value;
						}
					}
					itemView.properties = properties;
					itemView.createdAt = item.createdAt;
					itemView.updatedAt = item.updatedAt;

					return itemView;
				});

				List<ItemView> itemList = new List<ItemView>(itemViews);
				view.items[itemGroup.id] = itemList;
			}
		}
	}

	/// <summary>
	/// This type defines the multipliers response for the %InventoryService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class GetMultipliersResponse
	{
		public List<VipBonus> multipliers;
	}

	/// <summary>
	/// This type defines look-ahead %Currency data related to the %InventoryService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class PreviewCurrencyGainResponse
	{
		public List<CurrencyPreview> currencies;
	}

	/// <summary>
	/// This type defines look-ahead %Currency data related to the %InventoryService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class CurrencyPreview
	{
		public string id;
		public long amount;
		public long delta;
		public long originalAmount;
	}

	/// <summary>
	/// This type defines the %Beamable %currency %content related to the %InventoryService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class Currency
	{
		/// <summary>
		/// The currency content id.
		/// </summary>
		public string id;

		/// <summary>
		/// The amount of currency
		/// </summary>
		public long amount;

		/// <summary>
		/// Properties about the currency
		/// </summary>
		public List<CurrencyProperty> properties;
	}

	/// <summary>
	/// This type defines the %Beamable %currency feature's property structure
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class CurrencyProperty
	{
		/// <summary>
		/// The name of the property should be unique
		/// </summary>
		public string name;

		/// <summary>
		/// The value of the property can be any string
		/// </summary>
		public string value;
	}

	[Serializable]
	public class CurrencyPropertyList : DisplayableList<CurrencyProperty>
	{
		public List<CurrencyProperty> Properties = new List<CurrencyProperty>();
		
		public CurrencyPropertyList(List<CurrencyProperty> existing)
		{
			foreach (var elem in existing)
			{
				Add(elem);
			}
		}

		protected override IList InternalList => Properties;

		public override string GetListPropertyPath() => nameof(Properties);

		public new CurrencyProperty this[int index]
		{
			get => Properties[index];
			set => Properties[index] = value;
		}
	}

	[Serializable]
	public class
		SerializedDictionaryStringToCurrencyPropertyList : SerializableDictionaryStringToSomething<CurrencyPropertyList>
	{
		public SerializedDictionaryStringToCurrencyPropertyList() { }

		public SerializedDictionaryStringToCurrencyPropertyList(SerializedDictionaryStringToCurrencyPropertyList other)
		{
			foreach (var val in other)
			{
				Add(val.Key, new CurrencyPropertyList(val.Value.Properties));
			}
		}

		public SerializedDictionaryStringToCurrencyPropertyList(IDictionary<string, List<CurrencyProperty>> existing)
		{
			foreach (var kvp in existing)
			{
				Add(kvp.Key, new CurrencyPropertyList(kvp.Value));
			}
		}
	}
	
	/// <summary>
	/// This type defines the %Beamable item %content related to the %InventoryService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class Item
	{
		public string id;
		public Optional<string> proxyId;
		public List<ItemProperty> properties;
		public long createdAt;
		public long updatedAt;
	}

	/// <summary>
	/// This type defines a collection of inventory items.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class ItemGroup
	{
		public string id;
		public List<Item> items;
	}

	/// <summary>
	/// This type defines the %Inventory feature's update request.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class ItemProperty
	{
		public string name;
		public string value;
	}

	/// <summary>
	/// This type defines the render-friendly data of the %InventoryService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>

	[Serializable]
	public class InventoryView
	{
		public Dictionary<string, long> currencies = new Dictionary<string, long>();
		public Dictionary<string, List<CurrencyProperty>> currencyProperties = new Dictionary<string, List<CurrencyProperty>>();
		public Dictionary<string, List<ItemView>> items = new Dictionary<string, List<ItemView>>();
		
		public void Clear()
		{
			currencies.Clear();
			items.Clear();
		}
	}
	
	/// <summary>
	/// This type defines the render-friendly data of the %InventoryService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class ItemView
	{
		public long id;
		public Dictionary<string, string> properties = new Dictionary<string, string>();
		public long createdAt;
		public long updatedAt;
	}
}
