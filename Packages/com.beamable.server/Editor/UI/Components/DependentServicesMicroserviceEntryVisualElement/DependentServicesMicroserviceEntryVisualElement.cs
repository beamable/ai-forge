using Beamable.Editor.UI.Components;
using Beamable.Editor.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Microservice.UI.Components
{
	public class DependentServicesMicroserviceEntryVisualElement : MicroserviceComponent
	{
		public Action<MongoStorageModel, bool> OnServiceRelationChanged;
		public MicroserviceModel Model { get; set; }
		public List<DependentServicesCheckboxVisualElement> DependentServices { get; private set; }
		public Label MicroserviceName { get; private set; }
		private VisualElement _dependencyCheckboxes;
		private readonly IEnumerable<MongoStorageModel> _dependentServices;
		private readonly IEnumerable<MongoStorageModel> _visibleServices;

		public DependentServicesMicroserviceEntryVisualElement(IEnumerable<MongoStorageModel> dependentServices, IEnumerable<MongoStorageModel> visibleServices) : base(nameof(DependentServicesMicroserviceEntryVisualElement))
		{
			_dependentServices = dependentServices;
			_visibleServices = visibleServices;
		}
		public override void Refresh()
		{
			base.Refresh();
			QueryVisualElements();
			UpdateVisualElements();
		}
		private void QueryVisualElements()
		{
			MicroserviceName = Root.Q<Label>("microserviceName");
			_dependencyCheckboxes = Root.Q("dependencyCheckboxes");
		}
		private void UpdateVisualElements()
		{
			if (Model.Name.TryEllipseText(15, out var microserviceName))
			{
				MicroserviceName.tooltip = Model.Name;
			}

			MicroserviceName.text = microserviceName + (Model.IsArchived ? " (Archived)" : string.Empty);
			DependentServices = new List<DependentServicesCheckboxVisualElement>(_visibleServices.Count());

			foreach (var storageObjectModel in _visibleServices)
			{
				var isRelation = _dependentServices.Contains(storageObjectModel);
				var newElement = new DependentServicesCheckboxVisualElement(isRelation) { MongoStorageModel = storageObjectModel };
				newElement.OnServiceRelationChanged += TriggerServiceRelationChanged;
				newElement.Refresh();
				if (storageObjectModel.IsArchived || Model.IsArchived)
					newElement.SetEnabled(false);
				_dependencyCheckboxes.Add(newElement);
				DependentServices.Add(newElement);
			}

			this.SetEnabled(!Model.IsArchived);
		}
		private void TriggerServiceRelationChanged(MongoStorageModel storageObjectModel, bool isServiceRelation)
		{
			OnServiceRelationChanged?.Invoke(storageObjectModel, isServiceRelation);
		}
		public void SetEmptyEntries()
		{
			base.Refresh();
			QueryVisualElements();
			MicroserviceName.RemoveFromHierarchy();
			Root.AddToClassList("emptyColumnEntry");

			DependentServices = new List<DependentServicesCheckboxVisualElement>(_visibleServices.Count());
			foreach (var storageObjectModel in _visibleServices)
			{
				var newElement = new DependentServicesCheckboxVisualElement(false) { MongoStorageModel = storageObjectModel };
				newElement.Refresh();
				newElement.Q<BeamableCheckboxVisualElement>("checkbox").RemoveFromHierarchy();
				_dependencyCheckboxes.Add(newElement);
				DependentServices.Add(newElement);
			}
		}
	}
}
