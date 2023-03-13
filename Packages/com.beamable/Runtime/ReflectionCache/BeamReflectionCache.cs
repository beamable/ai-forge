using Beamable.Common;
using Beamable.Common.Assistant;
using Beamable.Common.Dependencies;
using Beamable.Common.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Beamable.Common.Constants.MenuItems.Assets;
using Debug = System.Diagnostics.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Beamable.Reflection
{
	/// <summary>
	/// <see cref="ReflectionSystemObject"/> that holds the <see cref="BeamReflectionCache.Registry"/> used for <see cref="Beam"/>'s initialization.
	/// <para/>
	/// Goes through all types, exhaustively, then finds and caches static methods with <see cref="RegisterBeamableDependenciesAttribute"/>s.
	/// <para/>
	/// <see cref="Beam"/> calls on <see cref="Registry.LoadCustomDependencies"/> during it's initialization process in order for users to be able to inject dependencies into
	/// <see cref="BeamContext"/>s.
	/// </summary>
#if BEAMABLE_DEVELOPER
	[CreateAssetMenu(fileName = "BeamReflectionCache", menuName = "Beamable/Reflection/Beam Initialization Cache", order = Orders.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1)]
#endif
	public class BeamReflectionCache : ReflectionSystemObject
	{
		private Registry Cache;

		public override IReflectionSystem System => Cache;
		public override IReflectionTypeProvider TypeProvider => Cache;
		public override Type SystemType => typeof(Registry);

		private BeamReflectionCache()
		{
			Cache = new Registry();
		}

		public class Registry : IReflectionSystem
		{
			private static readonly AttributeOfInterest REGISTER_BEAMABLE_DEPENDENCIES_ATTRIBUTE = new AttributeOfInterest(
				typeof(RegisterBeamableDependenciesAttribute),
				new Type[] { typeof(BeamContextSystemAttribute) },
				new Type[] { });

			public List<BaseTypeOfInterest> BaseTypesOfInterest => new List<BaseTypeOfInterest>();
			public List<AttributeOfInterest> AttributesOfInterest => new List<AttributeOfInterest>() { REGISTER_BEAMABLE_DEPENDENCIES_ATTRIBUTE };

			public IReadOnlyCollection<string> TypesContainingDependencyFunctions => _allTypesContainingDependencyFunctions;
			public IReadOnlyCollection<string> SampleTypesContainingDependencyFunctions => _allSampleTypesContainingDependencyFunctions;

			private List<MemberAttribute> _registerBeamableDependencyFunctions;
			private List<MemberAttribute> _sampleBeamableDependencyFunctions;

			private HashSet<string> _allTypesContainingDependencyFunctions;
			private HashSet<string> _allSampleTypesContainingDependencyFunctions;

			private IBeamHintGlobalStorage _hintGlobalStorage;

			public Registry()
			{
				_registerBeamableDependencyFunctions = new List<MemberAttribute>();
				_sampleBeamableDependencyFunctions = new List<MemberAttribute>();
				_allTypesContainingDependencyFunctions = new HashSet<string>();
				_allSampleTypesContainingDependencyFunctions = new HashSet<string>();
			}

			public void ClearCachedReflectionData()
			{
				if (_registerBeamableDependencyFunctions != null)
					_registerBeamableDependencyFunctions.Clear();
				else
					_registerBeamableDependencyFunctions = new List<MemberAttribute>();

				if (_allTypesContainingDependencyFunctions != null)
					_allTypesContainingDependencyFunctions.Clear();
				else
					_allTypesContainingDependencyFunctions = new HashSet<string>();
			}

			public void OnSetupForCacheGeneration()
			{ /* Do nothing as we need no setup. */ }
			public void OnReflectionCacheBuilt(PerBaseTypeCache perBaseTypeCache, PerAttributeCache perAttributeCache)
			{ /* Do nothing when the cache is fully built. We'll do most of the work in the attribute callback. */ }

			public void OnBaseTypeOfInterestFound(BaseTypeOfInterest baseType, IReadOnlyList<MemberInfo> cachedSubTypes)
			{ /* Do nothing as we don't care about any base types here. We'll do most of the work in the attribute callback. */ }

			public void OnAttributeOfInterestFound(AttributeOfInterest attributeType, IReadOnlyList<MemberAttribute> cachedMemberAttributes)
			{
				Debug.Assert(attributeType.Equals(REGISTER_BEAMABLE_DEPENDENCIES_ATTRIBUTE),
							 "This should never fail. If it does, there's a bug in the ReflectionCache parsing code!");

				// Initialize valid members local variable so we can easily #ifdef away the editor only validation.
				var validMembers = cachedMemberAttributes;

				// Run the validation and add hints if in editor.
#if UNITY_EDITOR
				var validationResults = cachedMemberAttributes.Validate();
				validationResults.SplitValidationResults(out var valid, out var warning, out var error);

				if (error.Count > 0)
					_hintGlobalStorage.AddOrReplaceHint(BeamHintType.Validation, BeamHintDomains.BEAM_INIT, BeamHintIds.ID_UNSUPPORTED_REGISTER_BEAMABLE_DEPENDENCY_SIGNATURE, error);

				validMembers = valid.Select(v => v.Pair).ToList();
#endif

				// Pass along the valid members, when not in the editor.
				_registerBeamableDependencyFunctions.AddRange(validMembers);
				_registerBeamableDependencyFunctions.Sort((a, b) =>
				{
					var attrA = a.AttrAs<RegisterBeamableDependenciesAttribute>();
					var attrB = b.AttrAs<RegisterBeamableDependenciesAttribute>();

					return attrA.Order.CompareTo(attrB.Order);
				});
				_allTypesContainingDependencyFunctions.UnionWith(_registerBeamableDependencyFunctions.Select(mb => mb.Info.DeclaringType?.Name));

#if UNITY_EDITOR && BEAMABLE_DEVELOPER
				// In order to keep our samples in project and make the workflow simple enough, we keep track of all the attributes in the Sample folder.
				// By default, none of them are registered. We have an editor utility that allows us to say "Register only the ones declared in these types".
				// It's not "pretty", but it strikes a nice enough balance of our workflow within UPM and complexity added to the code-base.
				_sampleBeamableDependencyFunctions.AddRange(_registerBeamableDependencyFunctions.Where(mb =>
				{
					var registerBeamableDependenciesAttribute = mb.AttrAs<RegisterBeamableDependenciesAttribute>();
					var declPath = registerBeamableDependenciesAttribute.DeclarationPath;
					var isBeamableSample =  declPath.Contains("Packages\\com.beamable") && declPath.Contains("Samples");
					return isBeamableSample;
				}));
				_allSampleTypesContainingDependencyFunctions.UnionWith(_sampleBeamableDependencyFunctions.Select(mb=>mb.Info.DeclaringType?.Name));
#endif

			}

			public void SetStorage(IBeamHintGlobalStorage hintGlobalStorage) => _hintGlobalStorage = hintGlobalStorage;

			/// <summary>
			/// Runs all functions annotated with <see cref="RegisterBeamableDependenciesAttribute"/>, in their correct order, with the given <paramref name="builderToConfigure"/>.
			/// </summary>
			public void LoadCustomDependencies(IDependencyBuilder builderToConfigure, RegistrationOrigin origin = RegistrationOrigin.RUNTIME)
			{
				IEnumerable<MemberAttribute> toRegister = null;


#if UNITY_EDITOR && BEAMABLE_DEVELOPER
				// In order to keep our samples in project and make the workflow simple enough, we keep track of all the attributes in the Sample folder.
				// By default, none of them are registered. We have an editor utility that allows us to say "Register only the ones declared in these types".
				// It's not "pretty", but it strikes a nice enough balance of our workflow within UPM and complexity added to the code-base.
				var allowedSampleFunctions = EditorPrefs.GetString(Constants.EditorPrefKeys.ALLOWED_SAMPLES_REGISTER_FUNCTIONS, "");
				var sampleRegisteredDependencies = _sampleBeamableDependencyFunctions.Where(mb =>
				{
					var registerBeamableDependenciesAttribute = mb.AttrAs<RegisterBeamableDependenciesAttribute>();
					var declPath = registerBeamableDependenciesAttribute.DeclarationPath;
					var memberName = mb.Info.DeclaringType?.Name;
					var isCurrentlyIncludedBeamableSample = allowedSampleFunctions.Split(';').Contains(memberName);

					if(!isCurrentlyIncludedBeamableSample)
						BeamableLogger.Log($"Skipping Register Beamable Dependency in Sample [{mb.Info.DeclaringType?.Name}].\nDeclPath={declPath}\nMemberName={memberName}");
					
					return !isCurrentlyIncludedBeamableSample;
				});
				
				toRegister = _registerBeamableDependencyFunctions.Except(sampleRegisteredDependencies);
#else
				toRegister = _registerBeamableDependencyFunctions;
#endif
				foreach (var registerBeamableDependencyFunction in toRegister)
				{
					var matchesOrigin = origin.HasFlag(registerBeamableDependencyFunction
													   .AttrAs<RegisterBeamableDependenciesAttribute>().Origin);
					if (!matchesOrigin) continue;
					try
					{
						var method = registerBeamableDependencyFunction.InfoAs<MethodInfo>();
						method.Invoke(null, new object[] { builderToConfigure });
					}
					catch (Exception ex)
					{
						var method = registerBeamableDependencyFunction.InfoAs<MethodInfo>();
						UnityEngine.Debug.LogError($"Failed to create instance of {method.GetBaseDefinition().Name}.\n{ex.GetType()}\n{ex.Message}\n{ex.StackTrace}");
					}
				}
			}
		}


	}
}
