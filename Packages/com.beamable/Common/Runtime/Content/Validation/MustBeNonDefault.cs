using System;

namespace Beamable.Common.Content.Validation
{
	public class MustBeNonDefault : ValidationAttribute
	{
		public override void Validate(ContentValidationArgs args)
		{
			var fieldValue = args.ValidationField.GetValue();
			var type = fieldValue.GetType();
			var defaultValue = GetDefaultValue(type);
			if (fieldValue.Equals(defaultValue))
			{
				throw new ContentValidationException(args.Content, args.ValidationField, "Value must not be default");
			}
		}

		private object GetDefaultValue(Type type)
		{
			if (type.IsValueType)
			{
				return Activator.CreateInstance(type);
			}

			return null;
		}
	}
}
