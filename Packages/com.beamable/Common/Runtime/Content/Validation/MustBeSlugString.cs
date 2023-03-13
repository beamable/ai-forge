using System.Text.RegularExpressions;

namespace Beamable.Common.Content.Validation
{
	public enum MustBeSlugStringConfig
	{
		STRICT,
		ALLOW_UNDERSCORE
	}

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
	public class MustBeSlugString : ValidationAttribute
	{
		private MustBeSlugStringConfig _config;

		public MustBeSlugString(MustBeSlugStringConfig config = MustBeSlugStringConfig.STRICT)
		{
			_config = config;
		}

		public override void Validate(ContentValidationArgs args)
		{
			var validationField = args.ValidationField;
			var obj = args.Content;
			var ctx = args.Context;

			if (validationField.FieldType == typeof(OptionalString))
			{
				var optional = validationField.GetValue<OptionalString>();
				if (optional.HasValue)
				{
					ValidateString(optional.Value, validationField, obj, ctx);
				}

				return;
			}

			if (validationField.FieldType == typeof(string))
			{
				var strValue = validationField.GetValue<string>();
				ValidateString(strValue, validationField, obj, ctx);
				return;
			}

			throw new ContentValidationException(obj, validationField, "string must not contain spaces and special characters (i.e. slug).");
		}

		public void ValidateString(string strValue, ValidationFieldWrapper validationField, IContentObject obj, IValidationContext ctx)
		{
			if (string.IsNullOrEmpty(strValue))
			{
				throw new ContentValidationException(obj, validationField, "value cannot be empty string.");
			}

			if (!IsSlug(strValue))
			{
				throw new ContentValidationException(obj, validationField, "value can contain alphanumeric, lowercase, underscore, and hyphen characters.");
			}
		}

		private bool IsSlug(string slug)
		{
			if (string.IsNullOrEmpty(slug)) return false;

			string str = slug.ToLower().Trim();
			if (_config == MustBeSlugStringConfig.STRICT)
				str = Regex.Replace(str, @"[^a-z0-9-]", "");
			else
				str = Regex.Replace(str, @"[^a-z0-9-_]", "");

			return str.Equals(slug);
		}
	}
}
