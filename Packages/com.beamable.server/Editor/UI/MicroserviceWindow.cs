#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using Beamable.Common;
using Beamable.Common.Api.Realms;
using Beamable.Editor.Microservice.UI.Components;
using Beamable.Editor.Modules.Account;
using Beamable.Editor.UI;
using Beamable.Editor.UI.Components;
using Beamable.Editor.UI.Model;
using Beamable.Server.Editor;
using Beamable.Server.Editor.DockerCommands;
using Beamable.Server.Editor.UI.Components;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Beamable.Common.Constants;
using static Beamable.Common.Constants.Features.Services.Dialogs;

namespace Beamable.Editor.Microservice.UI
{
	public class MicroserviceWindow : BeamEditorWindow<MicroserviceWindow>
	{
		static MicroserviceWindow()
		{
			var inspector = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
			WindowDefaultConfig = new BeamEditorWindowInitConfig()
			{
				Title = MenuItems.Windows.Names.MICROSERVICES_MANAGER,
				FocusOnShow = false,
				DockPreferenceTypeName = inspector.AssemblyQualifiedName,
				RequireLoggedUser = true,
			};

			CustomDelayClause = () => !MicroserviceEditor.IsInitialized && BeamEditorContext.Default.InitializePromise.IsCompleted;
		}

		[MenuItem(
			MenuItems.Windows.Paths.MENU_ITEM_PATH_WINDOW_BEAMABLE + "/" +
			Commons.OPEN + " " +
			MenuItems.Windows.Names.MICROSERVICES_MANAGER,
			priority = MenuItems.Windows.Orders.MENU_ITEM_PATH_WINDOW_PRIORITY_2
		)]
		public static async void Init() => _ = await GetFullyInitializedWindow();

		private VisualElement _windowRoot;
		private ActionBarVisualElement _actionBarVisualElement;
		private MicroserviceBreadcrumbsVisualElement _microserviceBreadcrumbsVisualElement;
		private MicroserviceContentVisualElement _microserviceContentVisualElement;
		private LoadingBarElement _loadingBar;

		public MicroservicesDataModel Model => ActiveContext.ServiceScope.GetService<MicroservicesDataModel>();

		private Promise<bool> checkDockerPromise;

		public void RefreshWindowContent()
		{
			checkDockerPromise = PerformCheck().Then(_ =>
			{
				Model.RefreshState().Then(__ =>
				{
					_microserviceBreadcrumbsVisualElement?.Refresh();
					_actionBarVisualElement?.Refresh();
					_microserviceContentVisualElement?.Refresh();
				});
			});
		}

		async Promise<bool> PerformCheck()
		{
			var result = await new CheckDockerCommand().StartAsync();

			if (MicroserviceConfiguration.Instance.DockerDesktopCheckInMicroservicesWindow)
			{
				var midResult = await DockerCommand.CheckDockerAppRunning();
				result |= midResult;
			}

			return result;
		}

		protected override async void Build()
		{
			minSize = new Vector2(425, 200);

			checkDockerPromise = PerformCheck();
			await checkDockerPromise;

			void OnUserChange(EditorUser _) => BuildWithContext();
			void OnRealmChange(RealmView _) => _microserviceContentVisualElement?.StopAllServices(true, RealmSwitchDialog.TITLE, RealmSwitchDialog.MESSAGE, RealmSwitchDialog.OK);

			ActiveContext.OnUserChange -= OnUserChange;
			ActiveContext.OnUserChange += OnUserChange;

			ActiveContext.OnRealmChange -= OnRealmChange;
			ActiveContext.OnRealmChange += OnRealmChange;

			await Model.FinishedLoading;
			SetForContent();


			ActiveContext.OnServiceArchived -= ServiceArchived;
			ActiveContext.OnServiceArchived += ServiceArchived;

			ActiveContext.OnServiceUnarchived -= ServiceArchived;
			ActiveContext.OnServiceUnarchived += ServiceArchived;

			ActiveContext.OnServiceDeleteProceed -= OnServiceDeleteProceed;
			ActiveContext.OnServiceDeleteProceed += OnServiceDeleteProceed;
		}

		private void OnDisable()
		{
			if (ActiveContext != null)
			{
				ActiveContext.OnServiceDeleteProceed -= OnServiceDeleteProceed;
				ActiveContext.OnServiceArchived -= ServiceArchived;
				ActiveContext.OnServiceUnarchived -= ServiceArchived;
			}
		}

		private void SetForContent()
		{
			var root = this.GetRootVisualContainer();
			root.Clear();

			var uiAsset =
				AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{Directories.BEAMABLE_SERVER_PACKAGE_EDITOR_UI}/MicroserviceWindow.uxml");
			_windowRoot = uiAsset.CloneTree();
			_windowRoot.AddStyleSheet($"{Directories.BEAMABLE_SERVER_PACKAGE_EDITOR_UI}/MicroserviceWindow.uss");
			_windowRoot.name = nameof(_windowRoot);
			_windowRoot.TryAddScrollViewAsMainElement();

			root.Add(_windowRoot);

			_actionBarVisualElement = root.Q<ActionBarVisualElement>("actionBarVisualElement");
			_actionBarVisualElement.Refresh();
			_actionBarVisualElement.UpdateButtonsState(Model.AllLocalServices.Count(x => !x.IsArchived));

			_microserviceBreadcrumbsVisualElement = root.Q<MicroserviceBreadcrumbsVisualElement>("microserviceBreadcrumbsVisualElement");
			_microserviceBreadcrumbsVisualElement.Refresh();

			_loadingBar = root.Q<LoadingBarElement>("loadingBar");
			_loadingBar.Hidden = true;
			_loadingBar.Refresh();
			var btn = _loadingBar.Q<Button>("button");
			btn.clickable.clicked -= HideAllLoadingBars;
			btn.clickable.clicked += HideAllLoadingBars;

			_microserviceContentVisualElement = root.Q<MicroserviceContentVisualElement>("microserviceContentVisualElement");
			_microserviceContentVisualElement.Model = Model;

			_microserviceContentVisualElement.Refresh();

			if (Model != null)
			{
				Model.OnServerManifestUpdated += (manifest) =>
				{
					_microserviceContentVisualElement?.Refresh();
				};
			}

			_microserviceBreadcrumbsVisualElement.OnNewServicesDisplayFilterSelected += HandleDisplayFilterSelected;

			_actionBarVisualElement.OnInfoButtonClicked += () =>
			{
				Application.OpenURL(URLs.Documentations.URL_DOC_MICROSERVICES);
			};

			_actionBarVisualElement.OnCreateNewClicked += serviceType => _microserviceContentVisualElement.DisplayCreatingNewService(serviceType, _actionBarVisualElement.Refresh);

			_actionBarVisualElement.OnPublishClicked += () => PublishWindow.ShowPublishWindow(this, ActiveContext);

			_actionBarVisualElement.OnRefreshButtonClicked += RefreshWindowContent;

			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			if (serviceRegistry != null)
			{
				serviceRegistry.OnDeploySuccess -= HandleDeploySuccess;
				serviceRegistry.OnDeploySuccess += HandleDeploySuccess;
				serviceRegistry.OnDeployFailed -= HandleDeployFailed;
				serviceRegistry.OnDeployFailed += HandleDeployFailed;
			}
		}

		private void HandleDisplayFilterSelected(ServicesDisplayFilter filter)
		{
			Model.Filter = filter;
			RefreshWindowContent();
		}

		private void HideAllLoadingBars()
		{
			foreach (var microserviceVisualElement in _windowRoot.Q<MicroserviceContentVisualElement>().ServiceVisualElements)
			{
				microserviceVisualElement.Q<LoadingBarElement>().Hidden = true;
			}
		}

		private void HandleDeploySuccess(ManifestModel model, int totalSteps) => RefreshWindowContent();

		private void HandleDeployFailed(ManifestModel model, string reason)
		{
			Debug.LogError(reason);
			_microserviceContentVisualElement?.Refresh();
		}

		private void ServiceArchived()
		{
			_microserviceBreadcrumbsVisualElement.RefreshFiltering();
			_actionBarVisualElement.UpdateButtonsState(Model.AllLocalServices.Count(x => !x.IsArchived));
		}

		private void OnServiceDeleteProceed()
		{
			var root = this.GetRootVisualContainer();
			root?.SetEnabled(false);
		}
	}
}
