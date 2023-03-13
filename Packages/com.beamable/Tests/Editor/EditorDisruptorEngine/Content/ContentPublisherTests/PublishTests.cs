using Beamable.Common.Content;
using Beamable.Editor.Content;
using Beamable.Editor.Content.SaveRequest;
using Beamable.Editor.Tests.Beamable.Content.ContentIOTests;
using Beamable.Platform.Tests;
using NUnit.Framework;
using System.Collections.Generic;

namespace Beamable.Editor.Tests.Beamable.Content.ContentPublisherTests
{
	public class PublishTests
	{
		private ContentPublisher _publisher;
		private MockContentIO _mockContentIo;
		private IEnumerable<ContentObject> _content;
		private List<ContentManifestReference> _serverContent;
		private MockPlatformRequester _requester;
		private ExampleContent _exampleContent;
		private ContentReference _exampleContentReference;

		[SetUp]
		public void Init()
		{
			_exampleContent = ContentObject.Make<ExampleContent>("test");
			_exampleContentReference = new ContentReference()
			{
				checksum = "fake",
				id = _exampleContent.Id,
				uri = "somewhere.come",
				version = "123",
				visibility = "public"
			};
			_content = new List<ContentObject>() { };
			_requester = new MockPlatformRequester();
			_serverContent = new List<ContentManifestReference>();
			_mockContentIo = new MockContentIO();

			_mockContentIo.ChecksumResult = c => c.Id.Equals(_exampleContent.Id) ? _exampleContentReference.checksum : "";

			_publisher = new ContentPublisher(_requester, _mockContentIo);
		}

		[TearDown]
		public void CleanUp()
		{
			_requester.Reset();
			_serverContent.Clear();
		}
	}
}
