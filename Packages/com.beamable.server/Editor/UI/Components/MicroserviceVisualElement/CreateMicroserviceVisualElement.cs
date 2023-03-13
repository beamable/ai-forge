using Beamable.Editor.UI.Model;
using Beamable.Server.Editor;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Editor.Microservice.UI.Components
{
	public class CreateMicroserviceVisualElement : CreateServiceBaseVisualElement
	{
		protected override string NewServiceName { get; set; } = "NewMicroService";
		protected override string ScriptName => nameof(MicroserviceVisualElement);
		protected override ServiceType ServiceType => ServiceType.MicroService;

		protected override bool ShouldShowCreateDependentService =>
			MicroservicesDataModel.Instance.Storages.Any(x => !x.IsArchived);

		protected override void CreateService(string serviceName, List<ServiceModelBase> additionalReferences = null)
		{
			MicroserviceEditor.CreateNewServiceFile(ServiceType, serviceName, additionalReferences);
			BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>().MicroserviceCreated(serviceName);
		}
		protected override void InitCreateDependentService()
		{
			_serviceCreateDependentService.Init(MicroservicesDataModel.Instance.Storages.Where(x => !x.IsArchived).ToList(), "StorageObjects");
		}
	}
}
