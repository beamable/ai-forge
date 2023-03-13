using Beamable.Common;
using Beamable.Editor;
using Beamable.Editor.Microservice.UI.Components;
using Beamable.Editor.UI.Components;
using Beamable.Server.Editor.DockerCommands;
using System;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Server.Editor.UI.Components.DockerLoginWindow
{
	public class DockerLoginVisualElement : MicroserviceComponent
	{
		private static BeamablePopupWindow _window;
		public static Promise<Unit> ShowUtility(EditorWindow parent = null)
		{
			if (_window)
			{
				_window.Close();
			}
			var elem = new DockerLoginVisualElement();
			_window = BeamablePopupWindow.ShowUtility("DockerHub Login", elem, parent, new Vector2(100, 250));
			_window.OnClosing += () => elem?._promise?.CompleteError(new Exception("Window Closed"));
			elem._promise.Then(_ => _window.Close());

			return elem._promise;
		}

		private Promise<Unit> _promise = new Promise<Unit>();
		private PrimaryButtonVisualElement _loginBtn;
		private TextField _usernameInput;
		private TextField _passwordInput;
		private Label _errorLbl;

		public DockerLoginVisualElement() : base(nameof(DockerLoginVisualElement))
		{
		}

		public override void Refresh()
		{
			base.Refresh();


			var aboutLbl = Root.Q<Label>("title");
			aboutLbl.AddTextWrapStyle();

			var helpLbl = Root.Q<Label>("help");
			helpLbl.AddTextWrapStyle();
			helpLbl.AddManipulator(new Clickable(() => Application.OpenURL("https://id.docker.com/reset-password/")));

			_usernameInput = Root.Q<TextField>("username");
			_usernameInput.AddPlaceholder("DockerHub username");

			_passwordInput = Root.Q<TextField>("password");
			_passwordInput.AddPlaceholder("DockerHub password");
			_passwordInput.isPasswordField = true;

			_errorLbl = Root.Q<Label>("error");

			_loginBtn = Root.Q<PrimaryButtonVisualElement>();
			_loginBtn.AddGateKeeper(_usernameInput.AddErrorLabel("Username", PrimaryButtonVisualElement.ExistErrorHandler));
			_loginBtn.AddGateKeeper(_passwordInput.AddErrorLabel("Password", PrimaryButtonVisualElement.ExistErrorHandler));

			_loginBtn.Button.clickable.clicked += OnLoginClicked;
		}

		private void OnLoginClicked()
		{
			var command = new DockerLoginCommand(_usernameInput.value, _passwordInput.value);
			var loginPromise = command.StartAsync();

			_loginBtn.SetText("Logging In");
			_loginBtn.Load(loginPromise);

			loginPromise.Then(isLoggedIn =>
			{
				_errorLbl.text = "";
				if (!isLoggedIn)
				{
					_errorLbl.text = "Username or Password was incorrect.";
				}
				else
				{
					_promise.CompleteSuccess(PromiseBase.Unit);

				}
			});
		}
	}
}
