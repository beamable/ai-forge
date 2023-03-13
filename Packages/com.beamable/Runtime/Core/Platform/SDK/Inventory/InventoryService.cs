using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Inventory;
using Beamable.Common.Dependencies;

namespace Beamable.Api.Inventory
{
	/// <summary>
	/// This type defines the reset/refresh of related data.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
	/// - See Beamable.Api.Inventory.InventoryService script reference
	///
	/// ![img beamable-logo]
	///
	/// Scopes are content ids!!
	///
	/// </summary>
	public class InventorySubscription : PlatformSubscribable<InventoryResponse, InventoryView>
	{
		private const string SERVICE = "inventory";

		private readonly InventoryView view = new InventoryView();

		private bool _everReceivedData;

		public InventorySubscription(IDependencyProvider provider)
			: base(provider, SERVICE, new BeamableGetApiResourceViaPost<InventoryResponse>(true, OfflineResponse))
		{
			UsesHierarchyScopes = true;
		}

		public InventorySubscription(IDependencyProvider provider, bool useCache)
			: base(provider, SERVICE, new BeamableGetApiResourceViaPost<InventoryResponse>(useCache, OfflineResponse))
		{
			UsesHierarchyScopes = true;
		}

		private static InventoryResponse OfflineResponse(string url)
		{
			return new InventoryResponse
			{
				scope = ""
			};
		}

		protected override void Reset()
		{
			view.Clear();
		}


		protected override Promise OnRefresh(InventoryResponse data, string[] scopes)
		{
			if (!connectivityService.HasConnectivity && _everReceivedData)
			{
				foreach (var scope in data.GetNotifyScopes(scopes))
				{
					Notify(scope, view);
				}
				return Promise.Success;
			}

			_everReceivedData = true;
			data.MergeView(view, scopes);
			foreach (var scope in data.GetNotifyScopes(scopes))
			{
				Notify(scope, view);
			}

			return Promise.Success;
		}

		public InventoryView GetCurrentView() => view;

	}

	/// <summary>
	/// This type defines the %Client main entry point for the %Inventory feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature">Inventory</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ### Example
	/// This demonstrates example usage.
	///
	/// ```
	///
	/// private async void SetupBeamable()
	/// {
	///
	///   var beamableAPI = await Beamable.API.Instance;
	///
	///   beamableAPI.InventoryService.Subscribe("items", view =>
	///   {
	///
	///     foreach (KeyValuePair<string, List<ItemView>> kvp in view.items)
	///     {
	///
	///       string inventoryItemName = $"{kvp.Key} x {kvp.Value.Count}"; // "Big Sword x 1"
	///
	///     }
	///
	///   });
	///
	/// }
	///
	/// ```
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class InventoryService : AbsInventoryApi, IHasPlatformSubscriber<InventorySubscription, InventoryResponse, InventoryView>
	{
		public InventorySubscription Subscribable { get; }

		public InventoryService(IDependencyProvider provider, InventorySubscription subscription) : base(provider.GetService<IBeamableRequester>(), provider.GetService<IUserContext>())
		{
			Subscribable = subscription;
		}

		public InventoryService(IDependencyProvider provider) : base(provider.GetService<IBeamableRequester>(), provider.GetService<IUserContext>())
		{
			Subscribable = new InventorySubscription(provider);
		}

		public InventoryService(IPlatformService platform, IBeamableRequester requester, IDependencyProvider provider) : base(requester, platform)
		{
			Subscribable = new InventorySubscription(provider);
		}

		public override Promise<InventoryView> GetCurrent(string scope = "") => Subscribable.GetCurrent(scope);

	}

	public class CachelessInventoryService : InventoryService
	{
		public CachelessInventoryService(IDependencyProvider provider) : base(provider, new InventorySubscription(provider, false))
		{
		}
	}
}

