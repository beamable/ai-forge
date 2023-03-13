using Beamable.Editor.UI.Common;
using Beamable.Editor.UI.Components;
using System.Linq;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants.URLs;
using static Beamable.Common.Constants.Features.LoginBase;

namespace Beamable.Editor.Login.UI.Components
{
	public class NewCustomerVisualElement : LoginBaseComponent
	{
		private Toggle _legalCheckbox;
		private TextField _cidTextField;
		private TextField _gameNameField;
		private TextField _emailField;
		private TextField _passwordField;
		private TextField _passwordConfField;
		private GenericButtonVisualElement _legalButton;
		private GenericButtonVisualElement _switchCustomerButton;
		private Label _errorText;
		private PrimaryButtonVisualElement _continueButton;

		public NewCustomerVisualElement() : base(nameof(NewCustomerVisualElement))
		{
		}

		public override string GetMessage()
		{
			return "Welcome to Beamable! Create an organization and your first Beamable game.";
		}

		public override void Refresh()
		{
			base.Refresh();

			_cidTextField = Root.Q<TextField>("organizationID");
			_cidTextField.AddPlaceholder(PLACEHOLDER_ALIAS_FIELD);
			var isAlias = _cidTextField.AddErrorLabel("Alias", PrimaryButtonVisualElement.AliasErrorHandler);

			_gameNameField = Root.Q<TextField>("projectID");
			_gameNameField.AddPlaceholder(PLACEHOLDER_GAMENAME_FIELD);
			var isGame = _gameNameField.AddErrorLabel("Game", PrimaryButtonVisualElement.GameNameErrorHandler);

			_emailField = Root.Q<TextField>("account");
			_emailField.AddPlaceholder(PLACEHOLDER_EMAIL_FIELD);
			var isEmail = _emailField.AddErrorLabel("Email", PrimaryButtonVisualElement.EmailErrorHandler);

			_passwordField = Root.Q<TextField>("password");
			_passwordField.AddPlaceholder(PLACEHOLDER_PASSWORD_FIELD);
			_passwordField.isPasswordField = true;
			var isPasswordValid = _passwordField.AddErrorLabel("Password", m => PrimaryButtonVisualElement.IsPassword(m)
			   ? null
			   : "A valid password must be at least 4 characters long");

			_passwordConfField = Root.Q<TextField>("confirmPassword");
			_passwordConfField.AddPlaceholder(PLACEHOLDER_PASSWORD_CONFIRM_FIELD);
			_passwordConfField.isPasswordField = true;
			var doPasswordsMatch = _passwordConfField.AddErrorLabel("Password Match", m => m != _passwordField.value
																								  ? "Passwords don't match"
																								  : null);
			_legalCheckbox = Root.Q<Toggle>();
			_legalCheckbox.SetValueWithoutNotify(Model.ReadLegalCopy);
			_legalCheckbox.RegisterValueChangedCallback(evt => Model.ReadLegalCopy = evt.newValue);
			var isLegal = _legalCheckbox.AddErrorLabel("Legal", PrimaryButtonVisualElement.LegalErrorHandler);

			_legalButton = Root.Q<GenericButtonVisualElement>("legalButton");
			_legalButton.OnClick += () => { Application.OpenURL(URL_BEAMABLE_LEGAL_WEBSITE); };

			_continueButton = Root.Q<PrimaryButtonVisualElement>();
			_continueButton.Button.clickable.clicked += CreateCustomer_OnClicked;

			var constraints = new[] { doPasswordsMatch, isPasswordValid, isEmail, isAlias, isGame, isLegal };
			_continueButton.AddGateKeeper(constraints);
			_continueButton.RegisterCallback<MouseEnterEvent>(evt => ContinueButton_OnMouseEnter(evt, constraints));

			_switchCustomerButton = Root.Q<GenericButtonVisualElement>("existingOrganization");
			_switchCustomerButton.OnClick += Manager.GotoExistingCustomer;

			_errorText = Root.Q<Label>("errorLabel");
			_errorText.AddTextWrapStyle();
			_errorText.text = "";
		}

		private void CreateCustomer_OnClicked()
		{
			Model.Customer.SetNewCustomer(_cidTextField.value, _gameNameField.value, _emailField.value,
			   _passwordField.value);
			var promise = Manager.AttemptNewCustomer(Model);
			_continueButton.Load(AddErrorLabel(promise, _errorText));
		}
		private void ContinueButton_OnMouseEnter(MouseEnterEvent evt, FormConstraint[] constraints)
		{
			var fieldsWithError = constraints.Where(kvp => !kvp.IsValid);
			foreach (var fieldWithError in fieldsWithError)
			{
				fieldWithError.Check(true);
			}
		}
	}
}
