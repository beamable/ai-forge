using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Config;
using Beamable.Editor;
using Beamable.Editor.Microservice.UI;
using Beamable.Editor.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Beamable.Common.Constants;

namespace Beamable.Server.Editor
{

	public class MicroserviceConfigConstants : IConfigurationConstants
	{
		public string GetSourcePath(Type type)
		{
			//
			// TODO: make this work for multiple config types
			//       but for now, there is just the one...

			return $"{Directories.BEAMABLE_SERVER_PACKAGE_EDITOR}/microserviceConfiguration.asset";

		}
	}

	public class MicroserviceConfiguration : AbsModuleConfigurationObject<MicroserviceConfigConstants>
	{
#if UNITY_EDITOR_OSX
		const string DOCKER_LOCATION = "/usr/local/bin/docker";
#else
		const string DOCKER_LOCATION = "docker";
#endif

		public static MicroserviceConfiguration Instance => Get<MicroserviceConfiguration>();

		public List<MicroserviceConfigurationEntry> Microservices;

		public List<StorageConfigurationEntry> StorageObjects;

#if !BEAMABLE_DEVELOPER
		[HideInInspector]
#endif
		public List<BeamServiceCodeHandle> ServiceCodeHandlesOnLastDomainReload;

#if !BEAMABLE_DEVELOPER
		[HideInInspector]
#endif
		public List<BeamServiceCodeHandle> LastBuiltDockerImagesCodeHandles;

#if !BEAMABLE_DEVELOPER
		[HideInInspector]
#endif
		[Tooltip("When you run a microservice in the Editor, the prefix controls the flow of traffic. By default, the prefix is your MAC address. If two developers use the same prefix, their microservices will share traffic. The prefix is ignored for games running outside of the Editor."), Delayed]
		public string CustomContainerPrefix;

		private string _cachedContainerPrefix = null;

		[Tooltip("When you build a microservice, any ContentType class will automatically be referenced if this field is set to true. Beamable recommends that you put your ContentTypes into a shared assembly definition instead.")]
		public bool AutoReferenceContent = false;

		[Tooltip("When true, Beamable automatically generates a common assembly called Beamable.UserCode.Shared that is auto-referenced by Unity code, and automatically imported by Microservice assembly definitions. ")]
		public bool AutoBuildCommonAssembly = true;

		[Tooltip("When true, Beamable guarantees any Assembly Definition referencing a StorageObject's AsmDef also references the required Mongo DLLs.")]
		public bool EnsureMongoAssemblyDependencies = true;

		[Tooltip("When you build and run microservices, the logs will be color coded if this field is set to true.")]
		public bool ColorLogs = true;

		[Tooltip("Docker Buildkit may speed up and increase performance on your microservice builds. It is also required to deploy Microservices from an ARM based computer, like a mac computer with an M1 silicon chipset. ")]
		public bool DisableDockerBuildkit = false;

		[Tooltip("It will enable checking if docker desktop is running before you can start microservices.")]
		public bool DockerDesktopCheckInMicroservicesWindow = true;

		[Tooltip("When you run a microservice, automatically reload code changes. This will not change how services are deployed to the realm.")]
		public bool EnableHotModuleReload = true;

		[Tooltip("When enabled, after you start a service, this will automatically prune unused and dangling docker images related to that service.")]
		public bool EnableAutoPrune = true;

		[Tooltip("It will enable microservice health check at the begining of publish process.")]
		public bool EnablePrePublishHealthCheck = true;

		[Tooltip("When enabled, you can override microservice health check timeout on publish. This could be helpfull for slower machines. Value is in seconds.")]
		public OptionalInt PrePublishHealthCheckTimeout;

		[Tooltip("When you enable debugging support for a microservice, if you are using Rider IDE, you can pre-install the debug tools. However, you'll need to specify some details about the version of Rider you are using.")]
		public OptionalMicroserviceRiderDebugTools RiderDebugTools;

		public string DockerCommand
		{
			get
			{
#if UNITY_EDITOR_WIN
				return WindowsDockerCommand;
#else
				return UnixDockerCommand;
#endif
			}
		}
		public string DockerDesktopPath
		{
			get
			{
#if UNITY_EDITOR_WIN
				return WindowsDockerDesktopPath;
#else
				return UnixDockerDesktopPath;
#endif
			}
		}

#pragma warning disable CS0219
		public string WindowsDockerCommand = DOCKER_LOCATION;
		public string UnixDockerCommand = "/usr/local/bin/docker";
		[Tooltip("When you build Microservices, they can be built to an AMD or ARM cpu architecture. By default, locally, Beamable will use whatever the default for your machine is. Allowed values are \"linux/arm64\" or \"linux/amd64\"")]
		public OptionalString LocalMicroserviceCPUArchitecturePreference = new OptionalString();

		[Obsolete("Beamable does not support deploying ARM images. Images will be forced to build as AMD.")]
		[Tooltip("This feature is deprecated. Images will be forced to build as linux/amd64. ")]
		public OptionalString RemoteMicroserviceCPUArchitecturePreference = new OptionalString();

		[FilePathSelector(true, DialogTitle = "Path to Docker Desktop", FileExtension = "exe", OnlyFiles = true)]
		public string WindowsDockerDesktopPath = "C:\\Program Files\\Docker\\Docker\\Docker Desktop.exe";
		[FilePathSelector(true, DialogTitle = "Path to Docker Desktop", FileExtension = "exe", OnlyFiles = true)]
		public string UnixDockerDesktopPath = "/Applications/Docker.app/";
#pragma warning restore CS0219
		private string _dockerCommandCached = DOCKER_LOCATION;
		private bool _dockerCheckCached = true;

		public string ValidatedDockerCommand => string.IsNullOrWhiteSpace(DockerCommand) ?
		   DOCKER_LOCATION :
		   DockerCommand;

#if !BEAMABLE_LEGACY_MSW
		[Tooltip("Microservice Logs are sent to a dedicated logging window. If you enable this field, then service logs will also be sent to the Unity Console.")]
		public bool ForwardContainerLogsToUnityConsole;
#endif

		public Color LogProcessLabelColor = Color.grey;
		public Color LogStandardOutColor = Color.blue;
		public Color LogStandardErrColor = Color.red;
		public Color LogDebugLabelColor = new Color(.25f, .5f, 1);
		public Color LogInfoLabelColor = Color.blue;
		public Color LogErrorLabelColor = Color.red;
		public Color LogWarningLabelColor = new Color(1, .6f, .15f);
		public Color LogFatalLabelColor = Color.red;


#if UNITY_EDITOR
      public override void OnFreshCopy()
      {
         var isDark = EditorGUIUtility.isProSkin;

         if (isDark)
         {
            LogProcessLabelColor = Color.white;
            LogStandardOutColor = new Color(.2f, .4f, 1f);
            LogStandardErrColor = new Color(1, .44f, .2f);
         }
         else
         {
            LogProcessLabelColor = Color.grey;
            LogStandardOutColor = new Color(.4f, .4f, 1f);
            LogStandardErrColor = new Color(1, .44f, .4f);
         }
         _dockerCommandCached = ValidatedDockerCommand;
         _dockerCheckCached = DockerDesktopCheckInMicroservicesWindow;
      }
#endif

		/// <summary>
		/// Get the user's microservice cpu choice.
		/// </summary>
		/// <returns>
		/// Should either be linux/amd64 or linux/arm64, or NULL if no choice is made
		/// </returns>
		public string GetCPUArchitecture(CPUArchitectureContext context)
		{
			switch (context)
			{
				case CPUArchitectureContext.LOCAL:
					return LocalMicroserviceCPUArchitecturePreference?.GetOrElse(() => null);
				case CPUArchitectureContext.DEPLOY:
					return Constants.Features.Docker.CPU_LINUX_AMD_64;
				case CPUArchitectureContext.DEFAULT:
				default:
					return null;
			}
		}

		public StorageConfigurationEntry GetStorageEntry(string storageName)
		{
			var existing = StorageObjects.FirstOrDefault(s => string.Equals(s.StorageName, storageName));
			if (existing == null)
			{
				existing = new StorageConfigurationEntry
				{
					StorageName = storageName,
					StorageType = "mongov1",
					Enabled = true,
					TemplateId = "small",
					LocalDataPort = 12100 + (uint)StorageObjects.Count,
					LocalUIPort = 13100 + (uint)StorageObjects.Count
				};
				StorageObjects.Add(existing);
			}
			return existing;
		}

		public MicroserviceConfigurationEntry GetEntry(string serviceName)
		{
			var existing = Microservices.FirstOrDefault(s => s.ServiceName == serviceName);
			if (existing == null)
			{
				existing = new MicroserviceConfigurationEntry
				{
					ServiceName = serviceName,
					TemplateId = "small",
					Enabled = true,

					DebugData = new MicroserviceConfigurationDebugEntry
					{
						Password = "Password!",
						Username = "root",
						SshPort = 11100 + Microservices.Count
					}
				};

				var isPotentialGenerator = serviceName.EndsWith(Features.Services.GENERATOR_SUFFIX);
				var serializeEntry = true;
				if (isPotentialGenerator)
				{
					var generatedServiceName = serviceName.Substring(0, serviceName.Length - Features.Services.GENERATOR_SUFFIX.Length);
					var existingService = Microservices.FirstOrDefault(s => s.ServiceName == generatedServiceName);
					if (existingService != null)
					{
						// yes, this is a generator, and therefor, we shouldn't serialize its data.
						serializeEntry = false;
					}
				}

				if (serializeEntry)
				{
					Microservices.Add(existing);
				}
			}
			return existing;
		}

		private void OnValidate()
		{
			ServiceCodeHandlesOnLastDomainReload = ServiceCodeHandlesOnLastDomainReload ?? new List<BeamServiceCodeHandle>();

			if (CustomContainerPrefix != _cachedContainerPrefix)
			{
				_cachedContainerPrefix = CustomContainerPrefix;
				ConfigDatabase.SetString("containerPrefix", _cachedContainerPrefix, true, true);
			}

			if (_dockerCommandCached != DockerCommand || _dockerCheckCached != DockerDesktopCheckInMicroservicesWindow)
			{
				_dockerCommandCached = DockerCommand;
				_dockerCheckCached = DockerDesktopCheckInMicroservicesWindow;
				if (MicroserviceWindow.IsInstantiated)
				{
					var tempQualifier = EditorWindow.GetWindow<MicroserviceWindow>();
					tempQualifier.RefreshWindowContent();
				}
			}

			if (string.IsNullOrEmpty(WindowsDockerDesktopPath))
				WindowsDockerDesktopPath = "C:\\Program Files\\Docker\\Docker\\Docker Desktop.exe";
			if (string.IsNullOrEmpty(UnixDockerDesktopPath))
				UnixDockerDesktopPath = "/Applications/Docker.app/";
		}

		public int GetIndex(string serviceName, ServiceType serviceType)
		{
			if (serviceType == ServiceType.StorageObject)
				return StorageObjects.FindIndex(m => m.StorageName == serviceName);
			else
				return Microservices.FindIndex(m => m.ServiceName == serviceName);
		}

		public void SetIndex(string serviceName, int newIndex, ServiceType serviceType)
		{
			if (serviceType == ServiceType.MicroService)
			{
				if (newIndex < 0 || newIndex >= Microservices.Count)
					throw new IndexOutOfRangeException();

				var currentIndex = GetIndex(serviceName, serviceType);
				if (currentIndex != -1)
				{
					var value = Microservices[currentIndex];
					Microservices.RemoveAt(currentIndex);
					Microservices.Insert(newIndex, value);
					Save();
				}
			}
			else
			{
				if (newIndex < 0 || newIndex >= StorageObjects.Count)
					throw new IndexOutOfRangeException();

				var currentIndex = GetIndex(serviceName, serviceType);
				if (currentIndex != -1)
				{
					var value = StorageObjects[currentIndex];
					StorageObjects.RemoveAt(currentIndex);
					StorageObjects.Insert(newIndex, value);
					Save();
				}
			}
		}

		public void Save()
		{
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		public void MoveIndex(string serviceName, int offset, ServiceType serviceType)
		{
			var newIndex = GetIndex(serviceName, serviceType) + offset;

			switch (serviceType)
			{
				case ServiceType.MicroService:
					if (newIndex < 0 || newIndex >= Microservices.Count)
						return;
					break;
				case ServiceType.StorageObject:
					if (newIndex < 0 || newIndex >= StorageObjects.Count)
						return;
					break;
				default:
					return;
			}

			SetIndex(serviceName, newIndex, serviceType);
		}

		public int OrderComparer(string a, string b, ServiceType serviceType)
		{
			var aIdx = GetIndex(a, serviceType);
			if (aIdx < 0) aIdx = Int32.MaxValue;
			var bIdx = GetIndex(b, serviceType);
			if (bIdx < 0) bIdx = Int32.MaxValue;
			if (aIdx > bIdx) return 1;
			return -1;
		}
	}

	[Serializable]
	public class StorageConfigurationEntry
	{
		public string StorageName;
		public string StorageType;
		public bool Enabled;
		public bool Archived;
		public string TemplateId;

		[Tooltip("When running locally, what port will the data be available on?")]
		public uint LocalDataPort;

		[Tooltip("When running locally, what port will the data tool be available on?")]
		public uint LocalUIPort;

		[Tooltip("When running locally, The MONGO_INITDB_ROOT_USERNAME env var for Mongo")]
		public string LocalInitUser = "beamable";
		[Tooltip("When running locally, The MONGO_INITDB_ROOT_PASSWORD env var for Mongo")]
		public string LocalInitPass = "beamable";
	}

	[Serializable]
	[HelpURL("https://www.jetbrains.com/help/rider/2021.3/SSH_Remote_Debugging.html#deployment-remote-debug-tools")]
	public class MicroserviceRiderDebugTools
	{
		[Tooltip("The version of Rider you use on your machine that you will be using to debug the Beamable Microservice. This should be in the format of MAJOR.MINOR.PATCH, like 2021.3.3 ")]
		public string RiderVersion = "2021.3.3";

		[Tooltip("The download link for the Rider debug tools. This may not always match the given Rider version itself.")]
		public string RiderToolsDownloadUrl = "https://download.jetbrains.com/resharper/dotUltimate.2021.3.2/JetBrains.Rider.RemoteDebuggerUploads.linux-x64.2021.3.2.zip";
	}

	[Serializable]
	public class OptionalMicroserviceRiderDebugTools : Optional<MicroserviceRiderDebugTools>
	{

	}

	[Serializable]
	public class MicroserviceConfigurationEntry
	{
		public string ServiceName;
		[Tooltip("If the service should be running on the cloud, in the current realm.")]
		public bool Enabled;
		public bool Archived;
		public string TemplateId;

		[Tooltip("When the container is built, inject the following string into the built docker file.")]
		public string CustomDockerFileStrings;

		[Tooltip("When building locally, should the service be build with debugging tools? If false, you cannot attach breakpoints.")]
		public bool IncludeDebugTools;

		public MicroserviceConfigurationDebugEntry DebugData;

		[HideInInspector] public string LastBuiltCheckSum;

		[HideInInspector]
		public string RobotId;
	}

	[Serializable]
	public class MicroserviceConfigurationDebugEntry
	{
		public string Username = "beamable";
		[Tooltip("The SSH password to use to connect a debugger. This is only supported for local development. SSH is completely disabled on cloud services.")]
		public string Password = "beamable";
		public int SshPort = -1;
	}

	/// <summary>
	/// An enum that describes the various scenarios in which a CPU architecture should be resolved
	/// </summary>
	public enum CPUArchitectureContext
	{
		LOCAL, DEPLOY, DEFAULT
	}
}
