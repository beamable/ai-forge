using Beamable.Editor.UI.Components;
using System.Collections.Generic;
using UnityEditor;
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
	public class ForgotVisualElement : LoginBaseComponent
	{
		private TextField _emailField;
		private TextField _passwordField;
		private TextField _passwordConfField;
		private TextField _codeField;
		private TextField _cidTextField;
		private Label _errorText;
		private PrimaryButtonVisualElement _getCodeButton;
		private GenericButtonVisualElement _backButton;
		private PrimaryButtonVisualElement _continueButton;
		private GenericButtonVisualElement _resendButton;

		public ForgotVisualElement() : base(nameof(ForgotVisualElement))
		{
		}

		public override string GetMessage()
		{
			return "We will send a password reset code to your email address.";
		}

		public override void Refresh()
		{
			base.Refresh();

			_cidTextField = Root.Q<TextField>("organizationID");
			_cidTextField.AddPlaceholder(PLACEHOLDER_CID_FIELD);
			_cidTextField.SetValueWithoutNotify(Model.Customer.CidOrAlias);
			var isAlias = _cidTextField.AddErrorLabel("Alias", PrimaryButtonVisualElement.AliasOrCidErrorHandler);


			_emailField = Root.Q<TextField>("account");
			_emailField.AddPlaceholder(PLACEHOLDER_EMAIL_FIELD);
			_emailField.SetValueWithoutNotify(Model.Customer.Email);
			var isEmail = _emailField.AddErrorLabel("Email", PrimaryButtonVisualElement.EmailErrorHandler);

			_codeField = Root.Q<TextField>("code");
			_codeField.AddPlaceholder(PLACEHOLDER_CODE_FIELD);
			var isCode = _codeField.AddErrorLabel("Code", m => string.IsNullOrEmpty(m) ? "Code is required" : null);

			_passwordField = Root.Q<TextField>("password");
			_passwordField.AddPlaceholder(PLACEHOLDER_PASSWORD_FIELD);
			_passwordField.isPasswordField = true;
			var isPasswordValid = _passwordField.AddErrorLabel("Password", PrimaryButtonVisualElement.PasswordErrorHandler);

			_passwordConfField = Root.Q<TextField>("confirmPassword");
			_passwordConfField.AddPlaceholder(PLACEHOLDER_PASSWORD_CONFIRM_FIELD);
			_passwordConfField.isPasswordField = true;
			var doPasswordsMatch = _passwordConfField.AddErrorLabel("Password Match", m => m != _passwordField.value
			  ? "Passwords don't match"
			  : null);

			_getCodeButton = Root.Q<PrimaryButtonVisualElement>("getCode");
			_getCodeButton.Button.clickable.clicked += GetCode_OnClicked;
			_getCodeButton.AddGateKeeper(isAlias, isEmail);

			_backButton = Root.Q<GenericButtonVisualElement>("login");
			_backButton.SetText(Manager.IsPreviousPage<AccountSummaryVisualElement>() ? "Back to account" : "Back to login");
			_backButton.OnClick += Manager.GoToPreviousPage;

			_resendButton = Root.Q<GenericButtonVisualElement>("resend");
			_resendButton.OnClick += ShowPhase1;

			_continueButton = Root.Q<PrimaryButtonVisualElement>("signIn");
			_continueButton.AddGateKeeper(isAlias, isEmail, isCode, isPasswordValid, doPasswordsMatch);
			_continueButton.Button.clickable.clicked += ResetPassword_OnClicked;

			_errorText = Root.Q<Label>("errorLabel");
			_errorText.AddTextWrapStyle();
			_errorText.text = "";

			ShowPhase1();
		}

		private void ResetPassword_OnClicked()
		{
			Model.Customer.SetPasswordCode(_codeField.value, _passwordField.value);
			var promise = Manager.SendPasswordResetCode(Model);
			_continueButton.Load(AddErrorLabel(promise, _errorText));
		}

		void ShowPhase1()
		{
			Phase1Elements.ForEach(e => e.RemoveFromClassList("hidden"));
			Phase2Elements.ForEach(e => e.AddToClassList("hidden"));
		}

		void ShowPhase2()
		{
			Phase1Elements.ForEach(e => e.AddToClassList("hidden"));
			Phase2Elements.ForEach(e => e.RemoveFromClassList("hidden"));
		}

		List<VisualElement> Phase1Elements => Root.Query<VisualElement>().Class("phase1").Build().ToList();
		List<VisualElement> Phase2Elements => Root.Query<VisualElement>().Class("phase2").Build().ToList();

		private void GetCode_OnClicked()
		{
			Model.Customer.SetPasswordForgetData(_cidTextField.value, _emailField.value);
			var promise = Manager.SendPasswordResetEmail(Model);
			_getCodeButton.Load(AddErrorLabel(promise, _errorText));

			var reenableAt = 0.0;
			var originalText = "";

			void UpdateClock()
			{
				var time = EditorApplication.timeSinceStartup;
				var timeLeft = (int)(reenableAt - time);
				_getCodeButton.SetText($"Resend in {timeLeft}s");
				if (time > reenableAt)
				{
					_getCodeButton.SetText(originalText);
					_getCodeButton.Enable();
				}
				else
				{
					EditorApplication.delayCall += UpdateClock;
				}
			}

			void StartClock()
			{
				originalText = _getCodeButton.Text;
				_getCodeButton.Disable();
				reenableAt = EditorApplication.timeSinceStartup + 5;

				EditorApplication.delayCall += UpdateClock;
			}

			promise.Then(result =>
			{
				if (!result.Success) return;
				StartClock();
				ShowPhase2();
			});
		}
	}
}
