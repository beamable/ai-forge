using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Content;
using Beamable.Content;
using Beamable.Editor.Content.SaveRequest;
using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Beamable.Editor.Content
{
	public class ContentPublisher
	{
		public static event Action OnContentPublished;

		private readonly IBeamableRequester _requester;
		private readonly IContentIO _io;

		public ContentPublisher(IBeamableRequester requester, IContentIO io)
		{
			_requester = requester;
			_io = io;
		}

		[RuntimeInitializeOnLoadMethod]
		private static void Init()
		{
			OnContentPublished = null;
		}

		public Promise<ContentPublishSet> CreatePublishSet(string manifestId = null)
		{
			if (string.IsNullOrEmpty(manifestId))
			{
				manifestId = ContentConfiguration.Instance.EditorManifestID;
			}
			return _io.FetchManifest(manifestId).Map(serverManifest =>
			{
				BeamEditorContext.Default.ContentDatabase.RecalculateIndex();
				var allContent = _io.FindAll();
				var allContentAsList = allContent.ToList();
				var allContentLookup = allContentAsList.ToDictionary(content => content.Id);
				var allReferences = allContentAsList.Select(content => new ContentManifestReference()
				{
					checksum = _io.Checksum(content),
					id = content.Id,
					type = content.ContentType,
					tags = content.Tags,
					uri = "",
					version = "",
					visibility = ""
				}).ToList();

				var localManifest = new Manifest(allReferences);
				var diffSet = Manifest.FindDifferences(localManifest, serverManifest);

				return new ContentPublishSet
				{
					ServerManifest = serverManifest,
					ToAdd = diffSet.Additions.Select(reference => allContentLookup[reference.id]).ToList(),
					ToDelete = diffSet.Deletions.Select(reference => reference.id).ToList(),
					ToModify = diffSet.Modifications.Select(reference => allContentLookup[reference.id]).ToList()
				};
			}).Error(err =>
			{
				Debug.LogError(err);
			});
		}

		ContentDefinition PrepareContentForPublish(ContentObject content)
		{
			var definition = new ContentDefinition
			{
				Checksum = _io.Checksum(content),
				Id = content.Id,
				Content = content,
				Tags = content.Tags,
				LastChanged = content.LastChanged
			};

			return definition;
		}

		public Promise<Unit> ClearManifest()
		{
			var manifest = new ManifestSaveRequest()
			{
				Id = ContentConfiguration.Instance.EditorManifestID,
				References = new List<ManifestReferenceSuperset>()
			};
			return _requester.RequestJson<ContentManifest>(Method.POST, $"/basic/content/manifest?id={manifest.Id}", manifest)
			   .Map(response => new Unit());
		}

		/// <summary>
		/// Publishes the set of content to changes to the Comet server.
		/// </summary>
		/// <param name="publishSet">Set of content to publish</param>
		/// <param name="progressCallback">Callback to be invoked as the operation makes progress.</param>
		public Promise<Unit> Publish(ContentPublishSet publishSet, Action<PublishProgress> progressCallback)
		{
			var operation =
			   PushEachPieceOfContentToComet(publishSet, progressCallback)
				  .FlatMap(referenceSet =>
					 PushManifestToComet(referenceSet.Values.ToList(), publishSet.ManifestId))
				  .Then(_ =>
				  {
					  var totalOperations = publishSet.totalOpsCount + 1; // one comes from saving the new manifest...
																		  // We're done here. Just call the final progressCallback with a "finished" PublishProgress.
					  progressCallback(new PublishProgress
					  {
						  TotalOperations = totalOperations,
						  CompletedOperations = totalOperations
					  });
				  })
				  .Then(_ => OnContentPublished?.Invoke())
				  .Map(_ => new Unit());

			return operation;
		}

		private Promise<Dictionary<string, ManifestReferenceSuperset>> PushEachPieceOfContentToComet(ContentPublishSet publishSet, Action<PublishProgress> progressCallback)
		{
			var totalOperations = publishSet.totalOpsCount + 1; // one comes from saving the new manifest...
			var workingReferenceSet = publishSet.ServerManifest.References.ToDictionary(r => r.Key);

			var progressPromises = new List<Promise<int>>();

			void UpdateReference(ContentReference entry)
			{
				var reference = new ManifestReferenceSuperset
				{
					Checksum = entry.checksum,
					Id = entry.id,
					Tags = entry.tags,
					Uri = entry.uri,
					Version = entry.version,
					Visibility = entry.visibility,
					Type = "content",
					LastChanged = entry.lastChanged
				};
				var key = reference.Key;

				if (workingReferenceSet.ContainsKey(key))
				{
					workingReferenceSet[key] = reference;
				}
				else
				{
					workingReferenceSet.Add(key, reference);
				}
			}

			void RemoveReference(string id)
			{
				workingReferenceSet.Remove(ManifestReferenceSuperset.MakeKey(id, "public"));
				workingReferenceSet.Remove(ManifestReferenceSuperset.MakeKey(id, "private"));
			}

			void CallProgressCallback()
			{
				{
					var completedOperations = 0;
					foreach (var progress in progressPromises)
					{
						if (progress.IsCompleted)
						{
							completedOperations += progress.GetResult();
						}
					}
					progressCallback(new PublishProgress
					{
						TotalOperations = totalOperations,
						CompletedOperations = completedOperations
					});
				}
			}

			var contentToSave = new List<ContentObject>();
			contentToSave.AddRange(publishSet.ToAdd);
			contentToSave.AddRange(publishSet.ToModify);

			var promiseGenerators = new List<Func<Promise<int>>>();
			const int batchSize = 20;
			// Push Content to comet in batches
			for (var i = 0; i < contentToSave.Count; i += batchSize)
			{
				var batch = contentToSave.GetRange(i, Math.Min(contentToSave.Count - i, batchSize));
				var promiseGenerator = new Func<Promise<int>>(() =>
				{
					var promise = PushContentToComet(batch)
				   .Map(response =>
				   {
					   response.content.ForEach(UpdateReference);
					   return batch.Count;
				   })
				   .Then(_ => CallProgressCallback());
					progressPromises.Add(promise);
					return promise;
				});
				promiseGenerators.Add(promiseGenerator);
			}

			// Delete references
			for (var i = 0; i < publishSet.ToDelete.Count; i += batchSize)
			{
				var batch = publishSet.ToDelete.GetRange(i, Math.Min(batchSize, publishSet.ToDelete.Count - i));
				batch.ForEach(RemoveReference);
				progressPromises.Add(Promise<int>.Successful(batch.Count).Then(_ => CallProgressCallback()));
			}

			// Remove corrupted flags
			for (var i = 0; i < publishSet.ToModify.Count; i++)
			{
				publishSet.ToModify[i].ContentException = null;
			}

			return Promise.ExecuteSerially(promiseGenerators).FlatMap(__ =>
			   Promise.Sequence(progressPromises).Map(_ => workingReferenceSet));
		}

		/// <summary>
		/// Publishes the content objects to the Comet server.
		/// </summary>
		/// <param name="content">The content objects to publish</param>
		/// <returns></returns>
		private Promise<ContentSaveResponse> PushContentToComet(IEnumerable<ContentObject> content)
		{
			var contentDefs = new List<ContentDefinition>();
			foreach (var contentObj in content)
			{
				contentDefs.Add(PrepareContentForPublish(contentObj));
			}

			var dict = new ArrayDict
		 {
			{"content", contentDefs}
		 };
			var reqJson = Json.Serialize(dict, new StringBuilder());
			return _requester.Request<ContentSaveResponse>(Method.POST, "/basic/content", reqJson);

		}

		/// <summary>
		/// Publishes the content manifest to the Comet server.
		/// </summary>
		/// <param name="references"></param>
		/// <returns></returns>
		private Promise<ContentManifest> PushManifestToComet(List<ManifestReferenceSuperset> references, string manifestId)
		{
			var manifest = new ManifestSaveRequest
			{
				Id = ContentConfiguration.IsValidManifestID(manifestId, out var _) ?
				  manifestId :
				  ContentConfiguration.Instance.EditorManifestID,
				References = references
			};
			return _requester.RequestJson<ContentManifest>(Method.POST, $"/basic/content/manifest?id={manifest.Id}", manifest);
		}
	}
}
