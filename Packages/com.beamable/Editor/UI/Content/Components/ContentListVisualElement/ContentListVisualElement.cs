using Beamable.Common.Content;
using Beamable.Editor.Content.Models;
using Beamable.Editor.UI;
using Beamable.Editor.UI.Common;
using Beamable.Editor.UI.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleSheets;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants.Features.ContentManager;

namespace Beamable.Editor.Content.Components
{
	public class ContentListVisualElement : ContentManagerComponent
	{
		public event Action<IList<ContentItemDescriptor>> OnSelectionChanged;
		public event Action<ContentItemDescriptor> OnItemDelete;
		public event Action<ContentTypeDescriptor> OnItemAdd;
		public event Action<ContentItemDescriptor> OnItemRename;
		public event Action<List<ContentItemDescriptor>> OnItemDownload;

		/// <summary>
		/// Provide explict height for every row so ListView can calculate
		/// how many items to actually display, as an optimization.
		/// </summary>
		private int ListViewItemHeight = 24;

		/// <summary>
		/// Indicates index when nothing is selected in the <see cref="ListView"/>
		/// </summary>
		private int NullIndex = -1;

		public ContentDataModel Model { get; set; }

		private VisualElement _mainVisualElement;
		private HeaderVisualElement _headerVisualElement;
		private ExtendedListView _listView;
		private List<HeaderSizeChange> _headerSizeChanges;
		private List<ContentVisualElement> _contentVisualElements = new List<ContentVisualElement>();
		private bool _isKeyboardInputBlocked;

		public ContentListVisualElement() : base(nameof(ContentListVisualElement)) { }

#if UNITY_2018
        protected override void DoRepaint(IStylePainter painter)
        {
            base.DoRepaint(painter);
            _headerVisualElement.EmitFlexValues();
        }
#endif
		public override void Refresh()
		{
			base.Refresh();

			_mainVisualElement = Root.Q<VisualElement>("mainVisualElement");
			_mainVisualElement.RegisterCallback<MouseDownEvent>(MainContent_OnMouseDownEvent,
																TrickleDown.NoTrickleDown);

			_headerVisualElement = Root.Q<HeaderVisualElement>("headerVisualElement");
			_headerVisualElement.Headers = new[] { "Content ID", "Content Type", "Tags", "Latest update" };
			_headerVisualElement.Refresh();
			_headerVisualElement.OnValuesChanged += Header_OnValuesResized;
			_headerSizeChanges = GetHeaderSizeChanges();

			EditorApplication.delayCall += () => { _headerVisualElement.EmitFlexValues(); };

			//List
			_listView = CreateListView();
			_mainVisualElement.Add(_listView);

			Model.OnSelectedContentChanged += Model_OnSelectedContentChanged;
			Model.OnFilteredContentsChanged += Model_OnFilteredContentChanged;
			Model.OnContentDeleted += Model_OnContentDeleted;
			Model.OnManifestChanged += ManifestChanged;

			var manipulator = new ContextualMenuManipulator(ContentVisualElement_OnContextMenuOpen);
			_listView.AddManipulator(manipulator);
			_contentVisualElements = new List<ContentVisualElement>();

			_listView.RefreshPolyfill();

			RegisterCallback<KeyDownEvent>(RegisterKeyDown, TrickleDown.TrickleDown);
			RegisterCallback<KeyUpEvent>(RegisterKeyUp, TrickleDown.TrickleDown);
		}

		private void RegisterKeyDown(KeyDownEvent evt)
		{
			if (_isKeyboardInputBlocked)
				return;

			if (evt.actionKey && evt.keyCode == KeyCode.D)
			{
				_isKeyboardInputBlocked = true;
				foreach (var contentItem in Model.SelectedContents.ToList())
					Duplicate(contentItem);
			}
			else if (evt.keyCode == KeyCode.Delete)
			{
				_isKeyboardInputBlocked = true;
				ContentVisualElement_OnItemDelete(Model.SelectedContents.ToArray());
			}
		}

		private void RegisterKeyUp(KeyUpEvent evt)
		{
			_isKeyboardInputBlocked = false;
		}

		private void ManifestChanged()
		{
			_listView.ClearSelection();
		}

		private void Header_OnValuesResized(List<HeaderSizeChange> headerFlexSizes)
		{
			_headerSizeChanges = headerFlexSizes;
			// update all content...
			foreach (var listElement in _contentVisualElements)
			{
				ApplyColumnSizes(listElement);
			}
		}

		private List<HeaderSizeChange> GetHeaderSizeChanges()
		{
			return _headerSizeChanges ??
				   (_headerSizeChanges = _headerVisualElement.ComputeSizes(new List<float> { 1, .5f, .25f, .25f }));
		}

		private void ApplyColumnSizes(VisualElement listElement)
		{
			if (listElement is ContentVisualElement contentElement && contentElement.ContentItemDescriptor == null)
			{
				return; // element isn't bound yet.
			}

			listElement.Q("nameTextField").style.flexGrow = (GetHeaderSizeChanges()[0].Flex);
			listElement.Q("pathLabel").style.flexGrow = (GetHeaderSizeChanges()[1].Flex);
			listElement.Q("tagListVisualElement").style.flexGrow = (GetHeaderSizeChanges()[2].Flex);
			listElement.Q("lastChanged").style.flexGrow = (GetHeaderSizeChanges()[3].Flex);


			listElement.Q("nameTextField").style.minWidth = (_headerSizeChanges[0].SafeMinWidth);
			listElement.Q("pathLabel").style.minWidth = (_headerSizeChanges[1].SafeMinWidth);
			listElement.Q("tagListVisualElement").style.minWidth = (_headerSizeChanges[2].SafeMinWidth);
			listElement.Q("lastChanged").style.minWidth = (_headerSizeChanges[3].SafeMinWidth);
		}

		private void Model_OnFilteredContentChanged()
		{
			_listView.RefreshPolyfill();
		}

		private ExtendedListView CreateListView()
		{
			var view = new ExtendedListView()
			{
				makeItem = CreateListViewElement,
				bindItem = BindListViewElement,
				selectionType = SelectionType.Multiple,
				itemsSource = Model.FilteredContents
			};

			view.SetItemHeight(ListViewItemHeight);
			view.BeamableOnItemChosen(ListView_OnItemChosen);
			view.BeamableOnSelectionsChanged(ListView_OnSelectionChanged);
			view.RefreshPolyfill();
			return view;
		}

		void BindListViewElement(VisualElement elem, int index)
		{
			ContentVisualElement contentVisualElement = (ContentVisualElement)elem;

			contentVisualElement.ContentItemDescriptor = Model.FilteredContents[index];
			contentVisualElement.OnRightMouseButtonClicked -= ContentVisualElement_OnRightMouseButtonClicked;
			contentVisualElement.OnRightMouseButtonClicked += ContentVisualElement_OnRightMouseButtonClicked;
			contentVisualElement.Refresh();
			if (index % 2 == 0)
			{
				contentVisualElement.Children().First().RemoveFromClassList("oddRow");
			}
			else
			{
				contentVisualElement.Children().First().AddToClassList("oddRow");
			}

			ApplyColumnSizes(contentVisualElement);
			contentVisualElement.MarkDirtyRepaint();
		}

		ContentVisualElement CreateListViewElement()
		{
			ContentVisualElement contentVisualElement = new ContentVisualElement();

			_contentVisualElements.Add(contentVisualElement);
			return contentVisualElement;
		}

		/// <summary>
		/// Capture when the background (Not an item) is clicked
		/// with ANY mouse button.
		/// </summary>
		private void MainContent_OnMouseDownEvent(MouseDownEvent evt)
		{
			VisualElement target = (VisualElement)evt.target;

			//HACK: Remove this hack. Instead, ideally observe the event in a way that
			//a click on an item does NOT call this handler - srivello
			if (target.name.Contains("ContentViewport"))
			{
				SetSelectedIndexSafe(NullIndex);
			}
		}

		private void SelectItemInInspectorWindow(params ContentItemDescriptor[] contentItemDescriptor)
		{
			if (contentItemDescriptor.Length.Equals(0))
				return;

			var selection = contentItemDescriptor
							.Where(item => !string.IsNullOrEmpty(item?.AssetPath))
							.Select(item =>
										new Tuple<UnityEngine.Object, ContentItemDescriptor>(
											AssetDatabase.LoadMainAssetAtPath(item.AssetPath), item))
							.ToList();

			foreach (var errorCase in selection.Where(obj => obj.Item1 == null))
			{
				Debug.LogError(new Exception("ListView_OnItemChosen() Error : " +
											 "no unityObject for " + errorCase.Item2.Name));
			}

			var objects = selection.Select(x => x.Item1).ToArray();
			Selection.objects = objects;
		}

		private void PingItemInProjectWindow(ContentItemDescriptor contentItemDescriptor)
		{
			if (string.IsNullOrEmpty(contentItemDescriptor.AssetPath))
			{
				Debug.LogWarning($"The selected content=[{contentItemDescriptor.Name}] does not exist locally. First, download the content from the server to be able to edit it.");
				return;
			}

			UnityEngine.Object unityObject =
				AssetDatabase.LoadMainAssetAtPath(contentItemDescriptor.AssetPath);

			if (unityObject == null)
			{
				Debug.LogError(new Exception("ListView_OnItemChosen() Error :" +
											 " no unityObject for " + contentItemDescriptor.Name));
				return;
			}

			EditorGUIUtility.PingObject(unityObject.GetInstanceID());
		}

		/// <summary>
		/// Set the selected <see cref="ListView"/> item like this.
		/// This prevents an infinite UI loop.
		/// </summary>
		/// <param name="index"></param>
		private void SetSelectedIndexSafe(int index)
		{
			if (_listView.selectedIndex != index && index >= 0 && index < _listView.itemsSource.Count)
			{
				_listView.selectedIndex = index;
			}
		}

		/// <summary>
		/// Lookup the <see cref="ContentVisualElement"/> by the <see cref="ContentItemDescriptor"/>
		/// </summary>
		/// <param name="contentItemDescriptor"></param>
		/// <returns></returns>
		private ContentVisualElement GetVisualItemByData(ContentItemDescriptor contentItemDescriptor)
		{
			List<VisualElement> visualElements = _listView.Children().ToList();

			return (ContentVisualElement)visualElements.Find((VisualElement visualElement) =>
			{
				ContentVisualElement nextContentVisualElement = (ContentVisualElement)visualElement;
				return string.Equals(nextContentVisualElement.ContentItemDescriptor?.Id, contentItemDescriptor?.Id);
			});
		}

		private void Model_OnSelectedContentChanged(IList<ContentItemDescriptor> contentItemDescriptors)
		{
			var x = contentItemDescriptors.FirstOrDefault<ContentItemDescriptor>();

			if (x == null)
			{
				SetSelectedIndexSafe(NullIndex);
			}
			else
			{
				SetSelectedIndexSafe(_listView.itemsSource.IndexOf(x));
			}
		}

		private void Model_OnContentDeleted(ContentItemDescriptor obj)
		{
			_listView.ClearSelection();
		}

		private void ContentVisualElement_OnRightMouseButtonClicked(ContentItemDescriptor contentItemDescriptor)
		{
			// Update selection to match the right clicked item
			if (!(Model.SelectedContents?.Contains(contentItemDescriptor) ?? false))
			{
				var index = _listView.itemsSource.IndexOf(contentItemDescriptor);
				SetSelectedIndexSafe(index);
			}
		}

		private void AddCreateItemMenu(ContextualMenuPopulateEvent evt)
		{
			var selectedTypes = Model.SelectedContentTypes;
			var types = Model.GetContentTypes()
				.OrderBy(x => x.TypeName)
				.ToList();
			string currentCategoryName = "";

			if (selectedTypes.FirstOrDefault() is ContentTypeTreeViewItem selectedType)
			{
				types = types.Where(t => selectedType.TypeDescriptor.ContentType.IsAssignableFrom(t.ContentType))
							 .ToList();
				currentCategoryName = selectedType.displayName;

				evt.menu.BeamableAppendAction(
					$"{ContentList.CONTENT_LIST_CREATE_ITEM} {selectedType.displayName}",
					(Action<Vector2>)((pos) => { OnItemAdd?.Invoke(selectedType.TypeDescriptor); }));
			}

			// If only noe type, no need to create a list
			if (types.Count <= 1)
				return;

			foreach (var type in types)
			{
				if (currentCategoryName.Equals(type.ShortName)) continue;

				evt.menu.BeamableAppendAction($"Create/{type.TypeName.Replace(".", "/")}",
											  _ => { OnItemAdd?.Invoke(type); });
			}
		}

		private void AddDuplicateButton(ContextualMenuPopulateEvent evt)
		{
			List<ContentItemDescriptor> selectionList = Model.SelectedContents.ToList();
			if (selectionList.Count != 1)
				return;

			ContentItemDescriptor selectedItem = selectionList[0];
			if (selectedItem.LocalStatus !=
				HostStatus.AVAILABLE) // cannot duplicate something that we don't have locally...
				return;

			evt.menu.BeamableAppendAction("Duplicate item", (Action<Vector2>)((pos) => Duplicate(selectedItem)));
		}

		private void Duplicate(ContentItemDescriptor contentItem)
		{
			if (contentItem.LocalStatus !=
				HostStatus.AVAILABLE) // cannot duplicate something that we don't have locally...
				return;

			var nextPath =
				Model.ContentIO.GetAvailableFileName(contentItem.AssetPath, contentItem.Id, Model.LocalManifest);
			var didCopy = AssetDatabase.CopyAsset(contentItem.AssetPath, nextPath);
			if (didCopy)
			{
				ContentObject contentObject = (ContentObject)AssetDatabase.LoadMainAssetAtPath(nextPath);
				var fileName = Path.GetFileNameWithoutExtension(nextPath);
				contentObject.SetContentName(fileName);
				contentObject.LastChanged = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				AssetDatabase.ForceReserializeAssets(new[] { nextPath });
				ContentIO.NotifyCreated(contentObject);
			}
		}

		private void ShowContextMenuForSingle(ContextualMenuPopulateEvent evt, ContentItemDescriptor item)
		{
			if (Model.GetLatestDescriptor(item, out var latest))
			{
				item = latest;
			}

			if (item.LocalStatus == HostStatus.AVAILABLE) // cannot rename something that we don't have locally...
			{
				evt.menu.BeamableAppendAction(ContentList.CONTENT_LIST_DELETE_ITEM,
											  (Action<Vector2>)((pos) =>
											  {
												  ContentVisualElement_OnItemDelete((ContentItemDescriptor)item);
											  }));
				evt.menu.BeamableAppendAction(ContentList.CONTENT_LIST_RENAME_ITEM,
											  (Action<Vector2>)((pos) =>
											  {
												  ContentVisualElement_OnItemRenameGestureBegin(
													  (ContentItemDescriptor)item);
											  }));

				if (item.Status == ContentModificationStatus.MODIFIED)
				{
					evt.menu.BeamableAppendAction(ContentList.CONTENT_LIST_REVERT_ITEM,
												  (Action<Vector2>)((pos) =>
												  {
													  ContentVisualElement_OnDownloadSingle(item);
												  }));
				}
			}

			if (item.LocalStatus == HostStatus.NOT_AVAILABLE && item.ServerStatus == HostStatus.AVAILABLE)
			{
				evt.menu.BeamableAppendAction(ContentList.CONTENT_LIST_DOWNLOAD_ITEM,
											  (Action<Vector2>)((pos) =>
											  {
												  ContentVisualElement_OnDownloadSingle(item);
											  }));
			}
		}

		private void ShowContextMenuForMany(ContextualMenuPopulateEvent evt, List<ContentItemDescriptor> items)
		{
			var allLocal = items.All(i => i.LocalStatus == HostStatus.AVAILABLE);
			if (allLocal)
			{
				evt.menu.BeamableAppendAction($"{ContentList.CONTENT_LIST_DELETE_ITEMS} ({items.Count})",
											  (Action<Vector2>)((pos) =>
											  {
												  ContentVisualElement_OnItemDelete(items.ToArray());
											  }));
			}

			var modifiedOrServerOnly = items.Where(i =>
													   i.LocalStatus == HostStatus.NOT_AVAILABLE ||
													   i.Status == ContentModificationStatus.MODIFIED).ToList();
			if (modifiedOrServerOnly.Count > 0)
			{
				evt.menu.BeamableAppendAction(
					$"{ContentList.CONTENT_LIST_DOWNLOAD_ITEMS} ({modifiedOrServerOnly.Count})",
					(Action<Vector2>)((pos) => { ContentVisualElement_OnDownloadMany(modifiedOrServerOnly); }));
			}
		}

		private void ContentVisualElement_OnContextMenuOpen(ContextualMenuPopulateEvent evt)
		{
			switch (Model.SelectedContents.Count)
			{
				case 0:
					AddCreateItemMenu(evt);
					break;
				case 1:
					AddCreateItemMenu(evt);
					AddDuplicateButton(evt);

					ShowContextMenuForSingle(evt, Model.SelectedContents.FirstOrDefault());
					break;
				default:
					ShowContextMenuForMany(evt, Model.SelectedContents.ToList());
					break;
			}
		}

		private async void ContentVisualElement_OnItemDelete(params ContentItemDescriptor[] contentItemDescriptors)
		{
			var contentManagerWindow = await BeamEditorWindow<ContentManagerWindow>.GetFullyInitializedWindow();
			contentManagerWindow.CloseCurrentWindow();

			ConfirmationPopupVisualElement confirmationPopup = new ConfirmationPopupVisualElement(CONFIRM_ITEM_DELETION,

				() => contentItemDescriptors.ToList().ForEach(e => OnItemDelete?.Invoke(e)),
				contentManagerWindow.CloseCurrentWindow
			);

			BeamablePopupWindow window = BeamablePopupWindow.ShowConfirmationUtility(
				CONFIRM_WINDOW_HEADER,
				confirmationPopup, contentManagerWindow);

			contentManagerWindow.SetCurrentWindow(window);
		}

		private void ContentVisualElement_OnItemRenameGestureBegin(ContentItemDescriptor contentItemDescriptor)
		{
			OnItemRename?.Invoke(contentItemDescriptor);
		}

		private void ContentVisualElement_OnDownloadSingle(ContentItemDescriptor contentItemDescriptor)
		{
			OnItemDownload?.Invoke(new List<ContentItemDescriptor> { contentItemDescriptor });
		}

		private void ContentVisualElement_OnDownloadMany(List<ContentItemDescriptor> contentItemDescriptors)
		{
			OnItemDownload?.Invoke(contentItemDescriptors);
		}

		/// <summary>
		/// Handles double-click of an <see cref="ContentItemDescriptor"/>
		/// </summary>
		private void ListView_OnItemChosen(object obj)
		{
			if (obj == null) return;

			ContentItemDescriptor contentItemDescriptor = (ContentItemDescriptor)obj;

			SelectItemInInspectorWindow(contentItemDescriptor);
			PingItemInProjectWindow(contentItemDescriptor);
		}

		/// <summary>
		/// Handles single-click of an <see cref="ContentItemDescriptor"/>
		/// </summary>
		private void ListView_OnSelectionChanged(IEnumerable<object> objs)
		{
			var contentItemDescriptors = objs.Cast<ContentItemDescriptor>().ToArray();

			SelectItemInInspectorWindow(contentItemDescriptors);
			OnSelectionChanged?.Invoke(contentItemDescriptors.ToList());
		}
	}
}
