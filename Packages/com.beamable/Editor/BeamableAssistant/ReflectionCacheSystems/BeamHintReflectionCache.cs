using Beamable.Common.Assistant;
using Beamable.Common.Reflection;
using Beamable.Editor.Assistant;
using Beamable.Reflection;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static Beamable.Common.Constants.MenuItems.Assets.Orders;

namespace Beamable.Editor.Reflection
{
	/// <summary>
	/// <see cref="BeamHint"/> related <see cref="IReflectionSystem"/> and <see cref="ReflectionSystemObject"/> that handles:
	/// <list type="bullet">
	/// <item><description>
	/// The parsing of <see cref="BeamHintDetailConverterAttribute"/> into cached converters that know how to render <see cref="BeamHintDetailsConfig"/> for matched <see cref="BeamHintHeader"/>s.
	/// </description></item>
	/// <item><description>
	/// An updated list of all <see cref="BeamHintDetailsConfig"/>s that exist in directories configured at <see cref="CoreConfiguration.BeamableAssistantHintDetailConfigPaths"/>.
	/// </description></item>
	/// <item><description>
	/// The parsing of <see cref="BeamHintIdAttribute"/>s and <see cref="BeamHintDomainAttribute"/>s TODO: to be used by UI systems to allow for better interaction with our system.
	/// </description></item>
	/// </list>
	/// </summary>
#if BEAMABLE_DEVELOPER
	[CreateAssetMenu(fileName = "BeamHintDetailsReflectionCache", menuName = "Beamable/Reflection/Beam Hints Cache", order = MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1)]
#endif
	public class BeamHintReflectionCache : ReflectionSystemObject
	{
		[NonSerialized] private Registry _cache;

		public override IReflectionSystem System => _cache;

		public override IReflectionTypeProvider TypeProvider => _cache;

		public override Type SystemType => typeof(Registry);

		private BeamHintReflectionCache()
		{
			_cache = new Registry();
		}

		private void OnValidate() => Priority = BeamableReflectionSystemPriorities.BEAM_HINT_REFLECTION_SYSTEM_PRIORITY;

		public delegate void DefaultConverter(in BeamHint hint, in BeamHintTextMap textMap, BeamHintVisualsInjectionBag injectionBag);

		public class Registry : IReflectionSystem
		{
			private static readonly BaseTypeOfInterest BEAM_HINT_SYSTEM_TYPE;
			private static readonly BaseTypeOfInterest BEAM_HINT_DETAIL_CONVERTER_PROVIDER_TYPE;
			private static readonly List<BaseTypeOfInterest> BASE_TYPES_OF_INTEREST;

			private static readonly AttributeOfInterest BEAM_HINT_SYSTEM_ATTRIBUTE;
			private static readonly AttributeOfInterest BEAM_HINT_DOMAIN_PROVIDER_ATTRIBUTE;
			private static readonly AttributeOfInterest BEAM_HINT_ID_PROVIDER_ATTRIBUTE;
			private static readonly AttributeOfInterest BEAM_HINT_DETAIL_CONVERTER_ATTRIBUTE;
			private static readonly List<AttributeOfInterest> ATTRIBUTES_OF_INTEREST;

			static Registry()
			{
				BEAM_HINT_SYSTEM_TYPE = new BaseTypeOfInterest(typeof(IBeamHintSystem));
				BEAM_HINT_DETAIL_CONVERTER_PROVIDER_TYPE = new BaseTypeOfInterest(typeof(BeamHintDetailConverterProvider), true);
				BEAM_HINT_DETAIL_CONVERTER_ATTRIBUTE = new AttributeOfInterest(typeof(BeamHintDetailConverterAttribute), new Type[] { }, new[] { typeof(BeamHintDetailConverterProvider) });

				BEAM_HINT_SYSTEM_ATTRIBUTE = new AttributeOfInterest(typeof(BeamHintSystemAttribute), new Type[] { }, new Type[] { });

				BEAM_HINT_DOMAIN_PROVIDER_ATTRIBUTE = new AttributeOfInterest(typeof(BeamHintDomainAttribute), new Type[] { }, new[] { typeof(BeamHintDomainProvider) });
				BEAM_HINT_ID_PROVIDER_ATTRIBUTE = new AttributeOfInterest(typeof(BeamHintIdAttribute), new Type[] { }, new Type[] { typeof(BeamHintIdProvider) });

				BASE_TYPES_OF_INTEREST = new List<BaseTypeOfInterest>() { BEAM_HINT_DETAIL_CONVERTER_PROVIDER_TYPE, BEAM_HINT_SYSTEM_TYPE };
				ATTRIBUTES_OF_INTEREST = new List<AttributeOfInterest>() { BEAM_HINT_DOMAIN_PROVIDER_ATTRIBUTE, BEAM_HINT_ID_PROVIDER_ATTRIBUTE, BEAM_HINT_DETAIL_CONVERTER_ATTRIBUTE, };
			}

			public List<BaseTypeOfInterest> BaseTypesOfInterest => BASE_TYPES_OF_INTEREST;
			public List<AttributeOfInterest> AttributesOfInterest => ATTRIBUTES_OF_INTEREST;

			// Domain/Ids Attributes data
			private readonly Dictionary<string, List<string>> _perProviderDomains;
			private readonly Dictionary<string, List<string>> _perProviderIds;

			// Converter Attributes data
			private readonly List<BeamHintDetailsConfig> _loadedConfigs;
			private readonly List<BeamHintTextMap> _loadedTextMaps;
			private readonly List<ConverterData<DefaultConverter>> _defaultConverterDelegates;

			// IBeamHintSystem sub-types data
			private readonly List<IBeamHintSystem> _globallyAccessibleHintSystems;
			private readonly List<ConstructorInfo> _beamContextAccessibleHintSystems;

			private IBeamHintGlobalStorage _hintStorage;

			public Registry()
			{
				_perProviderDomains = new Dictionary<string, List<string>>(16);
				_perProviderIds = new Dictionary<string, List<string>>(16);

				_loadedConfigs = new List<BeamHintDetailsConfig>(16);
				_loadedTextMaps = new List<BeamHintTextMap>(16);
				_defaultConverterDelegates = new List<ConverterData<DefaultConverter>>(16);
				_globallyAccessibleHintSystems = new List<IBeamHintSystem>(16);
				_beamContextAccessibleHintSystems = new List<ConstructorInfo>(16);
			}

			/// <summary>
			/// Readonly accessor to the list of cached and instantiated <see cref="IBeamHintSystem"/> that are not injected into Beam Context.
			/// </summary>
			public IReadOnlyList<IBeamHintSystem> GloballyAccessibleHintSystems => _globallyAccessibleHintSystems;

			public IReadOnlyList<ConstructorInfo> BeamContextAccessibleHintSystems => _beamContextAccessibleHintSystems;

			/// <summary>
			/// Called to load all <see cref="BeamHintTextMap"/>s in the given <paramref name="hintConfigPaths"/>.
			/// It ensures the loaded <see cref="ConverterData{T}"/> instances are up-to-date with their references to <see cref="BeamHintTextMap"/>.
			/// </summary>
			public void ReloadHintTextMapScriptableObjects(List<string> hintConfigPaths)
			{
				_loadedTextMaps.Clear();

				var beamHintTextMapGuids = BeamableAssetDatabase.FindAssets<BeamHintTextMap>(hintConfigPaths
																									.Where(Directory.Exists)
																									.ToArray());

				// Reload Detail Config Scriptable Objects
				foreach (string beamHintTextMapGuid in beamHintTextMapGuids)
				{
					var assetPath = AssetDatabase.GUIDToAssetPath(beamHintTextMapGuid);
					var hintTextMap = AssetDatabase.LoadAssetAtPath<BeamHintTextMap>(assetPath);
					_loadedTextMaps.Add(hintTextMap);
				}

				// Update Configured Converters to ensure they point to the correct HintDetailsConfigs after the reload
				for (var i = 0; i < _defaultConverterDelegates.Count; i++)
				{
					var converterData = _defaultConverterDelegates[i];
					_defaultConverterDelegates[i] = BuildConverterData(converterData.Matcher.MatchType, converterData.Matcher.DomainSubstring, converterData.Matcher.IdRegex,
																	   converterData.UserOverrideHintConfigDetailsConfigId, converterData.HintConfigDetailsConfigId, converterData.ConverterCall);
				}
			}

			/// <summary>
			/// Called to load all <see cref="BeamHintDetailsConfig"/>s in the given <paramref name="hintConfigPaths"/>.
			/// It ensures the loaded <see cref="ConverterData{T}"/> instances are up-to-date with their references to <see cref="BeamHintDetailsConfig"/>.
			/// </summary>
			public void ReloadHintDetailConfigScriptableObjects(List<string> hintConfigPaths)
			{
				_loadedConfigs.Clear();

				var beamHintDetailsConfigsGuids = BeamableAssetDatabase.FindAssets<BeamHintDetailsConfig>(hintConfigPaths
																												 .Where(Directory.Exists)
																												 .ToArray());

				// Reload Detail Config Scriptable Objects
				foreach (string beamHintDetailConfigGuid in beamHintDetailsConfigsGuids)
				{
					var assetPath = AssetDatabase.GUIDToAssetPath(beamHintDetailConfigGuid);
					var hintDetailsConfig = AssetDatabase.LoadAssetAtPath<BeamHintDetailsConfig>(assetPath);
					_loadedConfigs.Add(hintDetailsConfig);
				}

				// Update Configured Converters to ensure they point to the correct HintDetailsConfigs after the reload
				for (var i = 0; i < _defaultConverterDelegates.Count; i++)
				{
					var converterData = _defaultConverterDelegates[i];
					_defaultConverterDelegates[i] = BuildConverterData(converterData.Matcher.MatchType, converterData.Matcher.DomainSubstring, converterData.Matcher.IdRegex,
																	   converterData.UserOverrideHintConfigDetailsConfigId, converterData.HintConfigDetailsConfigId, converterData.ConverterCall);
				}
			}

			/// <summary>
			/// Tries to get a <see cref="ConverterData{T}"/> struct for a hint with the given <paramref name="header"/>.
			/// </summary>
			public bool TryGetConverterDataForHint(BeamHintHeader header, out ConverterData<DefaultConverter> converter)
			{
				var firstMatchingConverterIdx = _defaultConverterDelegates.FindIndex(cvt => cvt.Matcher.MatchAgainstHeader(header));
				if (firstMatchingConverterIdx != -1)
				{
					converter = _defaultConverterDelegates[firstMatchingConverterIdx];
					return true;
				}

				converter = default;
				return false;
			}

			/// <summary>
			/// Tries to get the title text defined in the <see cref="BeamHintTextMap"/> tied to the first matching hint id of the given <paramref name="header"/>.
			/// </summary>
			public bool TryGetHintTitleText(BeamHintHeader header, out string titleText)
			{
				for (var i = 0; i < _loadedTextMaps.Count; i++)
				{
					var txtMap = _loadedTextMaps[i];

					if (txtMap.TryGetHintTitle(header, out titleText))
						return true;
				}

				titleText = header.Id;
				return false;
			}

			/// <summary>
			/// Tries to get the title text defined in the <see cref="BeamHintTextMap"/> tied to the first matching domain of the given <paramref name="domainSubString"/>.
			/// </summary>
			public bool TryGetDomainTitleText(string domainSubString, out string titleText)
			{
				for (var i = 0; i < _loadedTextMaps.Count; i++)
				{
					var txtMap = _loadedTextMaps[i];

					if (txtMap.TryGetDomainTitle(domainSubString, out titleText))
						return true;
				}

				titleText = domainSubString;
				return false;
			}

			public void SetStorage(IBeamHintGlobalStorage hintGlobalStorage) => _hintStorage = hintGlobalStorage;

			public void ClearCachedReflectionData()
			{
				_perProviderDomains.Clear();
				_perProviderIds.Clear();
				_defaultConverterDelegates.Clear();
			}

			public void OnSetupForCacheGeneration()
			{
				ReloadHintTextMapScriptableObjects(BeamEditor.CoreConfiguration.BeamableAssistantHintDetailConfigPaths);
				ReloadHintDetailConfigScriptableObjects(BeamEditor.CoreConfiguration.BeamableAssistantHintDetailConfigPaths);
			}

			public void OnReflectionCacheBuilt(PerBaseTypeCache perBaseTypeCache,
											   PerAttributeCache perAttributeCache)
			{ }

			public void OnBaseTypeOfInterestFound(BaseTypeOfInterest baseType, IReadOnlyList<MemberInfo> cachedSubTypes)
			{
				// Handle BeamHint System types.
				if (baseType.Equals(BEAM_HINT_SYSTEM_TYPE))
				{
					var attributes = cachedSubTypes.GetAndValidateAttributeExistence(BEAM_HINT_SYSTEM_ATTRIBUTE,
																					 info => new AttributeValidationResult(null,
																														   info,
																														   ReflectionCache.ValidationResultType.Warning,
																														   "No Attribute Detected! Assuming it is not a BeamContext system!"));

					attributes.SplitValidationResults(out var valid, out var warning, out var errors);

					if (errors.Count > 0)
					{
						_hintStorage.AddOrReplaceHint(BeamHintType.Validation, BeamHintDomains.BEAM_ASSISTANT_CODE_MISUSE, BeamHintIds.ID_MISCONFIGURED_HINT_SYSTEM_ATTRIBUTE, errors);
					}

					// Get the types of hints based on whether or not they have the attribute (if not, we assume it is a Globally Accessible System
					var splitByTypeOfHintSystems = valid.Union(warning)
														.GroupBy(pair =>
														{
															var isGloballyAccessibleHintSystem = pair.Pair.Attribute == null || !pair.Pair.AttrAs<BeamHintSystemAttribute>().IsBeamContextSystem;
															return isGloballyAccessibleHintSystem;
														});

					// Handle globally accessible hint systems
					{
						var globallyAccessibleSystemTypes = splitByTypeOfHintSystems
															.Where(g => g.Key)
															.SelectMany(g => g)
															.ToList();

						var constructors = globallyAccessibleSystemTypes.Select(pair => pair.Pair.Info)
																		.Cast<Type>()
																		.Select(cachedSubType => cachedSubType
																					.GetConstructor(new Type[] { }))
																		.GroupBy(constructor => constructor != null)
																		.ToList();

						var validConstructors = constructors
												.Where(group => group.Key)
												.SelectMany(g => g)
												.ToList();

						var hintSystems = validConstructors
										  .Select(constructor => constructor.Invoke(null))
										  .Cast<IBeamHintSystem>();

						_globallyAccessibleHintSystems.Clear();
						_globallyAccessibleHintSystems.AddRange(hintSystems);
					}

					// Handle BeamContext injected hint systems
					{
						var beamContextSystemTypes = splitByTypeOfHintSystems
												 .Where(g => !g.Key)
												 .SelectMany(g => g)
												 .ToList();

						// For each type, finds a parameterless constructor --- this is by design. These dependency-less classes that have one or two pure functions (or at least that
						// only depend on internal state).
						var constructors = beamContextSystemTypes
							.Select(res =>
							{
								var type = (Type)res.Pair.Info;
								var maximumNumberOfParametersConstructor = type
									.GetConstructor(new Type[] { });

								return maximumNumberOfParametersConstructor;
							});

						// Store the constructor here. These are injected by BeamEditor.ConditionallyRegisterBeamHintsAsServices ONLY while in the editor.
						_beamContextAccessibleHintSystems.AddRange(constructors);
					}
				}
			}

			public void OnAttributeOfInterestFound(AttributeOfInterest attributeType, IReadOnlyList<MemberAttribute> cachedMemberAttributes)
			{
				// Handle Beam Hint Domain providers
				if (attributeType.Equals(BEAM_HINT_DOMAIN_PROVIDER_ATTRIBUTE))
				{
					// TODO: Store domains in whatever way makes it easier for users to get the list of domains they are interested in.
					foreach (var domainFields in cachedMemberAttributes.Select(result => result.Info).Cast<FieldInfo>())
					{
						var providerName = domainFields.DeclaringType?.FullName ?? string.Empty;
						if (!_perProviderDomains.TryGetValue(providerName, out var domainList))
						{
							domainList = new List<string>();
							_perProviderDomains.Add(providerName, domainList);
						}

						domainList.Add((string)domainFields.GetValue(null));
					}
				}

				// Handle Beam Hint Id providers
				if (attributeType.Equals(BEAM_HINT_ID_PROVIDER_ATTRIBUTE))
				{
					// TODO: Store domains in whatever way makes it easier for users to get the list of domains they are interested in.
					foreach (var idField in cachedMemberAttributes.Select(result => result.Info).Cast<FieldInfo>())
					{
						var providerName = idField.DeclaringType?.FullName ?? string.Empty;
						if (!_perProviderIds.TryGetValue(providerName, out var idsList))
						{
							idsList = new List<string>();
							_perProviderIds.Add(providerName, idsList);
						}

						idsList.Add((string)idField.GetValue(null));
					}
				}

				// Handle Beam Hint Id providers
				if (attributeType.Equals(BEAM_HINT_DETAIL_CONVERTER_ATTRIBUTE))
				{
					var validationResults = cachedMemberAttributes.Validate();
					validationResults.SplitValidationResults(out var valid, out _, out var invalid);

					if (invalid.Count > 0)
					{
						_hintStorage.AddOrReplaceHint(BeamHintType.Validation, BeamHintDomains.BEAM_ASSISTANT_CODE_MISUSE, BeamHintIds.ID_MISCONFIGURED_HINT_DETAILS_PROVIDER, invalid);
					}

					var validConverters = valid.Select(result => result.Pair);
					foreach (var cachedMemberAttribute in validConverters)
					{
						var attribute = (BeamHintDetailConverterAttribute)cachedMemberAttribute.Attribute;
						var methodInfo = (MethodInfo)cachedMemberAttribute.Info;

						// Cache a built delegate to be called as a converter.
						if (attribute.DelegateType == typeof(DefaultConverter))
						{
							var cachedDelegate = Delegate.CreateDelegate(attribute.DelegateType, methodInfo) as DefaultConverter;
							_defaultConverterDelegates.Add(BuildConverterData<DefaultConverter>(attribute.MatchType, attribute.DomainSubstring, attribute.IdRegex,
																								attribute.UserOverrideToHintDetailConfigId, attribute.HintDetailConfigId, cachedDelegate));
						}
					}
				}
			}

			/// <summary>
			/// Generates a <see cref="ConverterData{T}"/> tying together all references mapped by a <see cref="BeamHintDetailConverterAttribute"/>.
			/// </summary>
			private ConverterData<T> BuildConverterData<T>(BeamHintType type, string domain, string idRegex, string userOverrideHintDetailConfigId, string hintDetailConfigId, T cachedDelegate)
				where T : Delegate
			{
				BeamHintDetailsConfig userOverrideConfig = null;
				BeamHintDetailsConfig defaultConfig = null;
				BeamHintTextMap textMap = null;

				for (int configIndex = 0; configIndex < _loadedConfigs.Count; configIndex++)
				{
					if (userOverrideConfig == null && userOverrideHintDetailConfigId == _loadedConfigs[configIndex].Id)
					{
						userOverrideConfig = _loadedConfigs[configIndex];
					}

					if (defaultConfig == null && hintDetailConfigId == _loadedConfigs[configIndex].Id)
					{
						defaultConfig = _loadedConfigs[configIndex];
					}
				}

				Regex regexToCheck = new Regex(idRegex);
				for (int index = 0; index < _loadedTextMaps.Count; index++)
				{
					if (_loadedTextMaps[index].HintIdToHintTitle.Keys.Any(k => regexToCheck.IsMatch(k)))
					{
						textMap = _loadedTextMaps[index];
						break;
					}
				}

				return new ConverterData<T>
				{
					Matcher = new HeaderMatcher(type, domain, idRegex),
					HintConfigDetailsConfigId = hintDetailConfigId,
					UserOverrideHintConfigDetailsConfigId = userOverrideHintDetailConfigId,
					HintConfigDetailsConfig = userOverrideConfig == null ? defaultConfig : userOverrideConfig,
					HintTextMap = textMap,
					ConverterCall = cachedDelegate
				};
			}

			/// <summary>
			/// Gets the first text map that matches the given header. TODO: Replace when we have our in-editor localization solution
			/// </summary>
			public BeamHintTextMap GetTextMapForId(BeamHintHeader header)
			{
				return _loadedTextMaps.FirstOrDefault(txtMap => txtMap.TryGetHintTitle(header, out _));
			}
		}

		/// <summary>
		/// The result of the mapping configured via each <see cref="BeamHintDetailConverterAttribute"/>.
		/// <para/>
		/// This is used by <see cref="BeamHintHeaderVisualElement"/> to render the hint details as needed.
		/// </summary>
		[Serializable]
		public struct ConverterData<T> where T : Delegate
		{
			public HeaderMatcher Matcher;

			public string HintConfigDetailsConfigId;
			public string UserOverrideHintConfigDetailsConfigId;
			public BeamHintDetailsConfig HintConfigDetailsConfig;
			public BeamHintTextMap HintTextMap;

			public T ConverterCall;
		}

		/// <summary>
		/// A helper struct that defines how to match a Header to configured the configured <see cref="DomainSubstring"/> and <see cref="IdRegex"/>
		/// </summary>
		[Serializable]
		public struct HeaderMatcher
		{
			public BeamHintType MatchType;
			public string DomainSubstring;
			public string IdRegex;

			private Regex _regex;

			public HeaderMatcher(BeamHintType matchType, string domainSubstring, string idRegex) : this()
			{
				MatchType = matchType;
				DomainSubstring = domainSubstring;
				IdRegex = idRegex;
				_regex = new Regex(idRegex);
			}

			public bool MatchAgainstHeader([NotNull] BeamHintHeader other)
			{
				var matchType = MatchType.HasFlag(other.Type);
				var matchDomain = string.IsNullOrEmpty(DomainSubstring) || other.Domain.Contains(DomainSubstring);
				var idMatch = string.IsNullOrEmpty(IdRegex) || _regex.IsMatch(other.Id);

				return matchType && matchDomain && idMatch;
			}
		}
	}
}
