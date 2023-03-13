#if !BEAMABLE_DEVELOPER
#define NOT_BEAMABLE_DEVELOPER
#endif

using Beamable.Common;
using Beamable.Editor.UI.Model;
using Beamable.Server.Editor.DockerCommands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using static Beamable.Common.Constants.MenuItems.Windows;
using Debug = UnityEngine.Debug;

namespace Beamable.Server.Editor
{
	[InitializeOnLoad]
	public static class MicroserviceEditor
	{
		public const int portCounter = 3000;

		public static string commandoutputfile = "";
		public static bool isVerboseOutput = false;
		public static bool wasCompilerError = true;
#if UNITY_EDITOR && !UNITY_EDITOR_WIN
   public static string dockerlocation = "/usr/local/bin/docker";
#else
		public static string dockerlocation = "docker";
#endif

		public const string TEMPLATE_DIRECTORY = "Packages/com.beamable.server/Template";
		private const string TEMPLATE_MICROSERVICE_DIRECTORY = TEMPLATE_DIRECTORY;
		private const string DESTINATION_MICROSERVICE_DIRECTORY = "Assets/Beamable/Microservices";

		private const string TEMPLATE_STORAGE_OBJECT_DIRECTORY = TEMPLATE_DIRECTORY + "/StorageObject";
		private const string DESTINATION_STORAGE_OBJECT_DIRECTORY = "Assets/Beamable/StorageObjects";

		private static Dictionary<ServiceType, ServiceCreateInfo> _serviceCreateInfos =
			new Dictionary<ServiceType, ServiceCreateInfo>
			{
				{
					ServiceType.MicroService,
					new ServiceCreateInfo(ServiceType.MicroService, DESTINATION_MICROSERVICE_DIRECTORY, TEMPLATE_MICROSERVICE_DIRECTORY)
				},
				{
					ServiceType.StorageObject,
					new ServiceCreateInfo(ServiceType.StorageObject, DESTINATION_STORAGE_OBJECT_DIRECTORY, TEMPLATE_STORAGE_OBJECT_DIRECTORY)
				}
			};

		public static bool IsInitialized { get; private set; }

		static MicroserviceEditor()
		{
			/// Delaying until first editor tick so that the menu
			/// will be populated before setting check state, and
			/// re-apply correct action
			EditorApplication.delayCall += Initialize;
			void Initialize()
			{
				try
				{
					BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
					var _ = BeamEditorContext.Default; // access the beam context
				}
				catch (InvalidOperationException)
				{
					EditorApplication.delayCall += Initialize;
					return;
				}
				catch (NullReferenceException)
				{
					EditorApplication.delayCall += Initialize;
					return;
				}

				try
				{
					_ = MicroserviceConfiguration.Instance;
				}
				// Solves a specific issue on first installation of package ---
				catch (ModuleConfigurationNotReadyException)
				{
					EditorApplication.delayCall += Initialize;
					return;
				}


				TryToPreloadBaseImage();
				TryToPreloadMongoImage();

				IsInitialized = true;
			}
		}

		[Conditional("NOT_BEAMABLE_DEVELOPER")] // if we are a beamable developer, the image needs to be locally built anyway.
		public static async void TryToPreloadBaseImage()
		{
			if (Application.isPlaying) return;

			try
			{
				var local = PullImageCommand.PullBeamService(CPUArchitectureContext.LOCAL).StartAsync();
				var remote = PullImageCommand.PullBeamService(CPUArchitectureContext.DEPLOY).StartAsync();
				await local;
				await remote;
			}
			catch
			{
				// it does not matter if this request fails- because it is only a preload operation.
				// in the event this fails, the image will be downloaded later.
			}
		}

		/// <summary>
		/// A utility function that will wait for the microservice editor <see cref="IsInitialized"/> flag to be true.
		/// This method should only be called in a task-friendly environment.
		/// </summary>
		public static async Task WaitForInit()
		{
			await Task.Run(async () =>
			{
				while (!IsInitialized)
				{
					await Task.Delay(1);
				}
			});
		}

		public static async void TryToPreloadMongoImage()
		{
			if (Application.isPlaying) return;
			var registry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			if (registry.StorageDescriptors.Count == 0) return;
			var image = registry.StorageDescriptors[0]?.ImageName;
			if (image == null) return;

			try
			{
				await (new PullImageCommand(image).StartAsync());
			}
			catch
			{
				// it does not matter if this request fails- because it is only a preload operation.
				// in the event this fails, the image will be downloaded later.
			}
		}

		public static void CreateNewMicroservice(string microserviceName, List<ServiceModelBase> additionalReferences = null)
		{
			CreateNewServiceFile(ServiceType.MicroService, microserviceName, additionalReferences);
		}

		public static void CreateNewServiceFile(ServiceType serviceType, string serviceName, List<ServiceModelBase> additionalReferences = null)
		{
			AssetDatabase.StartAssetEditing();
			try
			{
				if (string.IsNullOrWhiteSpace(serviceName))
				{
					return;
				}

				var serviceCreateInfo = _serviceCreateInfos[serviceType];
				var rootPath = Directory.GetParent(Application.dataPath).FullName;
				var relativeDestPath = Path.Combine(serviceCreateInfo.DestinationDirectoryPath, serviceName);
				var absoluteDestPath = Path.Combine(rootPath, relativeDestPath);
				var destinationDirectory = Directory.CreateDirectory(absoluteDestPath);

				var scriptTemplatePath = Path.Combine(rootPath, serviceCreateInfo.TemplateDirectoryPath,
													  serviceCreateInfo.TemplateFileName);

				Debug.Assert(File.Exists(scriptTemplatePath));

				// create the asmdef by hand.
				var asmName = serviceType == ServiceType.MicroService
					? $"Beamable.Microservice.{serviceName}"
					: $"Beamable.Storage.{serviceName}";

				var asmPath = relativeDestPath +
						  $"/{asmName}.asmdef";

				var references = new List<string>
				{
					"Unity.Beamable.Runtime.Common",
					"Unity.Beamable.Server.Runtime",
					"Unity.Beamable.Server.Runtime.Shared",
					"Unity.Beamable",
					"Beamable.SmallerJSON",
					"Unity.Beamable.Server.Runtime.Common",
					"Unity.Beamable.Server.Runtime.Mocks",
				};
				if (MicroserviceConfiguration.Instance.AutoBuildCommonAssembly)
				{
					references.Add(CommonAreaService.GetCommonAsmDefName());
				}

				bool referencesStorage = false;
				if (additionalReferences != null && additionalReferences.Count != 0)
				{
					foreach (var additionalReference in additionalReferences)
					{
						// For creating Microservice
						if (additionalReference is MongoStorageModel mongoStorageModel)
						{
							var info = mongoStorageModel.Descriptor.ConvertToInfo();
							references.Add(info.Name);
							referencesStorage = true;
						}
					}
				}

				SetupServiceFileInfo(serviceName, scriptTemplatePath,
									 destinationDirectory.FullName + $"/{serviceName}.cs");
				AssemblyDefinitionHelper.CreateAssetDefinitionAssetOnDisk(
					asmPath,
					new AssemblyDefinitionInfo
					{
						Name = asmName,
						DllReferences =
							serviceType == ServiceType.StorageObject || referencesStorage
								? AssemblyDefinitionHelper.MongoLibraries
								: new string[] { },
						IncludePlatforms = new[] { "Editor" },
						References = references.ToArray()
					});

				CommonAreaService.EnsureCommon();

				if (!string.IsNullOrWhiteSpace(asmName) && additionalReferences != null &&
					additionalReferences.Count != 0)
				{
					// TODO TD000001 Code for adding dependencies to microservice require additional Assets refresh
					AssetDatabase.StopAssetEditing();
					AssetDatabase.Refresh();
					AssetDatabase.StartAssetEditing();
					foreach (var additionalReference in additionalReferences)
					{
						// For creating StorageObject
						if (additionalReference is MicroserviceModel microserviceModel)
						{
							var asm = microserviceModel.ServiceDescriptor.ConvertToAsset();
							Assert.IsNotNull(asm, $"Cannot find {microserviceModel.ServiceDescriptor.Name} assembly definition asset");
							var dict = asm.AddAndRemoveReferences(new List<string> { asmName }, null);

							if (serviceType == ServiceType.StorageObject)
							{
								var path = AssetDatabase.GetAssetPath(asm);
								Assert.IsFalse(string.IsNullOrWhiteSpace(path), $"Cannot find path for {asm}");
								AssemblyDefinitionHelper.AddMongoLibraries(dict, path);
							}
						}
					}
				}
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}

			AssetDatabase.Refresh();
		}

		public static void DeleteServiceFiles(IDescriptor descriptor)
		{
			AssetDatabase.StartAssetEditing();

			try
			{
				if (string.IsNullOrWhiteSpace(descriptor.Name))
				{
					return;
				}

				var rootPath = Directory.GetParent(Application.dataPath).FullName;

				foreach (var serviceCreateInfo in _serviceCreateInfos)
				{
					var relativeDestPath = Path.Combine(serviceCreateInfo.Value.DestinationDirectoryPath, descriptor.Name);
					var absoluteDestPath = Path.Combine(rootPath, relativeDestPath);

					if (Directory.Exists(absoluteDestPath))
					{
						FileUtil.DeleteFileOrDirectory(absoluteDestPath);
					}
				}

				if (descriptor is MicroserviceDescriptor desc)
				{
					FileUtil.DeleteFileOrDirectory(desc.SourcePath);
					FileUtil.DeleteFileOrDirectory(Path.ChangeExtension(desc.SourcePath, "meta"));
					FileUtil.DeleteFileOrDirectory(desc.HidePath);
					FileUtil.DeleteFileOrDirectory(desc.BuildPath);
				}
				else if (descriptor is StorageObjectDescriptor storageDesc)
				{
					string directoryPath = Path.GetDirectoryName(storageDesc.AttributePath);
					FileUtil.DeleteFileOrDirectory(Path.ChangeExtension(storageDesc.AttributePath, "meta"));
					FileUtil.DeleteFileOrDirectory(directoryPath);
				}
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}

			AssetDatabase.Refresh();
		}

		private static void SetupServiceFileInfo(string serviceName, string sourceFile, string targetFile)
		{
			var text = File.ReadAllText(sourceFile);
			text = text.Replace("XXXX", serviceName);
			text = text.Replace("//ZZZZ", "");
			text = text.Replace("xxxx", serviceName.ToLower());
			File.WriteAllText(targetFile, text);
		}

		private class ServiceCreateInfo
		{
			public ServiceType ServiceType { get; }

			public string ServiceTypeName
			{
				get
				{
					switch (ServiceType)
					{
						case ServiceType.MicroService: return "MicroService";
						case ServiceType.StorageObject: return "StorageObject";
					}
					return string.Empty;
				}
			}

			public string DestinationDirectoryPath { get; }
			public string TemplateDirectoryPath { get; }

			public string TemplateFileName
			{
				get
				{
					switch (ServiceType)
					{
						case ServiceType.MicroService: return "Microservice.cs";
						case ServiceType.StorageObject: return "StorageObject.cs";
					}

					return string.Empty;
				}
			}

			public ServiceCreateInfo(ServiceType serviceType, string destinationDirectoryPath, string templateDirectoryPath)
			{
				ServiceType = serviceType;
				DestinationDirectoryPath = destinationDirectoryPath;
				TemplateDirectoryPath = templateDirectoryPath;
			}
		}
	}

	// public enum ServiceType
	// {
	// 	MicroService,
	// 	StorageObject
	// }
}
