using Beamable.Common.Assistant;
using Beamable.Common.Reflection;
using Beamable.Editor.Content.Components;
using Beamable.Editor.Reflection;
using Beamable.Editor.ToolbarExtender;
using Beamable.Editor.UI;
using Beamable.Editor.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static Beamable.Common.Constants.Features.Assistant;

#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants;

namespace Beamable.Editor.Assistant
{
	/// <summary>
	/// Handles the rendering and initialization of the <see cref="BeamHint"/>s system as well as any other future system tied to the Beamable Assistant.
	/// </summary>
	public class BeamableAssistantWindow : BeamEditorWindow<BeamableAssistantWindow>
	{
		static BeamableAssistantWindow()
		{
			WindowDefaultConfig = new BeamEditorWindowInitConfig()
			{
				Title = MenuItems.Windows.Names.BEAMABLE_ASSISTANT,
				FocusOnShow = true,
				RequireLoggedUser = false,
				DockPreferenceTypeName = typeof(SceneView).AssemblyQualifiedName
			};
		}

		[MenuItem(MenuItems.Windows.Paths.MENU_ITEM_PATH_WINDOW_BEAMABLE + "/" +
				  Commons.OPEN + " " +
				  MenuItems.Windows.Names.BEAMABLE_ASSISTANT,
				  priority = MenuItems.Windows.Orders.MENU_ITEM_PATH_WINDOW_PRIORITY_2)]
		public static async Task<BeamableAssistantWindow> Init() => await GetFullyInitializedWindow();

		private readonly Vector2 MIN_SIZE = new Vector2(450, 200);

		private VisualElement _windowRoot;

		// Assistant Work Area
		private VisualElement _assistantContainer;

		// Beam Hints VisualElements
		private VisualElement _domainTreeContainer;
		private VisualElement _hintsContainer;
		private IMGUIContainer _imguiContainer;
		private SearchBarVisualElement _hintsSearchBar;
		private TreeViewIMGUI _treeViewIMGUI;
		[SerializeField] private TreeViewState _treeViewState;

		/// <summary>
		/// <see cref="BeamHintsDataModel"/> managing the state that is being rendered by this window.
		/// </summary>
		[SerializeField]
		private BeamHintsDataModel _beamHintsDataModel;

		protected override void Build()
		{
			Refresh();
		}

		private void OnFocus()
		{
			BeamEditor.DelayedInitializationCall(RunFocus, true);
			void RunFocus()
			{
				if (_windowRoot != null && ActiveContext?.ServiceScope != null) FillDisplayingBeamHints(_hintsContainer, _beamHintsDataModel.DisplayingHints);
				else Refresh();
				// TODO: Display NEW icon and clear notifications on hover on a per hint header basis.
				// For now, just clear notifications whenever the window is focused
				var hintNotificationManager = ActiveContext?.ServiceScope?.GetService<BeamHintNotificationManager>();
				hintNotificationManager?.ClearPendingNotifications();
			}
		}

		private void Update()
		{
			if (ActiveContext == null) return;

			var hintNotificationManager = ActiveContext.ServiceScope.GetService<BeamHintNotificationManager>();

			// If there are any new notifications, we refresh to get the new data rendered.
			if ((hintNotificationManager.AllPendingNotifications.Any()) || _beamHintsDataModel.RefreshDisplayingHints())
			{
				FillTreeViewFromDomains(_treeViewIMGUI, _beamHintsDataModel.SortedDomainsInStorage, _beamHintsDataModel.SelectedDomains);
				FillDisplayingBeamHints(_hintsContainer, _beamHintsDataModel.DisplayingHints);
				hintNotificationManager.ClearPendingNotifications();
				_windowRoot.MarkDirtyRepaint();
#if !DISABLE_BEAMABLE_TOOLBAR_EXTENDER
				BeamableToolbarExtender.Repaint();
#endif
			}
		}

		void Refresh()
		{
			minSize = MIN_SIZE;

			// Initialize a data model if we didn't deserialize one already.
			var beamHintsDataModel = _beamHintsDataModel = _beamHintsDataModel ?? new BeamHintsDataModel();
			beamHintsDataModel.AppendGlobalStorage(BeamEditor.HintGlobalStorage);
			beamHintsDataModel.SetPreferencesManager(BeamEditor.HintPreferencesManager);

			var root = this.GetRootVisualContainer();
			root.Clear();
			var uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{BASE_PATH}/BeamableAssistantWindow.uxml");
			_windowRoot = uiAsset.CloneTree();
			_windowRoot.AddStyleSheet($"{BASE_PATH}/BeamableAssistantWindow.uss");
			_windowRoot.name = nameof(_windowRoot);

			root.Add(_windowRoot);

			// Setup Assistant Visuals
			{
				_assistantContainer = root.Q<VisualElement>("assistant-container");
			}

			// Setup Beam Hints Mode Visuals
			{
				// Setup callback for clearing domain tree selection.
				_windowRoot.Q("domain-tree-scroll").RegisterCallback(new EventCallback<MouseUpEvent>(evt =>
				{
					// Clears tree selection
					_treeViewState.selectedIDs.Clear();
					_treeViewIMGUI.Repaint();

					// Select all domains.
					_beamHintsDataModel.SelectDomains(new List<string>());
					FillDisplayingBeamHints(_hintsContainer, beamHintsDataModel.DisplayingHints);
				}));

				//Create IMGUI, The VisualElement Wrapper, and add to the parent
				_treeViewState = _treeViewState ?? new TreeViewState();
				_treeViewIMGUI = new TreeViewIMGUI(_treeViewState) { SelectionType = SelectionType.Multiple, TreeViewItemRoot = new TreeViewItem { id = 0, depth = -1, displayName = "Root" } };
				_imguiContainer = new IMGUIContainer(() =>
				{
					// Necessary as in a re-import all flow with this window opened this will throw for some reason
					if (_treeViewIMGUI?.TreeViewItems?.Count > 0)
					{
						// Tree view - Re-render every frame
						Rect rect = GUILayoutUtility.GetRect(200,
															 200,
															 _treeViewIMGUI.GetCalculatedHeight(),
															 _treeViewIMGUI.GetCalculatedHeight());

						_treeViewIMGUI.OnGUI(rect);
					}
				})
				{ name = "domain-tree-imgui" };
				_domainTreeContainer = root.Q<VisualElement>("domain-tree-container");
				_domainTreeContainer.Add(_imguiContainer);

				// Get Hints View
				_hintsContainer = root.Q<VisualElement>("hints-container");

				// Setup Search Bar to filter Displaying Hints
				_hintsSearchBar = root.Q<SearchBarVisualElement>("hintsSearchBar");
				void OnSearchTextUpdated(string searchText)
				{
					_beamHintsDataModel.FilterDisplayedBy(searchText);
					FillDisplayingBeamHints(_hintsContainer, beamHintsDataModel.DisplayingHints);
				}
				_hintsSearchBar.OnSearchChanged -= OnSearchTextUpdated;
				_hintsSearchBar.OnSearchChanged += OnSearchTextUpdated;
				_hintsSearchBar.SetValueWithoutNotify(_beamHintsDataModel.CurrentFilter);

				SetupTreeViewCallbacks(
					_treeViewIMGUI,
					() => { },
					list =>
					{
						var allDomains = list
										 .SelectMany(a => a.children != null ? a.children.Cast<BeamHintDomainTreeViewItem>() : new List<BeamHintDomainTreeViewItem>())
										 .Concat(list.Cast<BeamHintDomainTreeViewItem>())
										 .Select(item => item.FullDomain).ToList();

						beamHintsDataModel.SelectDomains(allDomains);
						FillDisplayingBeamHints(_hintsContainer, beamHintsDataModel.DisplayingHints);
					},
					list => { });

				beamHintsDataModel.SelectDomains(beamHintsDataModel.SelectedDomains);
				FillTreeViewFromDomains(_treeViewIMGUI, beamHintsDataModel.SortedDomainsInStorage, beamHintsDataModel.SelectedDomains);
				FillDisplayingBeamHints(_hintsContainer, beamHintsDataModel.DisplayingHints);
			}
		}

		/// <summary>
		/// Inject a <see cref="VisualElement"/> with new <see cref="BeamHintHeaderVisualElement"/>s for each given <see cref="hintHeaders"/>.
		/// </summary>
		public void FillDisplayingBeamHints(VisualElement container, List<BeamHintHeader> hintHeaders)
		{
			var hintDetailsReflectionCache = ActiveContext?.ServiceScope?.GetService<ReflectionCache>()?.GetFirstSystemOfType<BeamHintReflectionCache.Registry>();
			if (hintDetailsReflectionCache == null)
				return;

			container.Clear();

			for (var headerIdx = 0; headerIdx < hintHeaders.Count; headerIdx++)
			{
				var beamHintHeader = hintHeaders[headerIdx];
				var hintVisualElement = new BeamHintHeaderVisualElement(_beamHintsDataModel, hintDetailsReflectionCache, beamHintHeader, headerIdx);

				hintVisualElement.Refresh();
				hintVisualElement.UpdateFromBeamHintHeader(in beamHintHeader, headerIdx);

				container.Add(hintVisualElement);
			}
		}

		/// <summary>
		/// Updates a <see cref="TreeViewGUI"/> to display the given list of <see cref="BeamHintDomains"/> strings. 
		/// </summary>
		public void FillTreeViewFromDomains(TreeViewIMGUI imgui, List<string> sortedDomains, List<string> selectedDomains)
		{
			var hintDetailsReflectionCache = ActiveContext?.ServiceScope?.GetService<ReflectionCache>()?.GetFirstSystemOfType<BeamHintReflectionCache.Registry>();
			if (hintDetailsReflectionCache == null)
				return;

			var treeViewItems = new List<BeamHintDomainTreeViewItem>();
			var selectedIds = new List<int>();
			var parentCache = new Dictionary<string, BeamHintDomainTreeViewItem>();
			var id = 1;

			foreach (string domain in sortedDomains)
			{
				var currDomainsDepth = BeamHintDomains.GetDomainDepth(domain);
				// Create parents for domain when necessary.
				for (int parentDepth = 0; parentDepth <= currDomainsDepth; parentDepth++)
				{
					if (!BeamHintDomains.TryGetDomainAtDepth(domain, parentDepth, out var parentDomain)) continue;

					var parentDomainStartIdx = domain.LastIndexOf(parentDomain, StringComparison.Ordinal);
					var domainSubstring = domain.Substring(0, parentDomainStartIdx + parentDomain.Length);

					// Guarantee uniqueness within first layer of domains
					if (!parentCache.TryGetValue(domainSubstring, out var item))
					{
						_ = hintDetailsReflectionCache.TryGetDomainTitleText(parentDomain, out var domainTitle);
						item = new BeamHintDomainTreeViewItem(id, parentDepth, domainSubstring, domainTitle);
						parentCache.Add(domainSubstring, item);
						treeViewItems.Add(item);
						id += 1;

						if (selectedDomains.Contains(domain))
						{
							selectedIds.Add(item.id);
						}

					}
				}
			}

			imgui.TreeViewItems = treeViewItems.Cast<TreeViewItem>().ToList();

			// Only select if  we have any selected domains (selected domains defaults to all sorted domains unless someone clicks a subdomain).
			// This means their counts greater than or the same when no selection is made. 
			if (sortedDomains.Count > selectedDomains.Count && selectedDomains.Count != 0)
			{
				imgui.SetSelectionSafe(selectedIds);
			}
		}

		public void SetupTreeViewCallbacks(TreeViewIMGUI imgui,
										   Action onContextClicked,
										   Action<IList<TreeViewItem>> onSelectionChange,
										   Action<IList<TreeViewItem>> onSelectionBranchChange)
		{
			imgui.OnContextClicked += onContextClicked;
			imgui.OnSelectionChanged += onSelectionChange;
			imgui.OnSelectedBranchChanged += onSelectionBranchChange;
		}

		public void ExpandHint(BeamHintHeader beamHintHeader)
		{
			// Clear domain selection
			_beamHintsDataModel.SelectDomains(new List<string>());
			_treeViewIMGUI.SetSelectionSafe(new List<int>());
			_imguiContainer.MarkDirtyRepaint();

			// Filter by the id of the hint you are asking to expand.
			_beamHintsDataModel.FilterDisplayedBy(beamHintHeader.Id);
			_beamHintsDataModel.OpenHintDetails(beamHintHeader);
			Refresh();
		}
	}
}
