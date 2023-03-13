using Beamable.Common;
using Beamable.Editor.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants.Features.ContentManager.Download;

namespace Beamable.Editor.Content.Components
{
	public class DownloadContentVisualElement : ContentManagerComponent
	{
		public Promise<DownloadSummary> Model { get; set; }

		// Proceed a refresh for content manager when downloads succeeded
		public event Action OnRefreshContentManager;
		public event Action OnCancelled;
		public event Action OnClosed;
		public event Action<DownloadSummary, HandleContentProgress, HandleDownloadFinished> OnDownloadStarted;

		private GenericButtonVisualElement _cancelBtn;
		private LoadingBarElement _loadingBar;
		private PrimaryButtonVisualElement _downloadBtn;

		private List<ContentPopupLinkVisualElement> _contentElements = new List<ContentPopupLinkVisualElement>();
		private ListView _modifiedList;
		private ListView _addList;
		private bool _allDownloadsComplete;
		protected Label _messageLabel;

		public DownloadContentVisualElement() : base(nameof(DownloadContentVisualElement))
		{
		}

		public DownloadContentVisualElement(string name) : base(name)
		{
		}

		public override void Refresh()
		{
			base.Refresh();
			var mainElement = Root.Q<VisualElement>("mainVisualElement");
			var loadingBlocker = Root.Q<LoadingIndicatorVisualElement>();
			var promise = Model.Then(summary =>
			{
				SetMessageLabel();

				_cancelBtn = Root.Q<GenericButtonVisualElement>("cancelBtn");
				_cancelBtn.OnClick += CancelButton_OnClicked;

				_downloadBtn = Root.Q<PrimaryButtonVisualElement>("downloadBtn");
				_downloadBtn.Button.clickable.clicked += DownloadButton_OnClicked;

				_loadingBar = Root.Q<LoadingBarElement>();
				_loadingBar.SmallBar = true;
				_loadingBar.Refresh();

				var noDownloadLabel = Root.Q<Label>("noDownloadLbl");
				noDownloadLabel.text = DOWNLOAD_NO_DATA_TEXT;
				noDownloadLabel.AddTextWrapStyle();

				// TODO show preview of download content.
				var modifiedFold = Root.Q<Foldout>("overwriteFoldout");
				modifiedFold.text = "Overwrites";
				var modifiedSource = new List<ContentDownloadEntryDescriptor>();
				_modifiedList = new ListView
				{
					itemsSource = modifiedSource,
					makeItem = MakeElement,
					bindItem = CreateBinder(modifiedSource)
				};
				_modifiedList.SetItemHeight(24);
				modifiedFold.contentContainer.Add(_modifiedList);

				var tmpModified = GetModiffiedSource(summary);
				SetFold(modifiedFold, tmpModified, modifiedSource, _modifiedList);
				var overrideCount = Root.Q<CountVisualElement>("overrideCount");
				overrideCount.SetValue(tmpModified.Count());

				var additionFold = Root.Q<Foldout>("addFoldout");
				additionFold.text = "Additions";
				var addSource = new List<ContentDownloadEntryDescriptor>();
				_addList = new ListView
				{

					itemsSource = addSource,
					makeItem = MakeElement,
					bindItem = CreateBinder(addSource)
				};
				_addList.SetItemHeight(24);
				additionFold.contentContainer.Add(_addList);

				var tmpAdditional = GetAdditionSource(summary);
				var addInCount = Root.Q<CountVisualElement>("addInCount");
				addInCount.SetValue(tmpAdditional.Count());

				//var tmpRemoved = GetRemoveSource(summary);
				//tmpAdditional.AddRange(tmpRemoved);

				SetFold(additionFold, tmpAdditional, addSource, _addList);

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

				var tmpDeletions = GetDeleteSource(summary);
				SetFold(deleteFoldoutElem, tmpDeletions, deleteSource, deleteList);

				if (tmpAdditional.Count > 0 || tmpModified.Count > 0 || tmpDeletions.Count > 0)
				{
					noDownloadLabel.parent.Remove(noDownloadLabel);
				}
				else
				{
					_allDownloadsComplete = true;
					_downloadBtn.SetText("Okay");
				}

				var deleteCount = Root.Q<CountVisualElement>("deleteCount");
				deleteCount.SetValue(tmpDeletions.Count());
				var removeLabel = Root.Q<Label>("deleteLabel");

				if (tmpDeletions.Count == 0)
				{
					deleteCount.parent.Remove(deleteCount);
					removeLabel.parent.Remove(removeLabel);
				}
			});
			loadingBlocker.SetPromise(promise, mainElement).SetText(DOWNLOAD_LOAD_TEXT);
		}

		private void SetFold(Foldout foldout, List<ContentDownloadEntryDescriptor> entries, List<ContentDownloadEntryDescriptor> source, ListView listView)
		{
			if (entries.Count > 0)
			{
				source.AddRange(entries);
				foldout.Q<ListView>().style.SetHeight(_modifiedList.GetItemHeight() * entries.Count(), true);
				listView.RefreshPolyfill();
			}
			else
			{
				foldout.parent.Remove(foldout);
			}
		}

		protected virtual List<ContentDownloadEntryDescriptor> GetModiffiedSource(DownloadSummary summary)
		{
			return summary.Overwrites.ToList();
		}

		protected virtual List<ContentDownloadEntryDescriptor> GetAdditionSource(DownloadSummary summary)
		{
			return summary.Additions.ToList();
		}

		protected virtual List<ContentDownloadEntryDescriptor> GetDeleteSource(DownloadSummary summary)
		{
			return new List<ContentDownloadEntryDescriptor>();
		}

		protected virtual void SetMessageLabel()
		{
			_messageLabel = Root.Q<Label>("message");
			_messageLabel.text = DOWNLOAD_MESSAGE_TEXT;
			_messageLabel.AddTextWrapStyle();
		}

		protected virtual void SetDownloadSuccessMessageLabel()
		{
			_messageLabel.text = DOWNLOAD_COMPLETE_TEXT;
		}

		protected virtual void OnDownloadSuccess()
		{
			_downloadBtn.SetText("Okay");
			_allDownloadsComplete = true;
			_loadingBar.Progress = 1;
			_loadingBar.RunWithoutUpdater = false;

			// Mark all as checked
			this.MarkDirtyRepaint();
			EditorApplication.delayCall += () =>
			{
				foreach (var contentElement in _contentElements)
					contentElement.MarkChecked();
				this.MarkDirtyRepaint();
			};
		}

		private ContentPopupLinkVisualElement MakeElement()
		{
			var contentPopupLinkVisualElement = new ContentPopupLinkVisualElement();
			_contentElements.Add(contentPopupLinkVisualElement);
			// return new ContentPopupLinkVisualElement();
			return contentPopupLinkVisualElement;
		}

		private Action<VisualElement, int> CreateBinder(List<ContentDownloadEntryDescriptor> source)
		{
			return (elem, index) =>
			{
				var link = elem as ContentPopupLinkVisualElement;
				link.Model = source[index];
				if (_allDownloadsComplete)
				{
					link.MarkChecked();
				}
				link.Refresh();
			};
		}

		private void CancelButton_OnClicked()
		{
			// TODO Be smarter about how we cancel the download.
			OnCancelled?.Invoke();
			OnRefreshContentManager?.Invoke();
		}

		private void DownloadButton_OnClicked()
		{
			if (_allDownloadsComplete)
			{
				OnClosed?.Invoke();
				OnRefreshContentManager?.Invoke();
			}
			else
			{
				HandleDownload();
			}
		}


		private void HandleDownload()
		{
			var lastProcessed = 0;
			_loadingBar.RunWithoutUpdater = true;
			OnDownloadStarted?.Invoke(Model.GetResult(), (progress, processed, total) =>
			{
				_loadingBar.Progress = progress;
				//Mark element as checked
				for (var i = lastProcessed; i < processed; i++)
				{
					var contentElement = _contentElements[i];
					contentElement.MarkChecked();
				}
				lastProcessed = processed;

			}, finalPromise =>
			   {
				   _downloadBtn.Load(finalPromise);

				   finalPromise.Then(_ =>
				{
					SetDownloadSuccessMessageLabel();
					OnDownloadSuccess();

				}).Error(_ =>
				   {
					   _loadingBar.Progress = 1;
					   _loadingBar.RunWithoutUpdater = false;
					   // TODO make this error reporting better.
					   EditorApplication.delayCall += () =>
					   {
						   EditorUtility.DisplayDialog("Download Failed", "See console for errors.", "OK");
						   OnClosed?.Invoke();
					   };
				   });
			   });
		}
	}
}
