using Beamable.Common.Api.Auth;
using Beamable.Common.Api.Realms;
using Beamable.Common.Content;
using Beamable.Editor.Content.Components;
using Beamable.Editor.Content.Models;
using Beamable.Editor.Login.UI;
using Beamable.Editor.NoUser;
using Beamable.Editor.UI;
using Beamable.Editor.UI.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#endif
using static Beamable.Common.Constants;
using static Beamable.Common.Constants.Features.ContentManager;

namespace Beamable.Editor.Content
{
	public class ContentManagerWindow : BeamEditorWindow<ContentManagerWindow>
	{
		static ContentManagerWindow()
		{
			WindowDefaultConfig = new BeamEditorWindowInitConfig()
			{
				Title = MenuItems.Windows.Names.CONTENT_MANAGER,
				DockPreferenceTypeName = typeof(SceneView).AssemblyQualifiedName,
				FocusOnShow = true,
				RequireLoggedUser = true,
			};
		}

		[MenuItem(
		MenuItems.Windows.Paths.MENU_ITEM_PATH_WINDOW_BEAMABLE + "/" +
		Commons.OPEN + " " +
		MenuItems.Windows.Names.CONTENT_MANAGER,
		priority = MenuItems.Windows.Orders.MENU_ITEM_PATH_WINDOW_PRIORITY_2
		)]
		public static async Task Init() => await GetFullyInitializedWindow();

		private ContentManager _contentManager;
		private VisualElement _windowRoot;
		private VisualElement _explorerContainer, _statusBarContainer;

		private ActionBarVisualElement _actionBarVisualElement;
		private ExplorerVisualElement _explorerElement;
		private StatusBarVisualElement _statusBarElement;
		private BeamablePopupWindow _currentWindow;

		private List<string> _cachedItemsToDownload;
		private bool _cachedCreateNewManifestFlag;

		[SerializeField]
		private ContentIO.MapOfValidationChecksums _checksumsChache;

		private void Update()
		{
			if (ActiveContext == null) return;

			_actionBarVisualElement?.RefreshPublishDropdownVisibility();
			_statusBarElement?.RefreshStatus();
		}


		protected override void Build()
		{
			// Refresh if/when the user logs-in or logs-out while this window is open
			ActiveContext.OnUserChange += HandleUserChange;
			ActiveContext.OnRealmChange += HandleRealmChange;
			ContentIO.OnManifestChanged += OnManifestChanged;

			minSize = new Vector2(600, 300);

			Refresh();
		}

		private void OnDisable()
		{
			if (ActiveContext == null) return;

			ActiveContext.OnUserChange -= HandleUserChange;
			ActiveContext.OnRealmChange -= HandleRealmChange;
			ContentIO.OnManifestChanged -= OnManifestChanged;
		}

		private void HandleRealmChange(RealmView realm) => EditorApplication.delayCall += Refresh;
		private void HandleUserChange(User user) => Refresh();
		private void OnManifestChanged(string manifestId) => SoftReset();

		public void Refresh()
		{
			_contentManager?.Destroy();
			_contentManager = new ContentManager();
			_contentManager.Initialize();

			var root = this.GetRootVisualContainer();

			root.Clear();
			var uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{BASE_PATH}/ContentManagerWindow.uxml");
			_windowRoot = uiAsset.CloneTree();
			_windowRoot.AddStyleSheet($"{BASE_PATH}/ContentManagerWindow.uss");
			_windowRoot.name = nameof(_windowRoot);
			_windowRoot.TryAddScrollViewAsMainElement();

			root.Add(_windowRoot);

			_actionBarVisualElement = root.Q<ActionBarVisualElement>("actionBarVisualElement");
			_actionBarVisualElement.Model = _contentManager.Model;
			_actionBarVisualElement.Refresh();

			// Handlers for Buttons (Left To Right in UX)
			_actionBarVisualElement.OnAddItemButtonClicked += (typeDescriptor) =>
			{
				_contentManager.AddItem(typeDescriptor);
			};

			_actionBarVisualElement.OnValidateButtonClicked += () =>
			{

				if (_currentWindow != null)
				{
					_currentWindow.Close();
				}

				_currentWindow = BeamablePopupWindow.ShowUtility(ActionNames.VALIDATE_CONTENT, GetValidateContentVisualElement(), this,
																 WindowSizeMinimum, async (window) =>
																 {
																	 // trigger after Unity domain reload
																	 var contentManagerWindow = await GetFullyInitializedWindow();
																	 window?.SwapContent(contentManagerWindow.GetValidateContentVisualElement());
																 });

				_currentWindow.minSize = WindowSizeMinimum;
			};

			_actionBarVisualElement.OnPublishButtonClicked += (createNew) =>
			{
				if (_currentWindow != null)
				{
					_currentWindow.Close();
				}

				// validate and create publish set.

				_cachedCreateNewManifestFlag = createNew;

				_currentWindow = BeamablePopupWindow.ShowUtility(ActionNames.VALIDATE_CONTENT, GetValidateContentVisualElementWithPublish(), this,
																 WindowSizeMinimum, async (window) =>
																 {
																	 // trigger after Unity domain reload
																	 var contentManagerWindow = await GetFullyInitializedWindow();
																	 window?.SwapContent(contentManagerWindow.GetValidateContentVisualElementWithPublish());
																 });

				_currentWindow.minSize = WindowSizeMinimum;

				if (_cachedCreateNewManifestFlag)
				{
					_currentWindow.minSize = new Vector2(_currentWindow.minSize.x, _currentWindow.minSize.y + 100);
				}
			};

			_actionBarVisualElement.OnDownloadButtonClicked += () =>
			{
				if (_currentWindow != null)
				{
					_currentWindow.Close();
				}

				_cachedItemsToDownload = null;
				_currentWindow = BeamablePopupWindow.ShowUtility(ActionNames.DOWNLOAD_CONTENT, GetDownloadContentVisualElement(), this,
																 WindowSizeMinimum, async (window) =>
																 {
																	 // trigger after Unity domain reload
																	 var contentManagerWindow = await GetFullyInitializedWindow();
																	 window?.SwapContent(contentManagerWindow.GetDownloadContentVisualElement());
																 });
				_currentWindow.minSize = WindowSizeMinimum;
			};

			_actionBarVisualElement.OnRefreshButtonClicked += () =>
			{
				_contentManager.RefreshWindow(true);
			};

			_actionBarVisualElement.OnDocsButtonClicked += () =>
			{
				_contentManager.ShowDocs();
			};

			_explorerContainer = root.Q<VisualElement>("explorer-container");
			_statusBarContainer = root.Q<VisualElement>("status-bar-container");

			_explorerElement = new ExplorerVisualElement();
			_explorerContainer.Add(_explorerElement);
			_explorerElement.OnAddItemButtonClicked += ExplorerElement_OnAddItemButtonClicked;
			_explorerElement.OnAddItemRequested += ExplorerElement_OnAddItem;
			_explorerElement.OnItemDownloadRequested += ExplorerElement_OnDownloadItem;
			_explorerElement.OnRenameItemRequested += ExplorerElement_OnItemRename;

			_explorerElement.Model = _contentManager.Model;
			_explorerElement.Refresh();

			_statusBarElement = new StatusBarVisualElement();
			_statusBarElement.Model = _contentManager.Model;
			_statusBarContainer.Add(_statusBarElement);
			_statusBarElement.Refresh();
		}

		public void SetCurrentWindow(BeamablePopupWindow window)
		{
			_currentWindow = window;
		}

		public void CloseCurrentWindow()
		{
			if (_currentWindow != null)
			{
				_currentWindow.Close();
				_currentWindow = null;
			}
		}

		public void SoftReset()
		{
			_contentManager.Model.TriggerSoftReset();
		}

		public override void OnBeforeSerialize()
		{
			_checksumsChache = ContentIO.GetCheckSumTable();
			_checksumsChache.OnBeforeSerialize();
			base.OnBeforeSerialize();
		}

		public override void OnAfterDeserialize()
		{
			_checksumsChache.OnAfterDeserialize();
			ContentIO.SetCheckSumTable(_checksumsChache);
			base.OnAfterDeserialize();
		}

		private void ExplorerElement_OnAddItemButtonClicked()
		{
			var newContent = _contentManager.AddItem();
			EditorApplication.delayCall += () =>
			{
				if (_contentManager.Model.GetDescriptorForId(newContent.Id, out var item))
				{
					item.ForceRename();
				}
			};
		}

		private void ExplorerElement_OnItemRename(ContentItemDescriptor contentItemDescriptor)
		{
			EditorApplication.delayCall += () =>
			{
				if (_contentManager.Model.GetDescriptorForId(contentItemDescriptor.Id, out var item))
				{
					item.ForceRename();
				}
			};
		}

		private void ExplorerElement_OnAddItem(ContentTypeDescriptor type)
		{
			var newContent = _contentManager.AddItem(type);
			EditorApplication.delayCall += () =>
			{
				if (_contentManager.Model.GetDescriptorForId(newContent.Id, out var item))
				{
					item.ForceRename();
				}
			};
		}

		private void ExplorerElement_OnDownloadItem(List<ContentItemDescriptor> items)
		{
			if (_currentWindow != null)
			{
				_currentWindow.Close();
			}

			_cachedItemsToDownload = items.Select(x => x.Id).ToList();
			_currentWindow = BeamablePopupWindow.ShowUtility(ActionNames.DOWNLOAD_CONTENT, GetDownloadContentVisualElement(), this,
			WindowSizeMinimum, async (window) =>
			{
				// trigger after Unity domain reload
				var contentManagerWindow = await GetFullyInitializedWindow();
				window?.SwapContent(contentManagerWindow.GetDownloadContentVisualElement());
				window?.FitToContent();

			}).FitToContent();

			_currentWindow.minSize = WindowSizeMinimum;
		}


		DownloadContentVisualElement GetDownloadContentVisualElement()
		{
			var downloadPopup = new DownloadContentVisualElement();

			if (_cachedItemsToDownload != null && _cachedItemsToDownload.Count > 0)
			{
				downloadPopup.Model = _contentManager.PrepareDownloadSummary(_cachedItemsToDownload.ToArray());
			}
			else
			{
				downloadPopup.Model = _contentManager.PrepareDownloadSummary();
			}

			downloadPopup.OnRefreshContentManager += () => _contentManager.RefreshWindow(true);
			downloadPopup.OnClosed += () =>
			{
				_currentWindow.Close();
				_currentWindow = null;
			};

			downloadPopup.OnCancelled += () =>
			{
				_currentWindow.Close();
				_currentWindow = null;
			};

			downloadPopup.OnDownloadStarted += (summary, prog, finished) =>
			{
				_contentManager?.DownloadContent(summary, prog, finished).Then(_ => SoftReset());
			};

			return downloadPopup;
		}

		ResetContentVisualElement GetResetContentVisualElement()
		{
			var clearPopup = new ResetContentVisualElement();
			clearPopup.Model = _contentManager.PrepareDownloadSummary();
			clearPopup.DataModel = _contentManager.Model;

			clearPopup.OnRefreshContentManager += () => _contentManager.RefreshWindow(true);
			clearPopup.OnClosed += () =>
			{
				_currentWindow.Close();
				_currentWindow = null;
			};

			clearPopup.OnCancelled += () =>
			{
				_currentWindow.Close();
				_currentWindow = null;
			};

			clearPopup.OnDownloadStarted += (summary, prog, finished) =>
			{
				_contentManager?.DownloadContent(summary, prog, finished).Then(_ =>
				{
					_contentManager?.Model.TriggerSoftReset();
				});
			};

			return clearPopup;
		}

		ValidateContentVisualElement GetValidateContentVisualElement()
		{
			var validatePopup = new ValidateContentVisualElement();
			validatePopup.DataModel = _contentManager.Model;

			validatePopup.OnCancelled += () =>
			{
				_currentWindow.Close();
				_currentWindow = null;
			};

			validatePopup.OnClosed += () =>
			{
				_currentWindow.Close();
				_currentWindow = null;
			};

			EditorApplication.delayCall += () =>
			{
				_contentManager?.ValidateContent(validatePopup.SetProgress, validatePopup.HandleValidationErrors)
			   .Then(_ => validatePopup.HandleFinished());
			};

			return validatePopup;
		}

		ValidateContentVisualElement GetValidateContentVisualElementWithPublish()
		{
			var validatePopup = new ValidateContentVisualElement();
			validatePopup.DataModel = _contentManager.Model;

			validatePopup.OnCancelled += () =>
			{
				_currentWindow.Close();
				_currentWindow = null;
			};

			validatePopup.OnClosed += () =>
			{
				_currentWindow.Close();
				_currentWindow = null;
			};

			EditorApplication.delayCall += () =>
			{
				_contentManager.ValidateContent(validatePopup.SetProgress, validatePopup.HandleValidationErrors)
					.Then(errors =>
					{
						validatePopup.HandleFinished();

						if (errors.Count != 0) return;

						_currentWindow.SwapContent(GetPublishContentVisualElement(), async (window) =>
						{
							// trigger after domain reload
							var contentManagerWindow = await GetFullyInitializedWindow();
							window?.SwapContent(contentManagerWindow.GetPublishContentVisualElement());
						});

						_currentWindow.titleContent = new GUIContent(ActionNames.PUBLISH_CONTENT);
					});
			};

			return validatePopup;
		}

		PublishContentVisualElement GetPublishContentVisualElement()
		{
			var publishPopup = new PublishContentVisualElement();
			publishPopup.CreateNewManifest = _cachedCreateNewManifestFlag;
			publishPopup.DataModel = _contentManager.Model;
			publishPopup.PublishSet = _contentManager.CreatePublishSet(_cachedCreateNewManifestFlag);

			publishPopup.OnCancelled += () =>
			{
				_currentWindow.Close();
				_currentWindow = null;
			};

			publishPopup.OnCompleted += () =>
			{
				_currentWindow.Close();
				_currentWindow = null;
			};

			bool createNewManifest = _cachedCreateNewManifestFlag;

			publishPopup.OnPublishRequested += (set, prog, finished) =>
			{
				if (createNewManifest)
				{
					ActiveContext.ContentIO.SwitchManifest(publishPopup.ManifestName).Then(_ =>
					{
						set.ManifestId = publishPopup.ManifestName;
						_contentManager?.PublishContent(set, prog, finished).Then(__ => SoftReset());
					});

				}
				else
				{
					_contentManager?.PublishContent(set, prog, finished).Then(_ => SoftReset());
				}
			};

			return publishPopup;
		}


		[MenuItem(MenuItems.Windows.Paths.MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES + "/Reset Content")]
		public static async Task ResetContent()
		{
			var contentManagerWindow = await GetFullyInitializedWindow();
			contentManagerWindow._currentWindow?.Close();
			contentManagerWindow._currentWindow = null;

			contentManagerWindow._currentWindow = BeamablePopupWindow.ShowUtility(ActionNames.REMOVE_LOCAL_CONTENT, contentManagerWindow.GetResetContentVisualElement(), null,
			WindowSizeMinimum, async (window) =>
			{
				// trigger after Unity domain reload
				var contentWindow = await GetFullyInitializedWindow();
				window?.SwapContent(contentWindow.GetResetContentVisualElement());
				window?.FitToContent();

			});

			contentManagerWindow._currentWindow.minSize = WindowSizeMinimum;
		}
	}
}
