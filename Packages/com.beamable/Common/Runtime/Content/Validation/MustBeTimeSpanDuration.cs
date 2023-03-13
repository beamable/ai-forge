using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

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
	public class MustBeTimeSpanDuration : ValidationAttribute
	{
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

			throw new ContentValidationException(obj, validationField, "duration must be a string field.");
		}

		public void ValidateString(string strValue, ValidationFieldWrapper validationField, IContentObject obj, IValidationContext ctx)
		{
			if (string.IsNullOrEmpty(strValue))
			{
				throw new ContentValidationException(obj, validationField, "duration cannot be an empty string.");
			}

			if (!TryParseTimeSpan(strValue, out _, out _))
			{
				throw new ContentValidationException(obj, validationField, "duration must be a valid ISO 8601 period code.");
			}
		}

		public static bool TryParseTimeSpan(string str, out TimeSpan span, out string humanReadable)
		{
			try
			{
				span = XmlConvert.ToTimeSpan(str);
				humanReadable = FormatTimeSpan(span);
				return true;
			}
			catch (FormatException)
			{
				span = default;
				humanReadable = "invalid";
				return false;
			}
		}

		// source: https://stackoverflow.com/questions/16689468/how-to-produce-human-readable-strings-to-represent-a-timespan
		private static string FormatTimeSpan(TimeSpan timeSpan)
		{
			Func<Tuple<int, string>, string> tupleFormatter = t => $"{t.Item1} {t.Item2}{(t.Item1 == 1 ? string.Empty : "s")}";
			var components = new List<Tuple<int, string>>
			{
				Tuple.Create((int) timeSpan.TotalDays, "day"),
				Tuple.Create(timeSpan.Hours, "hour"),
				Tuple.Create(timeSpan.Minutes, "minute"),
				Tuple.Create(timeSpan.Seconds, "second"),
			};

			components.RemoveAll(i => i.Item1 == 0);

			string extra = "";

			if (components.Count > 1)
			{
				var finalComponent = components[components.Count - 1];
				components.RemoveAt(components.Count - 1);
				extra = $" and {tupleFormatter(finalComponent)}";
			}

			return $"{string.Join(", ", components.Select(tupleFormatter))}{extra}";
		}
	}
}
