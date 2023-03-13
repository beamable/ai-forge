namespace Beamable.Common
{
	public static partial class Constants
	{
		public static partial class Features
		{
			public static partial class LoginBase
			{
				public const string BASE_PATH = Directories.BEAMABLE_PACKAGE_EDITOR_UI + "/Login/UI";
				public const string COMPONENTS_PATH = BASE_PATH + "/Components";

				public const string PLACEHOLDER_ALIAS_FIELD = "Enter A Customer Alias";
				public const string PLACEHOLDER_CID_FIELD = "Enter Your Organization's Alias or ID";
				public const string PLACEHOLDER_EMAIL_FIELD = "Enter Email";
				public const string PLACEHOLDER_PASSWORD_FIELD = "Enter Password";
				public const string PLACEHOLDER_PASSWORD_CONFIRM_FIELD = "Confirm Password";
				public const string PLACEHOLDER_GAMENAME_FIELD = "Enter Your Game's Name";
				public const string PLACEHOLDER_CODE_FIELD = "Enter Your Code";
				public const string PLACEHOLDER_CUSTOMENAME_FIELD = "Enter Your Organization's Name";

				public const string EXCEPTION_TYPE_NOCID = "RequestContextNotFoundExceptionForAlias";
				public const string EXCEPTION_TYPE_EMAIL_TAKEN = "EmailAlreadyRegisteredError";
				public const string EXCEPTION_TYPE_BAD_GAME_NAME = "InvalidProjectName";
				public const string EXCEPTION_TYPE_BAD_ALIAS = "AliasInvalid";
				public const string EXCEPTION_TYPE_BADCODE = "InvalidConfirmationCodeError";
				public const string EXCEPTION_TYPE_INVALID_SCOPE = "InvalidScopeError";

				public const string UNKNOWN_ERROR = "An unknown error occured. Please try again soon.";
				public const string CUSTOMER_CREATION_UNKNOWN_ERROR = "An unknown error occured. Please check the Unity console for more information.";
				public const string INVALID_CREDENTIALS_ERROR = "Your email or password is incorrect.";
				public const string NO_ALIAS_FOUND_ERROR = "No organization exists for the given CID/alias.";
				public const string BAD_CODE_ERROR = "The code you entered does not match the password reset code we sent to your email.";
				public const string BAD_GAME_NAME_ERROR = "The game name is invalid. Please try a different game name.";
				public const string BAD_ALIAS_ERROR =
					"The alias is invalid. It can only contain lower case letters, numbers, and dashes.";
				public const string EMAIL_TAKEN_ERROR = "Your chosen email address is already in use by a different account.";
				public const string NO_ACCOUNT_FOUND_ERROR = "No account exists for the given email.";
				public const string CID_TAKEN_ERROR =
					"Your chosen organization alias has already been reserved by a different organization.";
			}
		}
	}
}
