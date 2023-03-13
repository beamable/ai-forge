using System;
using System.Collections.Generic;
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
	public class ValidationFieldWrapper
	{
		public FieldInfo Field { get; }

		public object Target { get; }


		public ValidationFieldWrapper(FieldInfo field, object target)
		{
			Field = field;
			Target = target;
		}

		public Type FieldType => Field?.FieldType;

		public object GetValue() => Field?.GetValue(Target);

		public T GetValue<T>() => (T)Field?.GetValue(Target);
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
	public interface IValidationContext
	{
		bool ContentExists(string id);

		IEnumerable<string> ContentIds { get; }

		long Count { get; }

		bool Initialized { get; }

		string GetTypeName(Type type);

		bool TryGetContent(string id, out IContentObject content);
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
	public class ValidationContext : IValidationContext
	{
		public Dictionary<string, IContentObject> AllContent = new Dictionary<string, IContentObject>();

		public bool ContentExists(string id) => AllContent?.ContainsKey(id) ?? false;

		public IEnumerable<string> ContentIds => AllContent.Keys;

		public long Count => AllContent.Count;

		public bool Initialized { get; set; }

		public string GetTypeName(Type type)
		{
			return ContentTypeReflectionCache.GetContentTypeName(type);
		}


		public bool TryGetContent(string id, out IContentObject content)
		{
			return AllContent.TryGetValue(id, out content);
		}
	}
}
