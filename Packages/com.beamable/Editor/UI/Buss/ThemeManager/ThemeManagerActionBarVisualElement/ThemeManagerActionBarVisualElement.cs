using Beamable.Editor.UI.Common;
using Beamable.Editor.UI.Components;
using System;
using UnityEngine.UIElements;
using static Beamable.Common.Constants.Features.Buss.ThemeManager;

namespace Beamable.Editor.UI.Buss
{
	// TODO: TD213896
	public class ThemeManagerActionBarVisualElement : BeamableBasicVisualElement
	{
		private readonly Action _onAddStyle;
		private readonly Action _onCopy;
		private readonly Action _onRefresh;
		private readonly Action _onDocs;
		private readonly Action<string> _onSearch;

		public ThemeManagerActionBarVisualElement(Action onAddStyle,
													  Action onCopy,
													  Action onRefresh,
													  Action onDocs,
													  Action<string> onSearch) : base(
			$"{BUSS_THEME_MANAGER_PATH}/{nameof(ThemeManagerActionBarVisualElement)}/{nameof(ThemeManagerActionBarVisualElement)}.uss")
		{
			_onAddStyle = onAddStyle;
			_onCopy = onCopy;
			_onRefresh = onRefresh;
			_onDocs = onDocs;
			_onSearch = onSearch;
		}

		public override void Init()
		{
			base.Init();

			VisualElement leftContainer = new VisualElement { name = "leftContainer" };
			Root.Add(leftContainer);

			leftContainer.Add(CreateLabeledIconButton("addStyle", "Add style", _onAddStyle));
			leftContainer.Add(CreateLabeledIconButton("duplicateStylesheet", "Duplicate", _onCopy));

			VisualElement rightContainer = new VisualElement { name = "rightContainer" };
			Root.Add(rightContainer);

			SearchBarVisualElement searchBarVisualElement = new SearchBarVisualElement { name = "searchBar" };
			searchBarVisualElement.OnSearchChanged += _onSearch;
			rightContainer.Add(searchBarVisualElement);

			rightContainer.Add(CreateIconButton("refresh", _onRefresh));
			rightContainer.Add(CreateIconButton("doc", _onDocs));
		}

		private Button CreateLabeledIconButton(string buttonName, string buttonLabel, Action onClick)
		{
			Button button = new Button();
			button.AddToClassList("boundedButton");
			button.name = $"{buttonName}Button";
			if (onClick != null)
			{
				button.clickable.clicked += onClick;
			}

			Image icon = new Image();
			icon.AddToClassList("iconLabelButton");
			icon.name = $"{buttonName}Icon";
			button.Add(icon);

			Label label = new Label();
			label.AddToClassList("buttonText");
			label.text = buttonLabel;
			button.Add(label);

			return button;
		}

		private Button CreateIconButton(string buttonName, Action onClick)
		{
			Button button = new Button();
			button.AddToClassList("unboundedButton");
			button.name = $"{buttonName}Button";
			if (onClick != null)
			{
				button.clickable.clicked += onClick;
			}

			Image icon = new Image();
			icon.AddToClassList("iconButton");
			icon.name = $"{buttonName}Icon";
			button.Add(icon);

			return button;
		}
	}
}
