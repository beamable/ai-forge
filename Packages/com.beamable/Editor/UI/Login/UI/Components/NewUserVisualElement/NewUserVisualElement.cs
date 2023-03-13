using Beamable.Editor.UI.Components;
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
	public class NewUserVisualElement : LoginBaseComponent
	{
		private TextField _cidOrAliasTextField;
		private TextField _emailTextField;
		private TextField _passwordTextField;
		private TextField _passwordConfirmTextField;
		private PrimaryButtonVisualElement _continueButton;
		private GenericButtonVisualElement _legalButton;
		private Toggle _legalCheckbox;
		private Label _errorText;
		private GenericButtonVisualElement _existingAccountButton;
		private GenericButtonVisualElement _switchOrgButton;

		public NewUserVisualElement() : base(nameof(NewUserVisualElement))
		{
		}

		public override string GetMessage()
		{
			return "Create a new account. You will need to check with your Organization's administrator to receive full access to the project.";
		}


		public override void Refresh()
		{
			base.Refresh();

			_cidOrAliasTextField = Root.Q<TextField>("organizationID");
			_cidOrAliasTextField.AddPlaceholder(PLACEHOLDER_CID_FIELD);
			_cidOrAliasTextField.SetValueWithoutNotify(Model.Customer.CidOrAlias);
			var isAlias = _cidOrAliasTextField.AddErrorLabel("Alias", PrimaryButtonVisualElement.AliasErrorHandler);

			_emailTextField = Root.Q<TextField>("account");
			_emailTextField.AddPlaceholder(PLACEHOLDER_EMAIL_FIELD);
			var isEmail = _emailTextField.AddErrorLabel("Email", PrimaryButtonVisualElement.EmailErrorHandler);

			_passwordTextField = Root.Q<TextField>("password");
			_passwordTextField.AddPlaceholder(PLACEHOLDER_PASSWORD_FIELD);
			_passwordTextField.isPasswordField = true;
			var isPasswordValid = _passwordTextField.AddErrorLabel("Password", PrimaryButtonVisualElement.PasswordErrorHandler);


			_passwordConfirmTextField = Root.Q<TextField>("confirmPassword");
			_passwordConfirmTextField.AddPlaceholder(PLACEHOLDER_PASSWORD_CONFIRM_FIELD);
			_passwordConfirmTextField.isPasswordField = true;
			var doPasswordsMatch = _passwordConfirmTextField.AddErrorLabel("Password Match", m => m != _passwordTextField.value
			   ? "Passwords don't match"
			   : null);

			_legalCheckbox = Root.Q<Toggle>();
			_legalCheckbox.SetValueWithoutNotify(Model.ReadLegalCopy);
			_legalCheckbox.RegisterValueChangedCallback(evt => Model.ReadLegalCopy = evt.newValue);
			var isLegal = _legalCheckbox.AddErrorLabel("Legal", PrimaryButtonVisualElement.LegalErrorHandler);

			_continueButton = Root.Q<PrimaryButtonVisualElement>("signIn");
			_continueButton.Button.clickable.clicked += Continue_OnClicked;
			_continueButton.AddGateKeeper(isAlias, isEmail, isLegal, isPasswordValid, doPasswordsMatch);

			_legalButton = Root.Q<GenericButtonVisualElement>("legalButton");
			_legalButton.OnClick += () => { Application.OpenURL(URL_BEAMABLE_LEGAL_WEBSITE); };

			_existingAccountButton = Root.Q<GenericButtonVisualElement>("existingAccount");
			_existingAccountButton.OnClick += Manager.GotoExistingCustomer;

			//         _switchOrgButton = Root.Q<Button>("newOrganization");
			//         _switchOrgButton.clickable.clicked += Manager.GotoCustomerSelection;

			_errorText = Root.Q<Label>("errorLabel");
			_errorText.AddTextWrapStyle();
			_errorText.text = "";
		}

		private void Continue_OnClicked()
		{
			Model.Customer.SetExistingCustomerData(_cidOrAliasTextField.value, _emailTextField.value, _passwordTextField.value);
			var promise = Manager.AttemptNewUser(Model);
			_continueButton.Load(AddErrorLabel(promise, _errorText));
		}
	}
}
