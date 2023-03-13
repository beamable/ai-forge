using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Beamable.Common.Reflection
{
	/// <summary>
	/// Implement this interface on attributes to use our validation utilities declared in <see cref="ReflectionCache"/>.
	/// </summary>
	public interface IReflectionAttribute
	{
		/// <summary>
		/// Takes in the <see cref="MemberInfo"/> associated with this attribute and returns a <see cref="AttributeValidationResult"/>.
		/// </summary>
		AttributeValidationResult IsAllowedOnMember(MemberInfo member);
	}

	/// <summary>
	/// Result of a data structure that holds the result of a validation performed by <see cref="IReflectionAttribute"/>. 
	/// </summary>
	public readonly struct AttributeValidationResult
	{
		public readonly MemberAttribute Pair;
		public readonly ReflectionCache.ValidationResultType Type;
		public readonly string Message;

		public AttributeValidationResult(Attribute attribute, MemberInfo ownerMember, ReflectionCache.ValidationResultType type, string message)
		{
			if (attribute != null)
				System.Diagnostics.Debug.Assert(attribute is IReflectionAttribute, $"Attribute must implement the {nameof(IReflectionAttribute)}");

			Pair = new MemberAttribute(ownerMember, attribute);
			Type = type;
			Message = message;
		}
	}

	public static partial class ReflectionCacheExtensions
	{
		/// <summary>
		/// Helper that invokes the validation defined by <see cref="IReflectionAttribute"/> over a list of <see cref="MemberAttribute"/>
		/// </summary>
		public static List<AttributeValidationResult> Validate(this IReadOnlyList<MemberAttribute> cachedMemberAttributes)
		{
			return cachedMemberAttributes
				   .Select(pair => ((IReflectionAttribute)pair.Attribute).IsAllowedOnMember(pair.Info))
				   .ToList();
		}

		/// <summary>
		/// Helper that can be used to get a list with of <see cref="AttributeValidationResult"/> found by parsing <paramref name="membersToCheck"/> looking for <paramref name="attributeOfInterest"/>.
		/// For all members containing the <paramref name="attributeOfInterest"/>, we validate they are over an allowed member via <see cref="IReflectionAttribute.IsAllowedOnMember"/>.
		/// For all members missing the <paramref name="attributeOfInterest"/>, the <paramref name="validateOnMissing"/> function is called to validate it.
		/// This allows the caller to decide whether or not missing attributes are valid and the error message for it. 
		/// </summary>
		/// <param name="membersToCheck">The list <see cref="MemberInfo"/> to check. Usually built from <see cref="Type.GetMembers()"/>.</param>
		/// <param name="attributeOfInterest">The <see cref="AttributeOfInterest"/> that we'll look for over the given <paramref name="membersToCheck"/>.</param>
		/// <param name="validateOnMissing">Defines how the caller wants <paramref name="membersToCheck"/> to be validated when the attribute isn't found over a member.</param>
		/// <returns>A list of <see cref="AttributeValidationResult"/> that can be used to display error/warnings or parse valid results.</returns>
		public static List<AttributeValidationResult> GetAndValidateAttributeExistence(this IEnumerable<MemberInfo> membersToCheck,
																					   AttributeOfInterest attributeOfInterest,
																					   Func<MemberInfo, AttributeValidationResult> validateOnMissing)
		{
			var members = membersToCheck;
			var validationResults = new List<AttributeValidationResult>();

			foreach (var checkMember in members)
			{
				var attributes = checkMember.GetCustomAttributes(attributeOfInterest.AttributeType, false);
				if (attributes.Length == 0)
				{
					var result = validateOnMissing?.Invoke(checkMember);
					if (result.HasValue) validationResults.Add(result.Value);
				}
				else
				{
					for (int i = 0; i < attributes.Length; i++)
					{
						var attribute = (Attribute)attributes[i];
						if (attribute != null)
						{
							var cast = (IReflectionAttribute)attribute;
							var result = cast.IsAllowedOnMember(checkMember);

							validationResults.Add(result);
						}
					}
				}
			}

			return validationResults;
		}

		/// <summary>
		/// Given a list of members, fetches all attributes of <typeparamref name="TAttribute"/> type.
		/// </summary>
		/// <param name="membersToCheck">List of members to check.</param>
		/// <typeparam name="TAttribute">The <see cref="IReflectionAttribute"/> to look for in the given members.</typeparam>
		/// <returns>A list of <see cref="AttributeValidationResult"/> with the resulting validations and members.</returns>
		public static List<AttributeValidationResult> GetOptionalAttributeInMembers<TAttribute>(this IEnumerable<MemberInfo> membersToCheck)
			where TAttribute : IReflectionAttribute
		{
			return membersToCheck.GetAndValidateAttributeExistence(
				new AttributeOfInterest(typeof(TAttribute)),
				info => new AttributeValidationResult(null, info, ReflectionCache.ValidationResultType.Discarded, ""));
		}

		/// <summary>
		/// Helper that splits a list of <see cref="AttributeValidationResult"/> into it's three lists (for each possible <see cref="ReflectionCache.ValidationResultType"/>).
		/// The resulting lists can be used to process valid attributes or display context-sensitive error/warning messages. 
		/// </summary>        
		public static void SplitValidationResults(this IReadOnlyList<AttributeValidationResult> mainList,
												  out List<AttributeValidationResult> valid,
												  out List<AttributeValidationResult> warning,
												  out List<AttributeValidationResult> error)
		{
			var splitByType = mainList.GroupBy(res => res.Type).ToList();

			valid = splitByType
					.Where(group => group.Key == ReflectionCache.ValidationResultType.Valid)
					.SelectMany(group => group)
					.ToList();

			warning = splitByType
					  .Where(group => group.Key == ReflectionCache.ValidationResultType.Warning)
					  .SelectMany(group => group)
					  .ToList();

			error = splitByType
					.Where(group => group.Key == ReflectionCache.ValidationResultType.Error)
					.SelectMany(group => group)
					.ToList();
		}

		/// <summary>
		/// Creates from a list of <see cref="MemberAttribute"/>, normally containing methods/field members, builds a dictionary with the key being each <see cref="MemberInfo.DeclaringType"/>,
		/// normally containing the declaring class for the methods/field members.
		/// <para/>
		/// Use this to parse each declaring type individually after receiving a list of methods/field <see cref="MemberAttribute"/>s from the assembly sweep.
		/// 
		/// </summary>
		/// <param name="attributePairs">A list of <see cref="MemberAttribute"/>s containing methods/field members.</param>
		/// <returns>A dictionary mapping the <see cref="MemberInfo.DeclaringType"/> of each method/field members to the method/field members it contains.</returns>
		public static Dictionary<MemberInfo, List<MemberAttribute>> CreateMemberAttributeOwnerLookupTable(this IEnumerable<MemberAttribute> attributePairs)
		{
			return attributePairs.GroupBy(memberAttributePair => (MemberInfo)memberAttributePair.Info.DeclaringType)
								 .ToDictionary(groups => groups.Key, pairs => pairs.ToList());
		}
	}
}
