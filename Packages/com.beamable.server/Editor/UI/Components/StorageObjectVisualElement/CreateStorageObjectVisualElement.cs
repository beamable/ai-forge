using Beamable.Editor.UI.Model;
using Beamable.Server.Editor;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Editor.Microservice.UI.Components
{
	public class CreateStorageObjectVisualElement : CreateServiceBaseVisualElement
	{
		protected override string NewServiceName { get; set; } = "NewStorageObject";
		protected override string ScriptName => nameof(StorageObjectVisualElement);
		protected override ServiceType ServiceType => ServiceType.StorageObject;
		protected override bool ShouldShowCreateDependentService => MicroservicesDataModel.Instance.Services.Any(x => !x.IsArchived);


		protected override void CreateService(string serviceName, List<ServiceModelBase> additionalReferences = null)
		{
			MicroserviceEditor.CreateNewServiceFile(ServiceType, serviceName, additionalReferences);
		}
		protected override void InitCreateDependentService()
		{
			_serviceCreateDependentService.Init(MicroservicesDataModel.Instance.Services.Where(x => !x.IsArchived).ToList(), "MicroServices");
		}
	}
}
