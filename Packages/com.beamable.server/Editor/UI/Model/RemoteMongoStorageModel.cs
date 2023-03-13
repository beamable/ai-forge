using Beamable.Server.Editor;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.UI.Model
{
	public class RemoteMongoStorageModel : MongoStorageModel
	{
		public static new RemoteMongoStorageModel CreateNew(StorageObjectDescriptor descriptor, MicroservicesDataModel dataModel)
		{
			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			return new RemoteMongoStorageModel
			{
				RemoteReference = dataModel.GetStorageReference(descriptor),
				ServiceDescriptor = descriptor,
				ServiceBuilder = serviceRegistry.GetStorageBuilder(descriptor),
				Config = MicroserviceConfiguration.Instance.GetStorageEntry(descriptor.Name)
			};
		}

		public override void PopulateMoreDropdown(ContextualMenuPopulateEvent evt)
		{
			var remoteCategory = "Cloud";

			evt.menu.BeamableAppendAction($"{remoteCategory}/Goto data explorer", _ => OpenRemoteMongo());

			if (MicroserviceConfiguration.Instance.StorageObjects.Count > 1)
			{
				evt.menu.BeamableAppendAction($"Order/Move Up", pos =>
				{
					MicroserviceConfiguration.Instance.MoveIndex(Name, -1, ServiceType.StorageObject);
					OnSortChanged?.Invoke();
				}, MicroserviceConfiguration.Instance.GetIndex(Name, ServiceType.StorageObject) > 0);
				evt.menu.BeamableAppendAction($"Order/Move Down", pos =>
				{
					MicroserviceConfiguration.Instance.MoveIndex(Name, 1, ServiceType.StorageObject);
					OnSortChanged?.Invoke();
				}, MicroserviceConfiguration.Instance.GetIndex(Name, ServiceType.StorageObject) < MicroserviceConfiguration.Instance.StorageObjects.Count - 1);
			}
			AddArchiveSupport(evt);
		}
	}
}
