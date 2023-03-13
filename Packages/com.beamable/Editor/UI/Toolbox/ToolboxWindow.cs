using Beamable.Common;
using Beamable.Editor.Environment;
using Beamable.Editor.Toolbox.Components;
using Beamable.Editor.Toolbox.Models;
using Beamable.Editor.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.VspAttribution.Beamable;
using UnityEngine;
using UnityEngine.UIElements;

using static Beamable.Common.Constants;
using static Beamable.Common.Constants.Features.Toolbox;
using static Beamable.Common.Constants.Features.Toolbox.EditorPrefsKeys;

namespace Beamable.Editor.Toolbox.UI
{
	public class ToolboxWindow : BeamEditorWindow<ToolboxWindow>
	{
		static ToolboxWindow()
		{
			WindowDefaultConfig = new BeamEditorWindowInitConfig()
			{
				Title = MenuItems.Windows.Names.TOOLBOX,
				DockPreferenceTypeName = typeof(SceneView).AssemblyQualifiedName,
				FocusOnShow = false,
				RequireLoggedUser = true,
			};
		}

		[MenuItem(
			MenuItems.Windows.Paths.MENU_ITEM_PATH_WINDOW_BEAMABLE + "/" +
			Commons.OPEN + " " +
			MenuItems.Windows.Names.TOOLBOX,
			priority = MenuItems.Windows.Orders.MENU_ITEM_PATH_WINDOW_PRIORITY_1
		)]
		public static async void Init() => await GetFullyInitializedWindow();
		public static async void Init(BeamEditorWindowInitConfig initParameters) => await GetFullyInitializedWindow(initParameters);


		private VisualElement _windowRoot;

		private ToolboxActionBarVisualElement _actionBarVisualElement;
		private ToolboxBreadcrumbsVisualElement _breadcrumbsVisualElement;

		private ToolboxContentListVisualElement _contentListVisualElement;

		private IToolboxViewService _model;
		private ToolboxAnnouncementListVisualElement _announcementListVisualElement;

		protected override void Build()
		{
#if UNITY_2021_1_OR_NEWER
			// To hide horizontal scroll bar
			minSize = new Vector2(575, 300);
#else
			minSize = new Vector2(550, 300);
#endif

			// Refresh if/when the user logs-in or logs-out while this window is open
			ActiveContext.OnUserChange += _ => BuildWithContext();

			_model = ActiveContext.ServiceScope.GetService<IToolboxViewService>();

			// Force refresh to build the initial window
			_model?.Destroy();

			_model.UseDefaultWidgetSource();
			_model.Initialize();

			SetForContent();

			CheckAnnouncements();
			CheckForDeps();
			CheckForUpdate();
		}

		private void OnDisable()
		{
			BeamablePackageUpdateMeta.OnPackageUpdated -= ShowWhatsNewAnnouncement;
		}

		private void CheckAnnouncements()
		{
			if (BeamableEnvironment.IsUnityVsp) return;
			BeamablePackageUpdateMeta.OnPackageUpdated += ShowWhatsNewAnnouncement;
			BeamablePackages.IsPackageUpdated().Then(isUpdated =>
			{
				if (isUpdated &&
					!EditorPrefs.GetBool(IS_PACKAGE_WHATSNEW_ANNOUNCEMENT_IGNORED, true))
				{
					ShowWhatsNewAnnouncement();
				}
			});
		}

		private void SetForContent()
		{
			var root = this.GetRootVisualContainer();
			root.Clear();
			var uiAsset =
				AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{BASE_PATH}/ToolboxWindow.uxml");
			_windowRoot = uiAsset.CloneTree();
			_windowRoot.AddStyleSheet($"{BASE_PATH}/ToolboxWindow.uss");
			_windowRoot.name = nameof(_windowRoot);
			_windowRoot.TryAddScrollViewAsMainElement();

			root.Add(_windowRoot);

			_actionBarVisualElement = root.Q<ToolboxActionBarVisualElement>("actionBarVisualElement");
			_actionBarVisualElement.Refresh();

			_breadcrumbsVisualElement = root.Q<ToolboxBreadcrumbsVisualElement>("breadcrumbsVisualElement");
			_breadcrumbsVisualElement.Refresh();

			_contentListVisualElement = root.Q<ToolboxContentListVisualElement>("contentListVisualElement");
			_contentListVisualElement.Refresh();

			_announcementListVisualElement = root.Q<ToolboxAnnouncementListVisualElement>();
			_announcementListVisualElement.Refresh();
			_announcementListVisualElement.OnHeightChanged += AnnouncementList_OnHeightChanged;

			_actionBarVisualElement.OnInfoButtonClicked += () =>
			{
				Application.OpenURL(URLs.Documentations.URL_DOC_WINDOW_TOOLBOX);
			};

			CheckForDeps();
		}
		private void AnnouncementList_OnHeightChanged(float height)
		{
			// TODO: animate the height...
			_contentListVisualElement?.style.SetTop(65 + height);
			_contentListVisualElement?.MarkDirtyRepaint();
		}
		private void CheckForDeps()
		{
			if (_model.IsSpecificAnnouncementCurrentlyDisplaying(typeof(WelcomeAnnouncementModel)))
			{
				return;
			}

			if (BeamEditorContext.HasDependencies() || _model.IsSpecificAnnouncementCurrentlyDisplaying(typeof(WelcomeAnnouncementModel)))
				return;

			var descriptionElement = new VisualElement();
			descriptionElement.AddToClassList("announcement-descriptionSection");

			var label = new Label("Welcome to Beamable! This package includes official Unity assets");
			label.AddToClassList("noMarginNoPaddingNoBorder");
			label.AddToClassList("announcement-text");
			label.AddTextWrapStyle();
			descriptionElement.Add(label);

			var button = new Button(() => Application.OpenURL("https://docs.unity3d.com/Manual/com.unity.textmeshpro.html"));
			button.text = "TextMeshPro";
			button.AddToClassList("noMarginNoPaddingNoBorder");
			button.AddToClassList("announcement-hiddenButton");
			descriptionElement.Add(button);

			label = new Label("and");
			label.AddToClassList("noMarginNoPaddingNoBorder");
			label.AddToClassList("announcement-text");
			label.AddTextWrapStyle();
			descriptionElement.Add(label);

			button = new Button(() => Application.OpenURL("https://docs.unity3d.com/Manual/com.unity.addressables.html"));
			button.text = "Addressables";
			button.AddToClassList("noMarginNoPaddingNoBorder");
			button.AddToClassList("announcement-hiddenButton");
			descriptionElement.Add(button);

			label = new Label("in order to provide UI prefabs you can easily drag & drop into your game. To complete the installation, we must add them to your project now.");
			label.AddToClassList("noMarginNoPaddingNoBorder");
			label.AddToClassList("announcement-text");
			label.AddTextWrapStyle();
			descriptionElement.Add(label);

			var welcomeAnnouncement = new WelcomeAnnouncementModel();
			welcomeAnnouncement.DescriptionElement = descriptionElement;

			welcomeAnnouncement.OnImport = () =>
			{
				ActiveContext.CreateDependencies().Then(_ => { _model.RemoveAnnouncement(welcomeAnnouncement); });
			};
			_model.AddAnnouncement(welcomeAnnouncement);

		}
		private void CheckForUpdate()
		{
			if (BeamableEnvironment.IsUnityVsp)
			{
				CheckForUpdateVsp();
			}
			else
			{
				CheckForUpdateRegistry();
			}
		}

		private void CheckForUpdateVsp()
		{
			var lastIgnoredVersionStr = EditorPrefs.GetString(VSP_IGNORED_PACKAGE_VERSION, "0.0.0");
			PackageVersion.TryFromSemanticVersionString(lastIgnoredVersionStr, out var lastIgnoredVersion);

			// use the VSP pathway to check for updates...
			ActiveContext.ServiceScope.GetService<BeamableVsp>().GetLatestVersion().Then(metadata =>
			{
				var currentVersion = BeamableEnvironment.SdkVersion;
				var latestVersion = metadata.version;

				if (lastIgnoredVersion >= latestVersion)
				{
					return; // we've ignored this version.
				}

				if (latestVersion > currentVersion)
				{
					// show the announcement!
					var versionString = latestVersion.ToString();
					var updateAvailableAnnouncement = new UpdateAvailableAnnouncementModel();
					updateAvailableAnnouncement.SetDescription(versionString);

					updateAvailableAnnouncement.OnIgnore += () =>
					{
						EditorPrefs.SetString(VSP_IGNORED_PACKAGE_VERSION, versionString);
						_model.RemoveAnnouncement(updateAvailableAnnouncement);
					};
					updateAvailableAnnouncement.OnWhatsNew += () =>
					{
						BeamablePackages.OpenUrlForVersion(versionString);
					};
					updateAvailableAnnouncement.OnInstall += () =>
					{
						Application.OpenURL(metadata.storeUrl);
						_model.RemoveAnnouncement(updateAvailableAnnouncement);
					};

					_model.AddAnnouncement(updateAvailableAnnouncement);
				}
			});
		}

		private void CheckForUpdateRegistry()
		{
			BeamablePackages.IsPackageUpdated().Then(isUpdated =>
			{
				if (isUpdated || BeamablePackageUpdateMeta.IsInstallationIgnored)
				{
					return;
				}
				if (EditorPrefs.GetBool(IS_PACKAGE_UPDATE_IGNORED))
				{
					BeamablePackageUpdateMeta.IsInstallationIgnored = true;
					return;
				}
				ShowUpdateAvailableAnnouncement();
			});
		}

		private void ShowUpdateAvailableAnnouncement()
		{
			if (_model.IsSpecificAnnouncementCurrentlyDisplaying(typeof(UpdateAvailableAnnouncementModel)))
			{
				return;
			}

			var updateAvailableAnnouncement = new UpdateAvailableAnnouncementModel();
			updateAvailableAnnouncement.SetDescription(BeamablePackageUpdateMeta.NewestVersionNumber);

			updateAvailableAnnouncement.OnWhatsNew = () =>
			{
				BeamablePackages.OpenUrlForVersion(BeamablePackageUpdateMeta.NewestVersionNumber);


			};

			updateAvailableAnnouncement.OnIgnore = () =>
			{
				BeamablePackageUpdateMeta.IsInstallationIgnored = true;
				EditorPrefs.SetBool(IS_PACKAGE_UPDATE_IGNORED, true);
				_model.RemoveAnnouncement(updateAvailableAnnouncement);
			};

			updateAvailableAnnouncement.OnInstall = () =>
			{
				BeamableLogger.Log($"Updating the Beamable package to version=[{BeamablePackageUpdateMeta.NewestVersionNumber}]. It may take a while...");
				BeamablePackages.UpdatePackage().Then(_ =>
				{
					BeamableLogger.Log("The Beamable package update process completed successfully!");
					_model.RemoveAnnouncement(updateAvailableAnnouncement);
				});
			};

			_model.AddAnnouncement(updateAvailableAnnouncement);
		}
		private void ShowWhatsNewAnnouncement()
		{
			if (_model.IsSpecificAnnouncementCurrentlyDisplaying(typeof(WhatsNewAnnouncementModel)))
			{
				return;
			}

			var whatsNewAnnouncement = new WhatsNewAnnouncementModel();

			whatsNewAnnouncement.OnWhatsNew = () =>
			{
				BeamablePackages.OpenUrlForVersion(BeamableEnvironment.SdkVersion);
				_model.RemoveAnnouncement(whatsNewAnnouncement);
			};
			whatsNewAnnouncement.OnIgnore = () =>
			{
				EditorPrefs.SetBool(IS_PACKAGE_WHATSNEW_ANNOUNCEMENT_IGNORED, true);
				_model.RemoveAnnouncement(whatsNewAnnouncement);
			};
			_model.AddAnnouncement(whatsNewAnnouncement);
		}
	}
}
