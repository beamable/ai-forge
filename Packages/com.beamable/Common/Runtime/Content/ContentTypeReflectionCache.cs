using Beamable.Common.Assistant;
using Beamable.Common.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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
	public class ContentTypePair
	{
		public Type Type;
		public string Name;
	}

	public class ContentTypeReflectionCache : IReflectionSystem
	{
		private static readonly BaseTypeOfInterest ICONTENT_OBJECT_BASE_TYPE;
		private static readonly List<BaseTypeOfInterest> BASE_TYPES_OF_INTEREST;

		private static readonly AttributeOfInterest CONTENT_TYPE_ATTRIBUTE;
		private static readonly AttributeOfInterest CONTENT_TYPE_FORMERLY_SERIALIZED_AS_ATTRIBUTE;
		private static readonly List<AttributeOfInterest> ATTRIBUTES_OF_INTEREST;

		internal static ContentTypeReflectionCache Instance;
		static ContentTypeReflectionCache()
		{
			ICONTENT_OBJECT_BASE_TYPE = new BaseTypeOfInterest(typeof(IContentObject), false);
			CONTENT_TYPE_ATTRIBUTE = new AttributeOfInterest(typeof(ContentTypeAttribute), new Type[] { }, new[] { typeof(IContentObject) });
			CONTENT_TYPE_FORMERLY_SERIALIZED_AS_ATTRIBUTE = new AttributeOfInterest(typeof(ContentFormerlySerializedAsAttribute), new Type[] { }, new[] { typeof(IContentObject) });
			BASE_TYPES_OF_INTEREST = new List<BaseTypeOfInterest>() { ICONTENT_OBJECT_BASE_TYPE, };
			ATTRIBUTES_OF_INTEREST = new List<AttributeOfInterest>() { CONTENT_TYPE_ATTRIBUTE, CONTENT_TYPE_FORMERLY_SERIALIZED_AS_ATTRIBUTE };
		}

		public List<BaseTypeOfInterest> BaseTypesOfInterest => BASE_TYPES_OF_INTEREST;
		public List<AttributeOfInterest> AttributesOfInterest => ATTRIBUTES_OF_INTEREST;

		public IReadOnlyDictionary<string, Type> ContentTypeToClass => _contentTypeToClass;
		public IReadOnlyDictionary<Type, string> ClassToContentType => _classToContentType;

		private Dictionary<string, Type> _contentTypeToClass = new Dictionary<string, Type>();
		private Dictionary<Type, string> _classToContentType = new Dictionary<Type, string>();

		private IBeamHintGlobalStorage _hintStorage;

		public void SetStorage(IBeamHintGlobalStorage hintGlobalStorage)
		{
			_hintStorage = hintGlobalStorage;
		}

		public void ClearCachedReflectionData()
		{
			_contentTypeToClass.Clear();
			_classToContentType.Clear();
		}

		public void OnSetupForCacheGeneration() { }
		public void OnReflectionCacheBuilt(PerBaseTypeCache perBaseTypeCache, PerAttributeCache perAttributeCache) { }

		public void OnBaseTypeOfInterestFound(BaseTypeOfInterest baseType, IReadOnlyList<MemberInfo> cachedSubTypes)
		{
			// Gather all validation results ensuring the ContentTypeAttribute exists and Validation
			var contentTypeAttributePairs = new List<MemberAttribute>();
			{
				var warningMessage = "This type is not deserializable by Beamable and you will not be able to create content of this type directly via the Content Manager!";
				var validationResults = cachedSubTypes.GetAndValidateAttributeExistence(CONTENT_TYPE_ATTRIBUTE,
																						info => new AttributeValidationResult(
																							null, info, ReflectionCache.ValidationResultType.Warning, warningMessage));

				validationResults.SplitValidationResults(out var valid, out var warnings, out var errors);

				// manually remove the ContentObject from the warnings set.
				warnings.RemoveAll(r => r.Pair.Info == typeof(ContentObject));

#if UNITY_EDITOR
				// Warning level validations that happen for the content type attribute.
				if (warnings.Count > 0)
				{
					var hint = new BeamHintHeader(BeamHintType.Hint, BeamHintDomains.BEAM_CONTENT_CODE_MISUSE, BeamHintIds.ID_CONTENT_TYPE_ATTRIBUTE_MISSING);
					_hintStorage.AddOrReplaceHint(hint, warnings);
				}
#endif

#if UNITY_EDITOR
				// Error level validations that happen for the content type attribute.
				if (errors.Count > 0)
				{
					var hint = new BeamHintHeader(BeamHintType.Validation, BeamHintDomains.BEAM_CONTENT_CODE_MISUSE, BeamHintIds.ID_INVALID_CONTENT_TYPE_ATTRIBUTE);
					_hintStorage.AddOrReplaceHint(hint, errors);
				}
#endif

				contentTypeAttributePairs.AddRange(valid.Select(a => a.Pair));
			}

			// Gather all validation results ensuring the ContentTypeAttribute exists and Validation
			var formerContentTypeAttributePairs = new List<MemberAttribute>();
			{
				var validationResults = cachedSubTypes.GetAndValidateAttributeExistence(CONTENT_TYPE_FORMERLY_SERIALIZED_AS_ATTRIBUTE,
																						info => new AttributeValidationResult(null, info, ReflectionCache.ValidationResultType.Valid, ""));

				validationResults.SplitValidationResults(out var valid, out _, out var errors);

#if UNITY_EDITOR
				// Error validation for attributes placed on invalid classes/types over which they have no effect.
				if (errors.Count > 0)
				{
					var hint = new BeamHintHeader(BeamHintType.Validation, BeamHintDomains.BEAM_CONTENT_CODE_MISUSE, BeamHintIds.ID_INVALID_CONTENT_FORMERLY_SERIALIZED_AS_ATTRIBUTE);
					_hintStorage.AddOrReplaceHint(hint, errors);
				}
#endif

				formerContentTypeAttributePairs.AddRange(valid.Where(a => a.Pair.Attribute != null).Select(a => a.Pair));
			}
			// Quick hack to "fake" the ContentFormerlySerializedAttribute is a ContentType Attribute --- easy and simple way to verify name collisions is to pretend they are of the same type.
			var typeConversionOfAttributes = formerContentTypeAttributePairs
				.Select(input => new MemberAttribute(input.Info, new ContentTypeAttribute(input.AttrAs<ContentFormerlySerializedAsAttribute>().OldTypeName)));

			// Check if no name collisions happened ---- TODO: There's most definitely a better way to do this...
			var mappings = contentTypeAttributePairs.Union(typeConversionOfAttributes).ToList();
			var nameValidationResults = mappings.GetAndValidateUniqueNamingAttributes<ContentTypeAttribute>();
			{
				nameValidationResults.PerAttributeNameValidations.SplitValidationResults(out var valid,
																						 out _,
																						 out var errors);

#if UNITY_EDITOR
				// Print out any errors due to incorrect names
				if (errors.Count > 0)
				{
					var hint = new BeamHintHeader(BeamHintType.Validation, BeamHintDomains.BEAM_CONTENT_CODE_MISUSE, BeamHintIds.ID_INVALID_CONTENT_TYPE_ATTRIBUTE);
					_hintStorage.AddOrReplaceHint(hint, errors);
				}
#endif

#if UNITY_EDITOR
				// Print out any errors due to name collisions.
				if (nameValidationResults.PerNameCollisions.Count > 0)
				{
					var hint = new BeamHintHeader(BeamHintType.Validation, BeamHintDomains.BEAM_CONTENT_CODE_MISUSE, BeamHintIds.ID_CONTENT_TYPE_NAME_COLLISION);
					_hintStorage.AddOrReplaceHint(hint, nameValidationResults.PerNameCollisions);
				}
#endif

				var contentTypeToClassDict = new Dictionary<string, Type>();
				var classToContentTypeDict = new Dictionary<Type, string>();

				var validContentTypes = valid.Select(v => v.Pair.Info as Type).ToList();

				// Cache data 😃
				foreach (var type in validContentTypes)
				{
					AddContentTypeToDictionaries(type, contentTypeToClassDict, classToContentTypeDict);
				}

				_contentTypeToClass = contentTypeToClassDict;
				_classToContentType = classToContentTypeDict;

			}

			Instance = this;
		}

		public void AddContentTypeToDictionaries(Type type) => AddContentTypeToDictionaries(type, _contentTypeToClass, _classToContentType);
		public static void AddContentTypeToDictionaries(Type type, Dictionary<string, Type> contentTypeToClassDict, Dictionary<Type, string> classToContentTypeDict)
		{
			// Guaranteed to exist due to validation.
			var typeName = GetContentTypeName(type);
			var formerlySerializedTypeNames = GetAllValidContentTypeNames(type, true);
			foreach (var possibleTypeName in formerlySerializedTypeNames)
			{
				if (possibleTypeName == null) continue;

				if (contentTypeToClassDict.ContainsKey(possibleTypeName))
					contentTypeToClassDict[possibleTypeName] = type;
				else
					contentTypeToClassDict.Add(possibleTypeName, type);
			}

			// Adds to dictionaries caches for use in serialization/deserialization
			if (contentTypeToClassDict.ContainsKey(typeName))
				contentTypeToClassDict[typeName] = type;
			else
				contentTypeToClassDict.Add(typeName, type);

			if (classToContentTypeDict.ContainsKey(type))
				classToContentTypeDict[type] = typeName;
			else
				classToContentTypeDict.Add(type, typeName);
		}

		public void OnAttributeOfInterestFound(AttributeOfInterest attributeType, IReadOnlyList<MemberAttribute> cachedMemberAttributes) { }

		public static IEnumerable<string> GetAllValidContentTypeNames(Type contentType, bool includeFormerlySerialized)
		{
			if (contentType == null)
			{
				yield return null;
				yield break;
			}

#if !DB_MICROSERVICE
			if (contentType == typeof(ScriptableObject))
			{
				yield return null;
				yield break;
			}

#endif
			var contentTypeAttribute = contentType.GetCustomAttribute<ContentTypeAttribute>(false);

			if (contentTypeAttribute == null)
			{
				/*
				 * [ContentType("x")]
				 * class X : ContentObject
				 *
				 * class Y : X
				 *
				 * [ContentType("z")]
				 * class Z : Y
				 *
				 * x.z.foo
				 */
				//
				var baseNames = GetAllValidContentTypeNames(contentType.BaseType, includeFormerlySerialized);
				foreach (var baseName in baseNames)
				{
					yield return baseName;
				}

				yield break;
			}

			var startType = contentTypeAttribute.TypeName;

			var possibleNames = new HashSet<string> { startType };

			if (includeFormerlySerialized)
			{
				var formerlySerializedAsAttributes =
					contentType.GetCustomAttributes<ContentFormerlySerializedAsAttribute>(false);
				foreach (var formerlySerializedAsAttribute in formerlySerializedAsAttributes)
				{
					if (string.IsNullOrEmpty(formerlySerializedAsAttribute?.OldTypeName)) continue;
					possibleNames.Add(formerlySerializedAsAttribute.OldTypeName);
				}
			}

			var possibleEndNames = GetAllValidContentTypeNames(contentType.BaseType, includeFormerlySerialized);

			foreach (var possibleEnd in possibleEndNames)
			{
				foreach (var possibleStart in possibleNames)
				{
					if (possibleStart != null && possibleEnd != null)
					{
						yield return string.Join(".", possibleEnd, possibleStart);
					}
					else
					{
						yield return possibleEnd ?? possibleStart;
					}
				}
			}
		}

		public static string GetContentTypeName(Type contentType) => GetAllValidContentTypeNames(contentType, false).First();

		public IEnumerable<ContentTypePair> GetAll() => ClassToContentType.Select(kvp => new ContentTypePair { Type = kvp.Key, Name = kvp.Value });

		public IEnumerable<Type> GetContentTypes() => ClassToContentType.Keys;
		public IEnumerable<string> GetContentClassIds() => ContentTypeToClass.Keys;

		public static string GetTypeNameFromId(string id) => id.Substring(0, id.LastIndexOf("."));

		public static string GetContentNameFromId(string id) => id.Substring(id.LastIndexOf(".") + 1);

		public bool TryGetType(string typeName, out Type type) => ContentTypeToClass.TryGetValue(typeName, out type);

		public bool TryGetName(Type type, out string name) => ClassToContentType.TryGetValue(type, out name);

		public Type NameToType(string name) => ContentTypeToClass.TryGetValue(name, out var type) ? type : typeof(ContentObject);

		public string TypeToName(Type type) => ClassToContentType.TryGetValue(type, out var name) ? name : throw new ContentNotFoundException(type);
		public bool HasContentTypeValidClass(string contentId) => ContentTypeToClass.ContainsKey(contentId);

		public Type GetTypeFromId(string id)
		{
			var typeName = GetTypeNameFromId(id);

			if (!ContentTypeToClass.TryGetValue(typeName, out var type))
			{
				// the type doesn't exist, but maybe we can try again?

				var hasAnotherDot = typeName.IndexOf('.') > -1;
				if (hasAnotherDot)
				{
					return GetTypeFromId(typeName);
				}
				else
				{
					return typeof(ContentObject);
				}
			}

			return type;
		}
	}
}
