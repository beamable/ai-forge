using Beamable.Common.Reflection;
using System;
using System.Reflection;

namespace Beamable.Server
{
	[AttributeUsage(AttributeTargets.Class)]
	public class StorageObjectAttribute : Attribute, INamingAttribute
	{
		public string StorageName { get; }
		public string SourcePath { get; }

		public StorageObjectAttribute(string storageName, [System.Runtime.CompilerServices.CallerFilePath]
		 string sourcePath = "")
		{
			StorageName = storageName;
			SourcePath = sourcePath;
		}

		public string[] Names => new[] { StorageName };

		public AttributeValidationResult IsAllowedOnMember(MemberInfo member)
		{
			var type = (Type)member;

			if (!typeof(StorageObject).IsAssignableFrom(type))
			{
				return new AttributeValidationResult(this,
													 type,
													 ReflectionCache.ValidationResultType.Error,
													 $"StorageObject Attribute [{StorageName}] cannot be over type [{type.Name}] " +
													 $"since [{type.Name}] does not inherit from [{nameof(StorageObject)}].");
			}

			return new AttributeValidationResult(this, type, ReflectionCache.ValidationResultType.Valid, $"");
		}

		public AttributeValidationResult AreValidNameForType(MemberInfo member, string[] potentialNames)
		{
			// TODO: Validate no invalid characters are in the C#MS/Storage Object name
			return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Valid, "");
		}
	}
}
