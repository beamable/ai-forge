namespace Beamable.Common.Api.CloudData
{
	/// <summary>
	/// This type defines the %Client main entry point for the %A/B %Testing feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/ab-testing-feature-overview">A/B Testing</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public interface ICloudDataApi
	{
		/// <summary>
		/// This method will return the cloud data for the entire game.
		/// It cannot be called by a player. You must be an admin user to execute this method.
		/// </summary>
		/// <returns></returns>
		Promise<GetCloudDataManifestResponse> GetGameManifest();

		/// <summary>
		/// Get the current player's cloud trial data.
		/// The <see cref="GetCloudDataManifestResponse"/> will include data for the player's assigned cohorts.
		/// Remember, if that list is empty, it may be because the cohorts only work for game.private stats conditions.
		/// </summary>
		/// <returns>A <see cref="GetCloudDataManifestResponse"/> promise representing the player's cloud trial data.</returns>
		Promise<GetCloudDataManifestResponse> GetPlayerManifest();


		/// <summary>
		/// Get the cloud data content based on metadata from manifest
		/// </summary>
		/// <param name="metaData"></param>
		/// <returns>A string containing content of remote cloud data</returns>
		Promise<string> GetCloudDataContent(CloudMetaData metaData);
	}
}
