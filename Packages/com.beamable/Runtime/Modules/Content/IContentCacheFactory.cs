using Beamable.Common.Api;
using Beamable.Coroutines;
using System;

namespace Beamable.Content
{
	public interface IContentCacheFactory
	{
		/// <summary>
		/// Create a <see cref="ContentCache"/>
		/// </summary>
		ContentCache CreateCache(ContentService contentService, string manifestId, Type contentType);
	}

	public class DefaultContentCacheFactory : IContentCacheFactory
	{
		private readonly IHttpRequester _requester;
		private readonly IBeamableFilesystemAccessor _filesystemAccessor;
		private readonly CoroutineService _coroutineService;

		public DefaultContentCacheFactory(IHttpRequester requester,
										  IBeamableFilesystemAccessor filesystemAccessor,
										  CoroutineService coroutineService)
		{
			_requester = requester;
			_filesystemAccessor = filesystemAccessor;
			_coroutineService = coroutineService;
		}

		/// <summary>
		/// Create a <see cref="ContentCache{T}"/> for the given type of content that will use the Remote
		/// realm to resolve a cache miss. 
		/// </summary>
		/// <param name="contentService"></param>
		/// <param name="manifestId"></param>
		/// <param name="contentType">The type of content that will be available in the content cache</param>
		public ContentCache CreateCache(ContentService contentService, string manifestId, Type contentType)
		{
			var cacheType = typeof(ContentCache<>).MakeGenericType(contentType);
			var constructor = cacheType.GetConstructor(new[]
														   {typeof(IHttpRequester), typeof(IBeamableFilesystemAccessor), typeof(ContentService), typeof(CoroutineService)});
			return (ContentCache)constructor.Invoke(new object[] { _requester, _filesystemAccessor, contentService, _coroutineService });
		}
	}
}
