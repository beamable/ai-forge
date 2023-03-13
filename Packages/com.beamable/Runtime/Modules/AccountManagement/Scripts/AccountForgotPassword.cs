using Beamable.UI.Scripts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.AccountManagement
{
	public class AccountForgotPassword : MenuBase
	{
		public ForgotPasswordArguments Arguments;

		public GameObject SendEmailContainer, ConfirmContainer;

		public TextReferenceBase ErrorText;

		public Button ContinueButton;
		public List<InputValidationBehaviour> ValidationBehaviours;

		private bool requestSent;

		// Start is called before the first frame update
		void Start()
		{
			ErrorText.Value = "";
		}

		// Update is called once per frame
		void Update()
		{
			ContinueButton.interactable = !requestSent && ValidationBehaviours.TrueForAll(v => v.IsValid || !v.isActiveAndEnabled);
		}

		public override void OnOpened()
		{
			requestSent = false;
			Arguments.Password.Value = "";
			Arguments.Code.Value = "";
			SetForSendEmail();
		}

		public void SetEmail(string email)
		{
			Arguments.Email.Value = email;
		}

		public void SetForConfirm()
		{
			SendEmailContainer.SetActive(false);
			ConfirmContainer.SetActive(true);
		}

		public void SetForSendEmail()
		{
			SendEmailContainer.SetActive(true);
			ConfirmContainer.SetActive(false);
		}

		public void ChangePasswordRequestSent(bool wasSent)
		{
			requestSent = wasSent;
		}
	}
}
