using Beamable.Common.Reflection;
using System;
using System.Reflection;

namespace Beamable.Common.Assistant
{
	/// <summary>
	/// Abstract class that should be implemented in any class that will contain declarations of <see cref="BeamHintIdAttribute"/> on their "readonly static string" fields.
	/// <see cref="BeamHintIds"/> for a better understanding of this.
	/// </summary>
	public abstract class BeamHintIdProvider { }

	/// <summary>
	/// Used to declare that certain "static readonly string" fields are BeamHintIds --- this information is cached by the BeamHintReflectionCache system.
	/// </summary>
	public class BeamHintIdAttribute : Attribute, IReflectionAttribute
	{
		public AttributeValidationResult IsAllowedOnMember(MemberInfo member)
		{
			var field = (FieldInfo)member;
			if (field.FieldType == typeof(string) && field.IsStatic && field.IsInitOnly)
				return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Valid, $"");

			return new AttributeValidationResult(this,
												 member,
												 ReflectionCache.ValidationResultType.Error,
												 $"{member.Name} is not \"static readonly string\". It cannot be a BeamHintId.");
		}
	}
}
