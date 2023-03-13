using Beamable.Common;
using Beamable.Common.Dependencies;
using Beamable.Editor.Login.UI.Components;
using Beamable.Editor.Login.UI.Model;
using Beamable.Editor.UI.Components;
using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants.Features.LoginBase;
using static Beamable.Common.Constants;

namespace Beamable.Editor.Login.UI
{
	public class LoginWindow : EditorWindow
	{

		public static async Promise CheckLogin(params Type[] dockLocations)
		{
			var b = BeamEditorContext.Default;
			await b.InitializePromise;
			if (b.IsAuthenticated)
				return; // short circuit.

			var wnd = Show(dockLocations);
			await wnd.LoginManager.OnComplete;
			wnd.Close();
		}


#if BEAMABLE_DEVELOPER
        [MenuItem(
	        MenuItems.Windows.Paths.MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_BEAMABLE_DEVELOPER + "/" +
	        Commons.OPEN + " " +
	        MenuItems.Windows.Names.LOGIN,
            priority = MenuItems.Windows.Orders.MENU_ITEM_PATH_WINDOW_PRIORITY_2
        )]
#endif
		public static LoginWindow Init()
		{
			// Ensure at most one Beamable ContentManagerWindow exists
			// If exists, rebuild it from scratch (easy refresh mechanism)
			return Show(null);
		}

		public static LoginWindow FocusOrShow()
		{
			if (!IsInstantiated)
			{
				Init();
			}
			return Instance;
		}

		private NewCustomerVisualElement _newCustomerVisualElement;
		private NewUserVisualElement _newUserVisualElement;

		public static LoginWindow Show(params Type[] dockLocations)
		{
			if (dockLocations == null) dockLocations = new Type[] { };
			if (LoginWindow.IsInstantiated)
			{
				LoginWindow.Instance.Close();
				DestroyImmediate(LoginWindow.Instance);
			}

			// Create Beamable ContentManagerWindow and dock it next to Unity Hierarchy Window
			var loginWindow = dockLocations.Length == 0
				? ScriptableObject.CreateInstance<LoginWindow>()
				: GetWindow<LoginWindow>(MenuItems.Windows.Names.LOGIN, true, dockLocations);

			loginWindow.minSize = new Vector2(400, 590);

			if (dockLocations.Length == 0)
			{
				loginWindow.titleContent = new GUIContent(MenuItems.Windows.Names.LOGIN);
				loginWindow.ShowUtility();
				loginWindow.GetRootVisualContainer().AddToClassList("loginRoot");
			}
			else
			{
				loginWindow.Show(true);
			}
			return loginWindow;
		}

		public static LoginWindow Instance { get; private set; }
		public static bool IsInstantiated { get { return Instance != null; } }
		public static bool IsDomainReloaded { get { return Instance != null && Instance.LoginManager?.OnComplete?.IsCompleted == false; } }
		private VisualElement _windowRoot;

		private IDependencyProvider _provider;
		public IDependencyProvider Provider
		{
			get
			{
				if (_provider == null)
				{
					_provider = BeamEditorContext.Default.ServiceScope;
				}

				return _provider;
			}
		}

		public LoginManager LoginManager;
		public LoginModel Model;
		private VisualElement _pageContainer;
		private Label _welcomeMessage;
		private VisualElement _headerElement;

		private void OnEnable()
		{
			// Force refresh to build the initial window
			Refresh();
		}

		private void Refresh()
		{
			if (Instance != null && Instance.LoginManager != null)
			{
				Instance.LoginManager.Destroy();
			}

			Model = new LoginModel();
			Instance = this;
			LoginManager?.Destroy();
			LoginManager = new LoginManager();
			LoginManager.Initialize(Model);

			var root = this.GetRootVisualContainer();
			root.Clear();
			var uiAsset =
				AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{BASE_PATH}/LoginWindow.uxml");
			_windowRoot = uiAsset.CloneTree();
			root.AddStyleSheet($"{BASE_PATH}/LoginWindow.uss");
			_windowRoot.name = nameof(_windowRoot);

			_welcomeMessage = _windowRoot.Q<Label>("welcomeText");
			_welcomeMessage.AddTextWrapStyle();

			_headerElement = _windowRoot.Q<VisualElement>("header");

			var debugPanel = _windowRoot.Q<VisualElement>("debugButtons");
			SetDebugButtons(debugPanel);

			_pageContainer = _windowRoot.Q<VisualElement>("main");
			var _loader = _windowRoot.Q<LoadingIndicatorVisualElement>();
			_loader.SetPromise(LoginManager.InitializedModel, _pageContainer);

			LoginManager.InitializedModel.Then(model => { LoginManager_OnPageChanged(LoginManager.StartElement); });

			LoginManager.OnPageChanged += LoginManager_OnPageChanged;

			root.Add(_windowRoot);
			root.style.flexGrow = 1;

			Label versionLabel = _windowRoot.Q<Label>("versionNumber");
			if (versionLabel != null)
			{
				var version = Provider.GetService<EnvironmentData>().SdkVersion;
				versionLabel.text = version.ToString();
				versionLabel.tooltip = "Beamable version";
			}
		}

		private void LoginManager_OnPageChanged(LoginBaseComponent nextPage)
		{
			if (nextPage == null)
			{
				return;
			}

			if (nextPage.ShowHeader)
			{
				_headerElement.RemoveFromClassList("hidden");
			}
			else
			{
				_headerElement.AddToClassList("hidden");
			}


			_pageContainer.Clear(); // remove old page if it exists.
			_pageContainer.Add(nextPage);
			nextPage.Refresh();
			_welcomeMessage.text = nextPage.GetMessage();
		}

		void SetDebugButtons(VisualElement debugPanel)
		{
#if BEAMABLE_DEVELOPER
            debugPanel.Add(new Button(() => LoginManager.GotoExistingCustomer()) {text = "existing customer"});
            debugPanel.Add(new Button(() => LoginManager.GotoProjectSelectVisualElement()) {text = "switch"});
            debugPanel.Add(new Button(() => LoginManager.GotoLegalCopy()) {text = "legal"});
            debugPanel.Add(new Button(() => LoginManager.GotoForgotPassword()) {text = "forgot"});
            debugPanel.Add(new Button(() => LoginManager.GotoNewCustomer()) {text = "new customer"});
            debugPanel.Add(new Button(() => LoginManager.GotoNewUser()) {text = "new user"});
            debugPanel.Add(new Button(() => LoginManager.GotoNoRole()) {text = "no role"});
            debugPanel.Add(new Button(() => LoginManager.GotoSummary()) {text = "summary"});
#else
			debugPanel.parent.Remove(debugPanel);
#endif
		}
	}
}
