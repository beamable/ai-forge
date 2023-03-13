using static Beamable.Common.Constants.Features.Content;

namespace Beamable.Common.Content.Validation
{
	public class FederationMustBeValid : ValidationAttribute
	{
		public override void Validate(ContentValidationArgs args)
		{
			var field = args.ValidationField;
			var obj = args.Content;

			if (typeof(OptionalFederation) == field.FieldType)
			{
				var fieldValue = field.GetValue() as OptionalFederation;
				if (!fieldValue.HasValue) return;
				Validate(args, fieldValue.Value);
				return;
			}

			if (typeof(Federation) == field.FieldType)
			{
				var fieldValue = field.GetValue() as Federation;
				Validate(args, fieldValue);
				return;
			}

			throw new ContentValidationException(
				obj, field, $"{nameof(FederationMustBeValid)} is only valid on {nameof(Federation)}");
		}

		void Validate(ContentValidationArgs args, Federation federation)
		{
			if (string.IsNullOrEmpty(federation.Service))
			{
				throw new ContentValidationException(args.Content, args.ValidationField,
													 $"Microservice cannot be empty");
			}

			if (string.IsNullOrEmpty(federation.Namespace))
			{
				throw new ContentValidationException(args.Content, args.ValidationField, $"Namespace cannot be empty");
			}

			if (federation.Service.EndsWith(MISSING_SUFFIX))
			{
				throw new ContentValidationException(args.Content, args.ValidationField, $"Microservice must exist");
			}

			if (federation.Namespace.EndsWith(MISSING_SUFFIX))
			{
				throw new ContentValidationException(args.Content, args.ValidationField, $"Namespace must exist");
			}
		}
	}
}
