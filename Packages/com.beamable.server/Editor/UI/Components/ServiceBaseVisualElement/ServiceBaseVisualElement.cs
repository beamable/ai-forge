using Beamable.Common;
using Beamable.Editor.UI.Components;
using Beamable.Editor.UI.Model;
using Beamable.Server.Editor;
using System;
using System.Threading.Tasks;
using UnityEngine;
using static Beamable.Common.Constants.Features.Services;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleSheets;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

using static Beamable.Common.Constants;

namespace Beamable.Editor.Microservice.UI.Components
{
	public abstract class ServiceBaseVisualElement : MicroserviceComponent
	{
		protected ServiceBaseVisualElement() : base(nameof(ServiceBaseVisualElement))
		{
		}

		public ServiceModelBase Model { get; set; }
		protected abstract string ScriptName { get; }
		protected abstract bool IsRemoteEnabled { get; }

		private const float MIN_HEIGHT = 240.0f;
		private const float MAX_HEIGHT = 500.0f;
		private const float DETACHED_HEIGHT = 30.0f;
		protected const float DEFAULT_HEADER_HEIGHT = 30.0f;

		protected LoadingBarElement _loadingBar;
		protected VisualElement _moreBtn;
		protected Button _startButton;
		protected MicroserviceVisualElementSeparator _separator;
		private VisualElement _logContainerElement;
		private LogVisualElement _logElement;
		private VisualElement _header;
		private VisualElement _rootVisualElement;
		private VisualElement _mainParent;
		private VisualElement _serviceCard;
		private Button _foldButton;
		private VisualElement _foldIcon;
		protected Image _serviceIcon;
		private Label _serviceName;
		private VisualElement _openDocsBtn;
		private VisualElement _openScriptBtn;

		private bool _isDockerRunning;


		public Action OnServiceStartFailed { get; set; }
		public Action OnServiceStopFailed { get; set; }

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (Model == null) return;

			Model.OnStart -= SetupProgressBarForStart;
			Model.OnStop -= SetupProgressBarForStop;
			Model.OnLogsAttachmentChanged -= CreateLogSection;

			if (Model.Builder == null) return;

			Model.Builder.OnIsRunningChanged -= HandleIsRunningChanged;
		}
		public override void Refresh()
		{
			base.Refresh();
			name = Model.Name;
			QueryVisualElements();
			InjectStyleSheets();
			UpdateVisualElements();
		}

		public void Refresh(bool isDockerRunning)
		{
			_isDockerRunning = isDockerRunning;
			Refresh();
		}

		protected virtual void QueryVisualElements()
		{
			_rootVisualElement = Root.Q<VisualElement>("mainVisualElement");
			Root.Q("microserviceNewTitle")?.RemoveFromHierarchy();
			_moreBtn = Root.Q<VisualElement>("moreBtn");
			_startButton = Root.Q<Button>("startBtn");
			_logContainerElement = Root.Q<VisualElement>("logContainer");
			_header = Root.Q("logHeader");
			_separator = Root.Q<MicroserviceVisualElementSeparator>("separator");
			_serviceCard = Root.Q("serviceCard");
			_loadingBar = new LoadingBarElement();
			_serviceCard.Add(_loadingBar);
			_foldButton = Root.Q<Button>("foldButton");
			_foldIcon = Root.Q("foldIcon");
			_serviceIcon = Root.Q<Image>("serviceIcon");
			_serviceName = Root.Q<Label>("serviceName");
			_openDocsBtn = Root.Q<VisualElement>("openDocsBtn");
			_openScriptBtn = Root.Q<VisualElement>("openScriptBtn");
			_mainParent = _rootVisualElement.parent.parent;
		}
		private void InjectStyleSheets()
		{
			if (string.IsNullOrWhiteSpace(ScriptName)) return;
			_rootVisualElement.AddStyleSheet($"{COMPONENTS_PATH}/{ScriptName}/{ScriptName}.uss");
		}
		protected virtual void UpdateVisualElements()
		{
			_loadingBar.Hidden = true;
			_loadingBar.Refresh();
			_loadingBar.PlaceBehind(Root.Q("SubTitle"));

			Model.OnStart -= SetupProgressBarForStart;
			Model.OnStart += SetupProgressBarForStart;
			Model.OnStop -= SetupProgressBarForStop;
			Model.OnStop += SetupProgressBarForStop;

			var manipulator = new ContextualMenuManipulator(Model.PopulateMoreDropdown);
			manipulator.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
			_moreBtn.tooltip = Tooltips.Microservice.MORE;
			_moreBtn.AddManipulator(manipulator);

			_openScriptBtn.AddManipulator(new Clickable(Model.OpenCode));
			_openScriptBtn.tooltip = "Open C# Code";

			_openDocsBtn.AddManipulator(new Clickable(Model.OpenDocs));
			_openDocsBtn.SetEnabled(Model.IsRunning);
			_openDocsBtn.tooltip = "View Documentation";

			_serviceName.text = Model.Name;
			_serviceName.tooltip = Model.Name;

			if (_serviceIcon != null)
			{
				_serviceIcon.tooltip = Model.Descriptor.ServiceType == ServiceType.MicroService ?
					Tooltips.Microservice.MICROSERVICE : Tooltips.Microservice.STORAGE_OBJECT;
			}

			Model.OnLogsAttachmentChanged -= CreateLogSection;
			Model.OnLogsAttachmentChanged += CreateLogSection;

			Model.Builder.OnIsRunningChanged -= HandleIsRunningChanged;
			Model.Builder.OnIsRunningChanged += HandleIsRunningChanged;

			Model.Builder.OnBuildingProgress -= HandleStartingProgress;
			Model.Builder.OnBuildingProgress += HandleStartingProgress;

			_separator.Setup(OnDrag);
			_separator.Refresh();

			_foldButton.clickable.clicked += HandleCollapseButton;
			_mainParent.AddToClassList("folded");
			_rootVisualElement.AddToClassList("folded");

			CreateLogSection(Model.AreLogsAttached);
			UpdateLocalStatus();
			UpdateRemoteStatusIcon();
			ChangeCollapseState();
			UpdateModel();
		}

		private void HandleStartingProgress(int _, int __) => _header.ClearClassList();

		protected void UpdateRemoteStatusIcon(string customStatusClassName = "")
		{
			_serviceIcon.ClearClassList();

			var statusClassName = string.IsNullOrWhiteSpace(customStatusClassName)
				? IsRemoteEnabled
					? $"{Model.ServiceType}_remoteEnabled"
					: $"{Model.ServiceType}_remoteDisabled"
				: $"{Model.ServiceType}_{customStatusClassName}";

			_serviceIcon.tooltip = IsRemoteEnabled ? Tooltips.Microservice.ICON_REMOTE_RUNNING : Tooltips.Microservice.ICON_REMOTE_DISABLE;
			_serviceIcon.AddToClassList(statusClassName);
		}
		protected virtual void UpdateButtons()
		{
		}
		protected virtual void UpdateLocalStatus()
		{
			_header.EnableInClassList("running", Model.IsRunning);
			_openDocsBtn.SetEnabled(Model.IsRunning);
			UpdateButtons();
		}
		protected async void UpdateModel()
		{
			if (!_isDockerRunning)
				return;

			await Model.Builder.CheckIfIsRunning();
			UpdateLocalStatus();
		}

		protected void UpdateLocalStatusIcon(bool isRunning, bool isBuilding)
		{
			_serviceIcon.ClearClassList();
			// _header.EnableInClassList("building", isBuilding);
			_startButton.EnableInClassList("building", isBuilding);
			_startButton.EnableInClassList("running", isRunning);
			UpdateRemoteStatusIcon();
		}
		private void OnDrag(float value)
		{
			if (!Model.AreLogsAttached)
			{
				return;
			}

			var layoutHeight = _rootVisualElement.layout.height;
			SetHeight(layoutHeight + value);
		}

		private void SetHeight(float newHeight)
		{
			Model.VisualElementHeight = Mathf.Clamp(newHeight, MIN_HEIGHT, MAX_HEIGHT);
#if UNITY_2019_1_OR_NEWER
            _rootVisualElement.style.height = new StyleLength(Model.VisualElementHeight);
#elif UNITY_2018
            _rootVisualElement.style.height = StyleValue<float>.Create(Model.VisualElementHeight);
#endif
		}

		private void HandleStopButtonClicked()
		{
			if (Model.IsRunning)
			{
				Model.Stop();
			}
			else
			{
				Model.Start();
			}
		}
		private void HandleIsRunningChanged(bool isRunning)
		{
			UpdateLocalStatus();
		}
		protected void HandleProgressFinished(bool gotError) => _header.EnableInClassList("failed", gotError);

		private void CreateLogSection(bool areLogsAttached)
		{
			_logElement?.Destroy();
			_logContainerElement.Clear();
			if (areLogsAttached)
			{
				CreateLogElement();
				SetHeight(Model.VisualElementHeight);
			}
		}
		private void CreateLogElement()
		{
			_logElement = new LogVisualElement { Model = Model };
			_logElement.AddToClassList("logElement");
			_logElement.OnDetachLogs += OnLogsDetached;
			_logContainerElement.Add(_logElement);
			_logElement.Refresh();
		}
		private void OnLogsDetached()
		{
			_logElement.OnDetachLogs -= OnLogsDetached;
			Model.VisualElementHeight = _rootVisualElement.layout.height;

#if UNITY_2019_1_OR_NEWER
            _rootVisualElement.style.height = new StyleLength(DETACHED_HEIGHT);
#elif UNITY_2018
            _rootVisualElement.style.height =
                StyleValue<float>.Create(DETACHED_HEIGHT);
#endif
		}
		protected virtual void SetupProgressBarForStart(Task task)
		{
			// We have two ways. Either store reference or return instance as event parameter
			new RunImageLogParser(_loadingBar, Model);
		}
		private void OnStartFailed()
		{
			OnServiceStartFailed?.Invoke();
		}
		protected virtual void SetupProgressBarForStop(Task task)
		{
			var parser = new StopImageLogParser(_loadingBar, Model) { OnFailure = OnStopFailed };
			task?.ContinueWith(_ =>
			{
				_loadingBar.Hidden = true;
				parser.Kill();
			});
		}
		private void OnStopFailed()
		{
			OnServiceStopFailed?.Invoke();
		}
		private void HandleCollapseButton()
		{
			Model.IsCollapsed = !Model.IsCollapsed;
			ChangeCollapseState();
		}
		private void ChangeCollapseState()
		{
			_logContainerElement.EnableInClassList("--positionHidden", Model.IsCollapsed);
			_separator.EnableInClassList("--positionHidden", Model.IsCollapsed);
			_foldIcon.EnableInClassList("foldIcon", Model.IsCollapsed);
			_foldIcon.EnableInClassList("unfoldIcon", !Model.IsCollapsed);
			_rootVisualElement.EnableInClassList("folded", Model.IsCollapsed);
			_mainParent.EnableInClassList("folded", Model.IsCollapsed);
		}

		public virtual void ChangeStartButtonState(bool isOn,
										   string enabledTooltip = null,
										   string disabledTooltip = null)
		{
			var isAuthorized = Context.IsAuthenticated && Context.RealmSecret.HasValue;
			if (!isAuthorized)
			{
				_startButton.tooltip = Tooltips.Microservice.PLAY_NOT_LOGGED_IN;
				_startButton.SetEnabled(false);

			}
			else if (!isOn)
			{
				_startButton.tooltip = disabledTooltip ?? Tooltips.Microservice.PLAY_DISABLED_GENERAL;
				_startButton.SetEnabled(false);
			}
			else
			{
				_startButton.tooltip = enabledTooltip ?? (
						Model.IsRunning
							? Tooltips.Microservice.STOP_SERVICE_GENERAL
							: Tooltips.Microservice.PLAY_MICROSERVICE);
				_startButton.SetEnabled(true);
			}
		}
	}
}
