using Beamable.Editor.Content.Helpers;
using Beamable.Editor.Content.Models;
using Beamable.Editor.UI.Components;
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
	public enum BreadcrumbType
	{
		AllContents,
		ContenType,
		ContentItem
	}

	/// <summary>
	/// Represents a text string in the UI.
	/// Additional data helps handle the clickability
	/// </summary>
	public class Breadcrumb
	{
		public string Text { set; get; }
		public BreadcrumbType BreadcrumbType { set; get; }
		public ContentTypeTreeViewItem ContentTypeTreeViewItem { set; get; }
		public ContentItemDescriptor ContentItemDescriptor { set; get; }

		public Breadcrumb(string text,
		   BreadcrumbType breadcrumbType,
		   ContentTypeTreeViewItem contentTypeTreeViewItem = null,
		   ContentItemDescriptor contentItemDescriptor = null)
		{
			Text = text;
			BreadcrumbType = breadcrumbType;
			ContentTypeTreeViewItem = contentTypeTreeViewItem;
			ContentItemDescriptor = contentItemDescriptor;
		}
	}

	public class BreadcrumbsVisualElement : ContentManagerComponent
	{
		public event Action<Breadcrumb> OnBreadcrumbClicked;

		public ContentDataModel Model { get; set; }

		private VisualElement _tokenListVisualElement;
		private Label _counterLabel;
		private IList<ContentTypeTreeViewItem> _selectedContentTypeBranch;
		private ContentItemDescriptor _selectedContentItemDescriptor;
		private RealmButtonVisualElement _realmButton;
		private ManifestButtonVisualElement _manifestButton;
		private Button _contentSorterButton;
		private Label _contentSorterTitle;

		public BreadcrumbsVisualElement() : base(nameof(BreadcrumbsVisualElement))
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			_realmButton = Root.Q<RealmButtonVisualElement>("realmButton");
			_realmButton.Refresh();
			_manifestButton = Root.Q<ManifestButtonVisualElement>("manifestButton");
			_manifestButton.Refresh();
			_tokenListVisualElement = Root.Q<VisualElement>("tokenListVisualElement");
			_counterLabel = Root.Q<Label>("counterLabel");

			_contentSorterButton = Root.Q<Button>("contentSorter");
			_contentSorterButton.clickable.clicked -= HandleContentSorterButton;
			_contentSorterButton.clickable.clicked += HandleContentSorterButton;

			_contentSorterTitle = Root.Q<Label>("contentSorterTitle");
			_contentSorterTitle.text = ContentSorterHelper.GetContentSorterTitle(Model.CurrentSorter);

			Model_OnSelectedContentTypeBranchChanged(new List<TreeViewItem>());
			Model.OnSelectedContentTypeBranchChanged += Model_OnSelectedContentTypeBranchChanged;
			//
			HandleSelectedContentChanged(new List<ContentItemDescriptor>());
			Model.OnSelectedContentChanged += HandleSelectedContentChanged; ;

			//
			Model.OnFilterChanged += Model_OnFilterChanged;
		}

		/// <summary>
		/// Build a list of <see cref="Breadcrumb"/>s, then create a <see cref="VisualElement"/>
		/// for each with some arrows in between each. All non-arrows are clickable.
		/// </summary>
		private void RenderTokens()
		{
			List<Breadcrumb> breadCrumbs = new List<Breadcrumb>();

			// Add the master token
			breadCrumbs.Add(new Breadcrumb(BREADCRUMBS_ALL_CONTENT_TEXT, BreadcrumbType.AllContents));

			// Add on the types
			foreach (ContentTypeTreeViewItem contentTypeTreeViewItem in _selectedContentTypeBranch)
			{
				breadCrumbs.Add(new Breadcrumb(contentTypeTreeViewItem.displayName, BreadcrumbType.ContenType,
				   contentTypeTreeViewItem, null));
			}

			// Add on the item, if it exists
			if (_selectedContentItemDescriptor != null)
			{
				breadCrumbs.Add(new Breadcrumb(_selectedContentItemDescriptor.Name, BreadcrumbType.ContentItem,
				   null, _selectedContentItemDescriptor));
			}

			//Loop and render all tokens
			_tokenListVisualElement.Clear();
			for (int i = 0; i < breadCrumbs.Count; i++)
			{
				Breadcrumb breadcrumb = breadCrumbs[i];

				// Add a clickable token
				_tokenListVisualElement.Add(CreateNewToken(breadcrumb.Text, false, () =>
				{
					OnBreadcrumbClicked?.Invoke(breadcrumb);
				}));

				if (i < breadCrumbs.Count - 1)
				{
					// Add an arrow  token
					_tokenListVisualElement.Add(CreateNewToken(BREADCRUMB_TOKEN_ARROW, true));
				}
			}


			int min = Model.GetFilteredContents().Count();
			int max = Model.GetAllContents().Count();
			var counterText = string.Format(BREADCRUMBS_COUNT_TEXT, min, max);
			_counterLabel.text = counterText;

		}

		private VisualElement CreateNewToken(string text, bool isArrow, Action onMouseClick = null)
		{
			Label label = new Label();
			label.text = text;

			if (isArrow)
			{
				label.AddToClassList("tokenTextArrow");
			}
			else
			{
				label.AddToClassList("tokenTextWord");

				label.RegisterCallback<MouseDownEvent>((MouseDownEvent evt) =>
				{
					onMouseClick();
				}, TrickleDown.NoTrickleDown);
			}

			return label;
		}

		private void Model_OnSelectedContentTypeBranchChanged(IList<TreeViewItem> selectedContentTypeBranch)
		{
			_selectedContentTypeBranch = selectedContentTypeBranch.Cast<ContentTypeTreeViewItem>().ToList();
			RenderTokens();
		}

		private void Model_OnFilterChanged()
		{
			RenderTokens();
		}

		private void HandleSelectedContentChanged(IList<ContentItemDescriptor> contentItemDescriptors)
		{
			// Set the Selected Content
			_selectedContentItemDescriptor = contentItemDescriptors.FirstOrDefault();
			RenderTokens();
		}

		public void RefreshManifestButton()
		{
			_manifestButton.RefreshButtonVisibility();
		}

		private void HandleContentSorterButton() => HandleContentSorterButton(_contentSorterButton.worldBound);

		private void HandleContentSorterButton(Rect visualElementBounds)
		{
			var popupWindowRect = BeamablePopupWindow.GetLowerLeftOfBounds(visualElementBounds);
			var contentSorter = new ContentSorterDropdownVisualElement(Model);
			contentSorter.Refresh();
			var wnd = BeamablePopupWindow.ShowDropdown("Select", popupWindowRect, new Vector2(150, ContentSorterHelper.GetAllSorterOptions.Count * 25), contentSorter);
			contentSorter.OnSortingChanged += (sorter, title) =>
			{
				_contentSorterTitle.text = title;
				Model.SetSorter(sorter);
				wnd.Close();
			};
		}
	}
}
