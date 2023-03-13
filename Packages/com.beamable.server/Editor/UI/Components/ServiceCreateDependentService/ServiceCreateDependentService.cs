using Beamable.Editor.UI.Components;
using Beamable.Editor.UI.Model;
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
	public class ServiceCreateDependentService : MicroserviceComponent
	{
		private VisualElement _mainContainer;

		private readonly Dictionary<ServiceModelBase, LabeledCheckboxVisualElement> _serviceModelBases =
			new Dictionary<ServiceModelBase, LabeledCheckboxVisualElement>();

		public ServiceCreateDependentService() : base(nameof(ServiceCreateDependentService))
		{
		}

		public override void Refresh()
		{
			base.Refresh();
			_mainContainer = Root.Q("mainContainer");
		}

		public void Init<T>(List<T> services, string serviceTypeName) where T : ServiceModelBase
		{
			Root.Q<Label>("header").text = $"Optional dependencies ({serviceTypeName}):";
			foreach (var service in services)
			{
				var checkbox = new LabeledCheckboxVisualElement();
				checkbox.SetFlipState(true);
				checkbox.Refresh();
				checkbox.SetText(service.Name);
				checkbox.DisableIcon();
				_serviceModelBases.Add(service, checkbox);
				_mainContainer.Add(checkbox);
			}
		}

		public List<ServiceModelBase> GetReferences()
		{
			return _serviceModelBases.Where(x => x.Value.Value)
									 .Select(y => y.Key)
									 .ToList();
		}
	}
}
