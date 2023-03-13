using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Editor.Content.Models;
using Beamable.Editor.Modules.Account;
using Beamable.Editor.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static Beamable.Common.Constants;
using static Beamable.Common.Constants.Features.ContentManager;

namespace Beamable.Editor.Content
{
	public class ContentManager
	{
		public ContentDataModel Model { get; private set; } = new ContentDataModel();

		public void Initialize()
		{
			var de = BeamEditorContext.Default;
			Model.ContentIO = de.ContentIO;

			Model.UserCanPublish = de.Permissions.CanPushContent;
			de.OnUserChange -= HandleOnUserChanged;
			de.OnUserChange += HandleOnUserChanged;

			var localManifest = de.ContentIO.BuildLocalManifest();
			Model.SetLocalContent(localManifest);
			de.ContentIO.OnManifest.Then(manifest =>
			{
				Model.SetServerContent(manifest);
			});

			Model.OnSoftReset += () =>
			{
				var nextLocalManifest = de.ContentIO.BuildLocalManifest();
				Model.SetLocalContent(nextLocalManifest);
				RefreshServer();
			};

			var contentTypeReflectionCache = BeamEditor.GetReflectionSystem<ContentTypeReflectionCache>();
			Model.SetContentTypes(contentTypeReflectionCache.GetAll().ToList());

			ValidateContent(null, null); // start a validation in the background.

			ContentIO.OnContentsCreated += ContentIO_OnContentCreated;
			ContentIO.OnContentEntryDeleted += ContentIO_OnContentDeleted;
			ContentIO.OnContentRenamed += ContentIO_OnContentRenamed;
		}

		public void RefreshServer()
		{
			var de = BeamEditorContext.Default;
			de.ContentIO.FetchManifest().Then(manifest =>
			{
				Model.SetServerContent(manifest);
			});
		}

		public IContentObject AddItem()
		{
			TreeViewItem selectedTreeViewItem = Model.SelectedContentTypes.FirstOrDefault();
			ContentTypeTreeViewItem selectedContentTypeTreeViewItem = (ContentTypeTreeViewItem)selectedTreeViewItem;

			if (selectedContentTypeTreeViewItem == null)
			{
				BeamableLogger.LogError(new Exception("AddItem() failed. selectedContentTypeTreeViewItem must not be null."));
				return null;
			}

			return AddItem(selectedContentTypeTreeViewItem.TypeDescriptor);
		}

		public IContentObject AddItem(ContentTypeDescriptor typeDescriptor)
		{
			var itemType = typeDescriptor.ContentType;
			var itemName = GET_NAME_FOR_NEW_CONTENT_FILE_BY_TYPE(itemType);
			ContentObject content = ScriptableObject.CreateInstance(itemType) as ContentObject;
			content.SetContentName(itemName);
			content.LastChanged = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

			Model.CreateItem(content);
			return content;
		}

		public Promise<List<ContentExceptionCollection>> ValidateContent(HandleContentProgress progressHandler, HandleValidationErrors errorHandler)
		{
			var de = BeamEditorContext.Default;
			var contentValidator = new ContentValidator(de.ContentIO);
			var ctx = de.ContentIO.GetValidationContext();
			ContentObject.ValidationContext = ctx;
			var promise = contentValidator.Validate(ctx, Model.TotalContentCount, Model.GetAllContents(), progressHandler, errorHandler);
			return promise;
		}

		public Promise<Unit> PublishContent(ContentPublishSet publishSet, HandleContentProgress progressHandler, HandleDownloadFinished finishedHandler)
		{
			var de = BeamEditorContext.Default;
			var promise = de.ServiceScope.GetService<ContentPublisher>().Publish(publishSet, progress =>
			{
				progressHandler?.Invoke(progress.Progress, progress.CompletedOperations, progress.TotalOperations);
			});

			finishedHandler?.Invoke(promise);
			return promise.Map(_ =>
			{
				de.ContentIO.FetchManifest();
				return _;
			});

		}

		public Promise<Unit> DownloadContent(DownloadSummary summary, HandleContentProgress progressHandler, HandleDownloadFinished finishedHandler)
		{
			var de = BeamEditorContext.Default;
			var contentDownloader = new ContentDownloader(de.Requester, de.ContentIO);
			//Disallow updating anything while importing / refreshing
			var downloadPromise = contentDownloader.Download(summary, progressHandler);

			finishedHandler?.Invoke(downloadPromise);
			return downloadPromise;
		}

		/// <summary>
		/// Refresh the data and thus rendering of the <see cref="ContentManagerWindow"/>
		/// </summary>
		/// <param name="isHardRefresh">TODO: My though there is that false means keep the currently selected item. TBD if possible. - srivello</param>
		public async void RefreshWindow(bool isHardRefresh)
		{
			if (isHardRefresh)
			{
				var contentManagerWindow = await BeamEditorWindow<ContentManagerWindow>.GetFullyInitializedWindow();
				contentManagerWindow.BuildWithContext();
			}
			else
			{
				RefreshServer();
			}
		}

		public void ShowDocs()
		{
			Application.OpenURL(URLs.Documentations.URL_DOC_WINDOW_CONTENT_MANAGER);
		}

		private void ContentIO_OnContentDeleted(List<ContentDatabaseEntry> content)
		{
			Model.HandleContentsDeleted(content);
		}

		private void ContentIO_OnContentCreated(List<IContentObject> content)
		{
			Model.HandleContentAdded(content);
		}

		private void ContentIO_OnContentRenamed(string oldId, IContentObject content, string nextAssetPath)
		{
			Model.HandleContentRenamed(oldId, content, nextAssetPath);
		}

		public Promise<DownloadSummary> PrepareDownloadSummary(params ContentItemDescriptor[] filter)
		{
			// no matter what, we always want a fresh manifest locally and from the server.
			var de = BeamEditorContext.Default;
			return de.ContentIO.FetchManifest().Map(serverManifest =>
			{
				var localManifest = de.ContentIO.BuildLocalManifest();

				return new DownloadSummary(de.ContentIO, localManifest, serverManifest, filter.Select(x => x.Id).ToArray());
			});
		}

		public Promise<DownloadSummary> PrepareDownloadSummary(string[] ids)
		{
			var de = BeamEditorContext.Default;
			return de.ContentIO.FetchManifest().Map(serverManifest =>
			{
				var localManifest = de.ContentIO.BuildLocalManifest();
				return new DownloadSummary(de.ContentIO, localManifest, serverManifest, ids);
			});
		}

		public void Destroy()
		{
			var b = BeamEditorContext.Default;
			b.OnUserChange -= HandleOnUserChanged;
			ContentIO.OnContentsCreated -= ContentIO_OnContentCreated;
			ContentIO.OnContentEntryDeleted -= ContentIO_OnContentDeleted;
			ContentIO.OnContentRenamed -= ContentIO_OnContentRenamed;
		}

		private void HandleOnUserChanged(EditorUser user)
		{
			Model.UserCanPublish = BeamEditorContext.Default.Permissions.CanPushContent;
		}

		public Promise<ContentPublishSet> CreatePublishSet(bool newNamespace = false)
		{
			var manifestId = newNamespace
				? Guid.NewGuid().ToString()
				: null;

			var de = BeamEditorContext.Default;
			return de.ServiceScope.GetService<ContentPublisher>().CreatePublishSet(manifestId);
		}
	}
}
