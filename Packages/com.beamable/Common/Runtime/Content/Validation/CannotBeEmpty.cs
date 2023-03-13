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
	public class CannotBeEmpty : ValidationAttribute
	{
		public override void Validate(ContentValidationArgs args)
		{

			var type = args.ValidationField.FieldType;
			var value = args.ValidationField.GetValue();
			if (typeof(Optional).IsAssignableFrom(type))
			{
				var optional = value as Optional;
				if (!optional.HasValue) return;

				value = optional.GetValue();
				type = optional.GetOptionalType();
			}

			var isDisplayList = typeof(DisplayableList).IsAssignableFrom(type);
			if (isDisplayList)
			{
				var displayList = value as DisplayableList;
				if (displayList == null || displayList.Count == 0)
				{
					throw new ContentValidationException(args.Content, args.ValidationField, "Cannot be empty");
				}
			}
		}
	}
}
