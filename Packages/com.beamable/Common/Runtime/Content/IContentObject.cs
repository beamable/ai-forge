namespace Beamable.Common.Content
{
	/// <summary>
	/// This type defines the API for %Beamable %ContentObject and its many subclasses.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public interface IContentObject
	{
		/// <summary>
		/// The id. A content id is a dot separated string.
		/// The right most part is the name of the content.
		/// Every part to the left of the name denotes the type of the content.
		/// </summary>
		string Id { get; }

		/// <summary>
		/// The version
		/// </summary>
		string Version { get; }

		/// <summary>
		/// The tags
		/// </summary>
		string[] Tags { get; }
		string ManifestID { get; }
		long LastChanged { get; set; }
		ContentCorruptedException ContentException { get; set; }

		/// <summary>
		/// Set Id And Version
		/// </summary>
		/// <param name="id"></param>
		/// <param name="version"></param>
		void SetIdAndVersion(string id, string version);

		/// <summary>
		/// Convert content to JSON
		/// </summary>
		/// <returns>A JSON string</returns>
		string ToJson();
	}
}
