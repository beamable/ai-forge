using Beamable.Editor.Content.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants.Features.ContentManager;

namespace Beamable.Editor.Content.Components
{
	/// <summary>
	/// The <see cref="VisualElement"/> wrapper for the Unity IMGUI <see cref="TreeView"/>
	/// </summary>
	public class ContentTypeViewVisualElement : ContentManagerComponent
	{
		public ContentDataModel Model { get; set; }

		public event Action<IList<TreeViewItem>> OnSelectionChanged;
		public event Action<IList<TreeViewItem>> OnSelectedBranchChanged;
		public event Action<ContentTypeTreeViewItem> OnAddItemButtonClicked;

		private VisualElement _mainVisualElement;
		private HeaderVisualElement _headerVisualElement;
		private TreeViewIMGUI _treeViewIMGUI;
		private TreeViewState _treeViewState;
		private IMGUIContainer _imguiContainer;

		public ContentTypeViewVisualElement()
		   : base(nameof(ContentTypeViewVisualElement))
		{

		}

		public override void Refresh()
		{
			base.Refresh();

			_mainVisualElement = Root.Q<VisualElement>("mainVisualElement");

			_headerVisualElement = Root.Q<HeaderVisualElement>("headerVisualElement");
			_headerVisualElement.Headers = new[] { CONTENT_TYPE_VIEW_HEADER_TEXT };
			_headerVisualElement.Refresh();

			//Create IMGUI, The VisualElement Wrapper, and add to the parent
			_treeViewIMGUI = CreateTreeViewIMGUI();
			SetTreeViewItemsSafe(new List<TreeViewItem>());
			_imguiContainer = CreateTreeViewIMGUIContainer(_treeViewIMGUI);
			_mainVisualElement.Add(_imguiContainer);
			_imguiContainer.RegisterCallback<MouseDownEvent>(IMGUIContainer_OnMouseDownEvent,
			   TrickleDown.NoTrickleDown);

			VisualElement lowerBackgroundVisualElement = new VisualElement();
			lowerBackgroundVisualElement.name = "lowerBackgroundVisualElement";
			lowerBackgroundVisualElement.RegisterCallback<MouseDownEvent>(LowerBackgroundVisualElement_OnMouseDownEvent,
			   TrickleDown.NoTrickleDown);
			_mainVisualElement.Add(lowerBackgroundVisualElement);

			Model.OnSelectedContentTypesChanged += Model_OnSelectedContentTypeTreeViewItemsChanged;

			// Populate Tree now
			OnTypesReceived();

			// Populate Tree later
			Model.OnTypesReceived += OnTypesReceived;

			Root.Q<VisualElement>("showAllContent").RegisterCallback<MouseDownEvent>(evt => Model.ClearSelectedContentTypes());
		}

		private TreeViewIMGUI CreateTreeViewIMGUI()
		{
			_treeViewState = new TreeViewState();

			TreeViewIMGUI treeViewIMGUI = new TreeViewIMGUI(_treeViewState);
			treeViewIMGUI.SelectionType = SelectionType.Single;
			treeViewIMGUI.TreeViewItemRoot = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

			//
			treeViewIMGUI.OnSelectionChanged += TreeViewIMGUI_OnSelectionChanged;
			treeViewIMGUI.OnSelectedBranchChanged += TreeViewIMGUI_OnSelectedBranchChanged;
			treeViewIMGUI.OnContextClicked += TreeViewIMGUI_OnContextClicked;
			return treeViewIMGUI;

		}


		private IMGUIContainer CreateTreeViewIMGUIContainer(TreeViewIMGUI treeViewIMGUI)
		{
			IMGUIContainer treeView = new IMGUIContainer(() =>
			{
				// Tree view - Re-render every frame
				Rect rect = GUILayoutUtility.GetRect(200, 200,
				   treeViewIMGUI.GetCalculatedHeight(), treeViewIMGUI.GetCalculatedHeight());

				_treeViewIMGUI.OnGUI(rect);

			});
			treeView.name = "treeView";

			return treeView;
		}

		private void SetTreeViewItemsSafe(List<TreeViewItem> treeViewItem)
		{
			_treeViewIMGUI.TreeViewItems = treeViewItem;
			_imguiContainer?.MarkDirtyLayout();
			_imguiContainer?.MarkDirtyRepaint();
		}

		private void Model_OnSelectedContentTypeTreeViewItemsChanged(IList<TreeViewItem> treeViewItems)
		{
			List<int> treeViewItemIds = treeViewItems.Select((treeViewItem) =>
			{
				return treeViewItem.id;
			}).ToList<int>();

			_treeViewIMGUI.SetSelectionSafe(treeViewItemIds);
		}

		private void TreeViewIMGUI_OnSelectionChanged(IList<TreeViewItem> selectedTreeViewItems)
		{
			OnSelectionChanged?.Invoke(selectedTreeViewItems);
		}

		private void TreeViewIMGUI_OnSelectedBranchChanged(IList<TreeViewItem> selectionBranch)
		{
			OnSelectedBranchChanged?.Invoke(selectionBranch);
		}

		/// <summary>
		/// Capture when the background (Not an item) is clicked
		/// </summary>
		private void IMGUIContainer_OnMouseDownEvent(MouseDownEvent evt)
		{

		}

		private void LowerBackgroundVisualElement_OnMouseDownEvent(MouseDownEvent evt)
		{
			if (_treeViewIMGUI.HasSelection())
			{
				_treeViewIMGUI.ClearSelection();
				OnSelectionChanged?.Invoke(new List<TreeViewItem>());
			}
		}

		private void TreeViewIMGUI_OnContextClicked()
		{
			ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(ContentVisualElement_OnContextMenuOpen);
			contextualMenuManipulator.target = Root;
		}

		private void ContentVisualElement_OnContextMenuOpen(ContextualMenuPopulateEvent evt)
		{
			ContentTypeTreeViewItem selectedContentTypeTreeViewItem = (ContentTypeTreeViewItem)
			   Model?.SelectedContentTypes?.FirstOrDefault();

			if (selectedContentTypeTreeViewItem == null)
			{
				//Nothing selected. That's ok. Do nothing.
				return;
			}

			string actionTitle = string.Format(CREATE_NEW_POPUP_ADD_BUTTON_ENABLED_TEXT,
			   selectedContentTypeTreeViewItem.TypeDescriptor.ContentType.Name);

			evt.menu.BeamableAppendAction(actionTitle, (pos) =>
			{
				OnAddItemButtonClicked?.Invoke(selectedContentTypeTreeViewItem);
			});
		}

		private void OnTypesReceived()
		{
			SetTreeViewItemsSafe(Model.ContentTypeTreeViewItems().Cast<TreeViewItem>().ToList());
		}
	}
}


