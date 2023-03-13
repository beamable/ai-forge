using Beamable.AccountManagement;
using Beamable.Theme;
using System;
using TMPro;
using UnityEngine;

namespace Beamable.UI.Scripts
{
	public enum InputValidationType
	{
		NONE, EMAIL, PASSWORD, GUID, ALIAS
	}

	public class InputValidationBehaviour : MonoBehaviour
	{
		public InputReference InputReference;

		public bool InheritValidationTypeFromReference = true;
		public InputValidationType ValidationType;
		public StyleBehaviour InvalidStyle, ValidStyle;
		private InputValidationType _validationType;

		public bool CheckOnEnable = true;
		public bool IsValid { get; private set; }

		private bool _wasValid, _isFirst = true;

		void OnEnable()
		{
			_validationType = GetValidationType();
			InputReference.Field.onValueChanged.AddListener(DoValidation);

			if (CheckOnEnable)
			{
				DoValidation(InputReference.Value);
			}
			else
			{
				foreach (var image in ValidStyle.StyledImages.Components)
				{
					image.gameObject.SetActive(false);
				}
			}
		}

		private void OnDisable()
		{
			InputReference.Field.onValueChanged.RemoveListener(DoValidation);
		}

		private void OnDestroy()
		{
			InputReference.Field.onValueChanged.RemoveListener(DoValidation);
		}

		void DoValidation(string value)
		{
			IsValid = CheckValidation(InputReference.Value);

			if (IsValid != _wasValid || _isFirst)
			{
				SetStyle(IsValid);
				_isFirst = false;
			}

			_wasValid = IsValid;
		}

		void SetStyle(bool valid)
		{
			if (ValidStyle == null) return;
			if (ValidStyle.StyledImages == null) return;

			foreach (var image in ValidStyle.StyledImages.Components)
			{
				image.gameObject.SetActive(true);
			}
			ValidStyle.enabled = valid;
			InvalidStyle.enabled = !valid;
		}

		bool CheckValidation(string value)
		{
			switch (_validationType)
			{
				case InputValidationType.EMAIL:
					return CheckEmail(value);
				case InputValidationType.PASSWORD:
					return CheckPassword(value);
				case InputValidationType.GUID:
					return CheckGuid(value);
				case InputValidationType.ALIAS:
					return CheckAlias(value);
				default:
					return true;
			}
		}

		protected virtual bool CheckGuid(string value)
		{
			return Guid.TryParse(value, out var guid);
		}

		protected virtual bool CheckEmail(string value)
		{
			try
			{
				var addr = new System.Net.Mail.MailAddress(value);
				return addr.Address == value;
			}
			catch (SystemException)
			{
				return false;
			}
		}

		protected virtual bool CheckPassword(string value)
		{
			var valid = AccountManagementConfiguration.Instance.Overrides.IsPasswordStrong(value);
			return valid;
		}

		protected virtual bool CheckAlias(string value)
		{
			return !string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value);
		}

		InputValidationType GetValidationType()
		{
			if (!InheritValidationTypeFromReference)
			{
				return ValidationType;
			}


			if (InputReference == null || InputReference.Field == null)
			{
				return InputValidationType.NONE;
			}

			switch (InputReference.Field.contentType)
			{
				case TMP_InputField.ContentType.EmailAddress:
					return InputValidationType.EMAIL;
				case TMP_InputField.ContentType.Password:
					return InputValidationType.PASSWORD;
				default:
					return InputValidationType.NONE;
			}
		}

	}
}
