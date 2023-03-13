using System;
using System.Collections.Generic;
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
	public class CannotBeBlank : ValidationAttribute
	{
		public override void Validate(ContentValidationArgs args)
		{
			var validationField = args.ValidationField;
			// support optional
			if (typeof(Optional).IsAssignableFrom(validationField.FieldType))
			{
				var optional = validationField.GetValue() as Optional;
				if (!optional.HasValue) return;
				// get the underlying field...
				var value = optional.GetValue();
				Validate(value.GetType(), value, args);
				return;
			}

			Validate(validationField.FieldType, validationField.GetValue(), args);
		}

		void Validate(Type fieldType, object value, ContentValidationArgs args)
		{
			//
			//         if (typeof(DisplayableList).IsAssignableFrom(fieldType))
			//         {
			//            var collection = value as DisplayableList;
			//            if (collection.Count == 0)
			//            {
			//               throw new ContentValidationException(args.Content, args.ValidationField, "Cannot be empty");
			//            }
			//         }
			//         if (args.IsArray && typeof(DisplayableStringCollection).IsAssignableFrom(fieldType))
			//         {
			//            var collection = value as DisplayableStringCollection;
			//            var array = collection.rawData;
			//
			//            if (array.Count <= args.ArrayIndex) return;
			//            var elem = array[args.ArrayIndex];
			//            if (string.IsNullOrEmpty(elem))
			//            {
			//               throw new ContentValidationException(args.Content, args.ValidationField, "Cannot be empty string");
			//            }
			//
			//         }

			if (args.IsArray && typeof(IList<string>).IsAssignableFrom(fieldType))
			{
				var set = value as IEnumerable<string>;

				var array = set.ToArray();
				if (array.Length <= args.ArrayIndex) return;
				var elem = array[args.ArrayIndex];
				if (string.IsNullOrEmpty(elem))
				{
					throw new ContentValidationException(args.Content, args.ValidationField, "Cannot be empty string");
				}

				return;
			}

			//         if (args.IsArray && typeof(DisplayableStringCollection).IsAssignableFrom(fieldType))
			//         {
			//            var set = value as DisplayableStringCollection;
			//
			//            var array = set.rawData.ToArray();
			//            if (array.Length <= args.ArrayIndex) return;
			//            var elem = array[args.ArrayIndex];
			//            if (string.IsNullOrEmpty(elem))
			//            {
			//               throw new ContentValidationException(args.Content, args.ValidationField, "Cannot be empty string");
			//            }
			//
			//            return;
			//         }

			if (fieldType == typeof(string))
			{
				var strVal = value as string;
				if (string.IsNullOrEmpty(strVal))
				{
					throw new ContentValidationException(args.Content, args.ValidationField, "Cannot be empty string");
				}

				return;
			}
			//
			//         if (typeof(IEnumerable).IsAssignableFrom(fieldType))
			//         {
			//            var set = value as IEnumerable;
			//            if (!set.GetEnumerator().MoveNext())
			//            {
			//               throw new ContentValidationException(args.Content, args.ValidationField, "Cannot be empty");
			//            }
			//         }
		}
	}
}
