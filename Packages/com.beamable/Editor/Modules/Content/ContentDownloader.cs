using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Content;
using Beamable.Common.Content.Serialization;
using Beamable.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Content
{
	public delegate void HandleContentProgress(float progress, int processed, int total);

	public delegate void HandleDownloadFinished(Promise<Unit> promise);

	public class ContentDownloader
	{
		private readonly IBeamableRequester _requester;
		private readonly ContentIO _io;

		public ContentDownloader(IBeamableRequester requester, ContentIO io)
		{
			_requester = requester;
			_io = io;
		}

		public Promise<Unit> Download(DownloadSummary summary, HandleContentProgress progressCallback = null)
		{
			progressCallback?.Invoke(0, 0, summary.TotalDownloadEntries);

			var serializer = new ClientContentSerializer();
			var totalOperations = summary.TotalDownloadEntries + 1; // one operation for the mega finalization...

			var completed = 0f;

			var contentTypeReflectionCache = BeamEditor.GetReflectionSystem<ContentTypeReflectionCache>();
			bool disableExceptions = ContentConfiguration.Instance.DisableContentDownloadExceptions;

			var downloadPromiseGenerators = summary.GetAllDownloadEntries().Select(operation =>
			{
				var type = operation.ContentId.Split('.')[0];
				if (!contentTypeReflectionCache.HasContentTypeValidClass(type))
				{
					Debug.LogWarning($"No C# class found for type=[{type}]. Skipping download process for this type.");
					return null;
				}

				return new Func<Promise<Tuple<ContentObject, string>>>(() => FetchContentFromCDN(operation.Uri).Map(response =>
				{
					var contentType = contentTypeReflectionCache.GetTypeFromId(operation.ContentId);
					var newAsset = serializer.DeserializeByType(response, contentType, disableExceptions);
					newAsset.Tags = operation.Tags;

					newAsset.LastChanged = operation.LastChanged == 0
						? operation.Created == 0 ? 0 : operation.Created
						: operation.LastChanged;

					completed += 1;
					progressCallback?.Invoke(completed / totalOperations, (int)completed, totalOperations);

					return new Tuple<ContentObject, string>(newAsset, operation.AssetPath);
				}));
			}).ToList();

			downloadPromiseGenerators.RemoveAll(item => item == null);

			var downloadPromises = new Promise<Unit>();
			var batchSize = ContentConfiguration.Instance.EditorDownloadBatchSize.GetOrElse(100);
			Promise.ExecuteInBatchSequence(batchSize, downloadPromiseGenerators).Map(assetsToBeWritten =>
			{
				_io.CreateBatch(assetsToBeWritten);

				progressCallback?.Invoke(1, summary.TotalDownloadEntries, summary.TotalDownloadEntries);
				downloadPromises.CompleteSuccess(PromiseBase.Unit);
				return PromiseBase.Unit;
			});
			return downloadPromises;

		}

		private Promise<string> FetchContentFromCDN(string uri)
		{
			return _requester.Request(Method.GET, uri, includeAuthHeader: false, parser: s => s);
		}
	}

	public class DownloadSummary
	{
		private readonly LocalContentManifest _localManifest;
		private readonly Manifest _serverManifest;

		private Dictionary<string, ContentDownloadEntryDescriptor> _idToDescriptor;

		private IList<ContentDownloadEntryDescriptor> _overwrites;
		private IList<ContentDownloadEntryDescriptor> _additions;

		public DownloadSummary(ContentIO contentIO, LocalContentManifest localManifest, Manifest serverManifest, params string[] contentIdFilters)
		{
			_localManifest = localManifest;
			_serverManifest = serverManifest;

			_idToDescriptor = new Dictionary<string, ContentDownloadEntryDescriptor>();
			_overwrites = new List<ContentDownloadEntryDescriptor>();
			_additions = new List<ContentDownloadEntryDescriptor>();

			var set = new HashSet<string>(contentIdFilters);
			foreach (var reference in serverManifest.References)
			{
				if (set.Count > 0 && !set.Contains(reference.Id))
				{
					continue; // don't download this.
				}

				if (reference.Visibility != "public") continue;

				var assetPath = ""; // default.
				var exists = false;
				if (localManifest.Content.TryGetValue(reference.Id, out var localEntry))
				{
					assetPath = localEntry.AssetPath;
					exists = true;

					var checksum = contentIO.Checksum(localEntry.Content);
					var distinctTagsExist = ContentIO.AreTagsEqual(localEntry.Tags, reference.Tags);
					if (checksum == reference.Checksum && distinctTagsExist)
					{
						continue; // already up to date.
					}
				}

				var descriptor = new ContentDownloadEntryDescriptor
				{
					AssetPath = assetPath,
					ContentId = reference.Id,
					Uri = reference.Uri,
					Operation = exists ? "MODIFY" : "ADD",
					Tags = reference.Tags,
					Created = reference.Created,
					LastChanged = reference.LastChanged
				};

				var list = exists ? _overwrites : _additions;
				list.Add(descriptor);

				_idToDescriptor.Add(reference.Id, descriptor);
			}
		}

		public int TotalDownloadEntries => _idToDescriptor.Count;
		public IEnumerable<ContentDownloadEntryDescriptor> GetAllDownloadEntries()
		{
			foreach (var kvp in _idToDescriptor)
			{
				yield return kvp.Value;
			}
		}

		public IEnumerable<ContentDownloadEntryDescriptor> Overwrites => _overwrites.ToList();
		public IEnumerable<ContentDownloadEntryDescriptor> Additions => _additions.ToList();

		public bool AnyOverwrites => _overwrites.Count > 0;
		public bool AnyAdditions => _additions.Count > 0;
	}

	public struct ContentDownloadEntryDescriptor
	{
		public string ContentId;
		public string AssetPath;
		public string Uri;
		public string Operation;
		public string[] Tags;
		public long Created;
		public long LastChanged;
		public bool IsCorrupted;
	}
}
