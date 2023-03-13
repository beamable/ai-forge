using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Beamable.Common.Reflection
{
	/// <summary>
	/// Implement this interface over any <see cref="Attribute"/> to be able to use the existing <see cref="ReflectionCache"/> utilities that
	/// validate things with respect to attributes that are used to name members.
	/// </summary>
	public interface INamingAttribute : IReflectionAttribute
	{
		/// <summary>
		/// A list of names that must be unique between all uses of the implementing attribute.
		/// </summary>
		string[] Names
		{
			get;
		}

		/// <summary>
		/// A function that validates the list of names given to the implementing attribute instance.
		/// </summary>
		/// <returns>An <see cref="AttributeValidationResult{T}"/> with a clear message and <see cref="ReflectionCache.ValidationResultType"/>.</returns>
		AttributeValidationResult AreValidNameForType(MemberInfo member, string[] potentialNames);
	}

	/// <summary>
	/// Validation results of calling <see cref="ReflectionCacheExtensions.GetAndValidateUniqueNamingAttributes"/> on a list of <see cref="MemberAttribute"/>s containing
	/// attributes that implement <see cref="INamingAttribute"/>.
	/// </summary>
	public readonly struct UniqueNameValidationResults
	{
		/// <summary>
		/// List of results from each individual check of <see cref="INamingAttribute.AreValidNameForType"/>. 
		/// </summary>
		public readonly List<AttributeValidationResult> PerAttributeNameValidations;

		/// <summary>
		/// List of name collisions identified when running a validation sweep over a list of <see cref="MemberAttribute"/> containing attributes of type: <see cref="INamingAttribute"/>.
		/// </summary>
		public readonly List<UniqueNameCollisionData> PerNameCollisions;

		/// <summary>
		/// Creates a <see cref="UniqueNameValidationResults"/> with the given data.
		/// </summary>
		/// <param name="perAttributeNameValidations">See <see cref="PerAttributeNameValidations"/>.</param>
		/// <param name="perNameCollisions">See <see cref="PerNameCollisions"/>.</param>
		public UniqueNameValidationResults(List<AttributeValidationResult> perAttributeNameValidations, List<UniqueNameCollisionData> perNameCollisions)
		{
			PerAttributeNameValidations = perAttributeNameValidations;
			PerNameCollisions = perNameCollisions;
		}
	}

	/// <summary>
	/// Data struct holding information regarding a name collision.
	/// </summary>
	public readonly struct UniqueNameCollisionData
	{
		/// <summary>
		/// The collided name.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// The list of <see cref="MemberAttribute"/> that collided.
		/// </summary>
		public readonly MemberAttribute[] CollidedAttributes;

		/// <summary>
		/// Initializes the unique name collision data structure with its relevant information.
		/// </summary>
		/// <param name="name">The collided name.</param>
		/// <param name="collidedAttributes">A list of <see cref="MemberAttribute"/> that contain the collided name.</param>
		public UniqueNameCollisionData(string name, MemberAttribute[] collidedAttributes)
		{
			Name = name;
			CollidedAttributes = collidedAttributes;
		}
	}

	public static partial class ReflectionCacheExtensions
	{
		/// <summary>
		/// Gets and validates attributes the must enforce a unique name.
		/// Expects <paramref name="memberAttributePairs"/> to contain the entire selection of attributes whose names can't collide.
		/// </summary>
		/// <param name="memberAttributePairs">
		/// All <see cref="MemberAttribute"/> should contain attributes implementing <see cref="INamingAttribute{T}. 
		/// </param>
		/// <typeparam name="TNamingAttr">Any type implementing <see cref="INamingAttribute"/>.</typeparam>
		/// <returns>A <see cref="UniqueNameValidationResults"/> data structure with the validation results that you can use to display errors and warnings or parse valid pairs.</returns>
		public static UniqueNameValidationResults GetAndValidateUniqueNamingAttributes<TNamingAttr>(this IReadOnlyList<MemberAttribute> memberAttributePairs)
			where TNamingAttr : Attribute, INamingAttribute, IReflectionAttribute
		{
			// Allocates lists (assumes one name per-attribute, will re-allocate list if there's two attributes)
			var namesList = new List<(string name, MemberAttribute pair)>(memberAttributePairs.Count);
			var attributeNameStringValidations = new List<AttributeValidationResult>(memberAttributePairs.Count);

			// Iterate all MemberAttributes validating if their names are valid while also storing them in the name's list for name-collision detection.
			foreach (var memberAttributes in memberAttributePairs)
			{
				var info = memberAttributes.Info;
				var attr = memberAttributes.AttrAs<TNamingAttr>();

				var result = attr.AreValidNameForType(info, attr.Names);

				namesList.AddRange(attr.Names.Select(name => (name, memberAttributePair: memberAttributes)));
				attributeNameStringValidations.Add(result);
			}

			// Get the duplicate names and bake them into a proper data structure for consumption by other systems.
			var duplicateNames = namesList
								 .GroupBy(tuple => tuple.name)
								 .Where(group => group.Count() > 1)
								 .Select(group => new UniqueNameCollisionData(group.Key, group.Select(tuple => tuple.pair).ToArray()))
								 .ToList();

			return new UniqueNameValidationResults(attributeNameStringValidations, duplicateNames);
		}

		/// <summary>
		/// Gets the first non-null non-empty name in <see cref="INamingAttribute.Names"/> of the given <paramref name="attributePair"/>.
		/// </summary>
		/// <param name="attributePair">An <see cref="MemberAttribute"/> that contains an attribute implementing <see cref="INamingAttribute"/>.</param>
		/// <typeparam name="TNamingAttribute">The expected attribute type held by the <paramref name="attributePair"/>.</typeparam>
		public static string GetOptionalNameOrMemberName<TNamingAttribute>(this MemberAttribute attributePair)
			where TNamingAttribute : Attribute, INamingAttribute, IReflectionAttribute
		{
			var attr = attributePair.AttrAs<TNamingAttribute>();
			var type = attributePair.Info;

			var firstNonNullName = attr.Names.FirstOrDefault(s => !string.IsNullOrEmpty(s));

			return string.IsNullOrEmpty(firstNonNullName) ? type.Name : firstNonNullName;
		}
	}
}
