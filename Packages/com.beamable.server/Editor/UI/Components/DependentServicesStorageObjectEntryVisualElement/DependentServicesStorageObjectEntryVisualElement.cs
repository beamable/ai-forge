using Beamable.Editor.UI.Model;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Microservice.UI.Components
{
	public class DependentServicesStorageObjectEntryVisualElement : MicroserviceComponent
	{
		public MongoStorageModel Model { get; set; }
		public Label StorageObjectName { get; private set; }

		public DependentServicesStorageObjectEntryVisualElement() : base(nameof(DependentServicesStorageObjectEntryVisualElement))
		{
		}
		public override void Refresh()
		{
			base.Refresh();
			QueryVisualElements();
			UpdateVisualElements();
		}
		private void QueryVisualElements()
		{
			StorageObjectName = Root.Q<Label>("storageObjectName");
		}
		private void UpdateVisualElements()
		{
			StorageObjectName.text = Model.Name + (Model.IsArchived ? " (Archived)" : string.Empty);
			StorageObjectName.AddTextWrapStyle();
		}
	}
}
