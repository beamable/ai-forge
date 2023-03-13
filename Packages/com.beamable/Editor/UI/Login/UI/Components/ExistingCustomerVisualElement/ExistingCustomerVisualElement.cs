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
using static Beamable.Common.Constants.Features.LoginBase;

namespace Beamable.Editor.Login.UI.Components
{
	public class ExistingCustomerVisualElement : LoginBaseComponent
	{
		private GenericButtonVisualElement _switchCustomerButton;
		private TextField _cidTextField;
		private PrimaryButtonVisualElement _continueButton;
		private TextField _emailTextField;
		private TextField _passwordTextField;
		private Label _errorText;
		private GenericButtonVisualElement _newUserButton;
		private GenericButtonVisualElement _forgotPasswordButton;
		private FormConstraint[] _constraints;

		public ExistingCustomerVisualElement() : base(nameof(ExistingCustomerVisualElement))
		{
		}

		public override string GetMessage()
		{
			return "Welcome to Beamable. Please sign into your account.";
		}

		public override void Refresh()
		{
			base.Refresh();

			_switchCustomerButton = Root.Q<GenericButtonVisualElement>("newOrganization");
			_switchCustomerButton.OnClick += Manager.GotoNewCustomer;

			_forgotPasswordButton = Root.Q<GenericButtonVisualElement>("forgotPassword");
			_forgotPasswordButton.OnClick += () =>
			{
				Model.Customer.SetExistingCustomerData(_cidTextField.value, _emailTextField.value, null);
				Manager.GotoForgotPassword();
			};


			_newUserButton = Root.Q<GenericButtonVisualElement>("createNewLink");
			_newUserButton.OnClick += Manager.GotoNewUser;

			_continueButton = Root.Q<PrimaryButtonVisualElement>("signIn");
			_continueButton.Button.clickable.clicked += HandleContinueClicked;
			_continueButton.tooltip = "Enter all Data";

			_cidTextField = Root.Q<TextField>("organizationID");
			_cidTextField.AddPlaceholder(PLACEHOLDER_CID_FIELD);
			var isAlias = _cidTextField.AddErrorLabel("Alias", PrimaryButtonVisualElement.AliasOrCidErrorHandler);

			_emailTextField = Root.Q<TextField>("account");
			_emailTextField.AddPlaceholder(PLACEHOLDER_EMAIL_FIELD);
			var isEmail = _emailTextField.AddErrorLabel("Email", PrimaryButtonVisualElement.EmailErrorHandler);


			_passwordTextField = Root.Q<TextField>("password");
			_passwordTextField.AddPlaceholder(PLACEHOLDER_PASSWORD_FIELD);
			_passwordTextField.isPasswordField = true;
			var isPassword = _passwordTextField.AddErrorLabel("Password", m => { return null; });

			_cidTextField.SetValueWithoutNotify(Model.Customer.CidOrAlias);
			_emailTextField.SetValueWithoutNotify(Model.Customer.Email);
			_passwordTextField.RegisterCallback<KeyDownEvent>(HandlePasswordFieldKeyDown,
			   TrickleDown.TrickleDown);

			_constraints = new[] { isAlias, isEmail, isPassword };
			_continueButton.AddGateKeeper(_constraints);

			_errorText = Root.Q<Label>("errorLabel");
			_errorText.AddTextWrapStyle();
			_errorText.text = "";
		}

		private void HandlePasswordFieldKeyDown(KeyDownEvent evt)
		{
			if (evt.keyCode == KeyCode.Return && _constraints.All(constraint => constraint.IsValid))
			{
				HandleContinueClicked();
			}
		}

		private void HandleContinueClicked()
		{
			_errorText.text = "";
			Model.Customer.SetExistingCustomerData(_cidTextField.value, _emailTextField.value, _passwordTextField.value);
			var promise = Manager.AttemptLoginExistingCustomer(Model);
			_continueButton.Load(AddErrorLabel(promise, _errorText));

		}
	}
}
