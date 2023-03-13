using System.Collections.Generic;

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
	public class MustBeOneOf : ValidationAttribute
	{
		public HashSet<object> PossibleValues { get; }
		private string _errorMessage;

		public MustBeOneOf(params object[] possibleValues)
		{
			PossibleValues = new HashSet<object>(possibleValues);
			if (possibleValues.Length > 1)
			{
				_errorMessage = $"Must be one of [{string.Join(", ", possibleValues)}]";
			}
			else if (possibleValues.Length == 1)
			{
				_errorMessage = $"Must be {possibleValues[0]}";
			}
			else
			{
				_errorMessage = $"No value supported.";
			}
		}

		public override void Validate(ContentValidationArgs args)
		{
			var validationField = args.ValidationField;
			var obj = args.Content;
			var ctx = args.Context;

			if (validationField.GetValue() is Optional optional)
			{
				if (optional.HasValue && !PossibleValues.Contains(optional.GetValue()))
				{
					throw new ContentValidationException(obj, validationField, _errorMessage);
				}

				return;
			}

			if (PossibleValues.Contains(validationField.GetValue()))
			{
				return;
			}

			throw new ContentValidationException(obj, validationField, _errorMessage);
		}
	}
}
