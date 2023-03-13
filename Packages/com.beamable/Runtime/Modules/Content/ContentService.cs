using Beamable.Api;
using Beamable.Api.Caches;
using Beamable.Api.Connectivity;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Content;
using Beamable.Common.Content;
using Beamable.Common.Dependencies;
using Beamable.Coroutines;
using Beamable.Serialization.SmallerJSON;
using Core.Platform.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static Beamable.Common.Constants.Features.Content;

namespace Beamable.Content
{
	/// <summary>
	/// This type defines part of the %Beamable %ContentObject system.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class ManifestSubscription : PlatformSubscribable<ClientManifest, ClientManifest>
	{
		private readonly IDependencyProvider _provider;

		/// <summary>
		/// Every content manifest has an ID. Usually, a game will only have a single content manifest called "global", but it is possible to add more.
		/// </summary>
		public string ManifestID { get; } = "global";

		private ClientManifest _latestManifiest;

		private readonly Promise<Unit> _manifestPromise = new Promise<Unit>();

		private readonly Dictionary<string, ClientContentInfo> _contentIdTable =
			new Dictionary<string, ClientContentInfo>();

		/// <summary>
		/// This will be removed in a future release. Please do not use.
		/// </summary>
		[Obsolete]
		public Dictionary<Type, ContentCache> _contentCaches = new Dictionary<Type, ContentCache>();

		public ManifestSubscription(IDependencyProvider provider,
									string manifestID) : base(provider, "content")
		{
			_provider = provider;
			ManifestID = manifestID;
		}

		/// <summary>
		/// This method is obsolete. The scope field won't do anything, and there is no support for subscribing to individual content type updates.
		/// You should simply use the variant of this method that doesn't accept the scope field. <see cref="PlatformSubscribable{ScopedRsp,Data}.Subscribe()"/>
		/// </summary>
		/// <param name="scope"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
#pragma warning disable 0809
		[Obsolete("The ManifestSubscription doesn't support the scope field. Please use " + nameof(Subscribe) + " instead.")]
		public override PlatformSubscription<ClientManifest> Subscribe(string scope, Action<ClientManifest> callback)
		{
			return base.Subscribe(String.Empty, callback);
		}
#pragma warning restore 0809

		/// <summary>
		/// Try to retrieve a <see cref="ClientContentInfo"/> from the current manifest, by content id.
		/// </summary>
		/// <param name="contentId">A content ID</param>
		/// <param name="clientInfo">
		/// An out parameter for a <see cref="ClientContentInfo"/> that will be assigned
		/// the content associated with the <see cref="contentId"/>, or left null if the content ID
		/// isn't in the current manifest.
		/// </param>
		/// <returns>true if the content ID was found, false otherwise.</returns>
		public bool TryGetContentId(string contentId, out ClientContentInfo clientInfo)
		{
			return _contentIdTable.TryGetValue(contentId, out clientInfo);
		}

		/// <summary>
		/// Get the latest <see cref="ClientManifest"/> that was received by the game client.
		/// This method will not <i>start</i> a network request. Instead, it will reference the most
		/// recent network request that is fetching the manifest.
		/// </summary>
		/// <returns>A <see cref="Promise{ClientManifest}"/> representing the network call.</returns>
		public Promise<ClientManifest> GetManifest()
		{
			return _manifestPromise.Map(x => _latestManifiest);
		}

		/// <summary>
		/// <inheritdoc cref="GetManifest()"/>
		/// </summary>
		/// <param name="query">A <see cref="ContentQuery"/> that will filter the resulting <see cref="ClientManifest.entries"/> field.</param>
		/// <returns>A <see cref="Promise{ClientManifest}"/> representing the network call.</returns>
		public Promise<ClientManifest> GetManifest(ContentQuery query)
		{
			return _manifestPromise.Map(x => _latestManifiest.Filter(query));
		}

		/// <summary>
		/// <inheritdoc cref="GetManifest()"/>
		/// </summary>
		/// <param name="filter">A <see cref="ContentQuery"/> in string form that will filter the resulting <see cref="ClientManifest.entries"/> field</param>
		/// <returns>A <see cref="Promise{ClientManifest}"/> representing the network call.</returns>
		public Promise<ClientManifest> GetManifest(string filter)
		{
			return _manifestPromise.Map(x => _latestManifiest.Filter(filter));
		}

		protected override string CreateRefreshUrl(string scope)
		{
			return $"/basic/content/manifest/public?id={ManifestID}";
		}

		protected override Promise<ClientManifest> ExecuteRequest(IBeamableRequester requester, string url)
		{
			return _provider.GetService<IManifestResolver>().ResolveManifest(requester, url, this);
		}

		protected override void OnRefresh(ClientManifest data)
		{
			_latestManifiest = data;
			// TODO work on better refresh strategy. A total wipe isn't very performant.
			_contentIdTable.Clear();
			foreach (var entry in _latestManifiest.entries) _contentIdTable.Add(entry.contentId, entry);

			Notify(data);

			_manifestPromise.CompleteSuccess(new Unit());
		}
	}

	/// <summary>
	/// This type defines the %Client main entry point for the %Content feature.
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	/// #### Related Links
	///     - See the <a target="_blank" href="https://docs.beamable.com/docs/content-feature">Content</a> feature
	///     documentation
	/// - See Beamable.API script reference
	/// ![img beamable-logo]
	/// </summary>
	public class ContentService : IContentApi,
		IHasPlatformSubscriber<ManifestSubscription, ClientManifest, ClientManifest>,
		IHasPlatformSubscribers<ManifestSubscription, ClientManifest, ClientManifest>
	{
		private readonly IDependencyProvider _provider;
		private readonly ContentParameterProvider _config;
		private readonly IContentCacheFactory _cacheFactory;

		/// <summary>
		/// The default content manifest ID controls which manifest the <see cref="ContentService"/> will use.
		/// By default, the value will be "global". You can customize the value by using the <see cref="ContentParameterProvider"/>,
		/// or by calling the <see cref="SwitchDefaultManifestID"/> method.
		/// </summary>
		public string CurrentDefaultManifestID { get; private set; } = "global";

		/// <summary>
		/// The <see cref="IBeamableFilesystemAccessor"/> is used by the <see cref="ContentSerializer{TContentBase}"/> to handle
		/// file read and writes for content disk caches.
		/// </summary>
		public IBeamableFilesystemAccessor FilesystemAccessor { get; }

		/// <summary>
		/// The <see cref="ManifestSubscription"/> will be notified when there are content updates from the Beamable Cloud.
		/// <b>This field should not be used, and will be deprecated in a future release.</b>
		/// </summary>
		public ManifestSubscription Subscribable { get; private set; }

		/// <summary>
		/// A dictionary from manifest ID to <see cref="ManifestSubscription"/> that the game client is currently observing.
		/// <b>This field should not be used, and will be deprecated in a future release.</b>
		/// </summary>
		public Dictionary<string, ManifestSubscription> Subscribables { get; }

		/// <summary>
		/// This field is obsolete, please do not use.
		/// </summary>
		[Obsolete]
		public IBeamableRequester Requester => InternalRequester;

		private IBeamableRequester InternalRequester => _provider.GetService<IBeamableRequester>(); // TODO: when Requester is removed, rename this.

		/// <summary>
		/// An event that triggers when the default manifest has been changed using the <see cref="SwitchDefaultManifestID"/> method.
		/// The event has a string parameter that represents the new manifest ID.
		/// </summary>
		public Action<string> OnManifestChanged;

		private IPlatformService Platform => _provider.GetService<IPlatformService>();
		private IConnectivityService _connectivityService;
		private readonly Dictionary<Type, ContentCache> _contentCaches = new Dictionary<Type, ContentCache>();
		private readonly OfflineCache _offlineCache;
		private static bool _testScopeEnabled;

		/// <summary>
		/// A utility used during Content Baking.
		/// This field should not be used in regular use.
		/// To resolve content, please use the <see cref="GetContent(string,string)"/> method.
		/// </summary>
		public ContentDataInfoWrapper ContentDataInfo;

		private Dictionary<string, ClientContentInfo> bakedManifestInfo =
			new Dictionary<string, ClientContentInfo>();

		private Dictionary<string, ClientContentInfo> cachedManifestInfo;

		private ClientManifest bakedManifest;
		private ClientManifest BakedManifest
		{
			get => bakedManifest;
			set
			{
				bakedManifest = value;

				if (bakedManifest == null)
				{
					return;
				}

				foreach (var info in bakedManifest.entries)
				{
					bakedManifestInfo[info.contentId] = info;
				}
			}
		}



#if UNITY_EDITOR

		public class ContentServiceTestScope : IDisposable
		{
			internal static readonly ContentServiceTestScope Instance = new ContentServiceTestScope();

			private ContentServiceTestScope()
			{
			}

			public void Dispose()
			{
				_testScopeEnabled = false;
			}
		}
#endif

		public ContentService(IDependencyProvider provider,
							  IBeamableFilesystemAccessor filesystemAccessor, ContentParameterProvider config, OfflineCache offlineCache)
		{
			_provider = provider;
			_config = config;
			_cacheFactory = provider.GetService<IContentCacheFactory>();
			CurrentDefaultManifestID = config.manifestID;
			FilesystemAccessor = filesystemAccessor;
			_connectivityService = _provider.GetService<IConnectivityService>();
			_offlineCache = offlineCache;

			// Subscribable = _provider.GetService<IManifestSubscriptionFactory>()
			//                         .CreateSubscription(CurrentDefaultManifestID);
			Subscribable = new ManifestSubscription(_provider, CurrentDefaultManifestID);
			Subscribable.Subscribe(cb =>
			{
				// pay attention, server...
			});


			InitializeBakedContent();
			InitializeBakedManifest();

			Subscribables = new Dictionary<string, ManifestSubscription>();
			AddSubscriber(CurrentDefaultManifestID);
		}

		public T DeserializeDataCache<T>(string json) where T : new()
		{
			try
			{
				return JsonUtility.FromJson<T>(json);
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"Beamable - the cached disk {typeof(T).Name} was unreadable, so no cache will be used. error=[{ex.GetType().Name}] message=[{ex.Message}] stack=[{ex.StackTrace}] ");
				return new T();
			}
		}

		private void InitializeBakedContent()
		{
			// remove content in old format
			string contentDirectory = Path.Combine(FilesystemAccessor.GetPersistentDataPathWithoutTrailingSlash(), "content");
			const string contentFileName = "content.json";
			if (Directory.Exists(contentDirectory))
			{
				DirectoryInfo info = new DirectoryInfo(contentDirectory);
				foreach (var file in info.EnumerateFiles())
				{
					if (file.Name.Equals(contentFileName))
					{
						continue;
					}

					file.Delete();
				}
			}
			string contentPath = Path.Combine(contentDirectory, contentFileName);

			if (File.Exists(contentPath))
			{
				var json = File.ReadAllText(contentPath);
				ContentDataInfo = DeserializeDataCache<ContentDataInfoWrapper>(json);
			}
			else
			{
				var bakedFile = Resources.Load<TextAsset>(BAKED_FILE_RESOURCE_PATH);

				if (bakedFile == null)
				{
					return;
				}

				string json = bakedFile.text;
				var isValidJson = Json.IsValidJson(json);
				if (isValidJson)
				{
					ContentDataInfo = DeserializeDataCache<ContentDataInfoWrapper>(json);
				}
				else
				{
					json = Gzip.Decompress(bakedFile.bytes);
					ContentDataInfo = DeserializeDataCache<ContentDataInfoWrapper>(json);
				}

				// save baked data to disk
				try
				{
					Directory.CreateDirectory(Path.GetDirectoryName(contentPath));
					File.WriteAllText(contentPath, json);
				}
				catch (Exception e)
				{
					Debug.LogError($"[EXTRACT] Failed to write baked data to disk: {e.Message}");
				}
			}
		}

		private void InitializeBakedManifest()
		{
			var manifestAsset = Resources.Load<TextAsset>(BAKED_MANIFEST_RESOURCE_PATH);

			if (manifestAsset == null)
			{
				return;
			}

			string json = manifestAsset.text;
			var isValidJson = Json.IsValidJson(json);
			if (isValidJson)
			{
				BakedManifest = DeserializeDataCache<ClientManifest>(json);
			}
			else
			{
				json = Gzip.Decompress(manifestAsset.bytes);
				BakedManifest = DeserializeDataCache<ClientManifest>(json);
			}
		}

#if UNITY_EDITOR
		[Obsolete("Do not use.")]
		public static ContentServiceTestScope AllowInTests()
		{
			_testScopeEnabled = true;
			return ContentServiceTestScope.Instance;
		}
#endif

		private void AddSubscriber(string manifestID)
		{
			if (Subscribables.ContainsKey(manifestID))
				return;

			Subscribables.Add(manifestID, new ManifestSubscription(_provider, manifestID));
			Subscribables[manifestID].Subscribe(cb => { });
		}

		private void RemoveSubscriber(string manifestID)
		{
			if (!Subscribables.ContainsKey(manifestID))
				return;

			Subscribables[manifestID].Subscribe(_ => { }).Unsubscribe();
			Subscribables.Remove(manifestID);
		}


		public Promise<IContentObject> GetContent(string contentId, string manifestID = "")
		{
			var referencedType = Beam.GetReflectionSystem<ContentTypeReflectionCache>().GetTypeFromId(contentId);
			return GetContent(contentId, referencedType, DetermineManifestID(manifestID));
		}

		public Promise<IContentObject> GetContent(string contentId, Type contentType, string manifestID = "")
		{
			if (string.IsNullOrEmpty(contentId))
				return Promise<IContentObject>.Failed(new ContentNotFoundException(contentId));
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying && !_testScopeEnabled)
			{
				BeamableLogger.LogError("Cannot resolve content at edit time!");
				throw new Exception("Cannot resolve content at edit time!");
			}
#endif
			ContentCache rawCache;

			var determinedManifestID = DetermineManifestID(manifestID);
			if (!_contentCaches.TryGetValue(contentType, out rawCache))
			{
				rawCache = _cacheFactory.CreateCache(this, determinedManifestID, contentType);
				_contentCaches.Add(contentType, rawCache);
			}

			return GetManifestWithID(determinedManifestID).FlatMap(manifest =>
			{
				if (!_connectivityService.HasConnectivity)
				{
					// check cached content
					if (cachedManifestInfo.TryGetValue(contentId, out var cachedInfo))
					{
						return rawCache.GetContentObject(cachedInfo);
					}
				}

				var subscribable = GetSubscription(determinedManifestID);

				if (subscribable == null)
					AddSubscriber(determinedManifestID);

				if (!subscribable.TryGetContentId(contentId, out var info))
					return Promise<IContentObject>.Failed(new ContentNotFoundException(contentId));

				info.manifestID = determinedManifestID;
				return rawCache.GetContentObject(info);
			});
		}

		public Promise<IContentObject> GetContent(IContentRef reference, string manifestID = "")
		{
			var referencedType = Beam.GetReflectionSystem<ContentTypeReflectionCache>().GetTypeFromId(reference.GetId());
			return GetContent(reference.GetId(), referencedType, DetermineManifestID(manifestID));
		}

		public Promise<TContent> GetContent<TContent>(IContentRef reference, string manifestID = "")
			where TContent : ContentObject, new()
		{
			if (reference == null || string.IsNullOrEmpty(reference.GetId()))
				return Promise<TContent>.Failed(new ContentNotFoundException());
			var referencedType = reference.GetReferencedType();
			return GetContent(reference.GetId(), referencedType, DetermineManifestID(manifestID)).Map(c => (TContent)c);
		}

		public Promise<TContent> GetContent<TContent>(IContentRef<TContent> reference, string manifestID = "")
			where TContent : ContentObject, new()
		{
			return GetContent<TContent>(reference as IContentRef, DetermineManifestID(manifestID));
		}

		/// <summary>
		/// <inheritdoc cref="ManifestSubscription.GetManifest(ContentQuery)"/>
		/// </summary>
		/// <param name="query">A <see cref="ContentQuery"/> that will filter the resulting <see cref="ClientManifest.entries"/> field.</param>
		/// <returns>A <see cref="Promise{ClientManifest}"/> representing the network call.</returns>
		public Promise<ClientManifest> GetManifest(ContentQuery query)
		{
			if (TryGetCachedManifest(CurrentDefaultManifestID, out var promise))
			{
				return promise;
			}

			return GetSubscription(CurrentDefaultManifestID)?.GetManifest(query);
		}

		/// <summary>
		/// <inheritdoc cref="ManifestSubscription.GetManifest(ContentQuery)"/>
		/// </summary>
		/// <param name="manifestID">A specific manifest ID. By default, this will resolve to the default "global" manifest.</param>
		/// <returns>A <see cref="Promise{ClientManifest}"/> representing the network call.</returns>
		public Promise<ClientManifest> GetManifestWithID(string manifestID = "")
		{
			string determinedManifestID = DetermineManifestID(manifestID);

			if (TryGetCachedManifest(determinedManifestID, out var promise))
			{
				return promise;
			}

			return GetSubscription(DetermineManifestID(manifestID))?.GetManifest();
		}

		/// <summary>
		/// <inheritdoc cref="ManifestSubscription.GetManifest(ContentQuery)"/>
		/// </summary>
		/// <param name="filter">A <see cref="ContentQuery"/> in string form that will filter the resulting <see cref="ClientManifest.entries"/> field</param>
		/// <param name="manifestID">A specific manifest ID. By default, this will resolve to the default "global" manifest.</param>
		/// <returns>A <see cref="Promise{ClientManifest}"/> representing the network call.</returns>
		public Promise<ClientManifest> GetManifest(string filter = "", string manifestID = "")
		{
			string determinedManifestID = DetermineManifestID(manifestID);

			if (TryGetCachedManifest(determinedManifestID, out var promise))
			{
				return promise;
			}

			return GetSubscription(determinedManifestID)?.GetManifest(filter);
		}

		/// <summary>
		/// <inheritdoc cref="ManifestSubscription.GetManifest(ContentQuery)"/>
		/// </summary>
		/// <param name="query">A <see cref="ContentQuery"/> that will filter the resulting <see cref="ClientManifest.entries"/> field.</param>
		/// <param name="manifestID">A specific manifest ID. By default, this will resolve to the default "global" manifest.</param>
		/// <returns>A <see cref="Promise{ClientManifest}"/> representing the network call.</returns>
		public Promise<ClientManifest> GetManifest(ContentQuery query, string manifestID = "")
		{
			string determinedManifestID = DetermineManifestID(manifestID);

			if (TryGetCachedManifest(determinedManifestID, out var promise))
			{
				return promise;
			}

			return GetSubscription(determinedManifestID)?.GetManifest(query);
		}

		private bool TryGetCachedManifest(string manifestID, out Promise<ClientManifest> promise)
		{
			if (!_connectivityService.HasConnectivity && _offlineCache.UseOfflineCache)
			{
				string key = $"/basic/content/manifest/public?id={manifestID}";
				if (_offlineCache.Exists(key, InternalRequester.AccessToken, true))
				{
					promise = _offlineCache.Get<ClientManifest>(key, InternalRequester.AccessToken, true);
					promise.Then(manifest =>
					{
						if (cachedManifestInfo == null)
						{
							cachedManifestInfo = new Dictionary<string, ClientContentInfo>();
							foreach (var entry in manifest.entries)
							{
								cachedManifestInfo[entry.contentId] = entry;
							}
						}
					});
					return true;
				}

				if (manifestID.Equals("global") && BakedManifest != null)
				{
					promise = Promise<ClientManifest>.Successful(BakedManifest);
					cachedManifestInfo = bakedManifestInfo;
					return true;
				}
			}

			promise = null;
			return false;
		}

		/// <summary>
		/// Get the latest default <see cref="ClientManifest"/> by asking the Beamable Cloud.
		/// </summary>
		/// <param name="scope">The scope field should be ignored. Don't pass anything.</param>
		/// <returns>A <see cref="Promise{ClientManifest}"/> representing the network call.</returns>
		public Promise<ClientManifest> GetCurrent(string scope = "")
		{
			return Subscribables[CurrentDefaultManifestID].GetCurrent(scope);
		}

		/// <summary>
		/// It is possible to have more than one <see cref="ClientManifest"/> providing content data for a game.
		/// By default, there is only one manifest with an ID of "global". Every piece of content found using
		/// <see cref="GetContent(string, string)"/> has the opportunity to pass a custom manifestID.
		/// However, if no custom manifest ID is given, then the content will be resolved using the default manifest ID,
		/// which is configured with this method.
		/// After this method runs, all future calls to <see cref="GetContent(string,string)"/> will default to the new
		/// manifest.
		/// </summary>
		/// <param name="manifestID">Some manifest ID</param>
		public void SwitchDefaultManifestID(string manifestID)
		{
			if (string.IsNullOrEmpty(manifestID))
			{
				Debug.LogError($"Manifest can't be null or empty.");
				return;
			}

			if (manifestID == CurrentDefaultManifestID)
			{
				Debug.LogWarning($"Manifest id: \"{manifestID}\" is already set as default.");
				return;
			}

			var subscription = GetSubscription(manifestID);

			if (subscription == null)
			{
				Debug.LogError(
					$"Can't reach manifest for given \"{manifestID}\" id. Make sure that manifest with specific id is on the server.");
				return;
			}

			CurrentDefaultManifestID = manifestID;
			Subscribable = subscription;

			OnManifestChanged?.Invoke(CurrentDefaultManifestID);
		}

		/// <summary>
		/// Stop listening all notifications on <see cref="ClientManifest"/> deploy .
		/// </summary>
		public void StopListeningForUpdates()
		{
			foreach (KeyValuePair<string, ManifestSubscription> elem in Subscribables)
			{
				elem.Value.PauseAllNotifications();
			}

			Subscribable?.PauseAllNotifications();
		}

		/// <summary>
		/// Resume listening all notifications on <see cref="ClientManifest"/> deploy .
		/// </summary>
		public void ResumeListeningForUpdates()
		{
			foreach (KeyValuePair<string, ManifestSubscription> elem in Subscribables)
			{
				elem.Value.ResumeAllNotifications();
			}

			Subscribable?.ResumeAllNotifications();
		}


		private ManifestSubscription GetSubscription(string manifestID)
		{
			if (!Subscribables.ContainsKey(manifestID))
				AddSubscriber(manifestID);
			return Subscribables.FirstOrDefault(x => x.Key == manifestID).Value;
		}

		private string DetermineManifestID(string manifestID)
		{
			return string.IsNullOrEmpty(manifestID) ? CurrentDefaultManifestID : manifestID;
		}
	}
}
