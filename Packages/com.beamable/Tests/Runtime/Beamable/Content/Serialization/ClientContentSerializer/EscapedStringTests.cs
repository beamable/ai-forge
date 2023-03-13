using Beamable.Tests.Content.Serialization.Support;
using NUnit.Framework;
using UnityEngine;

namespace Beamable.Tests.Content.Serialization.EscapeTests
{
	public class EscapedStringTests
	{
		[TearDown]
		public void Teardown()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture =
				System.Globalization.CultureInfo.GetCultureInfo("en-US");
		}

		[Test]
		public void Serialize_BasicString()
		{

			var c = new StringContent
			{
				Id = "test.nothing",
				s = "test"
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""s"": { ""data"": ""test"" }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void Deserialize_BasicString()
		{
			var json = @"{
               ""id"": ""test.nothing"",
               ""version"": ""123"",
               ""properties"": {
                  ""s"": { ""data"": ""test"" },
               },
            }";

			var s = new TestSerializer();
			var o = s.Deserialize<StringContent>(json);
			Assert.AreEqual("test", o.s);
		}


		[Test]
		public void Serialize_StringWithEscapedCharacters()
		{

			var c = new StringContent
			{
				Id = "test.nothing",
				s = "test\"okay"
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""s"": { ""data"": ""test\""okay"" }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}


		[Test]
		public void Deserialize_StringWithEscapedCharacters()
		{
			var json = @"{
               ""id"": ""test.nothing"",
               ""version"": ""123"",
               ""properties"": {
                  ""s"": { ""data"": ""test\""okay"" },
               },
            }";

			var s = new TestSerializer();
			var o = s.Deserialize<StringContent>(json);
			Assert.AreEqual("test\"okay", o.s); // but was "test\\"okay"
		}

		[Test]
		public void JsonStringField()
		{
			var innerJson = JsonUtility.ToJson(new Vector2(1, 2));
			var content = new StringContent { Id = "test.nothing", s = innerJson };
			var s = new TestSerializer();

			var serializedForm = s.Serialize(content);
			var output = s.Deserialize<StringContent>(serializedForm);

			var vec = JsonUtility.FromJson<Vector2>(output.s);

			Assert.AreEqual(1, vec.x);
			Assert.AreEqual(2, vec.y);
		}

		[System.Serializable]
		class StringContent : TestContentObject
		{
			public string s;
		}

	}
}
