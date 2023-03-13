using Beamable.Common;
using Beamable.Editor.UI.Components;
using Beamable.Server;
using Beamable.Server.Editor;
using Beamable.Server.Editor.DockerCommands;
using Beamable.Server.Editor.ManagerClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static Beamable.Common.Constants.Features.Services;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

using static Beamable.Common.Constants.Features.Archive;

namespace Beamable.Editor.UI.Model
{
	[Serializable]
	public class MicroserviceModel : ServiceModelBase, IBeamableMicroservice
	{
		[SerializeField]
		private MicroserviceDescriptor _serviceDescriptor;

		[SerializeField]
		private string _assemblyQualifiedMicroserviceTypeName;
		public MicroserviceDescriptor ServiceDescriptor
		{
			get => _serviceDescriptor;
			set
			{
				_serviceDescriptor = value;
				if (_serviceDescriptor.Type != null)
					_assemblyQualifiedMicroserviceTypeName = _serviceDescriptor.Type.AssemblyQualifiedName;
			}
		}

		public string AssemblyQualifiedMicroserviceTypeName => _assemblyQualifiedMicroserviceTypeName;

		[field: SerializeField]
		public MicroserviceBuilder ServiceBuilder { get; protected set; }
		public override IBeamableBuilder Builder => ServiceBuilder;
		public override IDescriptor Descriptor => ServiceDescriptor;

		[field: SerializeField]
		public ServiceReference RemoteReference { get; protected set; }


		[field: SerializeField]
		public ServiceStatus RemoteStatus { get; protected set; }

		public override bool IsArchived
		{
			get => Config.Archived;
			protected set => Config.Archived = value;
		}

		public MicroserviceConfigurationEntry Config => MicroserviceConfiguration.Instance.GetEntry(Descriptor.Name);
		public List<MongoStorageModel> Dependencies { get; set; } = new List<MongoStorageModel>(); // TODO: This is whacky.
		public override bool IsRunning => ServiceBuilder?.IsRunning ?? false;
		public bool IsBuilding => ServiceBuilder?.IsBuilding ?? false;
		public bool SameImageOnRemoteAndLocally => string.Equals(ServiceBuilder?.LastBuildImageId, RemoteReference?.imageId);
		public bool IncludeDebugTools
		{
			get => Config.IncludeDebugTools;
			set
			{
				Config.IncludeDebugTools = value;
				EditorUtility.SetDirty(MicroserviceConfiguration.Instance);
			}
		}

		public Action<ServiceReference> OnRemoteReferenceEnriched;
		public Action<ServiceStatus> OnRemoteStatusEnriched;

		public override event Action<Task> OnStart;
		public override event Action<Task> OnStop;
		public event Action<Task> OnBuildAndRestart;
		public event Action<Task> OnBuildAndStart;
		public event Action<Task> OnBuild;
		public event Action<Promise<Unit>> OnDockerLoginRequired;

		public static MicroserviceModel CreateNew(MicroserviceDescriptor descriptor, MicroservicesDataModel dataModel)
		{
			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			return new MicroserviceModel
			{
				ServiceDescriptor = descriptor,
				ServiceBuilder = serviceRegistry.GetServiceBuilder(descriptor),
				RemoteReference = dataModel.GetReference(descriptor),
				RemoteStatus = dataModel.GetStatus(descriptor),
			};
		}

		public override Task Start()
		{
			OnLogsAttached?.Invoke();
			var task = ServiceBuilder.TryToStart();
			OnStart?.Invoke(task);
			return task;
		}
		public override Task Stop()
		{
			var task = ServiceBuilder.TryToStop();
			OnStop?.Invoke(task);
			return task;
		}

		public override void OpenDocs()
		{
			if (IsRunning)
				OpenLocalDocs();
		}

		public Task BuildAndRestart()
		{
			var task = ServiceBuilder.TryToBuildAndRestart(IncludeDebugTools);
			OnBuildAndRestart?.Invoke(task);
			return task;
		}
		public Task BuildAndStart()
		{
			var task = ServiceBuilder.TryToBuildAndStart(IncludeDebugTools);
			OnBuildAndStart?.Invoke(task);
			return task;
		}
		public Task Build()
		{
			var task = ServiceBuilder.TryToBuild(IncludeDebugTools);
			OnBuild?.Invoke(task);
			return task;
		}

		private void OpenLocalDocs()
		{
			var de = BeamEditorContext.Default;
			var url = $"{BeamableEnvironment.PortalUrl}/{de.CurrentCustomer.Alias}/games/{de.ProductionRealm.Pid}/realms/{de.CurrentRealm.Pid}/microservices/{ServiceDescriptor.Name}/docs?prefix={MicroserviceIndividualization.Prefix}&refresh_token={de.Requester.Token.RefreshToken}";
			Application.OpenURL(url);
		}
		public void EnrichWithRemoteReference(ServiceReference remoteReference)
		{
			RemoteReference = remoteReference;
			OnRemoteReferenceEnriched?.Invoke(remoteReference);
		}
		public void EnrichWithStatus(ServiceStatus status)
		{
			RemoteStatus = status;
			OnRemoteStatusEnriched?.Invoke(status);
		}

		public override void PopulateMoreDropdown(ContextualMenuPopulateEvent evt)
		{
			var existsOnRemote = RemoteReference?.enabled ?? false;
			var hasImageSuffix = ServiceBuilder.HasBuildDirectory ? string.Empty : " (Build first)";
			var localCategory = IsRunning ? "Local" : "Local (not running)";
			var remoteCategory = existsOnRemote ? "Cloud" : "Cloud (not deployed)";
			var debugToolsSuffix = IncludeDebugTools ? string.Empty : " (Debug tools disabled)";
			evt.menu.BeamableAppendAction($"Reveal build directory{hasImageSuffix}", pos =>
			{
				var full = Path.GetFullPath(ServiceDescriptor.BuildPath);
				EditorUtility.RevealInFinder(full);
			}, ServiceBuilder.HasBuildDirectory);

			evt.menu.BeamableAppendAction($"Run Snyk Tests{hasImageSuffix}", pos => RunSnykTests(), ServiceBuilder.HasImage);

			evt.menu.BeamableAppendAction($"{localCategory}/Open in CLI", pos => OpenInCli(), IsRunning);
			evt.menu.BeamableAppendAction($"{localCategory}/View Documentation", pos => OpenDocs(), IsRunning);
			evt.menu.BeamableAppendAction($"{localCategory}/Regenerate {_serviceDescriptor.Name}Client.cs", pos =>
			{
				BeamServicesCodeWatcher.GenerateClientSourceCode(_serviceDescriptor, true);
			});
			evt.menu.BeamableAppendAction($"{remoteCategory}/View Documentation", pos => { OpenOnRemote("docs/"); }, existsOnRemote);
			evt.menu.BeamableAppendAction($"{remoteCategory}/View Metrics", pos => { OpenOnRemote("metrics"); }, existsOnRemote);
			evt.menu.BeamableAppendAction($"{remoteCategory}/View Logs", pos => { OpenOnRemote("logs"); }, existsOnRemote);
			evt.menu.BeamableAppendAction($"Visual Studio Code/Copy Debug Configuration{debugToolsSuffix}", pos => { CopyVSCodeDebugTool(); }, IncludeDebugTools);
			evt.menu.BeamableAppendAction($"Open C# Code", _ => OpenCode());
			evt.menu.BeamableAppendAction("Build", pos => Build());

			evt.menu.AppendSeparator();

			var isFirst = MicroserviceConfiguration.Instance.GetIndex(Name, ServiceType.MicroService) == 0;
			var isLast = MicroserviceConfiguration.Instance.GetIndex(Name, ServiceType.MicroService) < MicroservicesDataModel.Instance.Services.Count - 1;

			evt.menu.BeamableAppendAction($"Move up", pos =>
			{
				MicroserviceConfiguration.Instance.MoveIndex(Name, -1, ServiceType.MicroService);
				OnSortChanged?.Invoke();
			}, !isFirst);
			evt.menu.BeamableAppendAction($"Move down", pos =>
			{
				MicroserviceConfiguration.Instance.MoveIndex(Name, 1, ServiceType.MicroService);
				OnSortChanged?.Invoke();
			}, isLast);
			evt.menu.BeamableAppendAction($"Move to top", pos =>
			{
				MicroserviceConfiguration.Instance.SetIndex(Name, 0, ServiceType.MicroService);
				OnSortChanged?.Invoke();
			}, !isFirst);
			evt.menu.BeamableAppendAction($"Move to bottom", pos =>
			{
				MicroserviceConfiguration.Instance.SetIndex(Name, MicroservicesDataModel.Instance.Services.Count - 1, ServiceType.MicroService);
				OnSortChanged?.Invoke();
			}, isLast);

			evt.menu.AppendSeparator();

			evt.menu.BeamableAppendAction(IncludeDebugTools
											  ? BUILD_DISABLE_DEBUG
											  : BUILD_ENABLE_DEBUG, pos =>
										  {
											  IncludeDebugTools = !IncludeDebugTools;
										  });

			if (!AreLogsAttached)
			{
				evt.menu.BeamableAppendAction($"Reattach Logs", pos => AttachLogs());
			}

			AddArchiveSupport(evt);
		}

		protected void AddArchiveSupport(ContextualMenuPopulateEvent evt)
		{
			evt.menu.AppendSeparator();
			if (Config.Archived)
			{
				evt.menu.AppendAction("Unarchive", _ => Unarchive());
			}
			else
			{
				evt.menu.AppendAction(ARCHIVE_WINDOW_HEADER, _ =>
				{
					var archiveServicePopup = new ArchiveServicePopupVisualElement();
					archiveServicePopup.ShowDeleteOption = !string.IsNullOrEmpty(this.Descriptor.AttributePath);
					BeamablePopupWindow popupWindow = BeamablePopupWindow.ShowUtility($"{ARCHIVE_WINDOW_HEADER} {Descriptor.Name}", archiveServicePopup, null, ARCHIVE_WINDOW_SIZE);
					archiveServicePopup.onClose += () => popupWindow.Close();
					archiveServicePopup.onConfirm += Archive;
				});
			}
		}

		private void RunSnykTests(bool suppressOutput = false)
		{
			var snykCommand = new SnykTestCommand(ServiceDescriptor);
			if (!suppressOutput)
			{
				Debug.Log($"Starting Docker Snyk tests for {ServiceDescriptor.Name}. The test results will appear momentarily.");
			}

			snykCommand.StartAsync().Then(res =>
			{
				if (res.RequiresLogin)
				{
					var onLogin = new Promise<Unit>();
					onLogin.Then(_ => RunSnykTests(true)).Error(_ =>
					{
						Debug.LogError("Cannot run Snyk Tests without being logged into DockerHub");
					});
					OnDockerLoginRequired?.Invoke(onLogin);

				}
				else
				{
					Debug.Log("Docker Snyk tests complete");
					Debug.Log(res.Output);
					var date = DateTime.UtcNow.ToFileTimeUtc().ToString();
					var filePath =
						$"{Directory.GetParent(Application.dataPath)}{Path.DirectorySeparatorChar}{ServiceDescriptor.Name}-snyk-results-{date}.txt";

					File.WriteAllText(filePath, res.Output);
					EditorUtility.OpenWithDefaultApp(filePath);
				}
			}).Error(err =>
			{
				Debug.LogError($"Failed to run Docker Snyk tests for {ServiceDescriptor.Name}. Reason=[{err?.Message}]");
			});
		}
		private void CopyVSCodeDebugTool()
		{

			EditorGUIUtility.systemCopyBuffer =
$@"{{
     ""name"": ""Attach {ServiceDescriptor.Name}"",
     ""type"": ""coreclr"",
     ""request"": ""attach"",
     ""processId"": ""${{command:pickRemoteProcess}}"",
     ""pipeTransport"": {{
        ""pipeProgram"": ""docker"",
        ""pipeArgs"": [ ""exec"", ""-i"", ""{ServiceDescriptor.ContainerName}"" ],
        ""debuggerPath"": ""/vsdbg/vsdbg"",
        ""pipeCwd"": ""${{workspaceRoot}}"",
        ""quoteArgs"": false
     }},
     ""sourceFileMap"": {{
        ""/subsrc"": ""{Path.GetFullPath(ServiceDescriptor.BuildPath)}/""
     }}
  }}";
		}

		protected void OpenRemoteDocs() => OpenOnRemote("docs");
		protected void OpenRemoteLogs() => OpenOnRemote("logs");
		protected void OpenRemoteMetrics() => OpenOnRemote("metrics");
		protected void OpenOnRemote(string relativePath)
		{
			var api = BeamEditorContext.Default;
			var path =
				$"{BeamableEnvironment.PortalUrl}/{api.CurrentCustomer.Alias}/" +
				$"games/{api.ProductionRealm.Pid}/realms/{api.CurrentRealm.Pid}/" +
				$"microservices/{ServiceDescriptor.Name}/{relativePath}?refresh_token={api.Requester.Token.RefreshToken}";
			Application.OpenURL(path);

		}
		private void OpenInCli()
		{
			System.Diagnostics.Process GetProcess(string command)
			{
				var baseProcess = new System.Diagnostics.Process();
#if UNITY_EDITOR_WIN
                baseProcess.StartInfo.FileName = "cmd.exe";
                baseProcess.StartInfo.Arguments = $"/C {command}";
#else
				baseProcess.StartInfo.FileName = "sh";
				baseProcess.StartInfo.Arguments = $"-c \"{command}\"";
#endif
				return baseProcess;
			}
			var baseCommand =
				$"{MicroserviceConfiguration.Instance.DockerCommand} container exec -it {ServiceDescriptor.ContainerName} sh";
#if UNITY_EDITOR_WIN
            var process = GetProcess(baseCommand);
            process.Start();
#else
			var tmpPath = Path.Combine(FileUtil.GetUniqueTempPathInProject(), "..");

			tmpPath = Path.Combine(tmpPath, $"{Descriptor.ContainerName}_cli_shell");
			tmpPath = Path.GetFullPath(tmpPath);
			if (File.Exists(tmpPath))
				FileUtil.DeleteFileOrDirectory(tmpPath);

			using (var file = new StreamWriter(tmpPath, false, Encoding.UTF8))
			{
				file.WriteLine("#!/bin/sh");
				file.WriteLine(baseCommand);
			}
			using (var process = GetProcess($"chmod +x {tmpPath}"))
			{
				process.Start();
			}
			using (var process = GetProcess($"open {tmpPath}"))
			{
				process.Start();
			}
#endif
		}

		public override void Refresh(IDescriptor descriptor)
		{
			// reset the descriptor and statemachines; because they aren't system.serializable durable.
			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			ServiceDescriptor = (MicroserviceDescriptor)descriptor;
			var oldBuilder = ServiceBuilder;
			oldBuilder.Descriptor = descriptor;
			ServiceBuilder = serviceRegistry.GetServiceBuilder(ServiceDescriptor);
			ServiceBuilder.ForwardEventsTo(oldBuilder);
		}

		// Chris took these out because they weren't being used yet, and were throwing warnings on package builds.
		// public event Action OnRenameRequested;
		// public event Action<MicroserviceModel> OnEnriched;
		// private string _name = "";
	}
}
