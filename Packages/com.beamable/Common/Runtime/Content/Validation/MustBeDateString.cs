using Beamable.Content.Utility;
using System;
using System.Globalization;
using System.Reflection;

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
	public class MustBeDateString : ValidationAttribute
	{
		private bool IsUsingCallbackMethod => !string.IsNullOrWhiteSpace(callbackMethodName);

		public readonly string callbackMethodName;
		public readonly BindingFlags bindingFlags;
		public MustBeDateString() { }
		public MustBeDateString(string callbackMethodNameNoArgumentsCallback, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
		{
			if (string.IsNullOrWhiteSpace(callbackMethodNameNoArgumentsCallback))
				throw new ArgumentException("Callback method name cannot be an empty string");

			callbackMethodName = callbackMethodNameNoArgumentsCallback;
			this.bindingFlags = bindingFlags;
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

				if (IsUsingCallbackMethod)
					validationField.Target.TryInvokeCallback(callbackMethodName, bindingFlags);

				return;
			}

			if (validationField.FieldType == typeof(string))
			{
				var strValue = validationField.GetValue<string>();
				ValidateString(strValue, validationField, obj, ctx);

				if (IsUsingCallbackMethod)
					validationField.Target.TryInvokeCallback(callbackMethodName, bindingFlags);

				return;
			}

			throw new ContentValidationException(obj, validationField, "date must be a string field.");
		}

		public void ValidateString(string strValue, ValidationFieldWrapper validationField, IContentObject obj, IValidationContext ctx)
		{
			if (string.IsNullOrEmpty(strValue))
			{
				throw new ContentValidationException(obj, validationField, $"date cannot be an empty string. {DateUtility.ISO_FORMAT}");
			}

			if (!DateTime.TryParseExact(strValue, DateUtility.ISO_FORMAT, CultureInfo.InvariantCulture,
			   DateTimeStyles.None, out _))
			{
				throw new ContentValidationException(obj, validationField, "date is not in 8601 iso format. yyyy-MM-ddTHH:mm:ssZ");
			}
		}
	}
}
