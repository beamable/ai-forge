using Beamable.Editor.Content.Models;
using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Content.Components
{
	public class ExplorerVisualElement : ContentManagerComponent
	{
		public event Action OnAddItemButtonClicked;
		public event Action<ContentTypeDescriptor> OnAddItemRequested;
		public event Action<ContentItemDescriptor> OnRenameItemRequested;
		public event Action<List<ContentItemDescriptor>> OnItemDownloadRequested;
		public ContentDataModel Model { get; set; }

		private VisualElement _treeContainer, _listContainer, _breadcrumbContainer;
		private BreadcrumbsVisualElement _breadcrumbsElement;
		private ContentTypeViewVisualElement _typeViewElement;
		private ContentListVisualElement _listElement;
		private List<ContentItemDescriptor> _itemsToDelete = new List<ContentItemDescriptor>();

		public ExplorerVisualElement() : base(nameof(ExplorerVisualElement)) { }

		public override void Refresh()
		{
			base.Refresh();

			_breadcrumbContainer = Root.Q<VisualElement>("breadcrumb-container");

			var contentElem = Root.Q<VisualElement>("content");


			_treeContainer = new VisualElement() { name = "tree-container" };
			_listContainer = new VisualElement() { name = "list-container" };
			contentElem.AddSplitPane(_treeContainer, _listContainer);

			_breadcrumbsElement = new BreadcrumbsVisualElement();
			_breadcrumbsElement.Model = Model;
			_breadcrumbsElement.OnBreadcrumbClicked += BreadcrumbsVisualElement_OnBreadcrumbClicked;
			_breadcrumbContainer.Add(_breadcrumbsElement);
			_breadcrumbsElement.Refresh();

			_typeViewElement = new ContentTypeViewVisualElement();
			_typeViewElement.OnSelectionChanged += ContentTypeViewVisualElement_OnSelectionChanged;
			_typeViewElement.OnSelectedBranchChanged += ContentTypeViewVisualElement_OnSelectedBranchChanged;
			_typeViewElement.OnAddItemButtonClicked += ContentTypeViewVisualElement_OnAddItemButtonClicked;
			_treeContainer.Add(_typeViewElement);
			_typeViewElement.Model = Model;
			_typeViewElement.Refresh();

			_listElement = new ContentListVisualElement();
			_listElement.OnSelectionChanged += ContentListVisualElement_OnSelectionChanged;
			_listElement.OnItemDelete += ContentListVisualElement_OnItemDelete;
			_listElement.OnItemAdd += ContentlistVisualElement_OnItemAdd;
			_listElement.OnItemDownload += ContentListVisualElement_OnDownload;
			_listElement.OnItemRename += ContentListVisualElement_OnItemRename;
			_listContainer.Add(_listElement);
			_listElement.Model = Model;
			_listElement.Refresh();

		}

		public void RefreshManifestButton()
		{
			_breadcrumbsElement.RefreshManifestButton();
		}

		/// <summary>
		/// Handle the <see cref="Breadcrumb"/> click.
		/// It may be an "All Contents", an item, or a content type.
		/// </summary>
		/// <param name="breadCrumb"></param>
		private void BreadcrumbsVisualElement_OnBreadcrumbClicked(Breadcrumb breadCrumb)
		{
			switch (breadCrumb.BreadcrumbType)
			{
				case BreadcrumbType.AllContents:
					Model.ClearSelectedContentTypes();
					break;
				case BreadcrumbType.ContenType:
					Model.SelectedContentTypes = new List<TreeViewItem> { breadCrumb.ContentTypeTreeViewItem };

					break;
				case BreadcrumbType.ContentItem:
					Model.SelectedContents = new List<ContentItemDescriptor> { breadCrumb.ContentItemDescriptor };
					break;
			}
		}

		private void ContentlistVisualElement_OnItemAdd(ContentTypeDescriptor typeDescriptor)
		{
			OnAddItemRequested?.Invoke(typeDescriptor);
		}

		private void ContentListVisualElement_OnDownload(List<ContentItemDescriptor> itemDescriptors)
		{
			OnItemDownloadRequested?.Invoke(itemDescriptors);
		}

		private void ContentListVisualElement_OnItemRename(ContentItemDescriptor contentItemDescriptor)
		{
			OnRenameItemRequested?.Invoke(contentItemDescriptor);
		}

		private void ContentListVisualElement_OnItemDelete(ContentItemDescriptor contentItemDescriptor)
		{
			_itemsToDelete.Add(contentItemDescriptor);
			EditorDebouncer.Debounce("content-deleting-lots-of-items", () =>
			{
				// by debouncing the deletes, we can batch many delete calls into one
				Model.DeleteItems(_itemsToDelete);
				_itemsToDelete.Clear();
			});
		}

		private void ContentTypeViewVisualElement_OnSelectionChanged(IList<TreeViewItem> treeViewItems)
		{
			Model.SelectedContentTypes = treeViewItems;
		}

		private void ContentTypeViewVisualElement_OnSelectedBranchChanged(IList<TreeViewItem> treeViewItems)
		{
			Model.SelectedContentTypeBranch = treeViewItems;
		}


		private void ContentTypeViewVisualElement_OnAddItemButtonClicked(ContentTypeTreeViewItem contentTypeTreeViewItem)
		{
			OnAddItemButtonClicked?.Invoke();
		}

		private void ContentListVisualElement_OnSelectionChanged(IList<ContentItemDescriptor> contentItemDescriptors)
		{
			Model.SelectedContents = contentItemDescriptors;
		}


	}
}
