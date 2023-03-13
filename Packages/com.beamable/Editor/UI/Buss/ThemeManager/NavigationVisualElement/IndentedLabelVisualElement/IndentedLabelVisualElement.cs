using Beamable.Editor.UI.Common;
using Beamable.UI.Buss;
using System;
#if UNITY_2018
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants.Features.Buss.ThemeManager;

namespace Beamable.Editor.UI.Components
{
	public class IndentedLabelVisualElement : BeamableBasicVisualElement
	{
		public const float DEFAULT_SINGLE_INDENT_WIDTH = 20.0f;

		private float _singleIndentWidth;
		private float _level;
		private string _label;

		private Action<BussElement> _onMouseClicked;

		private VisualElement _container;
		private TextElement _labelComponent;
		private BussElement _bussElement;
		private bool _selected;

		public IndentedLabelVisualElement() : base(
			$"{BUSS_THEME_MANAGER_PATH}/{nameof(NavigationVisualElement)}/{nameof(IndentedLabelVisualElement)}/{nameof(IndentedLabelVisualElement)}.uss")
		{ }

		public void Setup(BussElement bussElement,
						  string label,
						  Action<BussElement> onMouseClicked,
						  int level,
						  float width,
						  bool selected)
		{
			_bussElement = bussElement;
			_onMouseClicked = onMouseClicked;

			_label = label;
			_level = level;
			_singleIndentWidth = width;
			_selected = selected;
		}

		public override void Init()
		{
			base.Init();

			_container = new VisualElement { name = "indentedLabelContainer" };

			_labelComponent = new TextElement { name = "indentedLabel", text = _label };
			_container.SetSelected(_selected);

			float width = (_singleIndentWidth * _level) + _singleIndentWidth;

#if UNITY_2018
			_labelComponent.SetLeft(width);
#elif UNITY_2019_1_OR_NEWER
			_labelComponent.style.paddingLeft = new StyleLength(width);
#endif

			_container.Add(_labelComponent);

			Root.Add(_container);

			_container.RegisterCallback<MouseDownEvent>(OnMouseClicked);
			_container.RegisterCallback<MouseOverEvent>(OnMouseOver);
			_container.RegisterCallback<MouseOutEvent>(OnMouseOut);
		}

		protected override void OnDestroy()
		{
			_container?.UnregisterCallback<MouseDownEvent>(OnMouseClicked);
			_container?.UnregisterCallback<MouseOverEvent>(OnMouseOver);
			_container?.UnregisterCallback<MouseOutEvent>(OnMouseOut);
		}

		private void OnMouseOver(MouseOverEvent evt)
		{
			if (!Root.IsSelected())
			{
				Root.SetHovered(true);
			}
		}

		private void OnMouseOut(MouseOutEvent evt)
		{
			Root.SetHovered(false);
		}

		private void OnMouseClicked(MouseDownEvent evt)
		{
			_onMouseClicked?.Invoke(_bussElement);
		}
	}
}
