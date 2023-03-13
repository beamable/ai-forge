using Beamable.Theme;
using Beamable.Theme.Palettes;
using Beamable.UI.Scripts;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace Beamable.AccountManagement
{
	public class AccountEmailLogin : MenuBase
	{
		public const string MatchEmailPattern = "^(?(\")(\".+?(?<!\\\\)\"@)|(([0-9a-z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`{}|~\\w])*)(?<=[0-9a-z])@))(?([)([(\\d{1,3}.){3}\\d{1,3}])|(([0-9a-z][-0-9a-z]*[0-9a-z]*.)+[a-z0-9][-a-z0-9]{0,22}[a-z0-9]))$";

		public TMP_InputField _emailInput;
		public TMP_InputField _passwordInput;
		public TextMeshProUGUI _messageTxt;
		public Button _forgotButton;
		public Button ContinueButton;
		public List<InputValidationBehaviour> ValidationBehaviours;

		public StyleBehaviour _styledText;
		public ColorBinding ErrorColor;
		public ColorBinding RegularColor;

		public StringBinding CreatePasswordStringBinding;
		public StringBinding EnterEmailStringBinding;
		public StringBinding EnterExistingPasswordStringBinding;

		public LoadingIndicator LoadingIndicator;
		public AccountManagementSignals Signaler;

		public override void OnOpened()
		{
			_emailInput.text = "";
			_passwordInput.text = "";
			_messageTxt.text = "";

			_passwordInput.gameObject.SetActive(false);
			_forgotButton.gameObject.SetActive(false);
			_messageTxt.gameObject.SetActive(false);
			SetMessageColor(false);

		}

		private void Update()
		{
			SetContinueButtonClickable();
		}

		void SetContinueButtonClickable()
		{
			var allValid = true;

			for (var i = 0; i < ValidationBehaviours.Count; i++)
			{
				var behaviourValid = !ValidationBehaviours[i].isActiveAndEnabled || ValidationBehaviours[i].IsValid;
				allValid &= behaviourValid;
			}
			ContinueButton.interactable = allValid;
		}

		public void EmailChanged(InputReference emailText)
		{
			if (emailText.Value.Length > 0)
			{
				Signaler.UpdateLoginEmail(emailText.Value);
			}
		}

		public void OnForgotPassword()
		{
			var menu = Manager.Show<AccountForgotPassword>();
			menu.SetEmail(_emailInput.text.Replace("\u200B", ""));
		}

		public void EmailIsRegistered(string email)
		{
			_forgotButton.gameObject.SetActive(true);
			_passwordInput.gameObject.SetActive(true);
			_messageTxt.gameObject.SetActive(true);
			_messageTxt.text = EnterExistingPasswordStringBinding.Localize();

			_forgotButton.gameObject.SetActive(true);
			SetMessageColor(false);
		}

		public void EmailIsAvailable(string email)
		{
			_forgotButton.gameObject.SetActive(false);
			_passwordInput.gameObject.SetActive(true);

			_messageTxt.gameObject.SetActive(true);
			_messageTxt.text = CreatePasswordStringBinding.Localize();
			_forgotButton.gameObject.SetActive(false);
			SetMessageColor(false);
		}

		public void EmailIsInvalid(string email)
		{

			_forgotButton.gameObject.SetActive(false);
			_messageTxt.gameObject.SetActive(true);

			_messageTxt.text = EnterEmailStringBinding.Localize();
			SetMessageColor(true);

		}

		public void HandleError(string errorMessage)
		{
			_messageTxt.gameObject.SetActive(true);
			_messageTxt.text = errorMessage;
			SetMessageColor(true);
		}

		private void SetMessageColor(bool error)
		{
			_styledText.StyledTexts.ColorBinding = error ? ErrorColor : RegularColor;
			_styledText.Refresh();

			_messageTxt.ForceMeshUpdate();
			_messageTxt.SetAllDirty();
			_messageTxt.UpdateMeshPadding();
		}
	}
}
