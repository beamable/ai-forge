using Beamable.Editor.UI.Model;
using System;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Microservice.UI.Components
{
	public class ServiceFilterDropdownVisualElement : MicroserviceComponent
	{
		public event Action<ServicesDisplayFilter> OnNewServicesDisplayFilterSelected;
		private VisualElement _listRoot;
		public ServiceFilterDropdownVisualElement() : base(nameof(ServiceFilterDropdownVisualElement))
		{
		}

		public override void Refresh()
		{
			base.Refresh();
			_listRoot = Root.Q<VisualElement>("popupContent");
			_listRoot.Clear();
			AddButton(ServicesDisplayFilter.AllTypes);
			AddButton(ServicesDisplayFilter.Microservices);
			AddButton(ServicesDisplayFilter.Storages);
			AddButton(ServicesDisplayFilter.Archived);
		}

		void AddButton(ServicesDisplayFilter filter)
		{
			var currentFilter = MicroservicesDataModel.Instance.Filter;
			var realmSelectButton = new Button();
			switch (filter)
			{
				case ServicesDisplayFilter.AllTypes:
					realmSelectButton.text = "All types";
					break;
				default:
					realmSelectButton.text = filter.ToString();
					break;
			}

			realmSelectButton.SetEnabled(currentFilter != filter);
			realmSelectButton.clickable.clicked += () => OnNewServicesDisplayFilterSelected?.Invoke(filter);
			_listRoot.Add(realmSelectButton);
		}
	}
}
