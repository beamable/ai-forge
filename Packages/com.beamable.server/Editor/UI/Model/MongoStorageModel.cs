using Beamable.Editor.UI.Components;
using Beamable.Server.Editor;
using Beamable.Server.Editor.ManagerClient;
using System;
using System.Threading.Tasks;
using UnityEngine;
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
	[System.Serializable]
	public class MongoStorageModel : ServiceModelBase, IBeamableStorageObject
	{
		public ServiceStorageReference RemoteReference { get; protected set; }

		[SerializeField]
		private StorageObjectDescriptor _serviceDescriptor;

		[SerializeField]
		private string _assemblyQualifiedStorageTypeName;
		public StorageObjectDescriptor ServiceDescriptor
		{
			get => _serviceDescriptor;
			set
			{
				_serviceDescriptor = value;
				if (_serviceDescriptor.Type != null)
					_assemblyQualifiedStorageTypeName = _serviceDescriptor.Type.AssemblyQualifiedName;
			}
		}

		public string AssemblyQualifiedStorageTypeName => _assemblyQualifiedStorageTypeName;

		public MongoStorageBuilder ServiceBuilder { get; protected set; }
		public override IBeamableBuilder Builder => ServiceBuilder;
		public override IDescriptor Descriptor => ServiceDescriptor;
		public override bool IsRunning => ServiceBuilder?.IsRunning ?? false;

		public override bool IsArchived
		{
			get => Config.Archived;
			protected set => Config.Archived = value;
		}
		public StorageConfigurationEntry Config { get; protected set; }

		public Action<ServiceStorageReference> OnRemoteReferenceEnriched;

		public override event Action<Task> OnStart;
		public override event Action<Task> OnStop;

		public static MongoStorageModel CreateNew(StorageObjectDescriptor descriptor, MicroservicesDataModel dataModel)
		{
			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			return new MongoStorageModel
			{
				RemoteReference = dataModel.GetStorageReference(descriptor),
				ServiceDescriptor = descriptor,
				ServiceBuilder = serviceRegistry.GetStorageBuilder(descriptor),
				Config = MicroserviceConfiguration.Instance.GetStorageEntry(descriptor.Name)
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
				AssemblyDefinitionHelper.OpenMongoExplorer(ServiceDescriptor);
		}

		public void EnrichWithRemoteReference(ServiceStorageReference remoteReference)
		{
			RemoteReference = remoteReference;
			OnRemoteReferenceEnriched?.Invoke(remoteReference);
		}

		protected void OpenRemoteMongo()
		{
			var b = BeamEditorContext.Default;
			Application.OpenURL($"{BeamableEnvironment.BeamMongoExpressUrl}/create?cid={b.CurrentCustomer.Cid}&pid={b.CurrentRealm.Pid}&token={b.Requester.Token.Token}");
		}

		public override void PopulateMoreDropdown(ContextualMenuPopulateEvent evt)
		{
			var existsOnRemote = RemoteReference?.enabled ?? false;
			var localCategory = IsRunning ? "Local" : "Local (not running)";
			var remoteCategory = existsOnRemote ? "Cloud" : "Cloud (not deployed)";

			evt.menu.BeamableAppendAction($"{localCategory}/Erase data", _ => AssemblyDefinitionHelper.ClearMongo(ServiceDescriptor), IsRunning);
			evt.menu.BeamableAppendAction($"{localCategory}/Goto data explorer", _ => OpenDocs(), IsRunning);
			evt.menu.BeamableAppendAction($"{localCategory}/Create a snapshot", _ => AssemblyDefinitionHelper.SnapshotMongo(ServiceDescriptor), IsRunning);
			evt.menu.BeamableAppendAction($"{localCategory}/Download a snapshot", _ => AssemblyDefinitionHelper.RestoreMongo(ServiceDescriptor), IsRunning);
			evt.menu.BeamableAppendAction($"{localCategory}/Copy connection string", _ => AssemblyDefinitionHelper.CopyConnectionString(ServiceDescriptor), IsRunning);

			evt.menu.BeamableAppendAction($"{remoteCategory}/Goto data explorer", _ => OpenRemoteMongo(), existsOnRemote);

			evt.menu.BeamableAppendAction($"Open C# Code", _ => OpenCode());

			var isFirst = MicroserviceConfiguration.Instance.GetIndex(Name, ServiceType.StorageObject) == 0;
			var isLast = MicroserviceConfiguration.Instance.GetIndex(Name, ServiceType.StorageObject) < MicroservicesDataModel.Instance.Storages.Count - 1;

			evt.menu.BeamableAppendAction($"Move up", pos =>
			{
				MicroserviceConfiguration.Instance.MoveIndex(Name, -1, ServiceType.StorageObject);
				OnSortChanged?.Invoke();
			}, !isFirst);
			evt.menu.BeamableAppendAction($"Move down", pos =>
			{
				MicroserviceConfiguration.Instance.MoveIndex(Name, 1, ServiceType.StorageObject);
				OnSortChanged?.Invoke();
			}, isLast);
			evt.menu.BeamableAppendAction($"Move to top", pos =>
			{
				MicroserviceConfiguration.Instance.SetIndex(Name, 0, ServiceType.StorageObject);
				OnSortChanged?.Invoke();
			}, !isFirst);
			evt.menu.BeamableAppendAction($"Move to bottom", pos =>
			{
				MicroserviceConfiguration.Instance.SetIndex(Name, MicroservicesDataModel.Instance.Storages.Count - 1, ServiceType.StorageObject);
				OnSortChanged?.Invoke();
			}, isLast);

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

		public override void Refresh(IDescriptor descriptor)
		{
			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			// reset the descriptor and statemachines; because they aren't system.serializable durable.
			ServiceDescriptor = (StorageObjectDescriptor)descriptor;
			var oldBuilder = ServiceBuilder;
			ServiceBuilder = serviceRegistry.GetStorageBuilder(ServiceDescriptor);
			ServiceBuilder.ForwardEventsTo(oldBuilder);
			Config = MicroserviceConfiguration.Instance.GetStorageEntry(Name);
		}
	}
}
