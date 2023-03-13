using Beamable.Editor.UI.Components;
using UnityEditor;
using UnityEngine.UIElements;
using static Beamable.Common.Constants;
using static Beamable.Common.Constants.Features.Buss.ThemeManager;

namespace Beamable.Editor.UI.Buss
{
	public class ThemeManager : BeamEditorWindow<ThemeManager>
	{
		private ThemeManagerBreadcrumbsVisualElement _breadcrumbs;
		private ThemeManagerModel _model;
		private NavigationVisualElement _navigationWindow;
		private ScrollView _scrollView;
		private SelectedElementVisualElement _selectedElement;
		private BussStyleListVisualElement _stylesGroup;
		private VisualElement _windowRoot;

		static ThemeManager()
		{
			WindowDefaultConfig = new BeamEditorWindowInitConfig
			{
				Title = MenuItems.Windows.Names.THEME_MANAGER,
				DockPreferenceTypeName = typeof(SceneView).AssemblyQualifiedName,
				FocusOnShow = false,
				RequireLoggedUser = false,
			};
		}

		public override void OnDestroy()
		{
			base.OnDestroy();

			_navigationWindow?.Destroy();
			_selectedElement?.Destroy();
			_model?.Clear();
		}

		[MenuItem(
			MenuItems.Windows.Paths.MENU_ITEM_PATH_WINDOW_BEAMABLE + "/" +
			Commons.OPEN + " " +
			MenuItems.Windows.Names.THEME_MANAGER,
			priority = MenuItems.Windows.Orders.MENU_ITEM_PATH_WINDOW_PRIORITY_2 + 5)]
		public static void Init()
		{
			var inspector = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
			GetWindow<ThemeManager>(MenuItems.Windows.Names.THEME_MANAGER, true, inspector);
		}

		protected override void Build()
		{
			_model = new ThemeManagerModel();

			minSize = ThemeManagerWindowSize;

			VisualElement root = this.GetRootVisualContainer();
			root.Clear();

			VisualTreeAsset uiAsset =
				AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
					$"{BUSS_THEME_MANAGER_PATH}/{nameof(ThemeManager)}.uxml");
			_windowRoot = uiAsset.CloneTree();
			_windowRoot.AddStyleSheet($"{BUSS_THEME_MANAGER_PATH}/{nameof(ThemeManager)}.uss");
			_windowRoot.name = nameof(_windowRoot);
			_windowRoot.TryAddScrollViewAsMainElement();

			VisualElement mainVisualElement = _windowRoot.Q("window-main");

			mainVisualElement.AddStyleSheet($"{BUSS_THEME_MANAGER_PATH}/{nameof(ThemeManager)}.uss");
			mainVisualElement.TryAddScrollViewAsMainElement();

			ThemeManagerActionBarVisualElement actionBar =
				new ThemeManagerActionBarVisualElement(_model.OnAddStyleButtonClicked, _model.OnCopyButtonClicked,
													   _model.ForceRefresh, _model.OnDocsButtonClicked,
													   _model.OnSearch)
				{ name = "actionBar" };

			actionBar.Init();
			mainVisualElement.Add(actionBar);

			_breadcrumbs = new ThemeManagerBreadcrumbsVisualElement(_model) { name = "breadcrumbs" };
			_breadcrumbs.Refresh();
			mainVisualElement.Add(_breadcrumbs);

			VisualElement navigationGroup = new VisualElement { name = "navigationGroup" };
			mainVisualElement.Add(navigationGroup);

			_navigationWindow = new NavigationVisualElement(_model);
			_navigationWindow.Init();
			navigationGroup.Add(_navigationWindow);

			_selectedElement = new SelectedElementVisualElement(_model);
			_selectedElement.Init();
			mainVisualElement.Add(_selectedElement);

			_scrollView = new ScrollView { name = "themeManagerContainerScrollView" };
			_stylesGroup = new BussStyleListVisualElement(_model) { name = "stylesGroup" };
			_stylesGroup.Init();
			_scrollView.Add(_stylesGroup);

			InlineStyleVisualElement inlineStyle = new InlineStyleVisualElement(_model);
			inlineStyle.Init();
			mainVisualElement.Add(inlineStyle);
			mainVisualElement.Add(_scrollView);
			root.Add(_windowRoot);

			Undo.undoRedoPerformed -= HandleUndo;
			Undo.undoRedoPerformed += HandleUndo;

			_model.ForceRefresh();
		}

		void HandleUndo()
		{
			_model.ForceRefresh();
		}
	}
}
