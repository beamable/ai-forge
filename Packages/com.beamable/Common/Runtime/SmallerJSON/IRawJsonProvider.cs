namespace Beamable.Serialization.SmallerJSON
{
	/// <summary>
	/// This type defines part of the %Beamable %Json conversion.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Content.ContentObject script reference
	/// 
	/// ![img beamable-logo]
	///
	/// </summary>
	public interface IRawJsonProvider
	{
		string ToJson();
	}

	/// <summary>
	/// This type defines part of the %Beamable %Json conversion.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Content.ContentObject script reference
	/// 
	/// ![img beamable-logo]
	///
	/// </summary>
	public class RawJsonProvider : IRawJsonProvider
	{
		public string Json;

		public string ToJson()
		{
			return Json;
		}
	}
}
