using Beamable.Common.Reflection;
using System;
using System.Reflection;

namespace Beamable.Common.Assistant
{
	/// <summary>
	/// Abstract class that should be implemented in any class that will contain declarations of <see cref="BeamHintDomainAttribute"/> on their "readonly static string" fields.
	/// <see cref="BeamHintDomains"/> for a better understanding of this.
	/// </summary>
	public abstract class BeamHintDomainProvider { }

	/// <summary>
	/// Used to declare that certain "static readonly string" fields are BeamHintDomains --- this information is cached by the BeamHintReflectionCache system.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class BeamHintDomainAttribute : Attribute, IReflectionAttribute
	{
		public AttributeValidationResult IsAllowedOnMember(MemberInfo member)
		{
			var field = (FieldInfo)member;
			if (field.IsStatic && field.IsInitOnly)
				return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Valid, $"");

			return new AttributeValidationResult(this,
												 member,
												 ReflectionCache.ValidationResultType.Error,
												 $"{member.Name} is not \"static readonly string\". It cannot be a BeamHintDomain.");
		}
	}
}
