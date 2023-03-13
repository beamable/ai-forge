using Beamable.Api.Payments;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Dependencies;

namespace Beamable.Api.Commerce
{
	/// <summary>
	/// This type defines the %Client main entry point for the %Commerce feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/store-feature">Store</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class CommerceService : PlatformSubscribable<GetOffersResponse, PlayerStoreView>
	{
		public CommerceService(IDependencyProvider provider, IBeamableRequester requester) : base(provider, "commerce")
		{
		}

		protected override void OnRefresh(GetOffersResponse data)
		{
			foreach (var store in data.stores)
			{
				store.Init();

				Notify(store.symbol, store);
				if (store.nextDeltaSeconds > 0)
				{
					ScheduleRefresh(store.nextDeltaSeconds, store.symbol);
				}
			}
		}

		/// <summary>
		/// This method will be removed in a future version of Beamable. Please do not use.
		/// </summary>
		/// <param name="storeSymbol"></param>
		/// <param name="listingSymbol"></param>
		/// <returns></returns>
		public Promise<Unit> OnPurchase(string storeSymbol, string listingSymbol)
		{
			var store = GetLatest(storeSymbol);
			if (store == null)
			{
				return Promise<Unit>.Successful(PromiseBase.Unit);
			}

			var listing = store.listings.Find(lst => lst.symbol == listingSymbol);
			if (listing == null)
			{
				return Promise<Unit>.Successful(PromiseBase.Unit);
			}

			// Intercept purchase with a refresh if something fundamental about the listing is going to change by purchasing
			if (listing.queryAfterPurchase)
			{
				return Refresh(storeSymbol);
			}

			return Promise<Unit>.Successful(PromiseBase.Unit);
		}

		/// <summary>
		/// Purchase a listing from a store.
		/// After the purchase completes, the player will be granted whatever the listing stated.
		/// </summary>
		/// <param name="storeSymbol">The content ID of the store the listing is present in.</param>
		/// <param name="listingSymbol">The content ID of the listing to buy</param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call</returns>
		public Promise<EmptyResponse> Purchase(string storeSymbol, string listingSymbol)
		{
			long gamerTag = userContext.UserId;
			string purchaseId = $"{listingSymbol}:{storeSymbol}";
			return requester.Request<EmptyResponse>(
			   Method.POST,
			   $"/object/commerce/{gamerTag}/purchase?purchaseId={purchaseId}"
			);
		}

		/// <summary>
		/// This will be removed in a future version of Beamable. Please do not use.
		/// </summary>
		/// <param name="storeSymbol"></param>
		public void ForceRefresh(string storeSymbol)
		{
			Refresh(storeSymbol);
		}
	}
}
