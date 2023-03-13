using Beamable.Common.Reflection;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Beamable.Common.Content
{
	/// <summary>
	/// This type defines part of the %Beamable %ContentObject system.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Content.ContentObject script reference
	/// 
	/// ![img beamable-logo]
	///
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class ContentTypeAttribute : UnityEngine.Scripting.PreserveAttribute, IHasSourcePath, INamingAttribute
	{
		public string TypeName { get; }
		public string SourcePath { get; }

		public ContentTypeAttribute(string typeName, [CallerFilePath] string sourcePath = "")
		{
			TypeName = typeName;
			SourcePath = sourcePath;
			Names = new[] { typeName };
		}

		public string[] Names { get; }

		public AttributeValidationResult IsAllowedOnMember(MemberInfo member)
		{
			// Guaranteed due to AttributeUsage(AttributeTargets.Class)
			var type = (Type)member;

			bool isAssignableFromIContentObject = typeof(IContentObject).IsAssignableFrom(type);

#if !DB_MICROSERVICE
			bool isAssignableFromScriptableObject = typeof(UnityEngine.ScriptableObject).IsAssignableFrom(type);
#else
            bool isAssignableFromScriptableObject = true;
#endif
			if (isAssignableFromIContentObject && isAssignableFromScriptableObject)
			{
				return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Valid, "");
			}

			var message = $"This attribute should only be used on ScriptableObjects that implement the [{nameof(IContentObject)}] interface.";
			return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Error, message);
		}

		public AttributeValidationResult AreValidNameForType(MemberInfo member, string[] potentialNames)
		{
			if (potentialNames.Length > 1)
			{
				var msg = $"Multiple names not supported for ContentType attribute.";
				return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Error, msg);
			}

			var invalidNames = potentialNames.Where(name => name.Contains(".")).ToArray();
			if (invalidNames.Length <= 0)
				return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Valid, $"");

			var message = $"Names [{string.Join(", ", invalidNames)}] cannot have '.' in them. It is used by Beamable for (de)serialization purposes.";
			return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Error, message);
		}
	}

	/// <summary>
	/// This type defines part of the %Beamable %ContentObject system.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Content.ContentObject script reference
	/// 
	/// ![img beamable-logo]
	///
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class ContentFormerlySerializedAsAttribute : Attribute, INamingAttribute
	{
		public string OldTypeName { get; }
		public string[] Names { get; }

		public ContentFormerlySerializedAsAttribute(string oldTypeName)
		{
			OldTypeName = oldTypeName;
			Names = new[] { oldTypeName };
		}

		public AttributeValidationResult IsAllowedOnMember(MemberInfo member)
		{
			// Guaranteed due to AttributeUsage(AttributeTargets.Class)
			var type = (Type)member;
			var isAssignableFromIContentObject = typeof(IContentObject).IsAssignableFrom(type);

			// TODO: Check with CHRIS what validation cases does this cover and why, maybe we don't need to enforce this... (especially in TestCases)
			// TODO: Can easily ignore this from test Assemblies via type.Assembly.Name.Contains("Test") but its not a good long term solution... Or maybe it is?
#if !DB_MICROSERVICE
			var isAssignableFromScriptableObject = typeof(UnityEngine.ScriptableObject).IsAssignableFrom(type);
#else
         var isAssignableFromScriptableObject = true;
#endif
			if (!(isAssignableFromIContentObject && isAssignableFromScriptableObject))
			{
				var message = $"This attribute should only be used on ScriptableObjects that implement the [{nameof(IContentObject)}] interface.";
				return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Error, message);
			}

			return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Valid, "");
		}

		public AttributeValidationResult AreValidNameForType(MemberInfo member, string[] potentialNames)
		{
			if (potentialNames.Length > 1)
			{
				var msg = $"Multiple names not supported for ContentFormerlySerializedAs attribute.";
				return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Error, msg);
			}

			var invalidNames = potentialNames.Where(name => name.Contains(".")).ToArray();
			if (invalidNames.Length <= 0)
				return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Valid, $"");

			var message = $"Names [{string.Join(", ", invalidNames)}] cannot have '.' in them. It is used by Beamable for (de)serialization purposes.";
			return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Error, message);
		}
	}
}
