using Beamable.Editor.UI.Components;
using Beamable.Editor.UI.Model;
using Beamable.Server.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Microservice.UI.Components
{
	public class DependentServicesPopup : MicroserviceComponent
	{
		public Dictionary<MicroserviceModel, DependentServicesMicroserviceEntryVisualElement> MicroserviceEntries { get; private set; }
		public Dictionary<MongoStorageModel, DependentServicesStorageObjectEntryVisualElement> StorageObjectEntries { get; private set; }
		public bool IsAnyRelationChanged { get; private set; } = false;

		public Action OnClose;
		public Action OnConfirm;

		private VisualElement _storageObjectsContainer;
		private VisualElement _microservicesContainer;
		private PrimaryButtonVisualElement _confirmButton;
		private Button _cancelButton;

		private MicroserviceModel _lastRelationChangedMicroservice;
		private MongoStorageModel _lastRelationChangedStorageObject;
		private DependentServicesMicroserviceEntryVisualElement _emptyRowFillEntry;
		private readonly Dictionary<MicroserviceModel, List<ServiceRelationInfo>> _changedDependencies = new Dictionary<MicroserviceModel, List<ServiceRelationInfo>>();

		public DependentServicesPopup() : base(nameof(DependentServicesPopup))
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
			_confirmButton = Root.Q<PrimaryButtonVisualElement>("confirmBtn");
			_cancelButton = Root.Q<Button>("cancelBtn");
			_storageObjectsContainer = Root.Q("storageObjectsContainer");
			_microservicesContainer = Root.Q("microservicesContainer");
		}
		private void UpdateVisualElements()
		{
			_confirmButton.Button.clickable.clicked += () => OnConfirm?.Invoke();
			_cancelButton.clickable.clicked += () => OnClose?.Invoke();

			SetStorageObjectsContainer();
			SetMicroservicesContainer();
		}
		private void SetStorageObjectsContainer()
		{
			StorageObjectEntries = new Dictionary<MongoStorageModel, DependentServicesStorageObjectEntryVisualElement>(MicroservicesDataModel.Instance.Storages.Count);
			foreach (var storageObjectModel in MicroservicesDataModel.Instance.Storages)
			{
				if (storageObjectModel.IsArchived && !HasAnyDependentMicroservice(storageObjectModel))
				{
					continue;
				}

				var newElement = new DependentServicesStorageObjectEntryVisualElement { Model = storageObjectModel };
				newElement.Refresh();
				newElement.SetEnabled(!storageObjectModel.IsArchived);

				_storageObjectsContainer.Add(newElement);
				StorageObjectEntries.Add(storageObjectModel, newElement);
			}
		}
		private void SetMicroservicesContainer()
		{
			MicroserviceEntries = new Dictionary<MicroserviceModel, DependentServicesMicroserviceEntryVisualElement>(MicroservicesDataModel.Instance.Services.Count);
			foreach (var microserviceModel in MicroservicesDataModel.Instance.Services)
			{
				var dependentServices = GetDependentServices(microserviceModel);

				if (microserviceModel.IsArchived && !dependentServices.Any())
					continue;

				var newElement = new DependentServicesMicroserviceEntryVisualElement(dependentServices, StorageObjectEntries.Keys) { Model = microserviceModel };
				newElement.Refresh();
				newElement.OnServiceRelationChanged += (storageObjectModel, isServiceRelation) => HandleServiceRelationChange(microserviceModel, storageObjectModel, isServiceRelation);
				_microservicesContainer.Add(newElement);
				MicroserviceEntries.Add(microserviceModel, newElement);
			}

			_emptyRowFillEntry = new DependentServicesMicroserviceEntryVisualElement(new List<MongoStorageModel>(), StorageObjectEntries.Keys)
			{
				name = "EmptyRowFillEntry",
				style = { flexGrow = 1 }
			};
			_emptyRowFillEntry.SetEmptyEntries();
			_microservicesContainer.Add(_emptyRowFillEntry);
		}

		public void SetServiceDependencies()
		{
			var storageObjectAssemblyDefinitionsAssets = GetAllStorageObjectAssemblyDefinitionAssets();
			foreach (var service in MicroserviceEntries)
			{
				var microservice = service.Key;
				microservice.Dependencies.Clear();

				foreach (var dependentService in service.Value.DependentServices)
				{
					if (!dependentService.IsServiceRelation)
						continue;
					microservice.Dependencies.Add(dependentService.MongoStorageModel);
				}

				SetAssemblyReferences(microservice, storageObjectAssemblyDefinitionsAssets);
			}
		}
		private void HandleServiceRelationChange(MicroserviceModel microserviceModel, MongoStorageModel storageObjectModel, bool isServiceRelation)
		{
			if (_lastRelationChangedStorageObject != null && _lastRelationChangedMicroservice != null)
				ChangeSelectionHighlight(false);

			_lastRelationChangedMicroservice = microserviceModel;
			_lastRelationChangedStorageObject = storageObjectModel;
			ChangeSelectionHighlight(true);
			IsAnyRelationChanged = true;

			var serviceRelationInfo = new ServiceRelationInfo(storageObjectModel, isServiceRelation);

			if (!_changedDependencies.ContainsKey(microserviceModel))
				_changedDependencies[microserviceModel] = new List<ServiceRelationInfo>();

			if (_changedDependencies[microserviceModel].Contains(serviceRelationInfo))
				_changedDependencies[microserviceModel].Remove(serviceRelationInfo);
			else
				_changedDependencies[microserviceModel].Add(serviceRelationInfo);
		}
		private void ChangeSelectionHighlight(bool state)
		{
			// Row Highlight
			var microserviceEntry = MicroserviceEntries[_lastRelationChangedMicroservice];
			microserviceEntry.EnableInClassList("sectionHighlight", state);
			microserviceEntry.MicroserviceName.EnableInClassList("sectionHighlightLabel", state);

			// Column Highlight
			var storageObjectEntry = StorageObjectEntries[_lastRelationChangedStorageObject];
			storageObjectEntry.EnableInClassList("sectionHighlight", state);
			storageObjectEntry.StorageObjectName.EnableInClassList("sectionHighlightLabel", state);

			DependentServicesCheckboxVisualElement checkboxVisualElement;
			foreach (var entry in MicroserviceEntries.Values)
			{
				checkboxVisualElement = entry.DependentServices.FirstOrDefault(x => x.MongoStorageModel == _lastRelationChangedStorageObject);
				checkboxVisualElement?.EnableInClassList("sectionHighlight", state); ;
			}
			checkboxVisualElement = _emptyRowFillEntry.DependentServices.FirstOrDefault(x => x.MongoStorageModel == _lastRelationChangedStorageObject);
			checkboxVisualElement?.EnableInClassList("sectionHighlight", state); ;
		}
		private IEnumerable<MongoStorageModel> GetDependentServices(MicroserviceModel microserviceModel)
		{
			var microserviceAssemblyDefinition = AssemblyDefinitionHelper.ConvertToInfo(microserviceModel.Descriptor);
			var storageObjectAssemblyDefinitionsAssets = GetAllStorageObjectAssemblyDefinitionAssets();

			var serviceDependencies = new List<MongoStorageModel>();
			foreach (var storageObjectAssemblyDefinitionsAsset in storageObjectAssemblyDefinitionsAssets)
			{
				if (microserviceAssemblyDefinition.References.Contains(AssemblyDefinitionHelper.ConvertToInfo(storageObjectAssemblyDefinitionsAsset.Value).Name))
				{
					serviceDependencies.Add(storageObjectAssemblyDefinitionsAsset.Key);
				}
			}
			return serviceDependencies;
		}

		private bool HasAnyDependentMicroservice(MongoStorageModel storageModel)
		{
			var assemblyDefinition = AssemblyDefinitionHelper.ConvertToAsset(storageModel.Descriptor);

			foreach (var microserviceModel in MicroservicesDataModel.Instance.Services)
			{
				var microserviceAssemblyDefinition = AssemblyDefinitionHelper.ConvertToInfo(microserviceModel.Descriptor);

				if (microserviceAssemblyDefinition.References.Contains(AssemblyDefinitionHelper.ConvertToInfo(assemblyDefinition).Name))
				{
					return true;
				}
			}

			return false;
		}

		private Dictionary<MongoStorageModel, AssemblyDefinitionAsset> GetAllStorageObjectAssemblyDefinitionAssets()
		{
			var storageObjectAssemblyDefinitionsAssets = new Dictionary<MongoStorageModel, AssemblyDefinitionAsset>();
			foreach (var storageObject in MicroservicesDataModel.Instance.Storages)
			{
				var assemblyDefinition = AssemblyDefinitionHelper.ConvertToAsset(storageObject.Descriptor);
				if (assemblyDefinition == null)
					continue;
				storageObjectAssemblyDefinitionsAssets.Add(storageObject, assemblyDefinition);
			}
			return storageObjectAssemblyDefinitionsAssets;
		}
		private void SetAssemblyReferences(MicroserviceModel microserviceModel, Dictionary<MongoStorageModel, AssemblyDefinitionAsset> storageObjectAssemblyDefinitionsAssets)
		{
			if (!_changedDependencies.ContainsKey(microserviceModel))
				return;

			var intersect = storageObjectAssemblyDefinitionsAssets.Where(x => _changedDependencies[microserviceModel]
				.Where(y => y.IsServiceRelation)
				.Select(z => z.Model)
				.Contains(x.Key))
				.ToDictionary(x => x.Key, x =>
					AssemblyDefinitionHelper.ConvertToInfo(storageObjectAssemblyDefinitionsAssets[x.Key]).Name).Values.ToList();

			var nonIntersect = storageObjectAssemblyDefinitionsAssets.Where(x => _changedDependencies[microserviceModel]
				.Where(y => !y.IsServiceRelation)
				.Select(z => z.Model)
				.Contains(x.Key))
				.ToDictionary(x => x.Key, x =>
					AssemblyDefinitionHelper.ConvertToInfo(storageObjectAssemblyDefinitionsAssets[x.Key]).Name).Values.ToList();

			AssemblyDefinitionHelper.AddAndRemoveReferences(microserviceModel.ServiceDescriptor, intersect, nonIntersect);

			if (GetDependentServices(microserviceModel).Any())
				AssemblyDefinitionHelper.AddMongoLibraries(microserviceModel.ServiceDescriptor);
			else
				AssemblyDefinitionHelper.RemoveMongoLibraries(microserviceModel.ServiceDescriptor);
		}

		private class ServiceRelationInfo
		{
			public MongoStorageModel Model { get; }
			public bool IsServiceRelation { get; }

			public ServiceRelationInfo(MongoStorageModel model, bool isServiceRelation)
			{
				Model = model;
				IsServiceRelation = isServiceRelation;
			}
		}
	}
}
