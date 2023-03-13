using Beamable.Editor.UI.Buss;
using Beamable.Editor.UI.Common;
using Beamable.UI.Buss;
using System;
using UnityEngine.UIElements;
using static Beamable.Common.Constants.Features.Buss.ThemeManager;

namespace Beamable.Editor.UI.Components
{
	public class InlineStyleVisualElement : BeamableBasicVisualElement
	{
		private VisualElement _variableContainer;
		private VisualElement _propertyContainer;

		private readonly ThemeManagerModel _model;

		public InlineStyleVisualElement(ThemeManagerModel model) : base(
			$"{BUSS_THEME_MANAGER_PATH}/{nameof(InlineStyleVisualElement)}/{nameof(InlineStyleVisualElement)}.uss",
			false)
		{
			_model = model;
		}

		public override void Init()
		{
			base.Init();

			VisualElement header = new VisualElement();
			header.AddToClassList("header");

			Image foldIcon = new Image { name = "foldIcon" };
			foldIcon.AddToClassList("folded");
			header.Add(foldIcon);

			TextElement label = new TextElement();
			label.AddToClassList("headerLabel");
			label.text = "Inline Style";
			header.Add(label);

			Root.Add(header);

			var mainContainer = new VisualElement();
			mainContainer.AddToClassList("mainContainer");
			mainContainer.AddToClassList("hidden");
			Root.Add(mainContainer);

			header.RegisterCallback<MouseDownEvent>(evt =>
			{
				mainContainer.ToggleInClassList("hidden");
				foldIcon.ToggleInClassList("unfolded");
				foldIcon.ToggleInClassList("folded");
			});

			VisualElement variablesHeader = CreateSubheader("Variables", _model.AddInlineVariable);
			mainContainer.Add(variablesHeader);

			_variableContainer = new VisualElement();
			_variableContainer.AddToClassList("propertyContainer");
			mainContainer.Add(_variableContainer);

			VisualElement propertiesHeader = CreateSubheader("Properties", _model.AddInlineProperty);
			mainContainer.Add(propertiesHeader);

			_propertyContainer = new VisualElement();
			_propertyContainer.AddToClassList("propertyContainer");
			mainContainer.Add(_propertyContainer);

			_model.Change += Refresh;
		}

		protected override void OnDestroy()
		{
			_model.Change -= Refresh;
			ClearAll();
		}

		public override void Refresh()
		{
			ClearAll();

			if (_model.SelectedElement == null)
			{
				return;
			}

			SpawnProperties();
		}

		private void SpawnProperties()
		{
			var selectedElement = _model.SelectedElement;

			PropertySourceTracker propertySourceTracker = _model.PropertyDatabase.GetTracker(selectedElement);

			foreach (BussPropertyProvider property in selectedElement.InlineStyle.Properties)
			{
				StylePropertyModel model = new StylePropertyModel(selectedElement.StyleSheet, null,
																  property,
																  propertySourceTracker, selectedElement, selectedElement,
																  _model.RemoveInlineProperty, null, _model.SetInlinePropertyValueType);

				var element = new StylePropertyVisualElement(model);
				element.Init();
				(model.IsVariable ? _variableContainer : _propertyContainer).Add(element);
			}
		}

		private VisualElement CreateSubheader(string text, Action onAddClicked)
		{
			var header = new VisualElement();
			header.AddToClassList("subheader");
			var label = new TextElement();
			label.AddToClassList("subheaderLabel");
			label.text = text;
			header.Add(label);

			var separator = new VisualElement();
			separator.AddToClassList("headerSeparator");
			header.Add(separator);

			var addButton = new VisualElement();
			addButton.AddToClassList("addButton");
			header.Add(addButton);
			addButton.RegisterCallback<MouseDownEvent>(_ => onAddClicked());

			return header;
		}

		private void ClearAll()
		{
			_propertyContainer.Clear();
			_variableContainer.Clear();
		}
	}
}
