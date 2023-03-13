using Beamable.Common;
using Beamable.Common.Assistant;
using Beamable.Common.Spew;
using Beamable.Editor.UI.Model;
using Beamable.Server.Editor.DockerCommands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace Beamable.Server.Editor
{
	[Serializable]
	public struct BeamServiceCodeHandle : IEquatable<BeamServiceCodeHandle>, IEquatable<string>
	{
		public string ServiceName;
		public BeamCodeClass CodeClass;
		public string CodeDirectory;
		public AssemblyDefinitionInfo AsmDefInfo;
		public string Checksum;

		public override string ToString()
		{
			return $"{nameof(ServiceName)}: {ServiceName}, {nameof(CodeClass)}: {CodeClass}, {nameof(CodeDirectory)}: {CodeDirectory}, {nameof(AsmDefInfo)}: {AsmDefInfo}";
		}

		public bool Equals(BeamServiceCodeHandle other) => ServiceName == other.ServiceName;
		public bool Equals(string other) => Checksum == other;

		public override int GetHashCode() => (ServiceName != null ? ServiceName.GetHashCode() : 0);
	}

	[Serializable]
	public struct ServiceDependencyChecksum
	{
		public string ServiceName;
		public string Checksum;
	}

	public enum BeamCodeClass
	{
		Invalid,
		Microservice,
		StorageObject,
		SharedAssembly,
	}

	// ReSharper disable once ClassNeverInstantiated.Global
	public class BeamServicesCodeWatcher : IBeamHintSystem
	{
		private const int CLEANUP_CONTAINERS_TIMEOUT = 3;
		public static BeamServicesCodeWatcher Default
		{
			get
			{
				var codeWatcher = default(BeamServicesCodeWatcher);
				BeamEditor.GetBeamHintSystem(ref codeWatcher);
				return codeWatcher;
			}
		}

		public AssemblyDefinitionInfoCollection CachedUnityAssemblies { get; private set; }
		HashSet<string> CachedStorageAsmNames { get; set; }

		private IBeamHintPreferencesManager PreferencesManager;
		private IBeamHintGlobalStorage GlobalStorage;

		private List<BeamServiceCodeHandle> LatestCodeHandles;
		private Task CheckSumCalculation;

		private Dictionary<MicroserviceDescriptor, ServiceDependencyChecksum> ServiceToChecksum =
			new Dictionary<MicroserviceDescriptor, ServiceDependencyChecksum>();

		public void OnInitialized()
		{
			if (!MicroserviceEditor.IsInitialized)
			{
				EditorApplication.delayCall += OnInitialized;
				return;
			}

			LatestCodeHandles = new List<BeamServiceCodeHandle>(64);
			ServiceToChecksum = new Dictionary<MicroserviceDescriptor, ServiceDependencyChecksum>();

			var registry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			var msCodeHandles = new List<BeamServiceCodeHandle>();
			var cachedDeps = new List<MicroserviceDependencies>();
			CachedUnityAssemblies = new AssemblyDefinitionInfoCollection(AssemblyDefinitionHelper.EnumerateAssemblyDefinitionInfos());
			for (int i = 0; i < registry.Descriptors.Count; i++)
			{
				var cachedInfo = registry.Descriptors[i].ConvertToInfo();

				msCodeHandles.Add(new BeamServiceCodeHandle()
				{
					ServiceName = registry.Descriptors[i].Name,
					CodeClass = BeamCodeClass.Microservice,
					AsmDefInfo = cachedInfo,
					CodeDirectory = Path.GetDirectoryName(cachedInfo.Location),
				});

				cachedDeps.Add(DependencyResolver.GetDependencies(registry.Descriptors[i], CachedUnityAssemblies));
				ServiceToChecksum.Add
				(
					registry.Descriptors[i],
					new ServiceDependencyChecksum
					{
						ServiceName = registry.Descriptors[i].Name,
						Checksum = cachedDeps[i].GetDependencyChecksum()
					}
				);
			}

			var storageCodeHandles = new HashSet<BeamServiceCodeHandle>();

			for (int k = 0; k < registry.StorageDescriptors.Count; k++)
			{
				var cachedInfo = registry.StorageDescriptors[k].ConvertToInfo();
				storageCodeHandles.Add(new BeamServiceCodeHandle()
				{
					ServiceName = registry.StorageDescriptors[k].Name,
					CodeClass = BeamCodeClass.StorageObject,
					AsmDefInfo = cachedInfo,
					CodeDirectory = Path.GetDirectoryName(cachedInfo.Location),
				});
			}

			var sharedAssemblyHandles = new HashSet<BeamServiceCodeHandle>();

			for (int i = 0; i < cachedDeps.Count; i++)
			{
				var defs = cachedDeps[i].Assemblies.ToCopy.Distinct().
										 Except(msCodeHandles.Select(h => h.AsmDefInfo))
										.Except(storageCodeHandles.Select(h => h.AsmDefInfo)).ToArray();

				for (int k = 0; k < defs.Length; k++)
				{
					sharedAssemblyHandles.Add(new BeamServiceCodeHandle
					{
						ServiceName = defs[k].Name,
						CodeDirectory = Path.GetDirectoryName(defs[k].Location),
						CodeClass = BeamCodeClass.SharedAssembly,
						AsmDefInfo = defs[k],
					});
				}
			}

			LatestCodeHandles.Clear();
			LatestCodeHandles.AddRange(sharedAssemblyHandles);
			LatestCodeHandles.AddRange(msCodeHandles);
			LatestCodeHandles.AddRange(storageCodeHandles);
			LatestCodeHandles.Sort((h1, h2) => string.Compare(h1.ServiceName, h2.ServiceName, StringComparison.Ordinal));

			var tasks = new List<Task>(LatestCodeHandles.Count);
			tasks.AddRange(LatestCodeHandles.Select((beamServiceCodeHandle, index) => Task.Factory.StartNew(() =>
			{
				var path = beamServiceCodeHandle.CodeDirectory;
				var files = Directory.GetFiles(path)
									 .Where(file => !file.EndsWith(".meta"));
				if (MicroserviceConfiguration.Instance.EnableHotModuleReload)
				{
					files = files.Where(file => !file.EndsWith(".cs")).ToArray();
				}
				var filesBytes = files.SelectMany(File.ReadAllBytes).ToArray();
				var md5 = MD5.Create();
				var checksum = md5.ComputeHash(filesBytes);
				beamServiceCodeHandle.Checksum = BitConverter.ToString(checksum).Replace("-", string.Empty);
				LatestCodeHandles[index] = beamServiceCodeHandle;
			})));
			CheckSumCalculation = Task.WhenAll(tasks);
			CachedStorageAsmNames = GetStorageAsmNames();
		}

		public void SetPreferencesManager(IBeamHintPreferencesManager preferencesManager) => PreferencesManager = preferencesManager;
		public void SetStorage(IBeamHintGlobalStorage hintGlobalStorage) => GlobalStorage = hintGlobalStorage;

		public void UpdateBuiltImageCodeHandles(string serviceName)
		{
			var config = MicroserviceConfiguration.Instance;
			var currCodeHandles = config.ServiceCodeHandlesOnLastDomainReload;

			if (currCodeHandles == null || currCodeHandles.Count <= 0)
			{
				// there are no current code handles, so there is nothing to check.
				return;
			}

			var serviceToUpdate = currCodeHandles.FirstOrDefault(h => h.ServiceName == serviceName);
			if (string.IsNullOrEmpty(serviceToUpdate.ServiceName))
			{
				// none of the existing code handles reference this service yet, so there is nothing to do.
				return;
			}

			var builtCodeHandles = serviceToUpdate.AsmDefInfo.References
												  .Select(asmName => currCodeHandles.FirstOrDefault(
															  c => c.AsmDefInfo.Name == asmName))
												  .Where(handle => handle.CodeClass != BeamCodeClass.Invalid)
												  .ToList();

			config.LastBuiltDockerImagesCodeHandles.Remove(serviceToUpdate);
			config.LastBuiltDockerImagesCodeHandles.RemoveAll(h => builtCodeHandles.Contains(h));

			config.LastBuiltDockerImagesCodeHandles.Add(serviceToUpdate);
			config.LastBuiltDockerImagesCodeHandles.AddRange(builtCodeHandles);
		}

		public bool IsServiceDependedOnStorage(MicroserviceDescriptor serviceDescriptor)
		{
			MicroserviceDependencies dep = DependencyResolver.GetDependencies(serviceDescriptor, CachedUnityAssemblies);
			var isDependent = dep.Assemblies.ToCopy.Any(asm => CachedStorageAsmNames.Contains(asm.Name));
			return isDependent;
		}

		public void AddMissingMongoDependencies(MicroserviceDescriptor microserviceDescriptor)
		{
			var missingMongoDepsAsmDefs = microserviceDescriptor.ConvertToAsset();
			if (missingMongoDepsAsmDefs.HasMongoLibraries())
			{
				return;
			}

			// Add Mongo Libraries to each of the ones that are missing them.
			AssetDatabase.StartAssetEditing();
			missingMongoDepsAsmDefs.AddMongoLibraries();
			AssetDatabase.StopAssetEditing();
		}

		// Find every declared Microservice that has a dependency on a storage object.
		private HashSet<string> GetStorageAsmNames()
		{
			var storageAsmNames = new HashSet<string>();

			for (int i = 0; i < LatestCodeHandles.Count; i++)
			{
				if (LatestCodeHandles[i].CodeClass == BeamCodeClass.StorageObject)
				{
					storageAsmNames.Add(LatestCodeHandles[i].AsmDefInfo.Name);
				}
			}

			return storageAsmNames;
		}

		public void CheckForLocalChangesNotYetDeployed()
		{
			var microserviceConfiguration = MicroserviceConfiguration.Instance;
			var servicesInNeedOfImageRebuild = new List<BeamServiceCodeHandle>();

			// Get the list of detected Code-based services (see OnInitialized for how these are detected)
			var latestMSHandles = LatestCodeHandles.Where(h => h.CodeClass == BeamCodeClass.Microservice).ToList();
			var latestStorageHandles = LatestCodeHandles.Where(h => h.CodeClass == BeamCodeClass.StorageObject).ToList();
			var latestCommonCodeHandle = LatestCodeHandles.FirstOrDefault((a) => a.CodeClass == BeamCodeClass.SharedAssembly);

			// Check and resolve changes to the common assembly.
			{
				var serializedCommonCodeHandle = microserviceConfiguration.LastBuiltDockerImagesCodeHandles
																		  .FirstOrDefault((a) => a.CodeClass == BeamCodeClass.SharedAssembly);

				// If it changes, we need to inform that all services that depend on it must be rebuilt.
				if (latestCommonCodeHandle.CodeClass != BeamCodeClass.Invalid && serializedCommonCodeHandle.CodeClass != BeamCodeClass.Invalid &&
					!latestCommonCodeHandle.Checksum.Equals(serializedCommonCodeHandle.Checksum))
				{
					servicesInNeedOfImageRebuild
						.AddRange(
							LatestCodeHandles.Where(handle => handle.AsmDefInfo.References.Contains(latestCommonCodeHandle.AsmDefInfo.Name))
						);
					//Debug.Log($"CHANGED IMAGE REBUILD - COMMON => {string.Join(", ", servicesInNeedOfImageRebuild)}");
				}
			}

			// Check and resolve changes to the C#MS assemblies.
			{
				// For each C#MS that DOES exist, we see if they have never been built or have changes in them that haven't been built.
				var changedFromBuild = latestMSHandles.Where(h => DetectChangesInCodeHandle(h, microserviceConfiguration.LastBuiltDockerImagesCodeHandles)).ToList();
				// Add changed or new handle to the list of services in need of a rebuild
				servicesInNeedOfImageRebuild.AddRange(changedFromBuild);
				//Debug.Log($"CHANGED IMAGE REBUILD - C#MS => {string.Join(", ", servicesInNeedOfImageRebuild)}");
			}

			// Check and resolve changes to the StorageObject Assemblies
			{
				// For each C#MS that DOES exist, we see if they are new or if there are were changes made to files in their Assembly.
				var changedOrNewStorages = latestStorageHandles.Where((h) => DetectChangesInCodeHandle(h, microserviceConfiguration.LastBuiltDockerImagesCodeHandles)).ToList();

				// Find the C#MS that depend on this storage
				var msThatDependOnChangedStorages = latestMSHandles.Where(msHandle =>
																			  msHandle.AsmDefInfo.References.Any(asmName =>
																													 changedOrNewStorages.Select(h => h.AsmDefInfo.Name).Contains(asmName)));

				// Add changed or new handle to the list of services in need of a rebuild
				servicesInNeedOfImageRebuild.AddRange(msThatDependOnChangedStorages);
				//Debug.Log($"CHANGED IMAGE REBUILD - Storages => {string.Join(", ", servicesInNeedOfImageRebuild)}");
			}

			// Handle notification of services in need of rebuilds
			servicesInNeedOfImageRebuild = servicesInNeedOfImageRebuild.Distinct().ToList();
			if (microserviceConfiguration.EnableHotModuleReload)
			{
				var registry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
				var dirtyServices = registry.Descriptors.Where(desc =>
				{
					if (!ServiceToChecksum.TryGetValue(desc, out var currentDependency))
					{
						return true;
					}

					var existingChecksum = MicroservicesDataModel.Instance.ServiceDependencyChecksums.FirstOrDefault(
						service => service.ServiceName == desc.Name);

					return !string.Equals(existingChecksum.Checksum, currentDependency.Checksum);
				}).ToList();

				MicroservicesDataModel.Instance.ServiceDependencyChecksums =
					ServiceToChecksum.Select(kvp => kvp.Value).ToList();

				foreach (var service in dirtyServices)
				{
					var _ = RebootContainer(service);
				}
			}
			else
			{
				if (servicesInNeedOfImageRebuild.Count > 0)
				{
					GlobalStorage.AddOrReplaceHint(BeamHintType.Hint,
												   BeamHintDomains.BEAM_CSHARP_MICROSERVICES_DOCKER,
												   BeamHintIds.ID_CHANGES_NOT_DEPLOYED_TO_LOCAL_DOCKER,
												   servicesInNeedOfImageRebuild);
				}
				else
				{
					GlobalStorage.RemoveAllHints(idRegex: BeamHintIds.ID_CHANGES_NOT_DEPLOYED_TO_LOCAL_DOCKER);
				}
			}
		}

		private async Promise RebootContainer(MicroserviceDescriptor descriptor)
		{
			var model = MicroservicesDataModel.Instance.GetMicroserviceModel(descriptor);
			await model.Builder.CheckIfIsRunning();
			if (!model.IsRunning) return;
			await model.BuildAndRestart();
		}

		[DidReloadScripts]
		private static void WatchMicroserviceFiles()
		{
			EditorApplication.wantsToQuit -= WantsToQuit;
			EditorApplication.wantsToQuit += WantsToQuit;

			// If we are not initialized, delay the call until we are.
			if (!BeamEditor.IsInitialized || !MicroserviceEditor.IsInitialized)
			{
				EditorApplication.delayCall += WatchMicroserviceFiles;
				return;
			}

			var codeWatcher = Default;

			// If we are not initialized, delay the call until we are.
			if (codeWatcher == null || codeWatcher.CheckSumCalculation == null || !codeWatcher.CheckSumCalculation.IsCompleted)
			{
				EditorApplication.delayCall += WatchMicroserviceFiles;
				return;
			}

			// Check for the hint regarding local changes that are not deployed to your local docker environment
			codeWatcher.CheckForLocalChangesNotYetDeployed();

			// Handle the client code generation for C#MSs.
			try
			{

				AssetDatabase.StartAssetEditing();
				var registry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
				var microserviceConfiguration = MicroserviceConfiguration.Instance;

				CleanupRunningGeneratorProcesses(registry.Descriptors).Then(_ =>
				{
					if (DockerCommand.DockerNotRunning)
					{
						PlatformLogger.Log("<b><color=red>[Beamable]</color></b> Docker is not running- there would be no code regeneration for microservices.");
						return;
					}
					// Gets the list of currently detected code handles.
					var latestMSHandles = codeWatcher.LatestCodeHandles.Where(h => h.CodeClass == BeamCodeClass.Microservice).ToList();

					// Gets the sub-list of C#MS that were serialized but are no longer detected --- meaning they were deleted
					var msHandlesOnPreviousDomainReload = microserviceConfiguration.ServiceCodeHandlesOnLastDomainReload.Where(h => h.CodeClass == BeamCodeClass.Microservice).ToList();
					var deletedMicroservices = msHandlesOnPreviousDomainReload.Except(latestMSHandles).ToList();

					// For each of those that were deleted, remove the AutoGenerated client code for the C#MS.
					foreach (var msCodeHandle in deletedMicroservices)
					{
						var serviceName = msCodeHandle.ServiceName;
						var generatedFilePath = $"Assets/Beamable/AutoGenerated/Microservices/{serviceName}Client.cs";
						Debug.Log($"Deleting => {generatedFilePath}");
						AssetDatabase.DeleteAsset(generatedFilePath);

						MicroserviceConfiguration.Instance.Microservices.RemoveAll(s => s.ServiceName == serviceName);
					}

					// For every handle, simply by existing, gets the descriptor for the C#MS and [re]-generates the client source code.
					var latestDescriptors = latestMSHandles.Select(h => registry.Descriptors.FirstOrDefault(d => d.Type.Name == h.ServiceName));
					foreach (var descriptor in latestDescriptors)
					{
						Assert.IsTrue(descriptor != null, $"You should never see this! The final substring of the Assembly Name must be the same as the ServiceName.");
						GenerateClientSourceCode(descriptor);
					}

					// Update the serialized ServiceCodeHandles so that the next time we go through this code we can accurately detect the changes to C#MSs and other services.
					microserviceConfiguration.ServiceCodeHandlesOnLastDomainReload = codeWatcher.LatestCodeHandles;

				});

			}
			finally
			{
				AssetDatabase.StopAssetEditing();
				AssetDatabase.Refresh();
			}
		}

		private static bool DetectChangesInCodeHandle(BeamServiceCodeHandle handle, List<BeamServiceCodeHandle> handlesToCheckAgainst)
		{
			// Check to see if the C#MS already existed.
			var indexIntoSerializedCodeHandles = handlesToCheckAgainst.IndexOf(handle);
			var alreadyExisted = indexIntoSerializedCodeHandles != -1;

			// If it did exist, check to see if the checksum of the files in their Assembly are different.
			var serializedCodeHandle = alreadyExisted ? handlesToCheckAgainst[indexIntoSerializedCodeHandles] : default;
			var checksumDiffers = !handle.Equals(serializedCodeHandle.Checksum);

			return (!alreadyExisted || checksumDiffers);
		}

		private static bool WantsToQuit()
		{
			try
			{
				var registry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();

				int allDesc = registry.Descriptors.Count + registry.StorageDescriptors.Count;
				if (allDesc > 0)
				{
					Task task = Task.Run(async () => { await CleanupRunningContainers(); });
					TimeSpan timeout = TimeSpan.FromSeconds(CLEANUP_CONTAINERS_TIMEOUT);
					task.Wait(timeout);
				}
			}
			catch
			{
				Debug.LogError("Failed to clean up running docker containers");
			}

			return true;
		}

		private static Task CleanupRunningContainers()
		{
			var registry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();

			int allDesc = registry.Descriptors.Count + registry.StorageDescriptors.Count;

			if (allDesc > 0)
			{
				foreach (var service in registry.Descriptors)
				{
					var generatorDesc = GetGeneratorDescriptor(service);

					var kill = new StopImageCommand(service, true);
					var killGenerator = new StopImageCommand(generatorDesc, true);

					kill.Start();
					killGenerator.Start();
				}

				foreach (var storage in registry.StorageDescriptors)
				{
					var kill = new StopImageCommand(storage, true);
					var killTool = new StopImageCommand(storage.LocalToolContainerName, true);

					kill.Start();
					killTool.Start();
				}
			}

			return Task.CompletedTask;
		}

		public static MicroserviceDescriptor GetGeneratorDescriptor(IDescriptor service)
		{
			return new MicroserviceDescriptor
			{
				Name = service.Name + Constants.Features.Services.GENERATOR_SUFFIX,
				AttributePath = service.AttributePath,
				Type = service.Type,
				IsGenerator = true
			};
		}

		public static async Promise StopClientSourceCodeGenerator(IDescriptor service)
		{
			var generatorDesc = GetGeneratorDescriptor(service);
			var command = new StopImageReturnableCommand(generatorDesc);
			await command.StartAsync();
		}

		public static void GenerateClientSourceCode(MicroserviceDescriptor service, bool force = false)
		{
			// create silly descriptor
			var generatorDesc = GetGeneratorDescriptor(service);

			var clientPath = Constants.Features.Services.AUTOGENERATED_CLIENT_PATH;
			if (!Directory.Exists(clientPath))
			{
				Directory.CreateDirectory(clientPath);
			}

			var check = new CheckImageReturnableCommand(generatorDesc);


			check.StartAsync().Then(isRunning =>
			{
				if (isRunning && !force)
				{
					FollowGeneratorLogs(service, generatorDesc);
				}
				else
					RebuildAndRegenerate(service, generatorDesc);

			});
		}

		private static void FollowGeneratorLogs(MicroserviceDescriptor service, MicroserviceDescriptor generatorDesc)
		{
			var follow = new FollowLogCommand(service, generatorDesc.ContainerName);
			follow.AddGlobalFilter(message =>
			{
				if (message?.Contains(Constants.Features.Services.Logs.GENERATED_CLIENT_PREFIX) ?? false) return true;
				return MicroserviceLogHelper.TryGetErrorCode(message, out _);
			});
			follow.MapGlobal(log =>
			{
				if (log.Message.Contains(Constants.Features.Services.Logs.GENERATED_CLIENT_PREFIX))
				{
					log.Level = LogLevel.INFO;
					return log;
				}

				var parts = log.Message.Split(' ');
				var otherParts = parts.Skip(1).ToArray();
				log.Message = "Failed to generated client code\n" + Path.GetFileName(parts[0]) + " " + string.Join(" ", otherParts);
				log.Level = LogLevel.ERROR;
				return log;
			});
			follow.Start();
		}

		private static void RebuildAndRegenerate(MicroserviceDescriptor service, MicroserviceDescriptor generatorDesc)
		{
			// definately stop the image, even if there was doubt it was running. Because if we do a "build", any existing image will ABSOLUTELY be ruined by the overcopy.
			new StopImageReturnableCommand(generatorDesc).StartAsync().Then(__ =>
			{
				var getArchitecturesCommand = new GetBuildOutputArchitectureCommand();
				getArchitecturesCommand.StartAsync().Then(arch =>
				{
					var buildCommand = new BuildImageCommand(generatorDesc, arch, false, true);
					buildCommand.StartAsync().Then(_ =>
					{
						var clientCommand = new RunClientGenerationCommand(generatorDesc);
						clientCommand.Start();
						FollowGeneratorLogs(service, generatorDesc);

						// TODO: add some sort of "cleanup" operation
						// TODO: consider add info hint when the generator image is running
					});
				});
			});
		}

		private static async Promise CleanupRunningGeneratorProcesses(List<MicroserviceDescriptor> descriptors)
		{
			List<string> descNames = descriptors.Select(ms => ms.Name.ToLower()).ToList();

#if UNITY_EDITOR && !UNITY_EDITOR_WIN
			await Cleanup("sh");
#else
			await Cleanup("cmd");
#endif
			await Cleanup("docker");

			async Promise Cleanup(string name)
			{
				Process[] allProcesses = Process.GetProcessesByName(name);

				if (allProcesses.Length > 0)
				{
					foreach (var singleProcess in allProcesses)
					{
						if (!singleProcess.HasExited)
						{
							var checkProccessCommand = await new GetProcessComand(singleProcess.Id).StartAsync();

							if (!string.IsNullOrEmpty(checkProccessCommand))
							{
								foreach (var singleDescName in descNames)
								{
									if (checkProccessCommand.Contains($"{singleDescName}_generator"))
									{
										singleProcess.Kill();
										break;
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
