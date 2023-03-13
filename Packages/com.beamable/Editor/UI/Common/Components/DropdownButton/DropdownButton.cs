using Beamable.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Beamable.Editor.UI.Components
{
	public class DropdownButton : BeamableVisualElement
	{
		public new class UxmlFactory : UxmlFactory<DropdownButton, UxmlTraits>
		{ }

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			readonly UxmlStringAttributeDescription _text = new UxmlStringAttributeDescription
			{ name = "text", defaultValue = "" };
			readonly UxmlStringAttributeDescription _tooltip = new UxmlStringAttributeDescription
			{ name = "tooltip", defaultValue = "" };
			readonly UxmlBoolAttributeDescription _forceDropdown = new UxmlBoolAttributeDescription
			{ name = "forceDropdown", defaultValue = false };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				if (ve is DropdownButton component)
				{
					component.Text = _text.GetValueFromBag(bag, cc);
					component.Tooltip = _tooltip.GetValueFromBag(bag, cc);
					component.ForceDropdown = _forceDropdown.GetValueFromBag(bag, cc);
					component.Refresh();
				}
			}
		}

		public event Action OnBaseClick;
		public event Action<ContextualMenuPopulateEvent> OnDropdownClick;

		private string Text { get; set; }
		private string Tooltip { get; set; }
		private bool ForceDropdown { get; set; }
		private Button _baseButton;
		private Button _dropdownButton;
		private VisualElement _dropdownImg;
		private bool _mouseOverPublishDropdown;
		private bool _dropdownEnabled = true;
		public DropdownButton() : base(
			$"{Constants.Directories.COMMON_COMPONENTS_PATH}/{nameof(DropdownButton)}/{nameof(DropdownButton)}")
		{ }


		public override void Refresh()
		{
			base.Refresh();

			var label = Root.Q<Label>("label");
			label.text = Text;
			label.tooltip = Tooltip;
			_baseButton = Root.Q<Button>("baseButton");
			_dropdownButton = Root.Q<Button>("dropdownButton");
			_dropdownImg = Root.Q("dropDownImg");
			_dropdownButton.RegisterCallback<MouseEnterEvent>(evt => _mouseOverPublishDropdown = true);
			_dropdownButton.RegisterCallback<MouseLeaveEvent>(evt => _mouseOverPublishDropdown = false);
			_dropdownButton.clickable.activators.Clear();

			var publishButtonManipulator = new ContextualMenuManipulator(HandlePublishButtonClick);
			publishButtonManipulator.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
			_baseButton.clickable.activators.Clear();
			_baseButton.AddManipulator(publishButtonManipulator);
		}

		public void EnableDropdown(bool enable)
		{
			_dropdownEnabled = enable;
			_dropdownImg.EnableInClassList("hidden", !_dropdownEnabled);
		}

		private void HandlePublishButtonClick(ContextualMenuPopulateEvent populateEvent)
		{
			if (_dropdownEnabled && (ForceDropdown || _mouseOverPublishDropdown))
			{
				OnDropdownClick?.Invoke(populateEvent);
			}
			else
			{
				OnBaseClick?.Invoke();
			}
		}
	}
}
