using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Beamable.Common.Reflection
{
	/// <summary>
	/// Struct that holds data for all Attribute-related reflection data caches.
	/// </summary>
	public readonly struct PerAttributeCache
	{
		/// <summary>
		/// List of attribute types that can only be placed in non-type-members (any <see cref="AttributeTargets"/> not in <see cref="AttributeOfInterest.INTERNAL_TYPE_SEARCH_WHEN_ATTRIBUTE_TARGETS"/>).
		/// </summary>
		public readonly List<AttributeOfInterest> AttributeTypes;

		/// <summary>
		/// List of attribute types that can only be placed in type-members (any <see cref="AttributeTargets"/> in <see cref="AttributeOfInterest.INTERNAL_TYPE_SEARCH_WHEN_ATTRIBUTE_TARGETS"/>).
		/// </summary>
		public readonly List<AttributeOfInterest> MemberAttributeTypes;

		/// <summary>
		/// Cached list of <see cref="AttributeOfInterest"/> and all <see cref="MemberAttribute"/>s that were found in the assembly sweep.
		/// </summary>
		public readonly Dictionary<AttributeOfInterest, List<MemberAttribute>> AttributeMappings;

		/// <summary>
		/// Total number of <see cref="AttributeOfInterest"/> that are registered.
		/// </summary>
		public int TotalAttributesOfInterestCount => AttributeTypes.Count + MemberAttributeTypes.Count;

		/// <summary>
		/// Constructs a new <see cref="PerAttributeCache"/>.
		/// If planning to build with <see cref="ReflectionCache.BuildTypeCaches"/>, call this with large pre-allocated
		/// lists since allocation is expensive and list's "expansion when full" re-allocates.
		/// </summary>
		public PerAttributeCache(List<AttributeOfInterest> attributeTypes, List<AttributeOfInterest> memberAttributeTypes, Dictionary<AttributeOfInterest, List<MemberAttribute>> attributeMappings)
		{
			AttributeTypes = attributeTypes;
			AttributeMappings = attributeMappings;
			MemberAttributeTypes = memberAttributeTypes;
		}
	}

	/// <summary>
	/// Struct that defines an attribute of interest and gives us information on where to look for it.
	/// </summary>
	public readonly struct AttributeOfInterest
	{
		/// <summary>
		/// Mask for all possible <see cref="AttributeTargets"/> declaring an attribute must only exist on a "Member" of classes/structs.
		/// </summary>
		public const AttributeTargets INTERNAL_TYPE_SEARCH_WHEN_ATTRIBUTE_TARGETS = AttributeTargets.Constructor |
																					AttributeTargets.Event |
																					AttributeTargets.Field |
																					AttributeTargets.Method |
																					AttributeTargets.Property;

		/// <summary>
		/// Mask for all possible <see cref="MemberTypes"/> that can be "Members" of declared classes/structs.
		/// </summary>
		public const MemberTypes INTERNAL_TYPE_SEARCH_WHEN_IS_MEMBER_TYPES = MemberTypes.Constructor |
																			 MemberTypes.Event |
																			 MemberTypes.Field |
																			 MemberTypes.Method |
																			 MemberTypes.Property;

		/// <summary>
		/// Type of the attribute you are interested in.
		/// </summary>
		public readonly Type AttributeType;

		/// <summary>
		/// Over which types of language constructs the attribute can be found. To use <see cref="ReflectionCache"/> to find attributes,
		/// the attributes MUST have an <see cref="AttributeUsageAttribute"/> on them. This allows us some performance optimizations to make this interfere less with your editor experience.
		/// </summary>
		public readonly AttributeTargets Targets;

		/// <summary>
		/// List of all base types whose implementations we should look through the members to find this attribute. Only relevant if <see cref="TargetsDeclaredMember"/> returns true.
		/// </summary>
		public readonly List<Type> FoundInBaseTypes;

		/// <summary>
		/// List of all attribute types whose user types (classes/structs that have the attribute over them) we should look through the members to find this attribute. Only relevant if <see cref="TargetsDeclaredMember"/> returns true.
		/// </summary>
		public readonly List<Type> FoundInTypesWithAttributes;

		/// <summary>
		/// Whether or not the attribute targets a Non-Type-Member (see <see cref="INTERNAL_TYPE_SEARCH_WHEN_ATTRIBUTE_TARGETS"/> and <see cref="INTERNAL_TYPE_SEARCH_WHEN_IS_MEMBER_TYPES"/>).
		/// </summary>
		public bool TargetsDeclaredMember => INTERNAL_TYPE_SEARCH_WHEN_ATTRIBUTE_TARGETS.ContainsAnyFlag(Targets);

		/// <summary>
		/// Tries to get an attribute from the given member info.
		/// Has a guard against passing in members whose <see cref="MemberInfo.MemberType"/> don't respect <see cref="INTERNAL_TYPE_SEARCH_WHEN_IS_MEMBER_TYPES"/>.
		/// </summary>
		public bool TryGetFromMemberInfo(MemberInfo info, out Attribute attribute)
		{
			attribute = null;

			foreach (var customAttributeData in info.CustomAttributes)
			{
				if (customAttributeData.AttributeType == AttributeType)
				{
					attribute = info.GetCustomAttribute(AttributeType, false);
					break;
				}
			}

			// Assert instead of failing silently. Failing silently here means we could fail due to the member not having the correct flag. This is a case where we should fail loudly, as it's
			// supposed to be impossible.
#if UNITY_EDITOR
			if (!INTERNAL_TYPE_SEARCH_WHEN_IS_MEMBER_TYPES.ContainsAnyFlag(info.MemberType) && info.GetCustomAttribute(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)) == null)
				throw new ArgumentException(
					$"Calling this with a member info that is not a declared member. Please ensure all MemberInfos passed to this function respect this clause. member=[{info.MemberType}] name=[{info.Name}] type=[{info.ReflectedType}] attr=[{attribute}]");
#endif

			return attribute != null;
		}

		/// <summary>
		/// Checks if the given type should have it's declared members searched for the given attribute.
		/// </summary>
		public bool CanBeFoundInType(Type type)
		{
			for (int i = 0; i < FoundInBaseTypes.Count; i++)
			{
				if (FoundInBaseTypes[i].IsAssignableFrom(type))
					return true;
			}

			if (FoundInTypesWithAttributes.Count > 0)
			{
				foreach (var element in type.CustomAttributes) // get only once because CustomAttributes GET are heavy
				{
					for (int i = 0; i < FoundInTypesWithAttributes.Count; i++)
					{
						if (element.AttributeType == FoundInTypesWithAttributes[i])
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Constructs a new <see cref="AttributeOfInterest"/> with guards against incorrect usage.
		/// </summary>
		///
		/// <param name="attributeType">
		/// The Attribute's type. Expects to have <see cref="AttributeUsageAttribute"/> with correctly declared <see cref="AttributeTargets"/>.
		/// </param>
		///
		/// <param name="foundInTypesWithAttributes">
		/// Only relevant when <see cref="AttributeOfInterest.Targets"/> match <see cref="INTERNAL_TYPE_SEARCH_WHEN_ATTRIBUTE_TARGETS"/>.
		/// List of attributes types whose using types (classes/structs that use the attributes) should be have their members searched for the <paramref name="attributeType"/>.
		/// </param>
		/// <param name="foundInBaseTypes">
		/// Only relevant when <see cref="AttributeOfInterest.Targets"/> match <see cref="INTERNAL_TYPE_SEARCH_WHEN_ATTRIBUTE_TARGETS"/>.
		/// List of types whose subclasses should be have their members searched for the <paramref name="attributeType"/>.
		/// </param>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="attributeType"/> does not have a <see cref="AttributeUsageAttribute"/> or if <see cref="TargetsDeclaredMember"/> and both
		/// <paramref name="foundInBaseTypes"/> and <paramref name="foundInTypesWithAttributes"/> have no types.
		/// </exception>
		public AttributeOfInterest(Type attributeType, Type[] foundInTypesWithAttributes = null, Type[] foundInBaseTypes = null)
		{
			AttributeType = attributeType;
			Targets = AttributeType.GetCustomAttribute<AttributeUsageAttribute>()?.ValidOn ??
					  throw new ArgumentException($"To use Attribute Of Interest, you must declare a AttributeUsage attribute with the correct usage targets.");

			FoundInBaseTypes = new List<Type>(foundInBaseTypes ?? new Type[] { });
			FoundInTypesWithAttributes = new List<Type>(foundInTypesWithAttributes ?? new Type[] { });

			if (TargetsDeclaredMember)
			{
				Debug.Assert(foundInTypesWithAttributes != null || foundInBaseTypes != null,
							 "Attributes targeting members of classes and structs must specify either a base class/struct or " +
							 "an attribute over the classes/structs whose members we must check for the attribute of interest.");
			}
		}
	}

	/// <summary>
	/// Utility struct that represents a pairing of a <see cref="MemberInfo"/> with an <see cref="Attribute"/> instance.
	/// This is used to keep the declared attribute and the <see cref="MemberInfo"/> over which it is declared together throughout our <see cref="ReflectionCache"/>-ing process.
	/// </summary>
	public readonly struct MemberAttribute
	{
		public MemberTypes MemberType => Info.MemberType;

		public readonly MemberInfo Info;
		public readonly Attribute Attribute;

		public MemberAttribute(MemberInfo info, Attribute attribute)
		{
			Info = info;
			Attribute = attribute;
		}

		public T InfoAs<T>() where T : MemberInfo => (T)Info;

		public T AttrAs<T>() where T : Attribute, IReflectionAttribute
		{
			return (T)Attribute;
		}
	}

	public partial class ReflectionCache
	{
		/// <summary>
		/// Call to see if a given type matches any attributes of interest or if their members have any attributes they care about.
		/// Fills <paramref name="foundAttributes"/> with the results.
		/// </summary>
		/// <param name="member">The <see cref="MemberInfo"/> to check against the <see cref="AttributeOfInterest"/>.</param>
		/// <param name="attributesToSearchFor">List of pre-filtered <see cref="AttributeOfInterest"/> that fails <see cref="AttributeOfInterest.TargetsDeclaredMember"/>.</param>
		/// <param name="declaredMemberAttributesToSearchFor">List of pre-filtered <see cref="AttributeOfInterest"/> that passes <see cref="AttributeOfInterest.TargetsDeclaredMember"/>.</param>
		/// <param name="foundAttributes">Dictionary with pre-allocated lists for all registered <see cref="AttributeOfInterest"/>.</param>
		private void GatherMembersFromAttributesOfInterest(MemberInfo member,
														   IReadOnlyList<AttributeOfInterest> attributesToSearchFor,
														   IReadOnlyList<AttributeOfInterest> declaredMemberAttributesToSearchFor,
														   Dictionary<AttributeOfInterest, List<MemberAttribute>> foundAttributes)
		{

			var customAttributes = member.CustomAttributes; // moved out of loops because we don't want to use this GET many times (heavy)

			bool HasType(Type type)
			{
				foreach (var customAttributeData in customAttributes)
				{
					if (customAttributeData.AttributeType == type)
						return true;
				}

				return false;
			}

			// Check for attributes over the type itself.

			for (int i = 0; i < attributesToSearchFor.Count; i++)
			{
				if (HasType(attributesToSearchFor[i].AttributeType))
				{
					var attributes = member.GetCustomAttributes(attributesToSearchFor[i].AttributeType, false);

					for (int j = 0; j < attributes.Length; j++)
					{
						var attribute = attributes[j] as Attribute;
						foundAttributes[attributesToSearchFor[i]].Add(new MemberAttribute(member, attribute));
					}
				}
			}

			// Checks for Attributes declared over types' members
			if (member.MemberType == MemberTypes.TypeInfo || member.MemberType == MemberTypes.NestedType)
			{
				var type = (Type)member;
				for (int i = 0; i < declaredMemberAttributesToSearchFor.Count; i++)
				{
					// See if this type that we are checking can actually have an attribute of this type. Skip it if we can't.
					var canHaveDeclaredMembers = declaredMemberAttributesToSearchFor[i].CanBeFoundInType(type);
					if (!canHaveDeclaredMembers) continue;

					// For each declared member, check if they have the current attribute of interest -- if they do, add them to the found attribute list.
					// In this step we catch every member with the attribute --- individual systems are welcome to parse and yield errors at a later step.

					var filteredMembers = type.FindMembers(AttributeOfInterest.INTERNAL_TYPE_SEARCH_WHEN_IS_MEMBER_TYPES, BindingFlags.Public |
														   BindingFlags.NonPublic |
														   BindingFlags.Instance |
														   BindingFlags.Static, null, null);

					for (int k = 0; k < filteredMembers.Length; k++)
					{
						if (declaredMemberAttributesToSearchFor[i].TryGetFromMemberInfo(filteredMembers[k], out var attribute))
							foundAttributes[declaredMemberAttributesToSearchFor[i]].Add(new MemberAttribute(filteredMembers[k], attribute));
					}
				}
			}
		}
	}
}
