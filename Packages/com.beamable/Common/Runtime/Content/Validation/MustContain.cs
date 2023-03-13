using System.Linq;

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
	public class MustContain : ValidationAttribute
	{
		public string[] Parts { get; }

		public MustContain(params string[] parts)
		{
			Parts = parts;
		}

		public override void Validate(ContentValidationArgs args)
		{
			var field = args.ValidationField;
			var obj = args.Content;
			var ctx = args.Context;

			if (field.FieldType != typeof(string))
			{
				throw new ContentValidationException(obj, field, "mustContain only works for string fields.");
			}

			var strValue = field.GetValue<string>();
			if (string.IsNullOrEmpty(strValue) && Parts.Length > 0)
			{
				throw new ContentValidationException(obj, field, "string is empty");
			}

			var missingParts = Parts.Where(part => !strValue.Contains(part)).ToList();
			if (missingParts.Count > 0)
			{
				throw new ContentValidationException(obj, field, $"must contain {string.Join(",", missingParts)}");
			}

		}
	}
}
