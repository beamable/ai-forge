using System.ComponentModel;
using Beamable;
using Beamable.AccountManagement;
using Beamable.Common;
using UnityWeld.Binding;

namespace Game.ViewModel
{
    [Binding]
    public class LoginViewModel : BaseViewModel
    {
        public Promise LoginSucceced = new Promise();
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _error = string.Empty;
        private bool _canSend;

        [Binding]
        public string Email
        {
            get => _email;
            set
            {
                if (_email.Equals(value)) return;
                _email = value;
                OnPropertyChanged();
            }
        }

        [Binding]
        public string Password
        {
            get => _password;
            set
            {
                if (_password.Equals(value)) return;
                _password = value;
                OnPropertyChanged();
            }
        }

        [Binding]
        public string Error
        {
            get => _error;
            set
            {
                if (_error.Equals(value)) return;
                _error = value;
                OnPropertyChanged();
            }
        }

        [Binding]
        public bool CanSend
        {
            get => _canSend;
            set
            {
                if (_canSend.Equals(value)) return;
                _canSend = value;
                OnPropertyChanged();
            }
        }

        private void Awake()
        {
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object _, PropertyChangedEventArgs __)
        {
            if (!IsValidEmail(Email))
            {
                Error = "Email is not valid";
                CanSend = false;
            }
            else if (string.IsNullOrWhiteSpace(Password) ||
                     !AccountManagementConfiguration.Instance.Overrides.IsPasswordStrong(Password))
            {
                Error = AccountManagementConfiguration.Instance.Overrides
                    .GetErrorMessage(ErrorEvent.PASSWORD_STRENGTH);
                CanSend = false;
            }
            else
            {
                Error = string.Empty;
                CanSend = true;
            }
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        [Binding]
        public void SkipLogin()
        {
            LoginSucceced.CompleteSuccess();
            gameObject.SetActive(false);
        }

        [Binding]
        public void PerformLogin()
        {
            var beam = BeamContext.InParent(this);
            var api = beam.Api;
            api.IsEmailRegistered(Email).Then(registered =>
            {
                var currentUserHasEmail = api.User.HasDBCredentials();

                var shouldSwitchUser = registered;
                var shouldCreateNewUser = !registered && currentUserHasEmail;
                var shouldAttachToCurrentUser = !registered && !currentUserHasEmail;

                if (shouldSwitchUser)
                {
                    api.AuthService.Login(Email, Password, false).Then(response =>
                    {
                        beam.ChangeAuthorizedPlayer(response).Then(_ =>
                        {
                            LoginSucceced.CompleteSuccess();
                            gameObject.SetActive(false);
                        }).Error(e => Error = e.Message);
                    }).Error(e => Error = e.Message);
                }

                if (shouldCreateNewUser)
                {
                    beam.Accounts.CreateNewAccount().Then(account => account.AddEmail(Email, Password).Then(_ =>
                    {
                        LoginSucceced.CompleteSuccess();
                        gameObject.SetActive(false);
                    }).Error(e => Error = e.Message)).Error(e => Error = e.Message);
                }

                if (shouldAttachToCurrentUser)
                {
                    api.AuthService.RegisterDBCredentials(Email, Password).Then(user =>
                    {
                        api.UpdateUserData(user);
                        LoginSucceced.CompleteSuccess();
                        gameObject.SetActive(false);
                    }).Error(e => Error = e.Message);
                }
            }).Error(e => Error = e.Message);
        }
    }
}