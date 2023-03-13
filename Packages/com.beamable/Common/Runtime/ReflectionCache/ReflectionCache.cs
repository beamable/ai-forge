using Beamable.Common.Assistant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static System.Diagnostics.Debug;

namespace Beamable.Common.Reflection
{
	/// <summary>
	/// Implement on systems that want to perform an assembly sweep to cache reflection-related information.
	/// </summary>
	public interface IReflectionSystem : IReflectionTypeProvider
	{
		/// <summary>
		/// Called once on each <see cref="IReflectionSystem"/> before building the reflection cache.
		/// Exists mostly to deal with the fact that Unity's initialization hooks are weird and seem to trigger twice when entering playmode.
		/// </summary>
		void ClearCachedReflectionData();

		/// <summary>
		/// Called once on each <see cref="IReflectionSystem"/> before building the reflection cache.
		/// </summary>
		void OnSetupForCacheGeneration();

		/// <summary>
		/// Called once per <see cref="ReflectionCache.GenerateReflectionCache"/> invocation after the assembly sweep <see cref="ReflectionCache.RebuildReflectionCache"/> is completed.
		/// </summary>
		/// <param name="perBaseTypeCache">
		/// Current cached Per-Base Type information.
		/// </param>
		/// <param name="perAttributeCache">
		/// Currently cached Per-Attribute information.
		/// </param>
		void OnReflectionCacheBuilt(PerBaseTypeCache perBaseTypeCache, PerAttributeCache perAttributeCache);

		/// <summary>
		/// Called once per declared <see cref="IReflectionTypeProvider.BaseTypesOfInterest"/> with each base type and
		/// the cached list of types for which <see cref="Type.IsAssignableFrom"/> returns true.
		/// </summary>
		/// <param name="baseType">The base type of interest.</param>
		/// <param name="cachedSubTypes">The list of types for which <see cref="Type.IsAssignableFrom"/> returns true.</param>
		void OnBaseTypeOfInterestFound(BaseTypeOfInterest baseType, IReadOnlyList<MemberInfo> cachedSubTypes);

		/// <summary>
		/// Called once per declared <see cref="IReflectionTypeProvider.AttributesOfInterest"/>.
		/// </summary>
		/// <param name="attributeType">The attribute type of interest.</param>
		/// <param name="cachedMemberAttributes">
		/// The list of all <see cref="MemberInfo"/> and <see cref="Attribute"/> instances matching <paramref name="attributeType"/> that were found in the assembly sweep.
		/// </param>
		void OnAttributeOfInterestFound(AttributeOfInterest attributeType, IReadOnlyList<MemberAttribute> cachedMemberAttributes);

		/// <summary>
		/// Injection point for reflections systems that wish to generate hints. Leave without implementation if no hints are generated.
		/// <para/>
		/// Remember to wrap hint code in "#if UNITY_EDITOR" directives as this storage instance is null during non-editor builds.
		/// </summary>
		void SetStorage(IBeamHintGlobalStorage hintGlobalStorage);
	}

	/// <summary>
	/// Implement this interface and call <see cref="ReflectionCache.RegisterTypeProvider"/> to inform the reflection cache that these types are of interest to you.
	/// </summary>
	public interface IReflectionTypeProvider
	{
		/// <summary>
		/// List of <see cref="BaseTypeOfInterest"/> this provider adds to the assembly sweep.
		/// </summary>
		List<BaseTypeOfInterest> BaseTypesOfInterest
		{
			get;
		}

		/// <summary>
		/// List of <see cref="AttributeOfInterest"/> this provider adds to the assembly sweep.
		/// </summary>
		List<AttributeOfInterest> AttributesOfInterest
		{
			get;
		}
	}

	/// <summary>
	/// We use this class to control the resolution order of our <see cref="IReflectionSystem"/>s.
	/// </summary>
	public static class BeamableReflectionSystemPriorities
	{
		/// <summary>
		/// Priority for the <see cref="BeamHint"/>-related Reflection system.
		/// Since we need to gather some data before we resolve any BeamContexts when we are in the editor, this must happen before.
		/// </summary>
		public const int BEAM_HINT_REFLECTION_SYSTEM_PRIORITY = 0;

		/// <summary>
		/// Priority for the BeamContext user-dependency reflection system.
		/// </summary>
		public const int BEAM_CONTEXT_DEPENDENCIES_REFLECTION_SYSTEM_PRIORITY = 100;
	}

	/// <summary>
	/// Used to initialize all reflection based systems with consistent validation and to ensure we are only doing the assembly sweeping once.
	/// We can also use this to setup up compile-time validation of our Attribute-based systems such as Content and Microservices.
	/// </summary>
	// ReSharper disable once ClassNeverInstantiated.Global
	public partial class ReflectionCache
	{
		/// <summary>
		/// Validation results used by <see cref="AttributeValidationResult"/> and, potentially, other validation structs.
		/// </summary>
		public enum ValidationResultType
		{
			Valid,
			Warning,
			Error,

			Discarded
		}

		/// <summary>
		/// Just a pre-allocation so we don't keep re-allocating the list mid-loop
		/// (TODO: this may need to go up or down based on numbers we see after we start using the system more heavily).
		/// </summary>
		private const int PRE_ALLOC_SYSTEM_AND_PROVIDER_AMOUNT = 16;

		/// <summary>
		/// Just a pre-allocation so we don't keep re-allocating the list mid-loop
		/// (TODO: this may need to go up or down based on numbers we see after we start using the system more heavily).
		/// </summary>
		private const int PRE_ALLOC_TYPE_CACHES_AMOUNT = 256;

		/// <summary>
		/// List of registered <see cref="IReflectionTypeProvider"/>s.
		/// </summary>
		private readonly List<IReflectionTypeProvider> _registeredProvider;

		/// <summary>
		/// List of registered <see cref="IReflectionSystem"/>s that'll get the callbacks after <see cref="GenerateReflectionCache"/> finishes the assembly sweep.
		/// </summary>
		private readonly List<IReflectionSystem> _registeredCacheUserSystems;

		/// <summary>
		/// A <see cref="PerBaseTypeCache"/> holding all the cached reflection data for our <see cref="BaseTypeOfInterest"/> flows.
		/// </summary>
		private readonly PerBaseTypeCache _perBaseTypeCache;

		/// <summary>
		/// A <see cref="PerAttributeCache"/> holding all the cached reflection data for our <see cref="AttributeOfInterest"/> flows.
		/// </summary>
		private readonly PerAttributeCache _perAttributeCache;

		/// <summary>
		/// Creates a new <see cref="ReflectionCache"/> instance.
		/// </summary>
		public ReflectionCache()
		{
			_registeredProvider = new List<IReflectionTypeProvider>(PRE_ALLOC_SYSTEM_AND_PROVIDER_AMOUNT);
			_registeredCacheUserSystems = new List<IReflectionSystem>(PRE_ALLOC_SYSTEM_AND_PROVIDER_AMOUNT);

			_perBaseTypeCache = new PerBaseTypeCache(
				new List<BaseTypeOfInterest>(PRE_ALLOC_TYPE_CACHES_AMOUNT),
				new Dictionary<BaseTypeOfInterest, List<Type>>(PRE_ALLOC_TYPE_CACHES_AMOUNT)
			);

			_perAttributeCache = new PerAttributeCache(
				new List<AttributeOfInterest>(PRE_ALLOC_SYSTEM_AND_PROVIDER_AMOUNT),
				new List<AttributeOfInterest>(PRE_ALLOC_SYSTEM_AND_PROVIDER_AMOUNT),
				new Dictionary<AttributeOfInterest, List<MemberAttribute>>(PRE_ALLOC_TYPE_CACHES_AMOUNT)
			);
		}

		public void SetStorage(IBeamHintGlobalStorage hintGlobalStorage)
		{
			foreach (var userSystem in _registeredCacheUserSystems)
				userSystem.SetStorage(hintGlobalStorage);
		}

		/// <summary>
		/// Call to unregister all <see cref="IReflectionTypeProvider"/> from the <see cref="ReflectionCache"/>.
		/// </summary>
		public void ClearProviders()
		{
			_registeredProvider.Clear();
		}

		/// <summary>
		/// Returns the first instance of the <see cref="IReflectionSystem"/> of type <typeparamref name="TReflectionUserSystem"/> that was previously registered.
		/// Fails if no system of type <typeparamref name="TReflectionUserSystem"/> is found.
		/// </summary>
		public TReflectionUserSystem GetFirstSystemOfType<TReflectionUserSystem>() where TReflectionUserSystem : IReflectionSystem
		{
			return (TReflectionUserSystem)_registeredCacheUserSystems.First(system => system.GetType() == typeof(TReflectionUserSystem));
		}

		/// <summary>
		/// Register a <see cref="IReflectionTypeProvider"/> with the cache.
		/// This must be called before Beamable's initialization or you must manage the initialization of this cache yourself.
		/// <para/>
		/// Type providers define which types should the ReflectionCache gather up and build maps around.
		/// See the properties of <see cref="IReflectionTypeProvider"/> for more info on how they are used.
		/// </summary>
		public void RegisterTypeProvider(IReflectionTypeProvider provider)
		{
			Assert(provider != null, "Provider cannot be null. Please ensure the provider instance exists when passing it in here.");
			Assert(!_registeredProvider.Contains(provider), "Already registered this provider --- Please ensure providers are registered a single time. " +
															"This is makes the Assembly Sweep more efficient.");

			// Guard so people don't accidentally shoot themselves in the foot when defining their attributes of interest.
			foreach (var attributeOfInterest in provider.AttributesOfInterest)
			{
				// What this does is:
				//   - If the attribute of interest Has a Method/Constructor/Property/Field/Event Target, we'll look for them into each individual type that's given in the two lists declared here.,
				//   - Will work with structs, classes both declared at root or internal as the Assembly.GetTypes() returns all of these.
				//
				// Assumption 1 ===> Does not need work for parameters or return values --- this is specific enough that each individual user system can do their own thing here.
				if (attributeOfInterest.Targets.HasFlag(AttributeOfInterest.INTERNAL_TYPE_SEARCH_WHEN_ATTRIBUTE_TARGETS))
				{
					// If you didn't tell us where to look, we'd have to look everywhere -- which is terrible for editor performance so we don't support it.
					if (attributeOfInterest.FoundInBaseTypes.Count == 0 && attributeOfInterest.FoundInTypesWithAttributes.Count == 0)
					{
						throw new ArgumentException(
							$"{nameof(AttributeOfInterest)} [{attributeOfInterest.AttributeType.Name}] with these {nameof(AttributeTargets)} [{AttributeOfInterest.INTERNAL_TYPE_SEARCH_WHEN_ATTRIBUTE_TARGETS.ToString()}]" +
							$"must have at least one entry into the {nameof(attributeOfInterest.FoundInBaseTypes)} or {nameof(attributeOfInterest.FoundInTypesWithAttributes)} lists.\n" +
							$"Without it, we would need to go into every existing type which would be bad for editor performance.");
					}
				}
			}

			_registeredProvider.Add(provider);
		}

		/// <summary>
		/// Register a <see cref="IReflectionTypeProvider"/> with the cache.
		/// This must be called before Beamable's initialization or you must manage the initialization of this cache yourself.
		/// <para/>
		/// Type providers define which types should the ReflectionCache gather up and build maps around.
		/// See the properties of <see cref="IReflectionTypeProvider"/> for more info on how they are used.
		/// </summary>
		/// <returns>True if the type provider was successfully added, or false if the provider already existed in the cache.</returns>
		public bool TryRegisterTypeProvider(IReflectionTypeProvider provider)
		{
			Assert(provider != null, "Provider cannot be null. Please ensure the provider instance exists when passing it in here.");
			if (_registeredProvider.Contains(provider)) return false;
			RegisterTypeProvider(provider);
			return true;
		}

		/// <summary>
		/// Registers a <see cref="IReflectionSystem"/> with the Reflection Cache System.
		/// This must be called before Beamable's initialization or you must manage the initialization of this cache yourself.
		/// <para/>
		/// You can find Beamable's systems at: API.cs and EditorAPI.cs, for runtime dependencies and editor dependencies respectively.
		/// </summary>
		public void RegisterReflectionSystem(IReflectionSystem system)
		{
			Assert(system != null, "System cannot be null. Please ensure the system instance exists when passing it in here.");
			Assert(!_registeredCacheUserSystems.Contains(system), "Already registered this system --- Please ensure systems are registered a single time. " +
																  "This is makes the Assembly Sweep more efficient and makes it so that you run the system callbacks run only once.");

			_registeredCacheUserSystems.Add(system);
		}

		/// <summary>
		/// Registers a <see cref="IReflectionSystem"/> with the Reflection Cache System.
		/// This must be called before Beamable's initialization or you must manage the initialization of this cache yourself.
		/// <para/>
		/// You can find Beamable's systems at: API.cs and EditorAPI.cs, for runtime dependencies and editor dependencies respectively.
		/// </summary>
		/// <returns>True if the system was successfully added, or false if the system already existed in the cache.</returns>
		public bool TryRegisterReflectionSystem(IReflectionSystem system)
		{
			Assert(system != null, "System cannot be null. Please ensure the system instance exists when passing it in here.");
			if (_registeredCacheUserSystems.Contains(system)) return false;
			RegisterReflectionSystem(system);
			return true;
		}


		/// <summary>
		/// This is a very slow function call. It triggers a full sweep of all assemblies in the project and regenerate <see cref="_perBaseTypeCache"/> and <see cref="_perAttributeCache"/>.
		/// Then, it invokes all callbacks of all <see cref="IReflectionSystem"/> registered via <see cref="RegisterReflectionSystem"/>.
		/// </summary>
		/// <param name="assembliesToSweep">A list of Assembly names for us to sweep in.</param>
		public void GenerateReflectionCache(IReadOnlyList<string> assembliesToSweep)
		{
			RebuildReflectionCache(assembliesToSweep);
			RebuildReflectionUserSystems();
		}

		/// <summary>
		/// This is a very slow function call. It triggers a full sweep of all assemblies in the project and regenerate <see cref="_perBaseTypeCache"/> and <see cref="_perAttributeCache"/>.
		/// Strive to call this once at initialization or editor reload.
		/// </summary>
		public void RebuildReflectionCache(IReadOnlyList<string> sortedAssembliesToSweep = null)
		{
			// Clear existing cache
			_perBaseTypeCache.BaseTypes.Clear();
			_perBaseTypeCache.MappedSubtypes.Clear();

			_perAttributeCache.AttributeTypes.Clear();
			_perAttributeCache.MemberAttributeTypes.Clear();
			_perAttributeCache.AttributeMappings.Clear();

			// Prepare lists of base types and attributes that we care about.
			var baseTypesOfInterest = (IReadOnlyList<BaseTypeOfInterest>)_registeredProvider.SelectMany(provider => provider.BaseTypesOfInterest).ToList();
			var attributesOfInterest = (IReadOnlyList<AttributeOfInterest>)_registeredProvider.SelectMany(provider => provider.AttributesOfInterest).ToList();

			// Prepare lists of assemblies we don't care about or care about preventing people from defining types/attributes of interest in them.
			sortedAssembliesToSweep = sortedAssembliesToSweep ?? new List<string>();

			BuildTypeCaches(in _perBaseTypeCache,
							in _perAttributeCache,
							in baseTypesOfInterest,
							in attributesOfInterest,
							in sortedAssembliesToSweep);
		}

		/// <summary>
		/// Goes through all <see cref="_registeredCacheUserSystems"/> and invokes their callbacks with the currently cached data.
		/// </summary>
		public void RebuildReflectionUserSystems(List<Type> userSystemTypesToRebuild = null)
		{
			_registeredCacheUserSystems.ForEach(sys =>
			{
				if (userSystemTypesToRebuild != null && !userSystemTypesToRebuild.Contains(sys.GetType()))
					return;

				sys.ClearCachedReflectionData();
				sys.OnSetupForCacheGeneration();
			});

			// Pass down to each given system only the types they are interested in
			foreach (var reflectionBasedSystem in _registeredCacheUserSystems)
			{
				if (userSystemTypesToRebuild != null && !userSystemTypesToRebuild.Contains(reflectionBasedSystem.GetType()))
				{

					// TODO: Add a conditional log line.
					continue;
				}

				reflectionBasedSystem.OnReflectionCacheBuilt(_perBaseTypeCache, _perAttributeCache);

				for (int index = 0; index < reflectionBasedSystem.BaseTypesOfInterest.Count; index++)
				{
					if (!_perBaseTypeCache.MappedSubtypes.TryGetValue(reflectionBasedSystem.BaseTypesOfInterest[index], out var mappedSubtypes))
					{
						// TODO: Add a conditional log line.
						continue;
					}
					reflectionBasedSystem.OnBaseTypeOfInterestFound(reflectionBasedSystem.BaseTypesOfInterest[index], mappedSubtypes);
				}

				for (int index = 0; index < reflectionBasedSystem.AttributesOfInterest.Count; index++)
				{
					if (!_perAttributeCache.AttributeMappings.TryGetValue(reflectionBasedSystem.AttributesOfInterest[index], out var mappedAttributes))
					{
						// TODO: Add a conditional log line
						continue;
					}
					reflectionBasedSystem.OnAttributeOfInterestFound(reflectionBasedSystem.AttributesOfInterest[index], mappedAttributes);
				}
			}
		}

		/// <summary>
		/// Internal method that generates, given a list of base types, a dictionary of each type that <see cref="Type.IsAssignableFrom"/> to each base type.
		/// </summary>
		private void BuildTypeCaches(in PerBaseTypeCache perBaseTypeLists,
									 in PerAttributeCache perAttributeLists,
									 in IReadOnlyList<BaseTypeOfInterest> baseTypesOfInterest,
									 in IReadOnlyList<AttributeOfInterest> attributesOfInterest,
									 in IReadOnlyList<string> sortedAssembliesToSweep)
		{
			// Initialize Per-Base Cache
			{
				perBaseTypeLists.BaseTypes.AddRange(baseTypesOfInterest);
				foreach (var baseType in baseTypesOfInterest)
				{
					perBaseTypeLists.MappedSubtypes.Add(baseType, new List<Type>());
				}
			}

			// Initialize Per-Attribute Cache
			{
				// Clear the existing list
				perAttributeLists.AttributeTypes.Clear();
				perAttributeLists.MemberAttributeTypes.Clear();

				// Split attributes between declared over types and declared over members
				for (int i = 0; i < attributesOfInterest.Count; i++)
				{
					if (!attributesOfInterest[i].TargetsDeclaredMember)
					{
						perAttributeLists.AttributeTypes.Add(attributesOfInterest[i]);
					}
					else
					{
						perAttributeLists.MemberAttributeTypes.Add(attributesOfInterest[i]);
					}

					perAttributeLists.AttributeMappings.Add(attributesOfInterest[i], new List<MemberAttribute>());
				}
			}

			// TODO: Use TypeCache in editor and Unity 2019 and above... This path should go through BEAMABLE_MICROSERVICE || DB_MICROSERVICE so we can use the reflection cache stuff in the base image.
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			// Groups by whether or not the assembly is one we care about sweeping.

			var assembliesToSweepStr = "₢" + string.Join("₢", sortedAssembliesToSweep) + "₢";


			// Check all that don't have the IgnoreFromBeamableAssemblySweepAttribute and parse them
			{
				for (int i = 0; i < assemblies.Length; i++)
				{
					if (assembliesToSweepStr.Contains("₢" + assemblies[i].GetName().Name + "₢"))
					{
						var types = assemblies[i].GetTypes();

						for (int k = 0; k < types.Length; k++)
						{
							// Get a list of all attributes of interest that were found on this type.
							GatherMembersFromAttributesOfInterest(types[k],
																  perAttributeLists.AttributeTypes,
																  perAttributeLists.MemberAttributeTypes,
																  perAttributeLists.AttributeMappings);

							// Check for base types of interest
							if (TryFindBaseTypesOfInterest(types[k], baseTypesOfInterest, out var foundType))
							{
								if (perBaseTypeLists.MappedSubtypes.TryGetValue(foundType, out var baseTypesList))
									baseTypesList.Add(types[k]);
							}
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Helper class containing useful extensions for dealing with common reflection-related operations.
	/// </summary>
	public static partial class ReflectionCacheExtensions
	{
		/// <summary>
		/// Generates a human-readable signature from a <see cref="MethodInfo"/>.
		/// </summary>
		public static string ToHumanReadableSignature(this MethodInfo info)
		{
			var paramsDeclaration = string.Join(",", info.GetParameters().Select(param =>
			{
				var prefix = param.IsOut ? "out " :
					param.IsIn ? "in " :
					param.ParameterType.IsByRef ? "ref " :
					"";

				return $"{prefix}{param.ParameterType.Name} {param.Name}";
			}));
			var staticModifier = info.IsStatic ? "static " : "";
			return $"{staticModifier}{info.ReturnType.Name}({paramsDeclaration})";
		}
	}
}
