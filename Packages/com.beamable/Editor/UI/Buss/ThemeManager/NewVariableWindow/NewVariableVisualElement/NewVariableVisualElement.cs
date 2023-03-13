using Beamable.Editor.UI.Components;
using Beamable.UI.Buss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants.Features.Buss.ThemeManager;

namespace Beamable.Editor.UI.Buss
{
	public class NewVariableVisualElement : BeamableVisualElement
	{
		private readonly Action<string, IBussProperty> _onPropertyCreated;

		private LabeledTextField _variableName;
		private Label _propertyLabel;
		private VisualElement _propertyValue;
		private LabeledDropdownVisualElement _selectType;
		private LabeledDropdownVisualElement _selectEnum;

		private BussPropertyVisualElement _currentPropertyElement;
		private IBussProperty _selectedBussProperty;

		private const int LABEL_WIDTH = 160;

		// Can start exactly with two dashes ("--") OR with any letter 
		// Numbers and special characters are not valid
		private const string VARIABLE_NAME_REGEX = "^\\A(-{2}|[a-zA-Z])*$";

		public NewVariableVisualElement(Action<string, IBussProperty> onPropertyCreated) : base(
			$"{BUSS_THEME_MANAGER_PATH}/NewVariableWindow/{nameof(NewVariableVisualElement)}/{nameof(NewVariableVisualElement)}")
		{
			_onPropertyCreated = onPropertyCreated;
		}

		private readonly Dictionary<string, IBussProperty> _typesDict = new Dictionary<string, IBussProperty>
		{
			{"Color", new SingleColorBussProperty()},
			{"VertexColor", new VertexColorBussProperty()},
			{"Float", new FloatBussProperty()},
			//{"FloatFromFloat", new FractionFloatBussProperty()},
			{"Enum", null},
			{"Sprite", new SpriteBussProperty()},
			{"Font", new FontBussAssetProperty()},
		};

		private readonly Dictionary<string, IBussProperty> _enumsDict = new Dictionary<string, IBussProperty>
		{
			{"ImageType", new ImageTypeBussProperty()},
			{"SdfMode", new SdfModeBussProperty()},
			{"BorderMode", new BorderModeBussProperty()},
			{"BackgroundMode", new BackgroundModeBussProperty()},
			{"ShadowMode", new ShadowModeBussProperty()},
			{"Easing", new EasingBussProperty()},
			{"TextAlignmentOptions", new TextAlignmentOptionsBussProperty()},
		};

		private PrimaryButtonVisualElement _confirmButton;

		public override void Refresh()
		{
			base.Refresh();

			_propertyValue = Root.Q<VisualElement>("propertyValue");
			_propertyLabel = Root.Q<Label>("propertyLabel");

			_variableName = Root.Q<LabeledTextField>("variableName");
			_variableName.Setup("Variable name", string.Empty, OnValidate);
			_variableName.Refresh();
			_variableName.OverrideLabelWidth(LABEL_WIDTH);

			_selectEnum = Root.Q<LabeledDropdownVisualElement>("selectEnum");

			_selectType = Root.Q<LabeledDropdownVisualElement>("selectType");
			_selectType.Setup(_typesDict.Keys.ToList(), HandleTypeSwitchProperty);
			_variableName.AddErrorLabel(null, _ =>
			{

				IsNameValid(out var msg);
				return msg;
			});
			_selectType.Refresh();
			_selectType.OverrideLabelWidth(LABEL_WIDTH);

			_confirmButton = Root.Q<PrimaryButtonVisualElement>("confirmButton");
			_confirmButton.Button.clickable.clicked += HandleConfirmButton;

			var cancelButton = Root.Q<GenericButtonVisualElement>("cancelButton");
			cancelButton.OnClick += NewVariableWindow.CloseWindow;

			OnValidate(String.Empty);
		}

		private void OnValidate(string value)
		{
			if (!IsNameValid(out var message))
			{
				ChangeButtonState(false, message);
			}
			else
			{
				ChangeButtonState(true);
			}
		}

		private bool IsNameValid(out string message)
		{
			message = string.Empty;
			var variableName = _variableName.Value;

			if (string.IsNullOrWhiteSpace(variableName))
			{
				message = "Variable name can't be empty";
				return false;
			}

			if (!Regex.IsMatch(variableName, VARIABLE_NAME_REGEX))
			{
				message = "Variable name can contain only letters";
				return false;
			}

			return true;
		}

		private void HandleConfirmButton()
		{
			if (string.IsNullOrWhiteSpace(_variableName.Value))
			{
				Debug.LogError("Variable name can't be empty!");
				return;
			}

			_onPropertyCreated?.Invoke(BussNameUtility.AsVariableName(_variableName.Value), _selectedBussProperty);

			NewVariableWindow.CloseWindow();
		}

		private void HandleTypeSwitchProperty(int selectedIndex)
		{
			RemoveProperty();
			var kvp = _typesDict.ElementAt(selectedIndex);

			if (CanCreateSubDropdown(kvp.Key))
				return;
			PrepareProperty(kvp);
		}

		private void HandleEnumSwitchProperty(int selectedIndex)
		{
			RemoveProperty();
			var kvp = _enumsDict.ElementAt(selectedIndex);
			PrepareProperty(kvp);
		}

		private void PrepareProperty(KeyValuePair<string, IBussProperty> kvp)
		{
			_selectedBussProperty = kvp.Value;

			var propertyProvider = BussPropertyProvider.Create(kvp.Key, kvp.Value);
			_currentPropertyElement = propertyProvider.GetVisualElement();
			_propertyValue.Add(_currentPropertyElement);
			_propertyLabel.text = kvp.Key;
			_currentPropertyElement.Init();
		}

		private void RemoveProperty()
		{
			if (_currentPropertyElement == null || !_propertyValue.Contains(_currentPropertyElement))
				return;

			_propertyValue.Remove(_currentPropertyElement);
			_currentPropertyElement = null;
		}

		private bool CanCreateSubDropdown(string key)
		{
			if (key == "Enum")
			{
				_selectEnum.Setup(_enumsDict.Keys.ToList(), HandleEnumSwitchProperty);
				_selectEnum.Refresh();
				_selectEnum.OverrideLabelWidth(LABEL_WIDTH);
				_selectEnum.EnableInClassList("hide", false);
				return true;
			}

			_selectEnum.EnableInClassList("hide", true);
			return false;
		}

		private void ChangeButtonState(bool isEnabled, string tooltip = "")
		{
			_confirmButton.tooltip = tooltip;
			if (isEnabled)
				_confirmButton.Enable();
			else
				_confirmButton.Disable();
		}
	}
}
