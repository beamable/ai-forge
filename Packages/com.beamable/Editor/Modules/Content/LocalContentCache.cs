using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Content;
using Beamable.Coroutines;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Content
{
	public class LocalContentCache : ContentCache
	{
		private readonly Type _contentType;
		private readonly CoroutineService _coroutineService;
		private readonly ContentConfiguration _config;
		private readonly ContentDatabase _contentDatabase;

		public LocalContentCache(Type contentType, CoroutineService coroutineService, ContentConfiguration config, ContentDatabase contentDatabase)
		{
			_contentType = contentType;
			_coroutineService = coroutineService;
			_config = config;
			_contentDatabase = contentDatabase;
		}

		/// <summary>
		/// Resolve the requested content reference.
		/// If the content is requested from the current selected editor manifest namespace,
		///  then the content will always be reloaded from the AssetDatabase.
		///  Otherwise, the content must be downloaded from the remote realm. 
		/// </summary>
		public override Promise<IContentObject> GetContentObject(ClientContentInfo requestedInfo)
		{
			if (!_contentDatabase.TryGetContentById(requestedInfo.contentId, out var entry))
			{
				return null;
			}

			var content = (ContentObject)AssetDatabase.LoadAssetAtPath(entry.assetPath, _contentType);
			var delayPromise = new Promise();
			IEnumerator Delay()
			{
				var delayTime = _config.LocalContentReferenceDelaySeconds.GetOrElse(.15f);
				yield return new WaitForSecondsRealtime(delayTime);
				delayPromise.CompleteSuccess();
			}

			_coroutineService.StartNew("content-ref-delay", Delay());
			content.SetManifestID(requestedInfo.manifestID);
			return delayPromise.Map(_ => (IContentObject)content);
		}
	}
}
