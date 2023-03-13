using Beamable.Editor.Content.Models;
using System;
using System.Collections.Generic;
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
	public class CreateNewPopupVisualElement : ContentManagerComponent
	{
		public event Action<ContentTypeDescriptor> OnAddItemButtonClicked;
		public ContentDataModel Model { get; internal set; }
		private HashSet<ContentTypeDescriptor> selectedTypeDescriptors = new HashSet<ContentTypeDescriptor>();

		private Button _addContentGroupButton;
		private Button _addContentButton;
		// private Button _addItemButton;
		private VisualElement buttonListRoot;

		public CreateNewPopupVisualElement() : base(nameof(CreateNewPopupVisualElement)) { }

		public override void Refresh()
		{
			base.Refresh();

			buttonListRoot = Root.Q("mainVisualElement");
			// _addItemButton = Root.Q<Button>("addItemButton");

			// if (Model.SelectedContentTypes.Count == 1)
			// {
			//    TreeViewItem treeViewItem = Model.SelectedContentTypes[0];
			//    ContentTypeTreeViewItem contentTypeTreeViewItem = (ContentTypeTreeViewItem)treeViewItem;
			//    // Type type = contentTypeTreeViewItem.TypeDescriptor.ContentType;
			//
			//    _addItemButton.SetEnabled(true);
			//    _addItemButton.text = string.Format(ContentManagerConstants.CreateNewPopupAddButtonEnabledText,
			//       contentTypeTreeViewItem.TypeDescriptor.TypeName);
			//
			// }
			// else
			// {
			//    _addItemButton.SetEnabled(false);
			//    _addItemButton.text = ContentManagerConstants.CreateNewPopupAddButtonDisabledText;
			// }
			//
			// _addItemButton.clickable.clicked += () =>
			// {
			//    AddItemButton_OnClicked();
			// };
			GetSelectedItemCount();
			foreach (var typeDescriptor in selectedTypeDescriptors)
			{
				AddNewItem(typeDescriptor);
			}
		}

		private void AddNewItem(ContentTypeDescriptor typeDescriptor)
		{
			Button createItemButton = new Button();
			createItemButton.AddToClassList("addItemButton");
			createItemButton.SetEnabled(true);
			createItemButton.text = string.Format(CREATE_NEW_POPUP_ADD_BUTTON_ENABLED_TEXT,
			   typeDescriptor.TypeName);
			createItemButton.clickable.clicked += () =>
			{
				OnAddItemButtonClicked?.Invoke(typeDescriptor);
			};

			buttonListRoot.Add(createItemButton);
		}

		// Get a list of all items able to be created.
		public int GetSelectedItemCount()
		{
			selectedTypeDescriptors.Clear();

			if (Model.SelectedContentTypes.Count == 0)
			{
				// Add all
				var types = Model.GetContentTypes();
				foreach (var type in types)
				{
					string[] splitTypes = type.TypeName.Split('.');
					if (splitTypes.Length == 1)
						selectedTypeDescriptors.Add(type);
				}
			}
			else
			{
				var selectedViewItems = Model.SelectedContentTypes;
				foreach (var selectedViewItem in selectedViewItems)
				{
					ContentTypeTreeViewItem contentTypeTreeViewItem = (ContentTypeTreeViewItem)selectedViewItem;
					selectedTypeDescriptors.Add(contentTypeTreeViewItem.TypeDescriptor);
				}
			}

			return selectedTypeDescriptors.Count;
		}



		// private void AddItemButton_OnClicked()
		// {
		//    OnAddItemButtonClicked?.Invoke();
		// }
	}
}
