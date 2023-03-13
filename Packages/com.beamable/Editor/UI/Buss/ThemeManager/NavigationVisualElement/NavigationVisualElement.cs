using Beamable.Editor.UI.Buss;
using Beamable.Editor.UI.Common;
using Beamable.UI.Buss;
using System.Collections.Generic;
using UnityEngine.UIElements;
using static Beamable.Common.Constants.Features.Buss.ThemeManager;

namespace Beamable.Editor.UI.Components
{
	public class NavigationVisualElement : BeamableBasicVisualElement
	{
		private readonly List<IndentedLabelVisualElement> _spawnedLabels = new List<IndentedLabelVisualElement>();
		private bool _hasDelayedChangeCallback;
		private ScrollView _hierarchyContainer;
		private IndentedLabelVisualElement _selectedLabel;
		private readonly ThemeManagerModel _model;

		public NavigationVisualElement(ThemeManagerModel model) : base(
			$"{BUSS_THEME_MANAGER_PATH}/{nameof(NavigationVisualElement)}/{nameof(NavigationVisualElement)}.uss")
		{
			_model = model;
		}

		public override void Init()
		{
			base.Init();

			VisualElement header = new VisualElement { name = "header" };

			Image foldIcon = new Image { name = "foldIcon" };
			foldIcon.AddToClassList("unfolded");
			header.Add(foldIcon);

			TextElement label = new TextElement { name = "headerLabel", text = "Navigation" };
			header.Add(label);

			header.RegisterCallback<MouseDownEvent>(evt =>
			{
				_hierarchyContainer.ToggleInClassList("hidden");
				foldIcon.ToggleInClassList("unfolded");
				foldIcon.ToggleInClassList("folded");
			});

			Root.Add(header);

			_hierarchyContainer = new ScrollView { name = "elementsContainer" };
			Root.Add(_hierarchyContainer);

			_model.Change += Refresh;

			Refresh();
		}

		public override void Refresh()
		{
			foreach (IndentedLabelVisualElement child in _spawnedLabels)
			{
				child.Destroy();
			}

			_spawnedLabels.Clear();
			_hierarchyContainer.Clear();

			foreach (KeyValuePair<BussElement, int> pair in _model.FoundElements)
			{
				IndentedLabelVisualElement label = new IndentedLabelVisualElement();
				label.Setup(pair.Key, BussNameUtility.GetFormattedLabel(pair.Key), _model.NavigationElementClicked,
							pair.Value, IndentedLabelVisualElement.DEFAULT_SINGLE_INDENT_WIDTH, pair.Key == _model.SelectedElement);
				label.Init();
				_spawnedLabels.Add(label);
				_hierarchyContainer.Add(label);
			}
		}

		protected override void OnDestroy()
		{
			_model.Change -= Refresh;
		}
	}
}
