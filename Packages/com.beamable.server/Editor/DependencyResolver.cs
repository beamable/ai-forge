using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Serialization;
using Beamable.Serialization.SmallerJSON;
using Beamable.Server.Editor;
using MongoDB.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Beamable.Server
{
	[Serializable]
	public class AssemblyDefinitionInfo : IEquatable<AssemblyDefinitionInfo>
	{
		public string Name;
		public string[] References = new string[] { };
		public string[] DllReferences = new string[] { };
		public string Location;

		public string[] IncludePlatforms = new string[] { };
		public bool AutoReferenced = false;

		public bool Equals(AssemblyDefinitionInfo other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Name == other.Name;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != this.GetType())
			{
				return false;
			}

			return Equals((AssemblyDefinitionInfo)obj);
		}

		public override int GetHashCode()
		{
			return (Name != null ? Name.GetHashCode() : 0);
		}
	}

	public class AssemblyDefinitionInfoGroup
	{
		public AssemblyDefinitionInfoCollection ToCopy;
		public AssemblyDefinitionInfoCollection Stubbed;
		public AssemblyDefinitionInfoCollection Invalid;

		public HashSet<string> DllReferences = new HashSet<string>();

	}

	public class AssemblyDefinitionNotFoundException : Exception
	{
		public AssemblyDefinitionNotFoundException(string assemblyName) : base($"Cannot find unity assembly {assemblyName}") { }
	}

	public class DllReferenceNotFoundException : Exception
	{
		public DllReferenceNotFoundException(string dllReference) : base($"Cannot find dll reference {dllReference}") { }
	}

	public class AssemblyDefinitionInfoCollection : IEnumerable<AssemblyDefinitionInfo>
	{
		private Dictionary<string, AssemblyDefinitionInfo> _assemblies = new Dictionary<string, AssemblyDefinitionInfo>();

		public AssemblyDefinitionInfoCollection(IEnumerable<AssemblyDefinitionInfo> assemblies)
		{
			_assemblies = assemblies.ToDictionary(a => a.Name);
		}


		public AssemblyDefinitionInfo Find(string assemblyName)
		{
			const string guidPrefix = "GUID:";

			if (assemblyName.StartsWith(guidPrefix))
			{
				var path = AssetDatabase.GUIDToAssetPath(assemblyName.Replace(guidPrefix, string.Empty));
				var assemblyDefinitionInfo = _assemblies.Where(pair => pair.Value.Location == path)
													.Select(pair => pair.Value).FirstOrDefault();
				if (assemblyDefinitionInfo != null)
				{
					return assemblyDefinitionInfo;
				}
			}

			if (!_assemblies.TryGetValue(assemblyName, out var unityAssembly))
			{
				throw new AssemblyDefinitionNotFoundException(assemblyName);
			}

			return unityAssembly;
		}

		public AssemblyDefinitionInfo Find(Type type)
		{
			return Find(type.Assembly);
		}

		public AssemblyDefinitionInfo Find<T>()
		{
			return Find(typeof(T).Assembly);
		}

		public bool Contains(Type t)
		{
			try
			{
				Find(t);
				return true;
			}
			catch
			{
				var dll = System.IO.Path.GetFileName(t.Assembly.Location);
				var reference = _assemblies.Values.FirstOrDefault(asm => asm.DllReferences.Contains(dll));
				var inAnyReference = reference != null;
				return inAnyReference;
			}
		}

		public AssemblyDefinitionInfo Find(Assembly assembly)
		{
			// make sure this assembly exists in the Unity assembly definition list.
			var hasMoreThanOneModule = assembly.Modules.Count() > 1;
			if (hasMoreThanOneModule)
			{
				throw new Exception("Cannot handle multi-module assemblies");
			}

			var moduleName = assembly.Modules.FirstOrDefault().Name.Replace(".dll", "");
			if (!_assemblies.TryGetValue(moduleName, out var unityAssembly))
			{
				throw new Exception($"Cannot handle non unity assemblies yet. moduleName=[{moduleName}]");
			}

			return unityAssembly;
		}

		public IEnumerator<AssemblyDefinitionInfo> GetEnumerator()
		{
			return _assemblies.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public class MicroserviceFileDependencyComparer : IEqualityComparer<MicroserviceFileDependency>
	{
		public bool Equals(MicroserviceFileDependency x, MicroserviceFileDependency y)
		{
			return string.Equals(x.Agnostic.SourcePath, y.Agnostic.SourcePath);
		}

		public int GetHashCode(MicroserviceFileDependency obj)
		{
			return obj.Agnostic.SourcePath.GetHashCode();
		}
	}
	public class MicroserviceFileDependency
	{
		public Type Type;
		public IHasSourcePath Agnostic;

		public override int GetHashCode()
		{
			return Type.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return Type.Equals(obj);
		}


	}

	public class MicroserviceAssemblyDependency
	{
		public Assembly Assembly;
		public string DeveloperMachineLocation => Assembly.Location;
		public string DockerBuildLocation => $"/src/{MicroserviceDescriptor.ASSEMBLY_FOLDER_NAME}{Assembly.Location}"; // lib/path
	}

	public class MicroserviceDependencies
	{
		public List<MicroserviceFileDependency> FilesToCopy;
		public AssemblyDefinitionInfoGroup Assemblies;
		public List<PluginImporter> DllsToCopy;

		public string GetDependencyChecksum()
		{
			var sb = new StringBuilder();
			foreach (var fileDep in FilesToCopy)
			{
				sb.Append(fileDep.Agnostic.SourcePath);
			}

			foreach (var asm in Assemblies.ToCopy)
			{
				sb.Append(asm.Location);
			}

			foreach (var dll in DllsToCopy)
			{
				sb.Append(dll.assetPath);
			}

			using (var md5 = MD5.Create())
			{
				var bytes = Encoding.ASCII.GetBytes(sb.ToString());
				var hash = md5.ComputeHash(bytes);
				var checksum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
				return checksum;
			}
		}
	}

	public class DependencyResolver : MonoBehaviour
	{
		public static HashSet<Type> GetReferencedTypes(Type type)
		{
			var results = new HashSet<Type>();
			if (type == null) return results;

			void Add(Type t)
			{
				if (t == null) return;

				results.Add(t);
				if (t.IsGenericType)
				{
					for (int i = 0; i < t.GenericTypeArguments.Length; i++)
					{
						results.Add(t.GenericTypeArguments[i]);
					}
				}
			}

			// get all methods
			Add(type.BaseType);

#pragma warning disable CS0618

			AgnosticAttribute agnosticAttribute = null;

			if (HasAttribute(type, typeof(AgnosticAttribute)))
			{
				agnosticAttribute = type.GetCustomAttribute<AgnosticAttribute>();
			}

#pragma warning restore CS0618
			if (agnosticAttribute != null && agnosticAttribute.SupportTypes != null)
			{
				for (int i = 0; i < agnosticAttribute.SupportTypes.Length; i++)
				{
					Add(agnosticAttribute.SupportTypes[i]);
				}
			}

			foreach (var method in type.GetMethods())
			{
				// TODO: look at the method body itself for type references... https://github.com/jbevain/mono.reflection/blob/master/Mono.Reflection/MethodBodyReader.cs

				Add(method.ReturnType);

				foreach (var parameter in method.GetParameters())
				{
					Add(parameter.ParameterType);
				}
			}

			// get all fields
			foreach (var field in type.GetFields())
			{
				Add(field.FieldType);
			}

			// get all properties
			foreach (var property in type.GetProperties())
			{
				Add(property.PropertyType);
			}

			// TODO get all generic types

			return new HashSet<Type>(results.Where(t => t != null));
		}

		private static bool IsUnityEngineType(Type t)
		{
			var ns = t.Namespace ?? "";
			var isUnity = ns.StartsWith("UnityEngine") || ns.StartsWith("UnityEditor");
			return isUnity;
		}

		private static bool HasAttribute(MemberInfo memberInfo, Type type)
		{
			foreach (var element in memberInfo.CustomAttributes)
			{
				if (element.AttributeType == type)
				{
					return true;
				}
			}

			return false;
		}

		private static bool IsSystemType(Type t)
		{
			var ns = t.Namespace ?? "";
			return ns.StartsWith("System");
		}

		private static bool IsBeamableType(Type t)
		{
			var ns = t.Namespace ?? "";
			if (typeof(Microservice).IsAssignableFrom(t))
			{
				return false; // TODO: XXX hacky and gross, but we *DO* want the Microservice type to get considered...
			}

			return ns.StartsWith("Beamable.Common") || ns.StartsWith("Beamable.Server");
		}

		private static bool IsStubbedType(Type t)
		{
			var stubbed = new Type[]
			{
                //typeof(JsonSerializable.ISerializable),
                typeof(ArrayDict),
				typeof(JsonSerializable.IStreamSerializer),
				typeof(ContentObject),
				typeof(IContentRef),

				typeof(ContentDelegate)
			};

			// we stub out any generic references to ContentRef and ContentLink, because those themselves are stubbed.

			if (t.IsGenericType)
			{
				var gt = t.GetGenericTypeDefinition();
				if (typeof(ContentRef<>).IsAssignableFrom(gt) || typeof(ContentLink<>).IsAssignableFrom(gt))
				{
					return true;
				}
			}

			return stubbed.Any(s => s == t);
		}

		private static bool IsSourceCodeType(Type t, out IHasSourcePath attribute)
		{
#pragma warning disable CS0618
			attribute = t.GetCustomAttribute<AgnosticAttribute>(false);
#pragma warning restore CS0618
			if (attribute == null)
			{
				attribute = t.GetCustomAttribute<ContentTypeAttribute>(false);
			}

			return attribute != null;
		}

		public static bool IsMicroserviceRoot(Type t)
		{
			return typeof(Microservice).IsAssignableFrom(t);
		}

		public static string GetTypeName(Type t)
		{
			return t.FullName ?? (t.Namespace + "." + t.Name);
		}


		public static AssemblyDefinitionInfoCollection ScanAssemblyDefinitions()
		{
			var assemblies = AssemblyDefinitionHelper.EnumerateAssemblyDefinitionInfos();
			return new AssemblyDefinitionInfoCollection(assemblies);
		}

		private static bool IsInvalid(AssemblyDefinitionInfoCollection assemblies, AssemblyDefinitionInfo assembly)
		{
			var startsWithUnity = assembly.Name.StartsWith("Unity");
			var startsWithUnityBeamable = assembly.Name.StartsWith("Unity.Beamable");

			// disallow unity assemblies from being loaded...
			var isUnityAssembly = (startsWithUnity && !startsWithUnityBeamable);

			return isUnityAssembly;
		}

		private static bool IsStubbed(AssemblyDefinitionInfoCollection assemblies,
			string assemblyName)
		{
			// TODO: maybe don't rebuild this every check?
			var rejectedAssemblies = new HashSet<AssemblyDefinitionInfo>
			{
				assemblies.Find<MicroserviceDescriptor>(), // Server.Editor
                assemblies.Find<MicroserviceClient>(), // Server.Runtime
                assemblies.Find<Microservice>(), // Server.SharedRuntime
                assemblies.Find<PromiseBase>(), // Common
                assemblies.Find<PlatformRequester>(), // Beamable.Platform
                assemblies.Find<ArrayDict>(), // SmallerJson
                assemblies.Find<API>(), // Beamable
                assemblies.Find<StorageDocument>(), // Server Common Runtime
                assemblies.Find<BsonType>(), // Server Mocks

            };

			if (assemblyName.Equals("Unity.Addressables"))
			{
				return true; // this assembly is okay... We couldn't search for it above because the user may not even have it installed :/
			}

			return rejectedAssemblies.FirstOrDefault(a => a.Name.Equals(assemblyName)) != null;
		}

		private static List<MicroserviceFileDependency> GatherAllContentTypes(MicroserviceDescriptor descriptor)
		{
			// the job here is to get all Agnostic types that are marked with "alwaysInclude"

			var output = new List<MicroserviceFileDependency>();
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			for (int i = 0; i < assemblies.Length; i++)
			{
				var isTestAssembly = assemblies[i].FullName.Contains("Test");
				var isEditorAssembly = assemblies[i].FullName.Contains("Editor");
				if (isTestAssembly || isEditorAssembly) continue;
				var types = assemblies[i].GetTypes();

				for (int k = 0; k < types.Length; k++)
				{
					var contentAttr = types[k].GetCustomAttribute<ContentTypeAttribute>();
					if (contentAttr == null) continue;

					output.Add(new MicroserviceFileDependency
					{
						Agnostic = contentAttr,
						Type = types[k]
					});
				}
			}

			return output;
		}

		private static List<PluginImporter> GatherDllDependencies(MicroserviceDescriptor descriptor, AssemblyDefinitionInfoGroup knownAssemblies)
		{

			var importers = PluginImporter.GetAllImporters();
			var dllImporters = knownAssemblies.DllReferences.Select(dllReference =>
			{
				var importer = importers.FirstOrDefault(i =>
				{
					var isMatch = i.assetPath.EndsWith(dllReference);
					return isMatch;
				});
				if (importer == null)
				{
					throw new DllReferenceNotFoundException(dllReference);
				}
				return importer;
			}).ToList();
			return dllImporters;
		}

		public static AssemblyDefinitionInfoGroup GatherAssemblyDependencies(AssemblyDefinitionInfoCollection unityAssemblies, MicroserviceDescriptor descriptor)
		{
			/*
            * We can crawl the assembly definition itself...
            */

			// reject the assembly that represents this microservice, because that will be recompiled separately.
			var selfUnityAssembly = unityAssemblies.Find(descriptor.Type.Assembly);

			// crawl deps of unity assembly...
			var allRequiredUnityAssemblies = new HashSet<AssemblyDefinitionInfo>();
			var stubbedAssemblies = new HashSet<AssemblyDefinitionInfo>();
			var invalidAssemblies = new HashSet<AssemblyDefinitionInfo>();
			var unityAssembliesToExpand = new Queue<AssemblyDefinitionInfo>();
			var totalDllReferences = new HashSet<string>();
			unityAssembliesToExpand.Enqueue(selfUnityAssembly);

			while (unityAssembliesToExpand.Count > 0)
			{
				var curr = unityAssembliesToExpand.Dequeue();
				if (curr == null) continue;
				if (IsStubbed(unityAssemblies, curr.Name))
				{
					stubbedAssemblies.Add(curr);
					continue;
				}

				if (IsInvalid(unityAssemblies, curr))
				{
					invalidAssemblies.Add(curr);
					continue;
				}

				if (!allRequiredUnityAssemblies.Contains(curr))
				{
					allRequiredUnityAssemblies.Add(curr);

					for (int i = 0; i < curr.DllReferences.Length; i++)
					{
						totalDllReferences.Add(curr.DllReferences[i]);
					}

					var asset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(curr.Location);
					var info = asset.ConvertToInfo();

					for (int k = 0; k < info.References.Length; k++)
					{
						try
						{
							var referencedAssembly = unityAssemblies.Find(info.References[k]);
							unityAssembliesToExpand.Enqueue(referencedAssembly);
						}
						catch (AssemblyDefinitionNotFoundException) when (IsStubbed(unityAssemblies, info.References[k]))
						{
							Debug.LogWarning($"Skipping {info.References[k]} because it is a stubbed package. You should still install the package for general safety.");
						}
					}
				}
			}
			return new AssemblyDefinitionInfoGroup
			{
				ToCopy = new AssemblyDefinitionInfoCollection(allRequiredUnityAssemblies),
				Stubbed = new AssemblyDefinitionInfoCollection(stubbedAssemblies),
				Invalid = new AssemblyDefinitionInfoCollection(invalidAssemblies),
				DllReferences = totalDllReferences
			};
		}

		private static List<MicroserviceFileDependency> GatherSingleFileDependencies(MicroserviceDescriptor descriptor, AssemblyDefinitionInfoGroup knownAssemblies)
		{
			Queue<Type> toExpand = new Queue<Type>();
			HashSet<string> seen = new HashSet<string>();
			Dictionary<string, string> trace = new Dictionary<string, string>();

			var fileDependencies = new HashSet<MicroserviceFileDependency>();

			var contentTypes = new List<MicroserviceFileDependency>();
			if (MicroserviceConfiguration.Instance.AutoReferenceContent)
			{
				contentTypes.AddRange(GatherAllContentTypes(descriptor));
			}

			toExpand.Enqueue(descriptor.Type);

			for (int i = 0; i < contentTypes.Count; i++)
			{
				toExpand.Enqueue(contentTypes[i].Type);
				fileDependencies.Add(contentTypes[i]);
			}

			seen.Add(descriptor.Type.FullName);
			while (toExpand.Count > 0)
			{
				var curr = toExpand.Dequeue();
				var currName = GetTypeName(curr);
				seen.Add(currName);

				// run any sort of white list?

				// filter the types that are unityEngine specific...
				if (IsUnityEngineType(curr))
				{
					// TODO: Need to further white-list this, because not all Unity types will be stubbed on server.
					//Debug.Log($"Found Unity Type {currName}");
					//PrintTrace(currName);
					continue; // don't go nuts chasing unity types..
				}

				if (IsSystemType(curr))
				{
					//Debug.Log($"Found System Type {currName}");
					continue; // don't go nuts chasing system types..
				}

				if (IsBeamableType(curr))
				{
					continue;
				}

				if (IsStubbedType(curr))
				{
					//Debug.Log($"Found STUB TYPE {currName}");
					continue;
				}
				var isAssemblyStubbed = knownAssemblies.Stubbed.Contains(curr);
				var isAssemblyKnown = knownAssemblies.ToCopy.Contains(curr);
				var isValidFileDependency = isAssemblyKnown || isAssemblyStubbed;

				if (IsSourceCodeType(curr, out var agnosticAttribute))
				{
					// This is good, we can copy this code
					if (!isValidFileDependency)
					{
						Debug.LogWarning($"WARNING: The type will be pulled into your microservice through an Agnostic attribute. Beamable suggests you put this type into a shared assembly definition instead. type=[{curr}]");
						fileDependencies.Add(new MicroserviceFileDependency
						{
							Type = curr,
							Agnostic = agnosticAttribute
						});
					}
				}
				else if (!IsMicroserviceRoot(curr))
				{
					// check that this type exists in the known assemblies...

					if (!isValidFileDependency)
					{
						Debug.LogError($"Unknown type referenced. Expect Failure. {currName} {curr.Assembly.Location}");
					}
				}


				var references = GetReferencedTypes(curr);

				foreach (var reference in references)
				{
					var referenceName = GetTypeName(reference);
					if (reference == null || seen.Contains(referenceName))
					{
						continue; // we've already seen this type, so march on
					}

					seen.Add(referenceName);
					trace.Add(referenceName, currName);
					toExpand.Enqueue(reference);
				}

			}


			var unstubbedDistinctFileDependencies = fileDependencies
				.Where(d => !knownAssemblies.ToCopy.Contains(d.Type) && !knownAssemblies.Stubbed.Contains(d.Type))
				.Distinct(new MicroserviceFileDependencyComparer())
				.ToList();

			return unstubbedDistinctFileDependencies;
		}


		public static MicroserviceDependencies GetDependencies(MicroserviceDescriptor descriptor, AssemblyDefinitionInfoCollection unityAssemblies = null)
		{
			if (unityAssemblies == null)
			{
				unityAssemblies = ScanAssemblyDefinitions();
			}

			var assemblyRequirements = GatherAssemblyDependencies(unityAssemblies, descriptor);
			var dlls = GatherDllDependencies(descriptor, assemblyRequirements);
			var infos = GatherSingleFileDependencies(descriptor, assemblyRequirements);
			return new MicroserviceDependencies
			{
				FilesToCopy = infos,
				Assemblies = assemblyRequirements,
				DllsToCopy = dlls
			};
		}


	}

}
