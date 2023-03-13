using Beamable.Common;
using Beamable.Common.Api.Realms;
using Beamable.Common.Assistant;
using Beamable.Editor.Assistant;
using Beamable.Editor.Modules.Account;
using Beamable.Editor.Toolbox.Components;
using Beamable.Editor.Toolbox.Models;
using Beamable.Editor.UI.Components;
using Beamable.Editor.UI.Model;
using Beamable.Server.Editor;
using Beamable.Server.Editor.DockerCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using static Beamable.Common.Constants.Features.Services;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Microservice.UI.Components
{
	public class MicroserviceContentVisualElement : MicroserviceComponent
	{
		/// <summary>
		/// Action raised on selection change status of any service displayed in MicroserviceContentVisualElement,
		/// **First argument** represents currently selected services amount.
		/// **Second argument** represents services amount.
		/// </summary>
		public event Action<int, int> OnServiceSelectionAmountChange;

		private VisualElement _mainVisualElement;
		private ListView _listView;
		private ScrollView _scrollView;
		private VisualElement _servicesListElement;

		private readonly Dictionary<ServiceModelBase, ServiceBaseVisualElement> _modelToVisual = new Dictionary<ServiceModelBase, ServiceBaseVisualElement>();
		private Dictionary<ServiceType, CreateServiceBaseVisualElement> _servicesCreateElements;
		private MicroserviceActionPrompt _actionPrompt;
		private bool _dockerHubIsRunning;
		private Promise<DockerStatus> _dockerStatusPromise;

		public IEnumerable<ServiceBaseVisualElement> ServiceVisualElements =>
			_servicesListElement.Children().Where(ve => ve is ServiceBaseVisualElement)
				.Cast<ServiceBaseVisualElement>();

		public new class UxmlFactory : UxmlFactory<MicroserviceContentVisualElement, UxmlTraits>
		{
		}

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			UxmlStringAttributeDescription customText = new UxmlStringAttributeDescription
			{ name = "custom-text", defaultValue = "nada" };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				var self = ve as MicroserviceContentVisualElement;
			}
		}

		public MicroserviceContentVisualElement() : base(nameof(MicroserviceContentVisualElement))
		{
		}

		public MicroservicesDataModel Model { get; set; }

		public override void Refresh()
		{
			base.Refresh();
			SetView();

			Context.OnRealmChange -= RefreshFromRealmChange;
			Context.OnUserChange -= RefreshFromUserChange;
			Context.OnRealmChange += RefreshFromRealmChange;
			Context.OnUserChange += RefreshFromUserChange;
		}

		private void RefreshFromRealmChange(RealmView _) => Refresh();
		private void RefreshFromUserChange(EditorUser _) => Refresh();

		private void RefreshView()
		{
			SetView(false);
		}

		private void SetView(bool isInit = true) // we don't want to destroy & recreate whole view every time docker is disabled
		{
			_mainVisualElement = Root.Q<VisualElement>("mainVisualElement");
			_scrollView = Root.Q<ScrollView>();
			_servicesListElement = Root.Q<VisualElement>("listRoot");
			_servicesCreateElements = new Dictionary<ServiceType, CreateServiceBaseVisualElement>();
			_dockerHubIsRunning = !MicroserviceConfiguration.Instance.DockerDesktopCheckInMicroservicesWindow
								  || !DockerCommand.DockerNotRunning;

			if (DockerCommand.DockerNotInstalled || !_dockerHubIsRunning)
			{
				ShowDockerNotInstalledAnnouncement();
				EditorDebouncer.Debounce("Refresh C#MS Window", RefreshView, 1f);
			}
			else if (!isInit)
			{
				Refresh();
				return;
			}

			if (DockerCommand.DockerNotInstalled)
				return;

			if (isInit)
			{
				CreateNewServiceElement(ServiceType.MicroService, new CreateMicroserviceVisualElement());
				CreateNewServiceElement(ServiceType.StorageObject, new CreateStorageObjectVisualElement());
				_modelToVisual.Clear();
				SetupServicesStatus();
			}

			CheckLoginStatus();

			_actionPrompt = _mainVisualElement.Q<MicroserviceActionPrompt>("actionPrompt");
			_actionPrompt.Refresh();
			EditorApplication.delayCall +=
				() =>
				{
					if (_dockerStatusPromise != null && !_dockerStatusPromise.IsCompleted)
						return;

					var command = new GetDockerLocalStatus();
					_dockerStatusPromise = command.StartAsync();
				};
		}

		private void CheckLoginStatus()
		{
			foreach (var kvp in _modelToVisual)
			{
				kvp.Value.ChangeStartButtonState(true, Constants.Tooltips.Microservice.PLAY_MICROSERVICE, Constants.Tooltips.Microservice.PLAY_NOT_LOGGED_IN);
			}
		}

		private void HandleSelectionChanged(bool _)
		{
			int currentlySelectedAmount = Model.AllLocalServices.Count(beamService => beamService.IsSelected);
			int totalAmouont = Model.AllLocalServices.Count;
			OnServiceSelectionAmountChange?.Invoke(currentlySelectedAmount, totalAmouont);
		}

		private MicroserviceVisualElement GetMicroserviceVisualElement(string serviceName)
		{
			var service = Model.GetModel<MicroserviceModel>(serviceName);
			if (service == null)
			{
				return null;
			}

			var serviceElement = new MicroserviceVisualElement { Model = service };
			_modelToVisual[service] = serviceElement;
			service.OnLogsDetached += () => { ServiceLogWindow.ShowService(service); };

			serviceElement.Refresh(_dockerHubIsRunning);
			service.OnSelectionChanged -= HandleSelectionChanged;
			service.OnSelectionChanged += HandleSelectionChanged;

			service.OnSortChanged -= SortMicroservices;
			service.OnSortChanged += SortMicroservices;
			serviceElement.OnServiceStartFailed = MicroserviceStartFailed;
			serviceElement.OnServiceStopFailed = MicroserviceStopFailed;

			return serviceElement;
		}

		private RemoteMicroserviceVisualElement GetRemoteMicroserviceVisualElement(string serviceName)
		{
			var service = Model.GetModel<RemoteMicroserviceModel>(serviceName);

			if (service != null)
			{
				var serviceElement = new RemoteMicroserviceVisualElement { Model = service };

				_modelToVisual[service] = serviceElement;
				serviceElement.Refresh(_dockerHubIsRunning);

				service.OnSortChanged -= SortMicroservices;
				service.OnSortChanged += SortMicroservices;

				return serviceElement;
			}

			return null;
		}

		private StorageObjectVisualElement GetStorageObjectVisualElement(string serviceName)
		{
			var mongoService = Model.GetModel<MongoStorageModel>(serviceName);

			if (mongoService != null)
			{
				var mongoServiceElement = new StorageObjectVisualElement { Model = mongoService };
				_modelToVisual[mongoService] = mongoServiceElement;
				mongoService.OnLogsDetached += () => { ServiceLogWindow.ShowService(mongoService); };

				mongoServiceElement.Refresh(_dockerHubIsRunning);
				mongoService.OnSelectionChanged -= HandleSelectionChanged;
				mongoService.OnSelectionChanged += HandleSelectionChanged;

				mongoService.OnSortChanged -= SortStorages;
				mongoService.OnSortChanged += SortStorages;

				return mongoServiceElement;

			}

			return null;
		}

		private StorageObjectVisualElement GetRemoteStorageObjectVisualElement(string serviceName)
		{
			var mongoService = Model.GetModel<MongoStorageModel>(serviceName);

			if (mongoService != null)
			{
				var mongoServiceElement = new RemoteStorageObjectVisualElement { Model = mongoService };
				_modelToVisual[mongoService] = mongoServiceElement;
				mongoService.OnLogsDetached += () => { ServiceLogWindow.ShowService(mongoService); };

				mongoServiceElement.Refresh(_dockerHubIsRunning);

				mongoService.OnSortChanged -= SortStorages;
				mongoService.OnSortChanged += SortStorages;

				return mongoServiceElement;

			}

			return null;
		}

		private void MicroserviceStartFailed()
		{
			_actionPrompt.SetVisible(PROMPT_STARTED_FAILURE, true, false);
		}

		private void MicroserviceStopFailed()
		{
			_actionPrompt.SetVisible(PROMPT_STOPPED_FAILURE, true, false);
		}

		public void DisplayCreatingNewService(ServiceType serviceType, Action onClose)
		{
			if (_servicesCreateElements.Values.Any(x => x.hierarchy.childCount != 0))
				return;

			_servicesCreateElements[serviceType].Refresh(() => onClose?.Invoke());
			EditorApplication.delayCall += () => _scrollView.verticalScroller.value = 0f;
		}

		public void SetAllMicroserviceSelectedStatus(bool selected)
		{
			foreach (var microservice in Model.AllLocalServices)
			{
				microservice.IsSelected = selected;
			}
		}

		public void BuildAndStartAllMicroservices(ILoadingBar loadingBar)
		{
			var children = new List<LoadingBarUpdater>();
			var dependencyStorages = new List<string>();

			foreach (var microservice in Model.Services)
			{
				if (!microservice.IsSelected)
					continue;

				if (microservice.Dependencies.Count > 0)
				{
					foreach (MongoStorageModel dependencyService in microservice.Dependencies)
					{
						dependencyStorages.Add(dependencyService.Name);

						void OnDependencyRunFinished(bool isFinished)
						{
							if (isFinished)
							{
								if (microservice.IsRunning)
									microservice.BuildAndRestart();
								else
									microservice.BuildAndStart();
							}

							dependencyService.Builder.OnStartingFinished -= OnDependencyRunFinished;
						};

						dependencyService.Builder.OnStartingFinished += OnDependencyRunFinished;
						dependencyService.Start();
					}

				}
				else
				{
					if (microservice.IsRunning)
						microservice.BuildAndRestart();
					else
						microservice.BuildAndStart();
				}

				var element = _modelToVisual[microservice];
				var subLoader = element.Q<LoadingBarElement>();
				children.Add(subLoader.Updater);
			}

			foreach (var storage in Model.Storages)
			{
				if (!storage.IsSelected)
					continue;

				if (!storage.IsRunning && !dependencyStorages.Contains(storage.Name))
					storage.Start();
			}

			var _ = new GroupLoadingBarUpdater("Starting Microservices", loadingBar, false, children.ToArray());
		}

		public void SortServices(ServiceType serviceType)
		{
			var config = MicroserviceConfiguration.Instance;

			int Comparer(VisualElement a, VisualElement b)
			{
				if (a is CreateServiceBaseVisualElement) return -1;
				if (b is CreateServiceBaseVisualElement) return 1;

				// we want to sort Services only in their categories

				switch (serviceType)
				{
					case ServiceType.MicroService:
						if (b is StorageObjectVisualElement)
							return -1;
						break;
					case ServiceType.StorageObject:
						if (b is MicroserviceVisualElement)
							return 1;
						break;
					default:
						break;
				}

				return config.OrderComparer(a.name, b.name, serviceType);
			}

			_servicesListElement.Sort(Comparer);
		}

		public void SortMicroservices()
		{
			SortServices(ServiceType.MicroService);
		}

		public void SortStorages()
		{
			SortServices(ServiceType.StorageObject);
		}

		private bool ShouldDisplayService(ServiceType type, bool isArchived)
		{
			switch (Model.Filter)
			{
				case ServicesDisplayFilter.AllTypes:
					return !isArchived;
				case ServicesDisplayFilter.Microservices:
					return !isArchived && type == ServiceType.MicroService;
				case ServicesDisplayFilter.Storages:
					return !isArchived && type == ServiceType.StorageObject;
				case ServicesDisplayFilter.Archived:
					return isArchived;
				default:
					return false;
			}
		}

		private void ShowDockerNotInstalledAnnouncement()
		{
			var dockerAnnouncement = new DockerAnnouncementModel();
			dockerAnnouncement.IsDockerInstalled = !DockerCommand.DockerNotInstalled;
			Root.Q<VisualElement>("announcementList").Clear();

			if (DockerCommand.DockerNotInstalled)
			{
				dockerAnnouncement.OnInstall = async () =>
				{
					var window = await BeamableAssistantWindow.Init();
					window.ExpandHint(new BeamHintHeader(BeamHintType.Validation,
														 BeamHintDomains.BEAM_CSHARP_MICROSERVICES_DOCKER,
														 BeamHintIds.ID_INSTALL_DOCKER_PROCESS));

				};
			}
			else
			{
				dockerAnnouncement.OnInstall = async () =>
				{
					var window = await BeamableAssistantWindow.Init();
					window.ExpandHint(new BeamHintHeader(BeamHintType.Validation,
														 BeamHintDomains.BEAM_CSHARP_MICROSERVICES_DOCKER,
														 BeamHintIds.ID_DOCKER_PROCESS_NOT_RUNNING));
				};

			}
			var element = new DockerAnnouncementVisualElement() { DockerAnnouncementModel = dockerAnnouncement };
			Root.Q<VisualElement>("announcementList").Add(element);
			element.Refresh();
		}

		private void CreateNewServiceElement(ServiceType serviceType, CreateServiceBaseVisualElement service)
		{
			service.OnCreateServiceClicked += () => Root.SetEnabled(false);
			_servicesCreateElements.Add(serviceType, service);
			_servicesListElement.Add(service);
		}

		private void SetupServicesStatus()
		{
			var hasStorageDependency = false;
			foreach (var serviceStatus in Model.GetAllServicesStatus())
			{
				if (serviceStatus.Value == ServiceAvailability.Unknown)
					continue;

				var serviceType = Model.GetModelServiceType(serviceStatus.Key);
				var isArchived = serviceType == ServiceType.MicroService ?
					MicroserviceConfiguration.Instance.GetEntry(serviceStatus.Key).Archived :
					MicroserviceConfiguration.Instance.GetStorageEntry(serviceStatus.Key).Archived;
				if (!ShouldDisplayService(serviceType, isArchived))
					continue;

				ServiceBaseVisualElement serviceElement = null;

				switch (serviceType)
				{
					case ServiceType.MicroService:

						var val = false;
						if (serviceStatus.Value != ServiceAvailability.RemoteOnly)
							serviceElement = GetMicroserviceVisualElement(serviceStatus.Key);
						else
							serviceElement = GetRemoteMicroserviceVisualElement(serviceStatus.Key);

						hasStorageDependency |= val;
						break;
					case ServiceType.StorageObject:
						if (serviceStatus.Value != ServiceAvailability.RemoteOnly)
							serviceElement = GetStorageObjectVisualElement(serviceStatus.Key);
						else
							serviceElement = GetRemoteStorageObjectVisualElement(serviceStatus.Key);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				if (serviceElement != null)
				{
					serviceElement.SetEnabled(_dockerHubIsRunning);
					_servicesListElement.Add(serviceElement);
				}
			}
		}

		public void StopAllServices(bool showDialog = false, string dialogTitle = "", string dialogMessage = "", string dialogConfirm = "")
		{
			var isAnyServiceStopped = false;
			foreach (var service in _modelToVisual.Keys)
				if (service.IsRunning)
				{
					isAnyServiceStopped = true;
					service.Stop();
				}

			if (!showDialog || !isAnyServiceStopped)
				return;

			EditorUtility.DisplayDialog(dialogTitle, dialogMessage, dialogConfirm);
		}
	}
}
