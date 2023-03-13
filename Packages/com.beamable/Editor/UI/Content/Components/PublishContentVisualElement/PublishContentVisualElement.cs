using Beamable.Common;
using Beamable.Content;
using Beamable.Editor.Content.Models;
using Beamable.Editor.UI.Common;
using Beamable.Editor.UI.Common.Models;
using Beamable.Editor.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants;
using static Beamable.Common.Constants.Features.ContentManager.Publish;

namespace Beamable.Editor.Content.Components
{
	public class PublishContentVisualElement : ContentManagerComponent
	{
		private LoadingBarElement _loadingBar;
		private Label _messageLabel;
		public event Action OnCancelled;
		public event Action OnCompleted;
		public event Action<ContentPublishSet, HandleContentProgress, HandleDownloadFinished> OnPublishRequested;
		public ContentDataModel DataModel { get; set; }
		public Promise<ContentPublishSet> PublishSet { get; set; }
		private PrimaryButtonVisualElement _publishBtn;
		private bool _completed;

		private VisualElement _manifestNameContainer;
		private TextField _manifestNameField;
		private bool _createNewManifest;
		private ManifestModel _manifestModel;
		private FormConstraint _isManifestNameValid;
		private Label _manifestArchivedMessage;
		private List<ContentPopupLinkVisualElement> _contentElements = new List<ContentPopupLinkVisualElement>();

		public bool CreateNewManifest
		{
			get => _createNewManifest;
			set
			{
				_createNewManifest = value;
				if (_manifestNameContainer != null) _manifestNameContainer.visible = value;
			}
		}
		public string ManifestName => _manifestNameField.value;

		public PublishContentVisualElement() : base(nameof(PublishContentVisualElement))
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			_loadingBar = Root.Q<LoadingBarElement>();
			_loadingBar.SmallBar = true;
			_loadingBar.Refresh();


			var mainContent = Root.Q<VisualElement>("publish-mainVisualElement");
			var loadingBlocker = Root.Q<LoadingIndicatorVisualElement>();

			_publishBtn = Root.Q<PrimaryButtonVisualElement>("publishBtn");

			_manifestNameContainer = Root.Q<VisualElement>("manifestNameContainer");
			_manifestNameContainer.visible = CreateNewManifest;
			_manifestNameField = _manifestNameContainer.Q<TextField>("manifestName");
			_manifestNameField.AddPlaceholder("Enter new Content Namespace");
			_manifestNameField.AddTextWrapStyle();

			_manifestArchivedMessage = Root.Q<Label>("manifestArchivedMessage");
			_manifestArchivedMessage.AddTextWrapStyle();

			Root.Q<Label>("manifestWarningMessage").AddTextWrapStyle();
			var manifestDocsLink = Root.Q<Label>("manifestDocsLink");
			manifestDocsLink.RegisterCallback<MouseDownEvent>(evt =>
			{
				Application.OpenURL(URLs.Documentations.URL_DOC_WINDOW_CONTENT_NAMESPACES);
			});

			if (CreateNewManifest)
			{
				if (_manifestModel == null)
				{
					_manifestModel = new ManifestModel();
					_manifestModel.OnAvailableElementsChanged += _ => _isManifestNameValid.Check();
					_manifestModel.Initialize();
				}

				_isManifestNameValid = _manifestNameField.AddErrorLabel("Manifest Namespace", name =>
				{
					_manifestArchivedMessage.EnableInClassList("visible",
						_manifestModel.ArchivedManifestModels?.Any(m => m.id == name) ?? false);
					if (!_completed && !ValidateManifestName(name, out var msg)) return msg;
					return null;
				});
				_isManifestNameValid.Check();
				_publishBtn.AddGateKeeper(_isManifestNameValid);

				_manifestModel.RefreshAvailableManifests();
			}
			else
			{
				manifestDocsLink.parent.Remove(manifestDocsLink);
				_manifestNameField.parent.Remove(_manifestNameField);
				_manifestNameContainer.parent.Remove(_manifestNameContainer);
			}

			_messageLabel = Root.Q<Label>("message");
			_messageLabel.visible = false;



			var overrideCountElem = Root.Q<CountVisualElement>("overrideCount");
			var addCountElem = Root.Q<CountVisualElement>("addInCount");
			var deleteCountElem = Root.Q<CountVisualElement>("deleted");

			var addFoldoutElem = Root.Q<Foldout>("addFoldout");
			addFoldoutElem.text = "Additions";
			var addSource = new List<ContentDownloadEntryDescriptor>();
			var addList = new ListView
			{
				itemsSource = addSource,
				makeItem = MakeElement,
				bindItem = CreateBinder(addSource)
			};
			addList.SetItemHeight(24);
			addFoldoutElem.contentContainer.Add(addList);

			var modifyFoldoutElem = Root.Q<Foldout>("modifyFoldout");
			modifyFoldoutElem.text = "Modifications";
			var modifySource = new List<ContentDownloadEntryDescriptor>();
			var modifyList = new ListView
			{
				itemsSource = modifySource,
				makeItem = MakeElement,
				bindItem = CreateBinder(modifySource)
			};
			modifyList.SetItemHeight(24);
			modifyFoldoutElem.contentContainer.Add(modifyList);


			var deleteFoldoutElem = Root.Q<Foldout>("deleteFoldout");
			deleteFoldoutElem.text = "Deletions";
			var deleteSource = new List<ContentDownloadEntryDescriptor>();
			var deleteList = new ListView
			{
				itemsSource = deleteSource,
				makeItem = MakeElement,
				bindItem = CreateBinder(deleteSource)
			};
			deleteList.SetItemHeight(24);
			deleteFoldoutElem.contentContainer.Add(deleteList);

			var cancelBtn = Root.Q<GenericButtonVisualElement>("cancelBtn");
			cancelBtn.OnClick += CancelButton_OnClicked;

			var promise = PublishSet.Then(publishSet =>
			{
				SetPublishMessage();

				overrideCountElem.SetValue(publishSet.ToModify.Count);
				addCountElem.SetValue(publishSet.ToAdd.Count);
				deleteCountElem.SetValue(publishSet.ToDelete.Count);

				_publishBtn.Button.clickable.clicked += PublishButton_OnClicked;

				var noPublishLabel = Root.Q<Label>("noPublishLabel");
				noPublishLabel.text = PUBLISH_NO_DATA_TEXT;
				noPublishLabel.AddTextWrapStyle();
				if (publishSet.totalOpsCount > 0)
				{
					noPublishLabel.parent.Remove(noPublishLabel);
				}

				foreach (var toAdd in publishSet.ToAdd)
				{
					if (DataModel.GetDescriptorForId(toAdd.Id, out var desc))
					{
						var data = new ContentDownloadEntryDescriptor
						{
							AssetPath = desc.AssetPath,
							ContentId = toAdd.Id,
							Operation = "upload",
							Tags = toAdd.Tags,
							Uri = "",
							LastChanged = desc.LastChanged
						};
						addSource.Add(data);
					}
				}

				addFoldoutElem.Q<ListView>().style.SetHeight(addList.GetItemHeight() * addSource.Count, true);
				addList.RefreshPolyfill();

				foreach (var toModify in publishSet.ToModify)
				{
					if (DataModel.GetDescriptorForId(toModify.Id, out var desc))
					{
						var data = new ContentDownloadEntryDescriptor
						{
							AssetPath = desc.AssetPath,
							ContentId = toModify.Id,
							Operation = "modify",
							Tags = toModify.Tags,
							Uri = "",
							LastChanged = desc.LastChanged
						};
						modifySource.Add(data);
					}
				}

				modifyFoldoutElem.Q<ListView>().style.SetHeight(modifyList.GetItemHeight() * modifySource.Count, true);
				modifyList.RefreshPolyfill();

				foreach (var toDelete in publishSet.ToDelete)
				{
					if (DataModel.GetDescriptorForId(toDelete, out var desc))
					{
						var data = new ContentDownloadEntryDescriptor
						{
							AssetPath = desc.AssetPath,
							ContentId = toDelete,
							Tags = desc.ServerTags?.ToArray(),
							Operation = "delete",
							Uri = "",
							LastChanged = desc.LastChanged
						};
						deleteSource.Add(data);
					}
				}

				deleteFoldoutElem.Q<ListView>().style.SetHeight(deleteList.GetItemHeight() * deleteSource.Count, true);
				deleteList.RefreshPolyfill();



				if (publishSet.ToAdd.Count == 0)
				{
					addList.parent.Remove(addList);
					addFoldoutElem.parent.Remove(addFoldoutElem);

				}

				if (publishSet.ToModify.Count == 0)
				{
					modifyList.parent.Remove(modifyList);
					modifyFoldoutElem.parent.Remove(modifyFoldoutElem);
				}

				if (publishSet.ToDelete.Count == 0)
				{
					deleteList.parent.Remove(deleteList);
					deleteFoldoutElem.parent.Remove(deleteFoldoutElem);
				}


			});

			loadingBlocker.SetPromise(promise, mainContent).SetText(PUBLISH_MESSAGE_LOADING);
		}

		private void DetailButton_OnClicked()
		{
			DataModel.ToggleStatusFilter(ContentModificationStatus.LOCAL_ONLY, true);
			DataModel.ToggleStatusFilter(ContentModificationStatus.MODIFIED, true);
			DataModel.ToggleStatusFilter(ContentModificationStatus.SERVER_ONLY, true);
		}

		private void CancelButton_OnClicked()
		{
			OnCancelled?.Invoke();
		}

		private void PublishButton_OnClicked()
		{
			if (_completed)
			{
				OnCompleted?.Invoke();
			}
			else
			{
				var _ = HandlePublish();
			}
		}

		private async Task HandlePublish()
		{
			_manifestNameField.SetEnabled(false);
			if (_createNewManifest && _manifestModel.ArchivedManifestModels.Any(m => m.id == ManifestName))
			{
				var api = BeamEditorContext.Default;
				await api.InitializePromise;

				var unarchiveTask = api.ContentIO.UnarchiveManifest(ManifestName);
				_publishBtn.Load(unarchiveTask);
				await unarchiveTask;
			}
			var publishSet = PublishSet.GetResult();
			SetPublishMessage();

			_loadingBar.RunWithoutUpdater = true;
			OnPublishRequested?.Invoke(publishSet, (progress, processed, total) => { _loadingBar.Progress = progress; },
				promise =>
				{
					_publishBtn.Load(promise);
					promise.Then(_ =>
					{
						_completed = true;
						_messageLabel.text = PUBLISH_COMPLETE_MESSAGE;
						_publishBtn.SetText("Okay");
						_loadingBar.RunWithoutUpdater = false;
						MarkChecked();
					});
				});
		}

		private ContentPopupLinkVisualElement MakeElement()
		{
			var contentPopupLinkVisualElement = new ContentPopupLinkVisualElement();
			_contentElements.Add(contentPopupLinkVisualElement);
			return contentPopupLinkVisualElement;
		}

		private void MarkChecked()
		{
			MarkDirtyRepaint();
			EditorApplication.delayCall += () =>
			{
				foreach (var contentElement in _contentElements)
					contentElement.MarkChecked();
				MarkDirtyRepaint();
			};
		}

		private Action<VisualElement, int> CreateBinder(List<ContentDownloadEntryDescriptor> source)
		{
			return (elem, index) =>
			{
				var link = elem as ContentPopupLinkVisualElement;
				link.Model = source[index];
				link.Refresh();
			};
		}

		private bool ValidateManifestName(string name, out string message)
		{
			const int MAX_NAME_LENGTH = 36;
			const int MIN_NAME_LENGTH = 2;

			message = null;
			if (string.IsNullOrWhiteSpace(name))
			{
				message = "Name can not be empty string.";
				return false;
			}

			if (name.TrimStart(' ') != name)
			{
				message = "Name cannot start with leading spaces.";
				return false;
			}

			if (name.TrimEnd(' ') != name)
			{
				message = "Name cannot end with trailing spaces.";
				return false;
			}

			if (name.Length > MAX_NAME_LENGTH)
			{
				message = $"Name can not be longer then {MAX_NAME_LENGTH} characters.";
				return false;
			}


			if (char.IsDigit(name[0]))
			{
				message = "Name cannot start with a number.";
				return false;
			}

			if (name.Length < MIN_NAME_LENGTH)
			{
				message = $"Name must be at least {MIN_NAME_LENGTH} characters long.";
				return false;
			}

			if (name.Any(char.IsUpper))
			{
				message = $"Name cannot have any uppercase letters.";
				return false;
			}

			if (!PrimaryButtonVisualElement.IsSlug(name))
			{
				message = "Name can contain only letters, digits, and dashes.";
				return false;
			}

			if (_manifestModel?.Elements == null)
			{
				message = "Checking existing namespaces...";
				return false;
			}

			if (_manifestModel.Elements.Any(m => m.DisplayName == name))
			{
				message = "This namespace already exists.";
				return false;
			}

			return true;
		}

		private void SetPublishMessage()
		{
			var api = BeamEditorContext.Default;
			_messageLabel.visible = true;
			_messageLabel.AddTextWrapStyle();
			_messageLabel.text = string.Format(PUBLISH_MESSAGE_PREVIEW, api.CurrentRealm.DisplayName, ContentConfiguration.Instance.EditorManifestID);

		}
	}
}

