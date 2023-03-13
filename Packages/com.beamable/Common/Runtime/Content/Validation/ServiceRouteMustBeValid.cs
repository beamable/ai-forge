using static Beamable.Common.Constants.Features.Content;
// unset

namespace Beamable.Common.Content.Validation
{
	public class ServiceRouteMustBeValid : ValidationAttribute
	{
		public override void Validate(ContentValidationArgs args)
		{
			var field = args.ValidationField;
			var obj = args.Content;

			if (field.FieldType != typeof(ServiceRoute))
				throw new ContentValidationException(obj, field, $"{nameof(ServiceRouteMustBeValid)} is only valid on {nameof(ServiceRoute)}");


			var route = field.GetValue() as ServiceRoute;
			if (string.IsNullOrEmpty(route.Service))
			{
				throw new ContentValidationException(obj, field, $"Microservice cannot be empty");
			}

			if (string.IsNullOrEmpty(route.Endpoint))
			{
				throw new ContentValidationException(obj, field, $"Method cannot be empty");
			}


			if (route.Service.EndsWith(MISSING_SUFFIX))
			{
				throw new ContentValidationException(obj, field, $"Microservice must exist");
			}
			if (route.Endpoint.EndsWith(MISSING_SUFFIX))
			{
				throw new ContentValidationException(obj, field, $"Method must exist");
			}

		}
	}
}
