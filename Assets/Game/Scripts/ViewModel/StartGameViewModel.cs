using System;
using System.Linq;
using System.Threading.Tasks;
using Beamable;
using Beamable.Common;
using Beamable.Server.Clients;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;

namespace Game.ViewModel
{
    [Binding]
    public class StartGameViewModel : BaseViewModel
    {
        [SerializeField] private LoginViewModel loginViewModel;
        private bool _loadingEnded = false;
        private string text = "Init";
        private BeamContext _beam;
        private Promise _waitForInternet = Promise.Success;

        [Binding]
        public bool LoadingEnded
        {
            get => _loadingEnded;
            set
            {
                _loadingEnded = value;
                OnPropertyChanged();
            }
        }
        [Binding]
        public string Text
        {
            get => text;
            set
            {
                if (text.Equals(value)) return;
                text = value;
                OnPropertyChanged();
            }
        }

        // Start is called before the first frame update
        async void Start()
        {
            try
            {
                _beam = BeamContext.InParent(this);
                _beam.Api.ConnectivityService.OnConnectivityChanged += HandleConnectivityChanged;
                if (!_beam.Api.ConnectivityService.HasConnectivity)
                {
                    await _waitForInternet;
                }
                
                Text = "Loading Beamable";
                await _beam.OnReady;
                await _beam.Accounts.OnReady;
                if (!_beam.Accounts.Current.HasEmail)
                {
                    loginViewModel.gameObject.SetActive(true);
                    await loginViewModel.LoginSucceced;
                }

                Text = "Attaching AI identity";
                if (!_beam.Accounts.Current.ExternalIdentities.Any())
                {
                    await _beam.Accounts.AddExternalIdentity<AiCloudIdentity, AIMicroserviceClient>(_beam.Accounts
                        .Current
                        .GamerTag.ToString());
                }

                _beam.Api.ConnectivityService.OnConnectivityChanged -= HandleConnectivityChanged;
                LoadingEnded = true;
            }
            catch (Exception e)
            {
                Text = $"Reloading scene because of: {e.Message}";
                await Task.Delay(3500);
                SceneManager.LoadScene(0, LoadSceneMode.Single);
            }
        }

        private void HandleConnectivityChanged(bool connected)
        {
            if (!connected)
            {
                Text = "Cannot connect to Beamable backend";
                _waitForInternet = new Promise();
            }
            else
            {
                Text = "Connected to Beamable backend";
                _waitForInternet.CompleteSuccess();
            }
        }

        [Binding]
        public void StartGameClicked()
        {
            SceneManager.LoadScene("ForgeScene", LoadSceneMode.Single);
        }
    }
}