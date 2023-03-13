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
	public class RemoteMicroserviceModel : MicroserviceModel
	{
		public new static RemoteMicroserviceModel CreateNew(MicroserviceDescriptor descriptor, MicroservicesDataModel dataModel)
		{
			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			return new RemoteMicroserviceModel
			{
				ServiceDescriptor = descriptor,
				ServiceBuilder = serviceRegistry.GetServiceBuilder(descriptor),
				RemoteReference = dataModel.GetReference(descriptor),
				RemoteStatus = dataModel.GetStatus(descriptor)
			};
		}

		public override void PopulateMoreDropdown(ContextualMenuPopulateEvent evt)
		{
			var remoteCategory = "Cloud";

			evt.menu.BeamableAppendAction($"{remoteCategory}/View Documentation", pos => { OpenRemoteDocs(); });
			evt.menu.BeamableAppendAction($"{remoteCategory}/View Metrics", pos => { OpenRemoteMetrics(); });
			evt.menu.BeamableAppendAction($"{remoteCategory}/View Logs", pos => { OpenRemoteLogs(); });

			if (MicroserviceConfiguration.Instance.Microservices.Count > 1)
			{
				evt.menu.BeamableAppendAction($"Order/Move Up", pos =>
				{
					MicroserviceConfiguration.Instance.MoveIndex(Name, -1, ServiceType.MicroService);
					OnSortChanged?.Invoke();
				}, MicroserviceConfiguration.Instance.GetIndex(Name, ServiceType.MicroService) > 0);
				evt.menu.BeamableAppendAction($"Order/Move Down", pos =>
				{
					MicroserviceConfiguration.Instance.MoveIndex(Name, 1, ServiceType.MicroService);
					OnSortChanged?.Invoke();
				}, MicroserviceConfiguration.Instance.GetIndex(Name, ServiceType.MicroService) < MicroserviceConfiguration.Instance.Microservices.Count - 1);
			}
			AddArchiveSupport(evt);
		}
	}
}
