using Beamable.Common;
using Beamable.Editor.UI.Components;
using Beamable.Editor.UI.Model;
using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Microservice.UI.Components
{
	public class MicroserviceBreadcrumbsVisualElement : MicroserviceComponent
	{
		public new class UxmlFactory : UxmlFactory<MicroserviceBreadcrumbsVisualElement, UxmlTraits>
		{
		}

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			UxmlStringAttributeDescription customText = new UxmlStringAttributeDescription
			{ name = "custom-text", defaultValue = "nada" };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				var self = ve as MicroserviceBreadcrumbsVisualElement;

			}
		}

		public event Action<ServicesDisplayFilter> OnNewServicesDisplayFilterSelected;

		private RealmButtonVisualElement _realmButton;
		private Button _servicesFilter;
		private Label _servicesFilterLabel;
		private ServicesDisplayFilter _filter;

		public MicroserviceBreadcrumbsVisualElement() : base(nameof(MicroserviceBreadcrumbsVisualElement))
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			_realmButton = Root.Q<RealmButtonVisualElement>("realmButton");
			_realmButton.Refresh();

			_servicesFilter = Root.Q<Button>("servicesFilter");
			_servicesFilter.tooltip = Constants.Tooltips.Microservice.FILTER;
			_servicesFilterLabel = _servicesFilter.Q<Label>();
			_servicesFilter.clickable.clicked -= HandleServicesFilterButter;
			_servicesFilter.clickable.clicked += HandleServicesFilterButter;
			OnNewServicesDisplayFilterSelected -= UpdateServicesFilterText;
			OnNewServicesDisplayFilterSelected += UpdateServicesFilterText;
			UpdateServicesFilterText(MicroservicesDataModel.Instance.Filter);
			_servicesFilter.visible = true;
		}

		void UpdateServicesFilterText(ServicesDisplayFilter filter)
		{
			switch (filter)
			{
				case ServicesDisplayFilter.AllTypes:
					_servicesFilterLabel.text = "All types";
					break;
				default:
					_servicesFilterLabel.text = filter.ToString();
					break;
			}
		}

		private void HandleServicesFilterButter()
		{
			HandleServicesFilterButter(_servicesFilter.worldBound);
		}

		private void HandleServicesFilterButter(Rect visualElementBounds)
		{
			var popupWindowRect = BeamablePopupWindow.GetLowerLeftOfBounds(visualElementBounds);

			var content = new ServiceFilterDropdownVisualElement();
			content.Refresh();
			var wnd = BeamablePopupWindow.ShowDropdown("Select", popupWindowRect, new Vector2(150, 100), content);
			content.OnNewServicesDisplayFilterSelected += filter =>
			{
				wnd.Close();
				_filter = filter;
				OnNewServicesDisplayFilterSelected?.Invoke(filter);
			};
		}

		public void RefreshFiltering()
		{
			OnNewServicesDisplayFilterSelected?.Invoke(_filter);
		}
	}

}
