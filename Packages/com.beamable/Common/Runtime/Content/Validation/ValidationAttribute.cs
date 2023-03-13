using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Common.Content.Validation
{
	/// <summary>
	/// This type defines part of the %Beamable %ContentObject validation process.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Direct Subclasses
	/// - See Beamable.Common.Content.Validation.CannotBeBlank script reference
	/// - See Beamable.Common.Content.Validation.CannotBeEmpty script reference
	/// - See Beamable.Common.Content.Validation.MustBeDateString script reference
	/// - See Beamable.Common.Content.Validation.MustBeOneOf script reference
	/// - See Beamable.Common.Content.Validation.MustBePositive script reference
	/// - See Beamable.Common.Content.Validation.MustBeTimeSpanDuration script reference
	/// - See Beamable.Common.Content.Validation.MustContain script reference
	/// - See Beamable.Common.Content.Validation.MustReferenceContent script reference
	/// 
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code#content-validation">Content Validation</a> documentation
	/// - See Beamable.Common.Content.ContentObject script reference
	/// 
	/// ![img beamable-logo]
	///
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	public abstract class ValidationAttribute : PropertyAttribute
	{
		private static readonly List<Type> numericTypes = new List<Type>
	  {
		 typeof(byte),
		 typeof(sbyte),
		 typeof(short),
		 typeof(ushort),
		 typeof(int),
		 typeof(uint),
		 typeof(long),
		 typeof(ulong),
		 typeof(float),
		 typeof(double),
		 typeof(decimal)
	  };

		/// <summary>
		/// Determines if Type is a Numeric Type. Used by some %ValidationAttribute subclasses.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		protected static bool IsNumericType(Type type)
		{
			return numericTypes.Contains(type);
		}

		/// <summary>
		/// Performs the validation operation on the field marked with the %ValidationAttribute.
		/// </summary>
		/// <param name="args"></param>
		public abstract void Validate(ContentValidationArgs args);
	}


	/// <summary>
	/// This type defines part of the %Beamable %ContentObject validation process.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code#content-validation">Content Validation</a> documentation
	/// - See Beamable.Common.Content.ContentObject script reference
	/// 
	/// ![img beamable-logo]
	///
	/// </summary>
	public class ContentValidationArgs
	{
		public ValidationFieldWrapper ValidationField;
		public IContentObject Content;
		public IValidationContext Context;
		public int ArrayIndex;
		public bool IsArray;

		public static ContentValidationArgs Create(ValidationFieldWrapper field, IContentObject obj,
		   IValidationContext ctx)
		{
			return new ContentValidationArgs
			{
				ValidationField = field,
				Content = obj,
				Context = ctx
			};
		}
		public static ContentValidationArgs Create(ValidationFieldWrapper field, IContentObject obj,
		   IValidationContext ctx, int arrayIndex, bool isArray)
		{
			return new ContentValidationArgs
			{
				ValidationField = field,
				Content = obj,
				Context = ctx,
				ArrayIndex = arrayIndex,
				IsArray = isArray
			};
		}
	}
}
