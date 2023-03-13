using Beamable.Common;
using Beamable.Editor.Environment;
using Beamable.Editor.UI.Components;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static Beamable.Common.Constants.Features.Environment;
namespace Beamable.Editor.UI.Environment
{
	public class EnvironmentWindow : BeamEditorWindow<EnvironmentWindow>
	{
		static EnvironmentWindow()
		{
			WindowDefaultConfig = new BeamEditorWindowInitConfig()
			{
				Title = Constants.MenuItems.Windows.Names.ENVIRONMENT,
				DockPreferenceTypeName = null,
				FocusOnShow = true,
				RequireLoggedUser = false,
			};
		}

		[MenuItem(
			Constants.MenuItems.Windows.Paths.MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_ENV,
			priority = Constants.MenuItems.Windows.Orders.MENU_ITEM_PATH_WINDOW_PRIORITY_1
		)]
		public static async void Init() => await GetFullyInitializedWindow();
		public static async void Init(BeamEditorWindowInitConfig initParameters) => await GetFullyInitializedWindow(initParameters);



		private VisualElement _windowRoot;
		private EnvironmentData _data;
		private EnvironmentService _service;
		private TextField _envTextBox;
		private TextField _apiTextBox;
		private TextField _portalApiTextBox;
		private TextField _mongoExpressTextBox;
		private TextField _dockerRegTextBox;
		private TextField _sdkVersionTextBox;
		private Toggle _isVspToggle;
		private PrimaryButtonVisualElement _applyButton;

		protected override void Build()
		{
			position = new Rect(position.x, position.y, 350, 630);
			minSize = new Vector2(350, 500);
			// Refresh if/when the user logs-in or logs-out while this window is open
			ActiveContext.OnUserChange += _ => BuildWithContext();

			_data = ActiveContext.ServiceScope.GetService<EnvironmentData>().Clone();
			_service = ActiveContext.ServiceScope.GetService<EnvironmentService>();

			var root = this.GetRootVisualContainer();
			root.Clear();
			var uiAsset =
				AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{BASE_PATH}/EnvironmentWindow.uxml");
			_windowRoot = uiAsset.CloneTree();
			_windowRoot.AddStyleSheet($"{BASE_PATH}/EnvironmentWindow.uss");
			_windowRoot.name = nameof(_windowRoot);
			_windowRoot.TryAddScrollViewAsMainElement();

			root.Add(_windowRoot);

			var title = root.Q<Label>("title");
			title.AddTextWrapStyle();

			_envTextBox = root.Q<TextField>("env");
			_apiTextBox = root.Q<TextField>("api");
			_portalApiTextBox = root.Q<TextField>("portalApi");
			_mongoExpressTextBox = root.Q<TextField>("mongoExpress");
			_dockerRegTextBox = root.Q<TextField>("dockerReg");
			_sdkVersionTextBox = root.Q<TextField>("sdkVersion");
			_isVspToggle = root.Q<Toggle>("isUnityVsp");


			var validVersion = _sdkVersionTextBox.AddErrorLabel("valid version", version =>
			{
				if (!PackageVersion.TryFromSemanticVersionString(version, out _))
				{
					return "invalid semantic version";
				}

				return null;
			});


			string CheckUrl(string url)
			{
				if (!url.StartsWith("http://") && !url.StartsWith("https://"))
				{
					return "invalid url";
				}

				return null;
			}


			root.Q<GenericButtonVisualElement>("dev").OnClick += OnDevClicked;
			root.Q<GenericButtonVisualElement>("stage").OnClick += OnStagingClicked;
			root.Q<GenericButtonVisualElement>("prod").OnClick += OnProdClicked;
			root.Q<GenericButtonVisualElement>("cancel").OnClick += OnRevertClicked;

			_applyButton = root.Q<PrimaryButtonVisualElement>();
			_applyButton.AddGateKeeper(validVersion,
								_apiTextBox.AddErrorLabel("valid api url", CheckUrl),
								_portalApiTextBox.AddErrorLabel("valid portal url", CheckUrl),
								_mongoExpressTextBox.AddErrorLabel("valid mongo express url", CheckUrl),
								_dockerRegTextBox.AddErrorLabel("valid docker registry url", CheckUrl));
			_applyButton.Button.clickable.clicked += OnApplyClicked;

			SetUIFromData();
		}


		private void OnRevertClicked()
		{
			_service.ClearOverrides();
		}

		private void OnApplyClicked()
		{
			_data = new EnvironmentData(
				_envTextBox.value,
				_apiTextBox.value,
				_portalApiTextBox.value,
				_mongoExpressTextBox.value,
				_dockerRegTextBox.value,
				_isVspToggle.value,
				_sdkVersionTextBox.value
			);
			_service.SetOverrides(_data);
		}

		void SetUIFromData()
		{
			_envTextBox.SetValueWithoutNotify(_data.Environment);
			_apiTextBox.SetValueWithoutNotify(_data.ApiUrl);
			_portalApiTextBox.SetValueWithoutNotify(_data.PortalUrl);
			_mongoExpressTextBox.SetValueWithoutNotify(_data.BeamMongoExpressUrl);
			_dockerRegTextBox.SetValueWithoutNotify(_data.DockerRegistryUrl);
			_sdkVersionTextBox.SetValueWithoutNotify(_data.SdkVersion.ToString());
			_isVspToggle.SetValueWithoutNotify(_data.IsUnityVsp);

			_applyButton.CheckGateKeepers();
		}

		private void OnStagingClicked()
		{
			_data = _service.GetStaging();
			SetUIFromData();
		}

		private void OnDevClicked()
		{
			_data = _service.GetDev();
			SetUIFromData();
		}

		private void OnProdClicked()
		{
			_data = _service.GetProd();
			SetUIFromData();
		}
	}
}
