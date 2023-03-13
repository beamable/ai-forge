using Beamable.Editor.UI.Buss;
using System;
using UnityEngine.UIElements;

namespace Beamable.Editor.UI.Components
{
	public class PropertyFilterDropdownVisualElement : ThemeManagerComponent
	{
		public event Action<ThemeModel.PropertyDisplayFilter> OnNewPropertyDisplayFilterSelected;

		private VisualElement _listRoot;

		private readonly ThemeManagerBreadcrumbsVisualElement _themeManagerBreadcrumbsVisualElement;

		public PropertyFilterDropdownVisualElement(
			ThemeManagerBreadcrumbsVisualElement themeManagerBreadcrumbsVisualElement) : base(
			nameof(PropertyFilterDropdownVisualElement))
		{
			_themeManagerBreadcrumbsVisualElement = themeManagerBreadcrumbsVisualElement;
		}

		public override void Refresh()
		{
			base.Refresh();
			_listRoot = Root.Q<VisualElement>("popupContent");
			_listRoot.Clear();

			foreach (var propertyDisplayFilter in (ThemeModel.PropertyDisplayFilter[])Enum.GetValues(
						 typeof(ThemeModel.PropertyDisplayFilter)))
				AddButton(propertyDisplayFilter);
		}

		private void AddButton(ThemeModel.PropertyDisplayFilter filter)
		{
			var propertyFilterButton = new Button();
			propertyFilterButton.text = _themeManagerBreadcrumbsVisualElement.GetPropertyDisplayFilterText(filter);
			propertyFilterButton.SetEnabled(_themeManagerBreadcrumbsVisualElement.Model.DisplayFilter != filter);
			propertyFilterButton.clickable.clicked += () => OnNewPropertyDisplayFilterSelected?.Invoke(filter);
			_listRoot.Add(propertyFilterButton);
		}
	}
}
