using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
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
	public class LabeledCheckboxVisualElement : BeamableVisualElement
	{
		public static readonly string ComponentPath = $"{Directories.COMMON_COMPONENTS_PATH}/{nameof(LabeledCheckboxVisualElement)}/{nameof(LabeledCheckboxVisualElement)}";

		public new class UxmlFactory : UxmlFactory<LabeledCheckboxVisualElement, UxmlTraits>
		{
		}

		public Action<bool> OnValueChanged;
		public bool Value
		{
			get => _checkbox.Value;
			set
			{
				SetWithoutNotify(value);
				OnValueChanged?.Invoke(value);
			}
		}

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			readonly UxmlStringAttributeDescription _label = new UxmlStringAttributeDescription
			{ name = "label", defaultValue = "Label" };

			readonly UxmlStringAttributeDescription _icon = new UxmlStringAttributeDescription
			{ name = "icon", defaultValue = "" };

			// used for flip order of child elements from Label-Icon-Checkbox to Checkbox-Icon-Label
			readonly UxmlBoolAttributeDescription _flip = new UxmlBoolAttributeDescription
			{ name = "flip", defaultValue = false };

			readonly UxmlBoolAttributeDescription _flipIcon = new UxmlBoolAttributeDescription
			{ name = "flip-icon", defaultValue = false };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				if (ve is LabeledCheckboxVisualElement component)
				{
					component.Label = _label.GetValueFromBag(bag, cc);
					component.Flip = _flip.GetValueFromBag(bag, cc);
					component.Icon = _icon.GetValueFromBag(bag, cc);
					component.FlipIconCheckbox = _flipIcon.GetValueFromBag(bag, cc);
				}
			}
		}

		private Label _label;
		private Image _icon;
		private BeamableCheckboxVisualElement _checkbox;
		private string _labelText;

		private bool Flip { get; set; }
		private bool FlipIconCheckbox { get; set; }
		private string Label { get; set; }
		private string Icon { get; set; }

		public BeamableCheckboxVisualElement Checkbox => _checkbox;

		public LabeledCheckboxVisualElement() : base(ComponentPath)
		{
		}

		public LabeledCheckboxVisualElement(string labelText = "", bool isFlipped = false, bool isIconFlipped = false) : base(ComponentPath)
		{
			_labelText = labelText;
			Flip = isFlipped;
			FlipIconCheckbox = isIconFlipped;
		}

		public override void Refresh()
		{
			base.Refresh();

			_label = Root.Q<Label>("label");
			_label.text = string.IsNullOrWhiteSpace(_labelText) ? Label : _labelText;

			_icon = Root.Q<Image>("icon");
			_icon.image = !string.IsNullOrEmpty(Icon) ? (Texture)EditorGUIUtility.Load(Icon) : null;

			_checkbox = Root.Q<BeamableCheckboxVisualElement>("checkbox");
			_checkbox.OnValueChanged -= OnChanged;
			_checkbox.OnValueChanged += OnChanged;
			_checkbox.Refresh();

			if (FlipIconCheckbox)
			{
				_checkbox.SendToBack();
				_icon.SendToBack();
				_label.SendToBack();
			}
			if (Flip)
			{
				if (FlipIconCheckbox)
				{
					_checkbox.SendToBack();
					_icon.SendToBack();
				}
				else
				{
					_icon.SendToBack();
					_checkbox.SendToBack();
				}
			}

		}

		private void OnChanged(bool value)
		{
			OnValueChanged?.Invoke(value);
		}

		public void SetWithoutNotify(bool val) => _checkbox.SetWithoutNotify(val);

		public void SetText(string val) => _label.text = val;

		public void SetFlipState(bool val) => Flip = val;

		public void DisableIcon() => _icon.RemoveFromHierarchy();

		public void DisableLabel() => _label.RemoveFromHierarchy();
	}
}
