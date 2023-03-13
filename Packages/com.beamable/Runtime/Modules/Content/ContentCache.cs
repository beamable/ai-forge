using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Content;
using Beamable.Common.Content.Serialization;
using Beamable.Common.Spew;
using Beamable.Coroutines;
using Core.Platform.SDK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Beamable.Common.Constants.Features.Content;
using Debug = UnityEngine.Debug;

namespace Beamable.Content
{
	public struct ContentCacheEntry<TContent> where TContent : ContentObject
	{
		public readonly string Version;
		public readonly Promise<TContent> Content;

		public ContentCacheEntry(string version, Promise<TContent> content)
		{
			Version = version;
			Content = content;
		}
	}

	public abstract class ContentCache
	{
		public abstract Promise<IContentObject> GetContentObject(ClientContentInfo requestedInfo);
	}

	public class ContentCache<TContent> : ContentCache where TContent : ContentObject, new()
	{
		private static readonly ClientContentSerializer _serializer = new ClientContentSerializer();
		private static readonly string _bakedDataExtractedKey = "beamable_baked_data_extracted";

		private readonly Dictionary<string, ContentCacheEntry<TContent>> _cache =
			new Dictionary<string, ContentCacheEntry<TContent>>();

		private readonly IHttpRequester _requester;
		private readonly IBeamableFilesystemAccessor _filesystemAccessor;
		private readonly CoroutineService _coroutineService;
		private readonly ContentService _contentService;

		private const float WriteToFileDelay = 5;

		public ContentCache(IHttpRequester requester, IBeamableFilesystemAccessor filesystemAccessor, ContentService contentService, CoroutineService coroutineService)
		{
			_requester = requester;
			_filesystemAccessor = filesystemAccessor;
			_coroutineService = coroutineService;
			_contentService = contentService;
		}

		public override Promise<IContentObject> GetContentObject(ClientContentInfo requestedInfo)
		{
			return GetContent(requestedInfo).Map(content => (IContentObject)content);
		}

		private static string GetCacheKey(ClientContentInfo requestedInfo)
		{
			return $"{requestedInfo.contentId}:{requestedInfo.manifestID}:{requestedInfo.version}";
		}

		private void SetCacheEntry(string cacheId, ContentCacheEntry<TContent> entry)
		{
			_cache[cacheId] = entry;
		}

		public Promise<TContent> GetContent(ClientContentInfo requestedInfo)
		{
			var cacheId = GetCacheKey(requestedInfo);

			// First, try the in memory cache
			PlatformLogger.Log(
				$"ContentCache: Fetching content from cache for {requestedInfo.contentId}: version: {requestedInfo.version}");
			if (_cache.TryGetValue(cacheId, out var cacheEntry)) return cacheEntry.Content;

			// Then, try the on disk cache
			PlatformLogger.Log(
				$"ContentCache: Loading content from disk for {requestedInfo.contentId}: version: {requestedInfo.version}");
			if (TryGetValueFromDisk(requestedInfo, out var diskContent, _filesystemAccessor))
			{
				var promise = Promise<TContent>.Successful(diskContent);
				SetCacheEntry(cacheId, new ContentCacheEntry<TContent>(requestedInfo.version, promise));
				return promise;
			}

			// Check baked file for requested content
			PlatformLogger.Log(
				$"ContentCache: Loading content from baked file for {requestedInfo.contentId}: version: {requestedInfo.version}");
			if (TryGetValueFromBaked(requestedInfo, out var bakedContent))
			{
				var promise = Promise<TContent>.Successful(bakedContent);
				SetCacheEntry(cacheId, new ContentCacheEntry<TContent>(requestedInfo.version, promise));
				return promise;
			}

			// Finally, if not found, fetch the content from the CDN
			PlatformLogger.Log(
				$"ContentCache: Fetching content from CDN for {requestedInfo.contentId}: version: {requestedInfo.version}");
			var fetchedContent = FetchContentFromCDN(requestedInfo)
				.Map(raw =>
				{
					// Write the content to disk
					UpdateDiskFile(requestedInfo, raw);
					return DeserializeContent(requestedInfo, raw);
				})
				.Error(err =>
				{
					_cache.Remove(cacheId);
					PlatformLogger.Log(
						$"ContentCache: Failed to resolve {requestedInfo.contentId} {requestedInfo.version} {requestedInfo.uri} ; ERR={err}");
				});
			SetCacheEntry(cacheId, new ContentCacheEntry<TContent>(requestedInfo.version, fetchedContent));
			return fetchedContent;
		}

		private bool TryGetValueFromDisk(ClientContentInfo info, out TContent content,
			IBeamableFilesystemAccessor fsa)
		{
			var filePath = ContentPath(fsa);
			if (File.Exists(filePath) && _contentService.ContentDataInfo == null)
			{
				var fileContent = File.ReadAllText(filePath);
				_contentService.ContentDataInfo = _contentService.DeserializeDataCache<ContentDataInfoWrapper>(fileContent);
			}

			if (_contentService.ContentDataInfo != null)
			{
				var contentInfo = _contentService.ContentDataInfo.content.Find(item => item.contentId == info.contentId);
				if (contentInfo != null)
				{
					var deserialized = DeserializeContent(info, contentInfo.data);
					if (deserialized.Version == info.version)
					{
						content = deserialized;
						return true;
					}
				}
			}

			content = null;
			return false;
		}

		private bool TryGetValueFromBaked(ClientContentInfo info, out TContent contentObject)
		{
			contentObject = null;

			bool dataExtracted = PlayerPrefs.GetInt(_bakedDataExtractedKey) == 1;
			if (!dataExtracted)
			{
				if (ExtractContent())
				{
					return TryGetValueFromDisk(info, out contentObject, _filesystemAccessor);
				}
				PlayerPrefs.SetInt(_bakedDataExtractedKey, 1);
			}

			return false;
		}

		private bool ExtractContent()
		{
			var bakedFile = Resources.Load<TextAsset>(BAKED_FILE_RESOURCE_PATH);

			if (bakedFile == null)
			{
				return false;
			}

			string json = bakedFile.text;
			ContentDataInfoWrapper data;
			try
			{
				data = JsonUtility.FromJson<ContentDataInfoWrapper>(json);
			}
			catch
			{
				json = Gzip.Decompress(bakedFile.bytes);
				data = JsonUtility.FromJson<ContentDataInfoWrapper>(json);
			}

			if (data == null)
			{
				return false;
			}

			// save baked data to disk
			string path = ContentPath(_filesystemAccessor);

			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.WriteAllText(path, json);
			}
			catch (Exception e)
			{
				Debug.LogError($"[EXTRACT] Failed to write baked data to disk: {e.Message}");
				return false;
			}

			return true;
		}

		private Coroutine _updateDiskFileCoroutine;
		private IEnumerator WriteToDisk()
		{
			yield return new WaitForSeconds(WriteToFileDelay);
			var filePath = ContentPath(_filesystemAccessor);
			// Ensure the directory is created
			Directory.CreateDirectory(Path.GetDirectoryName(filePath));
			File.WriteAllText(filePath, JsonUtility.ToJson(_contentService.ContentDataInfo));
		}

		private void UpdateDiskFile(ClientContentInfo info, string raw)
		{
			try
			{
				if (_contentService.ContentDataInfo != null)
				{
					var existingContent = _contentService.ContentDataInfo.content.Find(obj => obj.contentId == info.contentId);
					if (existingContent != null)
					{
						existingContent.data = raw;
					}
					else
					{
						var newObject = new ContentDataInfo { contentId = info.contentId, data = raw };
						_contentService.ContentDataInfo.content.Add(newObject);
					}
				}
				else
				{
					_contentService.ContentDataInfo = new ContentDataInfoWrapper
					{
						content = { new ContentDataInfo { contentId = info.contentId, data = raw } }
					};
				}

				if (_updateDiskFileCoroutine != null)
				{
					_coroutineService.StopCoroutine(_updateDiskFileCoroutine);
				}

				_updateDiskFileCoroutine = _coroutineService.StartNew("ContentFileUpdate", WriteToDisk());
			}
			catch (Exception e)
			{
				PlatformLogger.Log($"ContentCache: Error saving content to disk: {e}");
			}
		}

		private static string ContentPath(IBeamableFilesystemAccessor fsa)
		{
			return fsa.GetPersistentDataPathWithoutTrailingSlash() + "/content/content.json";
		}

		private static TContent DeserializeContent(ClientContentInfo info, string raw)
		{
			var content = _serializer.Deserialize<TContent>(raw);
			content.SetManifestID(info.manifestID);
			return content;
		}


		private Promise<string> FetchContentFromCDN(ClientContentInfo info)
		{
			return _requester.ManualRequest(Method.GET, info.uri, parser: s => s);
		}
	}
}
