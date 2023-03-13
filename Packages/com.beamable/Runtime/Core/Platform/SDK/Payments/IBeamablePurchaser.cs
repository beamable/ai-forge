using Beamable.Common;
using Beamable.Common.Dependencies;

namespace Beamable.Api.Payments
{
	/// <summary>
	/// This type defines the %Client main entry point for the %In-App %Purchasing feature.
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
	public interface IBeamablePurchaser
	{

		/// <summary>
		/// Begin initialization of Beamable purchasing.
		/// </summary>
		/// <param name="provider">A <see cref="IDependencyProvider"/> that will be used to grant Beamable dependencies to the purchaser.</param>
		/// <returns>A <see cref="Promise"/> representing the completion of IAP initialization.</returns>
		Promise<Unit> Initialize(IDependencyProvider provider = null);

		/// <summary>
		/// Fetches the localized string from the provider.
		/// <param name="skuSymbol">
		/// The purchase symbol for the item. This is the skuSymbol for the offer.
		/// </param>
		/// </summary>
		string GetLocalizedPrice(string skuSymbol);

		/// <summary>
		/// Start a purchase through the chosen IAP implementation.
		/// </summary>
		/// <param name="listingSymbol">Listing symbol to buy.</param>
		/// <param name="skuSymbol">SKU within the mobile platform.</param>
		/// <returns>Promise with a completed transaction data structure.</returns>
		Promise<CompletedTransaction> StartPurchase(string listingSymbol, string skuSymbol);
	}
}
