namespace Beamable.Common.Content.Validation
{
	/// <summary>
	/// This type defines part of the %Beamable %ContentObject validation process.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Content.Validation.ValidationAttribute script reference
	/// 
	/// ![img beamable-logo]
	///
	/// </summary>
	public class MustBeComparatorString : MustBeOneOf
	{
		public const string EQUALS = "eq";
		public const string NOT_EQUALS = "ne";
		public const string GREATER_THAN = "gt";
		public const string GREATER_THAN_OR_EQUAL = "ge";
		public const string LESS_THAN = "lt";
		public const string LESS_THAN_OR_EQUAL = "le";
		public MustBeComparatorString() : base(EQUALS, NOT_EQUALS, GREATER_THAN, GREATER_THAN_OR_EQUAL, LESS_THAN, LESS_THAN_OR_EQUAL)
		{

		}
	}
}
