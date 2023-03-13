namespace Beamable.Common.Api
{
	/// <summary>
	/// this type is responsible for reporting connectivity
	/// status to the ConnectivityService.
	/// </summary>
	public interface IConnectivityChecker
	{
		/// <summary>
		/// When true, the service will report its connectivity data to the IConnectivityService.
		/// When false, the service may still be performing actions, but it won't report the data.
		/// </summary>
		bool ConnectivityCheckingEnabled { get; set; }

		/// <summary>
		/// Normally, the implementation of <see cref="IConnectivityChecker"/> should be responsible
		/// for periodically checking connectivity. However, this method will force the check to happen
		/// at a given moment, and the return value indicates connectivity.
		/// </summary>
		/// <returns>A <see cref="Promise"/> containing true when there is internet.</returns>
		Promise<bool> ForceCheck();
	}


	/// <summary>
	/// The connectivity strategy informs Beamable how to check for internet connectivity
	/// </summary>
	public enum ConnectivityStrategy
	{
		/// <summary>
		/// The default strategy.
		/// Send periodic HTTP requests to the Beamable gateway health endpoint.
		/// </summary>
		BeamableGateway,

		/// <summary>
		/// Use the presence API calls to infer connectivity.
		/// </summary>
		BeamablePresence,

		/// <summary>
		/// Allow the developer to register a custom <see cref="IConnectivityChecker"/> service with the
		/// dependency builder.
		/// </summary>
		None,
	}
}
