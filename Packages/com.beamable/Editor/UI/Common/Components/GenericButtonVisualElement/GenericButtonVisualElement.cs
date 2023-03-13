using System;
using System.Collections.Generic;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants;

namespace Beamable.Editor.UI.Components
{
	public class GenericButtonVisualElement : BeamableVisualElement
	{
		public enum ButtonType
		{
			Default,
			Confirm,
			Cancel,
			Link
		}

		public new class UxmlFactory : UxmlFactory<GenericButtonVisualElement, UxmlTraits>
		{
		}

		public event Action OnClick;

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			private ButtonType _defaultType = ButtonType.Default;

			readonly UxmlStringAttributeDescription _text = new UxmlStringAttributeDescription
			{ name = "text", defaultValue = "" };

			readonly UxmlStringAttributeDescription _tooltip = new UxmlStringAttributeDescription
			{ name = "tooltip", defaultValue = "" };

			readonly UxmlStringAttributeDescription _type = new UxmlStringAttributeDescription
			{ name = "type", defaultValue = "" };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				if (ve is GenericButtonVisualElement component)
				{
					component.Text = _text.GetValueFromBag(bag, cc);
					component.Tooltip = _tooltip.GetValueFromBag(bag, cc);

					string passedType = _type.GetValueFromBag(bag, cc);
					bool parsed = Enum.TryParse(passedType, true, out ButtonType parsedType);
					component.Type = parsed ? parsedType : _defaultType;
					component.Refresh();
				}
			}
		}

		private Button _button;
		private VisualElement _mainVisualElement;

		private ButtonType Type { get; set; }
		private string Text { get; set; }
		private string Tooltip { get; set; }

		public GenericButtonVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(GenericButtonVisualElement)}/{nameof(GenericButtonVisualElement)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			_button = Root.Q<Button>("button");
			_button.text = Text;
			_button.tooltip = Tooltip;
			_button.clickable.clicked += () => { OnClick?.Invoke(); };
			_mainVisualElement = Root.Q<VisualElement>("mainVisualElement");
			_button.AddToClassList(Type.ToString().ToLower());
		}

		public void SetText(string val) => _button.text = val;

		public void SetTooltip(string val) => _button.tooltip = val;
	}
}
