using Beamable.Common;
using Beamable.Editor.UI.Model;
using Beamable.Server.Editor.ManagerClient;
using Beamable.Server.Editor.UI.Components.DockerLoginWindow;
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
	public class RemoteMicroserviceVisualElement : MicroserviceVisualElement
	{
		public new class UxmlFactory : UxmlFactory<RemoteMicroserviceVisualElement, UxmlTraits>
		{ }

		protected override string ScriptName => nameof(MicroserviceVisualElement);

		private RemoteMicroserviceModel _microserviceModel;

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (_microserviceModel == null) return;

			_microserviceModel.OnDockerLoginRequired -= LoginToDocker;
			_microserviceModel.ServiceBuilder.OnLastImageIdChanged -= HandleLastImageIdChanged;
		}

		protected override void UpdateVisualElements()
		{
			base.UpdateVisualElements();

			Root.Q<Button>("startBtn").RemoveFromHierarchy();
			Root.Q<VisualElement>("logContainer").RemoveFromHierarchy();
			Root.Q("collapseContainer")?.RemoveFromHierarchy();
			Root.Q("statusSeparator")?.RemoveFromHierarchy();
			Root.Q<VisualElement>("openDocsBtn")?.RemoveFromHierarchy();
			Root.Q<VisualElement>("openScriptBtn")?.RemoveFromHierarchy();
			Root.Q<MicroserviceVisualElementSeparator>("separator")?.RemoveFromHierarchy();
			Root.Q("foldContainer").visible = false;
			Root.Q<VisualElement>("mainVisualElement").style.SetHeight(DEFAULT_HEADER_HEIGHT);

			var manipulator = new ContextualMenuManipulator(Model.PopulateMoreDropdown);
			manipulator.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
			_moreBtn.AddManipulator(manipulator);
			_moreBtn.tooltip = Tooltips.Microservice.MORE;

			_microserviceModel.OnDockerLoginRequired -= LoginToDocker;
			_microserviceModel.OnDockerLoginRequired += LoginToDocker;

			UpdateLocalStatus();
			UpdateRemoteStatusIcon("remoteEnabled");
			UpdateModel();
		}

		protected override void QueryVisualElements()
		{
			base.QueryVisualElements();

			_microserviceModel = (RemoteMicroserviceModel)Model;
		}

		private void LoginToDocker(Promise<Unit> onLogin)
		{
			DockerLoginVisualElement.ShowUtility().Then(onLogin.CompleteSuccess).Error(onLogin.CompleteError);
		}

		private void HandleLastImageIdChanged(string newId)
		{
			UpdateLocalStatus();
		}

		private void OnServiceReferenceChanged(ServiceReference serviceReference)
		{
			UpdateRemoteStatusIcon();
		}

		protected override void UpdateLocalStatus()
		{
			base.UpdateLocalStatus();
			UpdateLocalStatusIcon(false, false);
		}
	}
}
