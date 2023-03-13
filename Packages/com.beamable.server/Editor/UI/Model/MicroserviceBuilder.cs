using Beamable.Common;
using Beamable.Server;
using Beamable.Server.Editor;
using Beamable.Server.Editor.DockerCommands;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.UI.Model
{
	[Serializable]
	public class MicroserviceBuilder : ServiceBuilderBase
	{
		public bool IsBuilding
		{
			get => _isBuilding;
			private set
			{
				if (value == _isBuilding) return;
				_isBuilding = value;
				// XXX: If OnIsBuildingChanged is mutated at before delayCall triggers, non-deterministic behaviour could occur
				EditorApplication.delayCall += () => OnIsBuildingChanged?.Invoke(value);
			}
		}
		[SerializeField]
		private bool _isBuilding;

		public string LastBuildImageId
		{
			get => _lastImageId;
			private set
			{
				if (value == _lastImageId) return;
				_lastImageId = value;
				EditorApplication.delayCall += () => OnLastImageIdChanged?.Invoke(value);
			}
		}
		[SerializeField]
		private string _lastImageId;

		public bool HasImage => IsRunning || LastBuildImageId?.Length > 0;
		public bool HasBuildDirectory => Directory.Exists(Path.GetFullPath(_buildPath));

		public Action<bool> OnIsBuildingChanged;
		public Action<string> OnLastImageIdChanged;

		[SerializeField]
		private string _buildPath;

		private bool _BuildShouldRunCustomInitializationHooks;

		private Promise<string> _secret;

		public MicroserviceBuilder(bool runCustomHooks = true)
		{
			_BuildShouldRunCustomInitializationHooks = runCustomHooks;
		}

		public void ForwardEventsTo(MicroserviceBuilder oldBuilder)
		{
			if (oldBuilder == null) return;
			OnIsRunningChanged += oldBuilder.OnIsRunningChanged;
			OnIsBuildingChanged += oldBuilder.OnIsBuildingChanged;
			OnLastImageIdChanged += oldBuilder.OnLastImageIdChanged;
		}
		public override async void Init(IDescriptor descriptor)
		{
			base.Init(descriptor);
			_buildPath = ((MicroserviceDescriptor)descriptor).BuildPath;
			await TryToGetLastImageId();
		}


		protected override async Task<RunImageCommand> PrepareRunCommand()
		{
			var descriptor = (MicroserviceDescriptor)Descriptor;
			var beamable = BeamEditorContext.Default;
			await beamable.InitializePromise;
			var secret =
				beamable.RealmSecret.GetOrThrow(
					() => new Exception("Cannot run a microservice without a realm secret."));
			var cid = beamable.CurrentCustomer.Cid;
			var pid = beamable.CurrentRealm.Pid;
			// check to see if the storage descriptor is running.
			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			var isWatch = MicroserviceConfiguration.Instance.EnableHotModuleReload;
			var connectionStrings = await serviceRegistry.GetConnectionStringEnvironmentVariables((MicroserviceDescriptor)Descriptor);
			BeamEditorContext.Default.ServiceScope.GetService<MicroservicesDataModel>().AddLogMessage((MicroserviceDescriptor)Descriptor, new LogMessage
			{
				Message = $"Finished preparing {descriptor.Name}. Starting now...",
				Timestamp = LogMessage.GetTimeDisplay(DateTime.Now),
				Level = LogLevel.INFO
			});
			return new RunServiceCommand(descriptor, cid, pid, secret, connectionStrings, isWatch, _BuildShouldRunCustomInitializationHooks);
		}

		public async Task<bool> TryToBuild(bool includeDebuggingTools)
		{
			await TryToStop();
			if (IsBuilding) return true;

			IsBuilding = true;
			var serviceDescriptor = Descriptor as MicroserviceDescriptor;
			var isWatch = MicroserviceConfiguration.Instance.EnableHotModuleReload;
			if (isWatch)
			{
				// before we can build this container, we need to remove any contains that may have bind mounts open to the service's filesystems.
				//  because if we don't, its possible Docker might prevent the directory cleanup operations and flunk the build.
				await TryToStop(); // for it to stop.
				await BeamServicesCodeWatcher.StopClientSourceCodeGenerator(serviceDescriptor);
			}

			var availableArchitectures = await new GetBuildOutputArchitectureCommand().StartAsync();
			var codeWatcher = BeamServicesCodeWatcher.Default;
			bool ensureMongoDependencies = MicroserviceConfiguration.Instance.EnsureMongoAssemblyDependencies;

			if (ensureMongoDependencies && codeWatcher.IsServiceDependedOnStorage(serviceDescriptor))
			{
				codeWatcher.AddMissingMongoDependencies(serviceDescriptor);
			}

			var command = new BuildImageCommand(serviceDescriptor, availableArchitectures, includeDebuggingTools, isWatch);
			command.OnStandardOut += message => MicroserviceLogHelper.HandleBuildCommandOutput(this, message);
			command.OnStandardErr += message => MicroserviceLogHelper.HandleBuildCommandOutput(this, message);
			try
			{
				await command.StartAsync();
				await TryToGetLastImageId();

				// Update the config with the code handle identifying the version of the code this is building with (see BeamServicesCodeWatcher).
				// Check for any local code changes to C#MS or it's dependent Storage/Common assemblies and update the hint state.

				codeWatcher.UpdateBuiltImageCodeHandles(Descriptor.Name);
				codeWatcher.CheckForLocalChangesNotYetDeployed();

				return true;
			}
			catch (Exception e)
			{
				EditorApplication.delayCall += () =>
				{
					MicroservicesDataModel.Instance.AddLogMessage(
						Descriptor,
						new LogMessage
						{
							Level = LogLevel.ERROR,
							Message = e.Message,
							ParameterText = e.StackTrace,
							Timestamp = LogMessage.GetTimeDisplay(DateTime.Now)
						});
				};
				MicroserviceLogHelper.HandleBuildCommandOutput(this, "Error");
			}
			finally
			{
				IsBuilding = false;
				BeamServicesCodeWatcher.GenerateClientSourceCode(serviceDescriptor);
			}

			return false;
		}
		public async Task TryToGetLastImageId()
		{
			var getChecksum = new GetImageDetailsCommand(Descriptor);
			try
			{
				var details = await getChecksum.StartAsync();
				LastBuildImageId = details.imageId;
			}
			catch (DockerNotInstalledException)
			{
				// nothing to do here, because we don't know what the last image id was/is. 
			}
			catch (Exception e)
			{
				System.Console.WriteLine(e);
				throw;
			}
		}
		public async Task TryToBuildAndRestart(bool includeDebuggingTools)
		{
			bool isBuilt = await TryToBuild(includeDebuggingTools);

			if (isBuilt)
				await TryToRestart();
			else
				await TryToStop();
		}
		public async Task TryToBuildAndStart(bool includeDebuggingTools)
		{
			bool isBuilded = await TryToBuild(includeDebuggingTools);

			if (isBuilded)
				await TryToStart();
		}
	}
}
