using Beamable.Editor.UI.Buss;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Beamable.Editor.UI.Components
{
	public class ThemeManagerBreadcrumbsVisualElement : ThemeManagerComponent
	{
		public ThemeManagerModel Model { get; }

		private Button _propertiesFilter;
		private Label _propertiesFilterLabel;

		// Helper dict for custom names
		private readonly Dictionary<ThemeModel.PropertyDisplayFilter, string> _propertyDisplayFilterTexts =
			new Dictionary<ThemeModel.PropertyDisplayFilter, string>
			{
				{ThemeModel.PropertyDisplayFilter.IgnoreOverridden, "Ignore Overriden"}
			};

		public ThemeManagerBreadcrumbsVisualElement(ThemeManagerModel model) : base(nameof(ThemeManagerBreadcrumbsVisualElement))
		{
			Model = model;
			Model.Change += Refresh;
		}

		public override void Refresh()
		{
			base.Refresh();

			_propertiesFilter = Root.Q<Button>("propertiesFilter");
			_propertiesFilterLabel = _propertiesFilter.Q<Label>();
			_propertiesFilter.clickable.clicked -= HandlePropertiesFilterButton;
			_propertiesFilter.clickable.clicked += HandlePropertiesFilterButton;
			UpdateServicesFilterText(Model.DisplayFilter);
		}

		private void UpdateServicesFilterText(ThemeModel.PropertyDisplayFilter filter)
			=> _propertiesFilterLabel.text = GetPropertyDisplayFilterText(filter);

		private void HandlePropertiesFilterButton()
			=> HandlePropertiesFilterButton(_propertiesFilter.worldBound);

		private void HandlePropertiesFilterButton(Rect visualElementBounds)
		{
			var popupWindowRect = BeamablePopupWindow.GetLowerLeftOfBounds(visualElementBounds);
			var content = new PropertyFilterDropdownVisualElement(this);
			content.Refresh();
			var wnd = BeamablePopupWindow.ShowDropdown("Select", popupWindowRect, new Vector2(150, 50), content);
			content.OnNewPropertyDisplayFilterSelected += filter =>
			{
				wnd.Close();
				Model.DisplayFilter = filter;
				Model.ForceRefresh();
			};
		}

		public string GetPropertyDisplayFilterText(ThemeModel.PropertyDisplayFilter propertyDisplayFilter)
			=> !_propertyDisplayFilterTexts.ContainsKey(propertyDisplayFilter)
				? propertyDisplayFilter.ToString()
				: _propertyDisplayFilterTexts[propertyDisplayFilter];
	}


}
