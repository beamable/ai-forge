using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Common.Dependencies;
using Beamable.Content;
using Beamable.Coroutines;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Beamable.Editor.Content
{
	[BeamContextSystem()]
	public class RegisterEditorContentDependencies
	{
		[RegisterBeamableDependencies(Constants.SYSTEM_DEPENDENCY_ORDER + 1)]
		public static void Register(IDependencyBuilder builder)
		{
			var tempScope = builder.Build();
			var config = tempScope.GetService<ContentParameterProvider>();

			if (config.EnableLocalContentInEditor)
			{
				builder.AddSingleton<IContentIO, ContentIO>();
				builder.ReplaceSingleton<IManifestResolver, LocalManifestResolver>();
				builder.ReplaceSingleton<IContentCacheFactory, LocalContentCacheFactory>();
				builder.AddSingleton<DefaultContentCacheFactory>();
				builder.AddSingleton<IValidationContext>(p => p.GetService<ValidationContext>());
				builder.AddSingleton<ValidationContext>();
				builder.AddSingleton<ContentDatabase>();

			}
		}
	}

	public class LocalContentCacheFactory : IContentCacheFactory
	{
		private readonly IDependencyProvider _provider;
		private readonly CoroutineService _coroutineService;
		private readonly ContentConfiguration _config;
		private readonly IContentCacheFactory _defaultFactory;
		public LocalContentCacheFactory(IDependencyProvider provider, CoroutineService coroutineService, ContentConfiguration config)
		{
			_provider = provider;
			_coroutineService = coroutineService;
			_config = config;
			_defaultFactory = provider.GetService<DefaultContentCacheFactory>();
		}

		/// <summary>
		/// Creates a local content cache for the given content type. The content cache will use the local on-disk content
		/// to resolve cache misses. 
		/// </summary>
		public ContentCache CreateCache(ContentService service, string manifestId, Type contentType)
		{
			if (!string.Equals(manifestId, _config.EditorManifestID))
			{
				return _defaultFactory.CreateCache(service, manifestId, contentType);
			}
			return new LocalContentCache(contentType, _coroutineService, _config, _provider.GetService<ContentDatabase>());
		}
	}

	public class LocalManifestResolver : IManifestResolver
	{
		private readonly IContentIO _contentIO;
		private readonly ContentConfiguration _config;
		private readonly CoroutineService _coroutineService;
		private LocalContentManifest _localManifest;
		private readonly IManifestResolver _defaultResolver;

		public LocalManifestResolver(IContentIO contentIO, ContentConfiguration config, CoroutineService coroutineService)
		{
			_contentIO = contentIO;
			_config = config;
			_coroutineService = coroutineService;
			_localManifest = _contentIO.BuildLocalManifest();
			_defaultResolver = new DefaultManifestResolver();
		}

		/// <summary>
		/// Gets the manifest from the local on-disk content data.
		/// If the requested manifest ID is *NOT* the currently configured editor manifest,
		/// then the <see cref="DefaultManifestResolver.ResolveManifest"/> function must be used to download the manifest.
		///
		/// If this manifest can be sourced locally, artifical delay may be injected based on the value of <see cref="ContentConfiguration.LocalContentManifestDelaySeconds"/>
		/// </summary>
		public Promise<ClientManifest> ResolveManifest(IBeamableRequester requester, string url, ManifestSubscription subscription)
		{
			if (!string.Equals(subscription.ManifestID, _config.EditorManifestID))
			{
				// we don't have access to this manifest locally, so we'll need to go get it from the remote :( 
				return _defaultResolver.ResolveManifest(requester, url, subscription);
			}

			var manifest = new ClientManifest
			{
				entries = _localManifest.Content
										.Select(kvp => new ClientContentInfo
										{
											contentId = kvp.Value.Id,
											manifestID = subscription.ManifestID,
											tags = kvp.Value.Tags,
											uri = kvp.Value.AssetPath,
											type = kvp.Value.TypeName,
											version = kvp.Value.Version,
											visibility = ContentVisibility.Public
										})
										.ToList()
			};

			if (manifest.entries.Count == 0)
			{
				Debug.LogWarning(@"You are using local content mode, but there was no local content found! Did you forget to download content?
You can change the content mode with the <i>Project Settings/Beamable/Content/Enable Local Content In Editor</i> option.");
			}

			var delayPromise = new Promise();
			// simulate some delay... 
			IEnumerator Delay()
			{
				var delay = _config.LocalContentManifestDelaySeconds.GetOrElse(0);
				yield return new WaitForSeconds(delay);
				delayPromise.CompleteSuccess();
			}
			_coroutineService.StartNew("local-content-delay", Delay());
			return delayPromise.Map(_ => manifest);
		}
	}

}
