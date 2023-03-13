using Beamable.Common;
using Beamable.Common.Content;
using System;

namespace Beamable.Tests.Content.Serialization.Support
{

	public class TestSerializer : ContentSerializer<TestContentObject>
	{
		protected override TContent CreateInstance<TContent>()
		{
			return new TContent();
		}
	}

	public class TestContentObject : IContentObject
	{
		public string Id { get; set; }
		public string Version { get; set; }
		public string[] Tags { get; set; }
		public string ManifestID { get; }
		public long LastChanged { get; set; }

		public ContentCorruptedException ContentException { get; set; }

		public void SetIdAndVersion(string id, string version)
		{
			Id = id;
			Version = version;
		}

		public string ToJson()
		{
			throw new NotImplementedException();
		}
	}

	public class TestContentRef<TContent> : AbsContentRef<TContent> where TContent : IContentObject, new()
	{
		public override Promise<TContent> Resolve(string manifestID = "")
		{
			throw new NotImplementedException();
		}
	}

	public class TestContentLink<TContent> : AbsContentLink<TContent> where TContent : IContentObject, new()
	{
		public bool WasCreated;

		public override Promise<TContent> Resolve(string manifestID = "")
		{
			throw new NotImplementedException();
		}

		public override void OnCreated()
		{
			WasCreated = true;
			//throw new NotImplementedException();
		}
	}
}
