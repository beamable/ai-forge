using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Editor.Content;
using System;
using System.Collections.Generic;
using Manifest = Beamable.Editor.Content.Manifest;

namespace Beamable.Editor.Tests.Beamable.Content.ContentIOTests
{
	public class MockContentIO : IContentIO
	{
		public Func<Promise<Editor.Content.Manifest>> FetchManifestResult = () => null;
		public Func<IEnumerable<ContentObject>> FindAllResult = () => null;
		public Func<IContentObject, string> ChecksumResult = c => "";


		public Promise<Editor.Content.Manifest> FetchManifest()
		{
			return FetchManifestResult();
		}

		public Promise<Manifest> FetchManifest(string id)
		{
			return FetchManifestResult();
		}

		public IEnumerable<ContentObject> FindAll(ContentQuery query = null)
		{
			return FindAllResult();
		}

		public string Checksum(IContentObject content)
		{
			return ChecksumResult(content);
		}

		public LocalContentManifest BuildLocalManifest()
		{
			throw new NotImplementedException();
		}
	}
}
