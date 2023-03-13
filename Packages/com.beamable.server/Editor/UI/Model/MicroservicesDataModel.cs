using Beamable.Common;
using Beamable.Server.Editor;
using Beamable.Server.Editor.ManagerClient;
using Beamable.Server.Editor.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Editor.UI.Model
{
	[System.Serializable]
	public class MicroservicesDataModel : IServiceStorable
	{
		private readonly BeamEditorContext _ctx;

		public static MicroservicesDataModel GetInstance(BeamEditorContext context)
		{
			context = context ?? BeamEditorContext.Default;
			return context.ServiceScope.GetService<MicroservicesDataModel>();
		}

		public static MicroservicesDataModel Instance => GetInstance(BeamEditorContext.Default);

		private MicroserviceReflectionCache.Registry _serviceRegistry;

		public List<MicroserviceModel> localServices = new List<MicroserviceModel>();
		public List<RemoteMicroserviceModel> remoteServices = new List<RemoteMicroserviceModel>();
		public List<MongoStorageModel> localStorages = new List<MongoStorageModel>();
		public List<RemoteMongoStorageModel> remoteStorages = new List<RemoteMongoStorageModel>();

		public int AllUnarchivedServiceCount =>
			localServices.Count(model => !model.IsArchived) + localStorages.Count(model => !model.IsArchived) +
			remoteServices.Count(model => !model.IsArchived) + remoteStorages.Count(model => !model.IsArchived);

		public IReadOnlyList<IBeamableService> AllLocalServices
		{
			get
			{
				var temp = new List<IBeamableService>();
				temp.AddRange(localServices);
				temp.AddRange(localStorages);
				return temp;
			}
		}
		public IReadOnlyList<IBeamableService> AllRemoteOnlyServices
		{
			get
			{
				var temp = new List<IBeamableService>();
				temp.AddRange(remoteServices);
				temp.AddRange(remoteStorages);
				return temp;
			}
		}
		public IReadOnlyList<MongoStorageModel> AllStorages
		{
			get
			{
				var temp = new List<MongoStorageModel>();
				temp.AddRange(localStorages);
				temp.AddRange(remoteStorages);
				return temp;
			}
		}
		public List<MicroserviceModel> Services => localServices;
		public List<MongoStorageModel> Storages => localStorages;

		public ServiceManifest ServerManifest = new ServiceManifest();
		public GetStatusResponse Status = new GetStatusResponse();
		public ServicesDisplayFilter Filter = ServicesDisplayFilter.AllTypes;

		public Action<ServiceManifest> OnServerManifestUpdated;
		public Action<GetStatusResponse> OnStatusUpdated;

		public List<ServiceDependencyChecksum> ServiceDependencyChecksums = new List<ServiceDependencyChecksum>();

		public Promise FinishedLoading { get; private set; } = new Promise();

		[NonSerialized]
		public Guid InstanceId = Guid.NewGuid();

		public MicroservicesDataModel(BeamEditorContext ctx)
		{
			_ctx = ctx;

			ctx.OnRealmChange += _ =>
			{
				var __ = RefreshState(); // update self.
			};
		}

		public void RefreshLocal()
		{
			var unseen = new HashSet<IBeamableService>(AllLocalServices);
			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			if (serviceRegistry != null)
			{
				foreach (var descriptor in serviceRegistry.AllDescriptors)
				{
					var serviceExists = ContainsModel(descriptor.Name);
					if (serviceExists)
					{
						var service = GetModel<IBeamableService>(descriptor.Name);
						unseen.Remove(GetModel<IBeamableService>(descriptor.Name));
						service.Refresh(descriptor);
						continue;
					}

					if (descriptor.ServiceType == ServiceType.StorageObject)
					{
						var newService = MongoStorageModel.CreateNew(descriptor as StorageObjectDescriptor, this);
						localStorages.Add(newService);
					}
					else
					{
						var newService = MicroserviceModel.CreateNew(descriptor as MicroserviceDescriptor, this);
						localServices.Add(newService);
					}
				}
			}

			localServices.RemoveAll(model => unseen.Contains(model));
			localStorages.RemoveAll(model => unseen.Contains(model));
		}

		public async Promise RefreshServerManifest()
		{
			await _ctx.GetMicroserviceManager().GetStatus().Then(status =>
			{
				Status = status;
				foreach (var serviceStatus in status.services)
				{
					GetModel<MicroserviceModel>(serviceStatus.serviceName)?.EnrichWithStatus(serviceStatus);
				}
				OnStatusUpdated?.Invoke(status);
			});
			await _ctx.GetMicroserviceManager().GetCurrentManifest().Then(manifest =>
			{
				ServerManifest = manifest;
				foreach (var service in Services)
				{
					var remoteService = manifest.manifest.FirstOrDefault(remote => string.Equals(remote.serviceName, service.Name));
					service.EnrichWithRemoteReference(remoteService);
				}

				foreach (var storage in Storages)
				{
					var remoteStorage = manifest.storageReference.FirstOrDefault(remote => string.Equals(remote.id, storage.Name));
					storage.EnrichWithRemoteReference(remoteStorage);
				}

				foreach (var singleStorageManifest in ServerManifest.storageReference)
				{
					if (ContainsRemoteOnlyModel(singleStorageManifest.id))
						continue;


					var entry = MicroserviceConfiguration.Instance.GetStorageEntry(singleStorageManifest.id);
					var descriptor = new StorageObjectDescriptor
					{
						Name = singleStorageManifest.id
					};

					remoteStorages.Add(RemoteMongoStorageModel.CreateNew(descriptor, this));
				}

				foreach (var singleManifest in ServerManifest.manifest)
				{
					if (ContainsRemoteOnlyModel(singleManifest.serviceName))
						continue;

					var descriptor = new MicroserviceDescriptor
					{
						Name = singleManifest.serviceName
					};

					var model = RemoteMicroserviceModel.CreateNew(descriptor, this);
					model.Dependencies = singleManifest.dependencies
													   .Select(d => remoteStorages.FirstOrDefault(s => s.Name.Equals(d.id)))
													   .Cast<MongoStorageModel>()
													   .ToList();
					remoteServices.Add(model);
				}



				OnServerManifestUpdated?.Invoke(manifest);
			});

		}

		public void AddLogMessage(string name, LogMessage message)
		{
			AllLocalServices.FirstOrDefault(r => r.Descriptor.Name.Equals(name))
				?.Logs.AddMessage(message);
		}

		public void AddLogMessage(IDescriptor descriptor, LogMessage message) => AddLogMessage(descriptor.Name, message);

		public void AddLogException(IDescriptor descriptor, Exception ex)
		{
			AddLogMessage(descriptor, new LogMessage
			{
				Level = LogLevel.ERROR,
				Message = $"{ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}",
				Timestamp = LogMessage.GetTimeDisplay(DateTime.Now),
			});
		}

		public ServiceStatus GetStatus(MicroserviceDescriptor descriptor)
		{
			return Status?.services?.FirstOrDefault(r => r.serviceName.Equals(descriptor.Name));
		}

		public Dictionary<string, ServiceAvailability> GetAllServicesStatus()
		{
			var getServiceStatus = new Func<bool, bool, ServiceAvailability>((isLocally, isRemotely) =>
			{
				if (isLocally && isRemotely)
					return ServiceAvailability.LocalAndRemote;

				return isLocally ? ServiceAvailability.LocalOnly :
				isRemotely ? ServiceAvailability.RemoteOnly : ServiceAvailability.Unknown;
			});

			var result = new Dictionary<string, ServiceAvailability>();
			var servicesStatus = Status?.services ?? new List<ServiceStatus>();
			var storageStatus = ServerManifest?.storageReference ?? new List<ServiceStorageReference>();

			var allServiceNames = Services.Select(s => s.Name)
										  .Concat(servicesStatus.Select(x => x.serviceName))
										  .Distinct();
			var allStorageNames = Storages.Select(s => s.Name)
										  .Concat(storageStatus.Select(x => x.id))
										  .Distinct();

			foreach (var service in allServiceNames)
			{
				var configEntry = MicroserviceConfiguration.Instance.GetEntry(service);
				var name = configEntry.ServiceName;
				var remotely = servicesStatus?.Find(status => status.serviceName.Equals(name)) != null;
				if (!result.ContainsKey(name))
					result.Add(name, getServiceStatus(ContainsModel(configEntry.ServiceName), remotely));
			}

			foreach (var storage in allStorageNames)
			{
				var configEntry = MicroserviceConfiguration.Instance.GetStorageEntry(storage);
				var name = configEntry.StorageName;
				var remotely = storageStatus?.Find(status => storage.Equals(name)) != null;
				if (!result.ContainsKey(name))
					result.Add(name, getServiceStatus(ContainsModel(name), remotely));
			}

			return result;
		}

		public ServiceReference GetReference(MicroserviceDescriptor descriptor)
		{
			return ServerManifest?.manifest?.FirstOrDefault(r => r.serviceName.Equals(descriptor.Name));
		}

		public ServiceStorageReference GetStorageReference(StorageObjectDescriptor descriptor)
		{
			return ServerManifest?.storageReference?.FirstOrDefault(r => r.id.Equals(descriptor.Name));
		}

		public ServiceType GetModelServiceType(string name)
		{
			var allServices = new List<IBeamableService>();
			allServices.AddRange(AllLocalServices);
			allServices.AddRange(AllRemoteOnlyServices);

			var service = allServices
			   .FirstOrDefault(s => s.Descriptor.Name.Equals(name));
			return service?.ServiceType ?? ServiceType.MicroService;
		}

		public bool ContainsRemoteOnlyModel(string serviceName) => AllRemoteOnlyServices?.Any(s => s.Descriptor.Name.Equals(serviceName)) ?? false;
		public bool ContainsModel(string serviceName) => AllLocalServices?.Any(s => s.Descriptor.Name.Equals(serviceName)) ?? false;

		public T GetModel<T>(IDescriptor descriptor) where T : IBeamableService =>
		   GetModel<T>(descriptor.Name);

		public T GetModel<T>(string serviceName) where T : IBeamableService
		{
			var allServices = new List<IBeamableService>();
			allServices.AddRange(AllLocalServices);
			allServices.AddRange(AllRemoteOnlyServices);

			return (T)allServices?.FirstOrDefault(s => s is T && s.Descriptor.Name.Equals(serviceName));
		}

		public MicroserviceModel GetMicroserviceModel(IDescriptor descriptor) => GetModel<MicroserviceModel>(descriptor);

		public MongoStorageModel GetStorageModel(IDescriptor descriptor) => GetModel<MongoStorageModel>(descriptor);

		private void HandleMicroservicesDeploySuccess(ManifestModel oldManifest, int serviceCount)
		{
			var _ = RefreshServerManifest();
		}

		public void Destroy()
		{
			_serviceRegistry.OnDeploySuccess -= HandleMicroservicesDeploySuccess;
		}

		public void OnBeforeSaveState()
		{

		}

		public async Promise RefreshState()
		{
			remoteServices.Clear();
			remoteStorages.Clear();
			RefreshLocal();
			await RefreshServerManifest();
		}

		public void OnAfterLoadState()
		{
			RefreshState().Merge(FinishedLoading);
		}
	}

	public enum ServiceAvailability
	{
		LocalOnly,
		RemoteOnly,
		LocalAndRemote,
		Unknown
	}

	public enum ServicesDisplayFilter
	{
		AllTypes,
		Microservices,
		Storages,
		Archived
	}
}
