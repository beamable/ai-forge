using Beamable.Server.Editor;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Beamable.Editor.Microservice.UI.Components
{
	public class CreateNewServiceDropdownVisualElement : MicroserviceComponent
	{


		public CreateNewServiceDropdownVisualElement() : base(nameof(CreateNewServiceDropdownVisualElement)) { }
		public event Action<ServiceType> OnCreateNewClicked;

		private VisualElement _servicesList;

		private readonly Dictionary<ServiceType, string> _serviceTypeCustomNamesDict = new Dictionary<ServiceType, string>
		{
			{ ServiceType.MicroService, "Microservice" },
			{ ServiceType.StorageObject, "Storage Object" }
		};

		public override void Refresh()
		{
			base.Refresh();
			_servicesList = Root.Q<VisualElement>("servicesList");
			SetContent();
		}
		private void SetContent()
		{
			foreach (var serviceType in (ServiceType[])Enum.GetValues(typeof(ServiceType)))
			{
				var serviceEntryButton = new VisualElement { name = "serviceEntryButton" };
				serviceEntryButton.Add(new Image { name = $"image{serviceType}" });
				serviceEntryButton.Add(new Label(TryGetCustomServiceName(serviceType)) { name = "label" });
				serviceEntryButton.RegisterCallback<MouseDownEvent>(_ =>
				{
					OnCreateNewClicked?.Invoke(serviceType);
				});
				_servicesList.Add(serviceEntryButton);
			}
		}
		private string TryGetCustomServiceName(ServiceType serviceType)
			=> _serviceTypeCustomNamesDict.ContainsKey(serviceType) ? _serviceTypeCustomNamesDict[serviceType] : serviceType.ToString();
	}
}
