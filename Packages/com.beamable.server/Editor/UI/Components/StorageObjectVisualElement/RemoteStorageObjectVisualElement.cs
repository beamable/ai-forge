using Beamable.Editor.UI.Model;
using static Beamable.Common.Constants.Features.Services;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleSheets;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif


namespace Beamable.Editor.Microservice.UI.Components
{
	public class RemoteStorageObjectVisualElement : StorageObjectVisualElement
	{
		public new class UxmlFactory : UxmlFactory<RemoteStorageObjectVisualElement, UxmlTraits>
		{
		}

		protected override string ScriptName => nameof(StorageObjectVisualElement);

		private RemoteMongoStorageModel _mongoStorageModel;

		protected override void UpdateVisualElements()
		{
			base.UpdateVisualElements();

			Root.Q<VisualElement>("logContainer").RemoveFromHierarchy();
			Root.Q("collapseContainer")?.RemoveFromHierarchy();
			Root.Q("startBtn")?.RemoveFromHierarchy();
			Root.Q<VisualElement>("openDocsBtn")?.RemoveFromHierarchy();
			Root.Q<VisualElement>("openScriptBtn")?.RemoveFromHierarchy();
			Root.Q<MicroserviceVisualElementSeparator>("separator")?.RemoveFromHierarchy();

#if UNITY_2019_1_OR_NEWER
			Root.Q<VisualElement>("mainVisualElement").style.height = new StyleLength(DEFAULT_HEADER_HEIGHT);
#elif UNITY_2018
			Root.Q<VisualElement>("mainVisualElement").style.height = StyleValue<float>.Create(DEFAULT_HEADER_HEIGHT);
#endif

			// _statusIcon.RemoveFromHierarchy();
			Root.Q("foldContainer").visible = false;

			var manipulator = new ContextualMenuManipulator(Model.PopulateMoreDropdown);
			manipulator.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
			_moreBtn.AddManipulator(manipulator);
			_moreBtn.tooltip = "More...";

			UpdateLocalStatus();
			UpdateRemoteStatusIcon("remoteEnabled");
			UpdateModel();
		}

		protected override void QueryVisualElements()
		{
			base.QueryVisualElements();

			_mongoStorageModel = (RemoteMongoStorageModel)Model;
		}
	}
}
