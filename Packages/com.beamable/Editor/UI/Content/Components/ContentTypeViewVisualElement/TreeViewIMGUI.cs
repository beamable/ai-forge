using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
#if UNITY_2022_1_OR_NEWER
using TreeView = UnityEditor.IMGUI.Controls.TreeView;
#endif

namespace Beamable.Editor.Content.Components
{
	/// <summary>
	/// Adds a Beamable-friendly API on top of the existing
	/// Unity IMGUI <see cref="TreeView"/>.
	/// </summary>
	public class TreeViewIMGUI : TreeView
	{
		/// <summary>
		/// Invoked when the single-selection or multiselection changes
		/// </summary>
		public event Action<IList<TreeViewItem>> OnSelectionChanged;

		/// <summary>
		/// Invoked when the single-selection or multiselection changes.
		/// Conains full list of item and parents.
		/// </summary>
		public event Action<IList<TreeViewItem>> OnSelectedBranchChanged;

		/// <summary>
		/// Invoked when the TreeViewIMGUI background is clicked.
		/// </summary>
		public event Action OnContextClicked;

		private SelectionType _selectionType = SelectionType.Multiple;
		public SelectionType SelectionType
		{
			set
			{
				_selectionType = value;
			}

			get
			{
				return _selectionType;

			}
		}

		/// <summary>
		/// Here is the dynamically calculated height of the <see cref="TreeViewIMGUI"/>.
		/// THis works great. May be redundant to existing public api.
		/// </summary>
		/// <returns></returns>
		public float GetCalculatedHeight()
		{
			return GetRows().Count * RowHeight;
		}

		public float RowHeight
		{
			get => rowHeight;
			set => rowHeight = value;
		}

		public float Height => totalHeight;

		/// <summary>
		/// The displayed content items used in the content of the <see cref="TreeView"/>
		/// </summary>
		public List<TreeViewItem> TreeViewItems
		{
			set
			{
				_treeViewItems = value;
				SafeReload();
			}
			get
			{
				return _treeViewItems;

			}
		}

		private List<TreeViewItem> _treeViewItems;

		/// <summary>
		/// The non-displayed root content item of the <see cref="TreeView"/>
		/// </summary>
		public TreeViewItem TreeViewItemRoot
		{
			set
			{
				_treeViewItemRoot = value;
				SafeReload();
			}
			get
			{
				return _treeViewItemRoot;

			}
		}

		private TreeViewItem _treeViewItemRoot;
		private List<TreeViewItem> _selectionBranch;
		public List<TreeViewItem> MainSelectionBranch { private set { _selectionBranch = value; } get { return _selectionBranch; } }

		public TreeViewIMGUI(TreeViewState treeViewState)
			: base(treeViewState)
		{
			SafeReload();
		}

		/// <summary>
		/// Set the selection to null and refresh the layout
		/// </summary>
		public void ClearSelection()
		{
			SetSelectionSafe(new List<int>());
			Reload();
		}

		/// <summary>
		/// Set the selection without invok
		/// </summary>
		public void SetSelectionSafe(List<int> selectedIds)
		{
			//Do not invoke change when manually set like this
			SetSelection(selectedIds, TreeViewSelectionOptions.None);

			//Update the MainSelectionBranch
			IList<TreeViewItem> treeViewItems = GetTreeViewItemsFromInts(selectedIds);
			TreeViewItem treeViewItem = treeViewItems.FirstOrDefault();
			SetMainSelectionBranch(treeViewItem);
		}

		/// <summary>
		/// Rerender the <see cref="TreeView"/> without null-refs
		/// </summary>
		private void SafeReload()
		{
			if (_treeViewItemRoot != null && _treeViewItems != null)
			{
				Reload();
			}
		}

		/// <summary>
		/// Build the initial rendering structures the <see cref="TreeView"/> without null-refs
		/// </summary>
		protected override TreeViewItem BuildRoot()
		{
			SetupParentsAndChildrenFromDepths(_treeViewItemRoot, _treeViewItems);
			return _treeViewItemRoot;
		}

		protected override void ContextClicked()
		{
			base.ContextClicked();

			OnContextClicked?.Invoke();
		}

		protected override bool CanMultiSelect(TreeViewItem item)
		{
			if (_selectionType == SelectionType.Single)
			{
				return false;
			}
			return base.CanMultiSelect(item);
		}



		/// <summary>
		/// The branch contains the selected <see cref="TreeViewItem"/>
		/// and all its ancestor <see cref="TreeViewItem"/>(s) if they exist.
		/// </summary>
		/// <param name="treeViewItem"></param>
		private void SetMainSelectionBranch(TreeViewItem treeViewItem)
		{
			_selectionBranch = new List<TreeViewItem>();

			if (treeViewItem != null)
			{
				//Convert to objects for ease-of-use via API
				IList<TreeViewItem> ancestorTreeViewItems = GetTreeViewItemsFromInts(GetAncestors(treeViewItem.id));
				foreach (TreeViewItem ancestor in ancestorTreeViewItems)
				{
					_selectionBranch.Add(ancestor);
				}

				_selectionBranch.Add(treeViewItem);
			}

			OnSelectedBranchChanged?.Invoke(_selectionBranch);
		}

		protected override void SelectionChanged(IList<int> selectedIds)
		{
			base.SelectionChanged(selectedIds);

			//Update the MainSelectionBranch
			IList<TreeViewItem> treeViewItems = GetTreeViewItemsFromInts(selectedIds);
			TreeViewItem treeViewItem = treeViewItems.FirstOrDefault();
			SetMainSelectionBranch(treeViewItem);

			OnSelectionChanged?.Invoke(treeViewItems);
		}
		private IList<TreeViewItem> GetTreeViewItemsFromInts(IList<int> selectedIds)
		{
			return _treeViewItems.FindAll((treeViewItem) =>
			{
				return selectedIds.Contains(treeViewItem.id);
			});
		}
	}
}

