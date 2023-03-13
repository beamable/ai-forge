using Beamable.Common.Content;
using Beamable.Tests.Content.Serialization.Support;
using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Tests.Content.Serialization.ClientContentSerializationTests
{
	public class GetNullStringForTypeTests
	{


		[Test]
		public void Serialize()
		{
			var meta = new BundlesContent { Id = "metadata.bundles.tuna" };
			meta.Thematic2 = true;
			meta.Thematic3 = false;
			var s = new TestSerializer();
			var json = s.Serialize(meta);
			Debug.Log(json);
			var reconstructed = s.Deserialize<BundlesContent>(json);

			Assert.IsNotNull(reconstructed.Thematic2);
			Assert.IsNotNull(reconstructed.Thematic3);
			Assert.IsTrue(reconstructed.Thematic2);
			Assert.IsFalse(reconstructed.Thematic3);
			Assert.IsNull(reconstructed.Thematic);
			Assert.IsNull(reconstructed.x);
		}

		[Test]
		public void SerializeIntNotNull()
		{
			var meta = new BundlesContent { Id = "metadata.bundles.tuna" };
			meta.x = 3;
			var s = new TestSerializer();
			var json = s.Serialize(meta);
			Debug.Log(json);
			var reconstructed = s.Deserialize<BundlesContent>(json);

			Assert.IsNotNull(reconstructed.x);
			Assert.AreEqual(3, reconstructed.x);
		}

		[Test]
		public void SerializeNested()
		{
			var meta = new TopLevel() { Id = "top.pop" };
			meta.nested = new NestedComponent { x = 2 };
			meta.nestedList = new List<NestedComponent> { new NestedComponent { x = 3 }, new NestedComponent { x = null } };
			var s = new TestSerializer();
			var json = s.Serialize(meta);
			Debug.Log(json);
			var reconstructed = s.Deserialize<TopLevel>(json);

			Assert.IsNotNull(reconstructed.nested);
			Assert.AreEqual(2, reconstructed.nested.x);

			Assert.AreEqual(2, reconstructed.nestedList.Count);
			Assert.IsNotNull(reconstructed.nestedList[0].x);
			Assert.AreEqual(3, reconstructed.nestedList[0].x);
			Assert.IsNull(reconstructed.nestedList[1].x);

			Assert.IsNotNull(reconstructed.nested);

		}


		[System.Serializable]
		[ContentType(CONTENT_TYPE)]
		public class MetadataContent : TestContentObject
		{
			public const string CONTENT_TYPE = "metadata";
		}

		[ContentType(SUB_CONTENT_TYPE)]
		public class BundlesContent : MetadataContent
		{
			public const string SUB_CONTENT_TYPE = "bundles";
			public string BundleId;
			public int league;
			public int division;
			public int? x;
			public bool? Thematic;
			public bool? Thematic2;
			public bool? Thematic3;
			public int[] Common;
		}

		[ContentType("top")]
		public class TopLevel : TestContentObject
		{
			public List<NestedComponent> nestedList;
			public NestedComponent nested;
		}

		public class NestedComponent
		{
			public int? x;
		}
	}
}
