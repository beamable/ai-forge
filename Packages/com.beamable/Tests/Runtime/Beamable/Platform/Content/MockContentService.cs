using Beamable.Common;
using Beamable.Common.Api.Content;
using Beamable.Common.Content;
using System;
using System.Collections.Generic;

namespace Beamable.Platform.Tests.Content
{
	public class MockContentService : IContentApi
	{
		private Dictionary<string, IContentObject> _idToContent = new Dictionary<string, IContentObject>();
		private ClientManifest _manifest = new ClientManifest();
		public void Reset()
		{
			_idToContent.Clear();
			_manifest = new ClientManifest();
			ManifestGenerator = scope => _manifest;
		}

		public Func<string, ClientManifest> ManifestGenerator;

		public Promise<ClientManifest> GetCurrent(string scope = "")
		{
			return Promise<ClientManifest>.Successful(ManifestGenerator(scope));
		}

		public MockContentService Provide(IContentObject content)
		{
			_idToContent.Add(content.Id, content);
			return this;
		}

		public Promise<TContent> GetContent<TContent>(IContentRef<TContent> reference) where TContent : ContentObject, new()
		{
			return Promise<TContent>.Successful((TContent)_idToContent[reference.GetId()]);
		}

		public Promise<TContent> GetContent<TContent>(IContentRef<TContent> reference, string manifestID = "") where TContent : ContentObject, new()
		{
			return Promise<TContent>.Successful((TContent)_idToContent[reference.GetId()]);
		}

		public Promise<ClientManifest> GetManifestWithID(string manifestID = "")
		{
			return Promise<ClientManifest>.Successful(_manifest);

		}

		public Promise<IContentObject> GetContent(string contentId, Type contentType, string manifestID = "")
		{
			return Promise<IContentObject>.Successful(_idToContent[contentId]);
		}

		public Promise<IContentObject> GetContent(string contentId, string manifestID = "")
		{
			return Promise<IContentObject>.Successful(_idToContent[contentId]);
		}

		public Promise<IContentObject> GetContent(IContentRef reference, string manifestID = "")
		{
			return Promise<IContentObject>.Successful(_idToContent[reference.GetId()]);
		}

		public Promise<TContent> GetContent<TContent>(IContentRef reference, string manifestID = "") where TContent : ContentObject, new()
		{
			return Promise<TContent>.Successful((TContent)_idToContent[reference.GetId()]);

		}

		public Promise<IContentObject> GetContent(string contentId, Type contentType)
		{
			return Promise<IContentObject>.Successful(_idToContent[contentId]);
		}

		public Promise<IContentObject> GetContent(string contentId)
		{
			return Promise<IContentObject>.Successful(_idToContent[contentId]);
		}

		public Promise<IContentObject> GetContent(IContentRef reference)
		{
			return Promise<IContentObject>.Successful(_idToContent[reference.GetId()]);
		}

		public Promise<TContent> GetContent<TContent>(IContentRef reference) where TContent : ContentObject, new()
		{
			return Promise<TContent>.Successful((TContent)_idToContent[reference.GetId()]);
		}

		public Promise<ClientManifest> GetManifest()
		{
			return Promise<ClientManifest>.Successful(_manifest);
		}

		public Promise<ClientManifest> GetManifest(string filter)
		{
			throw new NotImplementedException();
		}

		public Promise<ClientManifest> GetManifestFiltered(string filter)
		{
			throw new NotImplementedException();
		}

		public Promise<ClientManifest> GetManifestFiltered(string filter, string manifestID)
		{
			throw new NotImplementedException();
		}

		public Promise<ClientManifest> GetManifestQueried(ContentQuery query)
		{
			throw new NotImplementedException();
		}

		public Promise<ClientManifest> GetManifestQueried(ContentQuery query, string manifestID)
		{
			throw new NotImplementedException();
		}

		public Promise<ClientManifest> GetManifest(string filter, string manifestID = "")
		{
			throw new NotImplementedException();
		}

		public Promise<ClientManifest> GetManifest(ContentQuery query, string manifestID = "")
		{
			throw new NotImplementedException();
		}

		public Promise<ClientManifest> GetManifest(ContentQuery query)
		{
			throw new NotImplementedException();
		}
	}
}
