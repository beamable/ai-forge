using System;

namespace Beamable.Content
{
	/// <summary>
	/// This type defines the field attribute that marks a %Beamable %ContentObject field
	/// as ignored from the %Content %Serialization process.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	/// 
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code">Content - IgnoreContentField</a> documentation
	/// 
	/// ![img beamable-logo]
	/// 
	/// </summary>
	[AttributeUsage(validOn: AttributeTargets.Field)]
	public class IgnoreContentFieldAttribute : Attribute
	{

	}
}
