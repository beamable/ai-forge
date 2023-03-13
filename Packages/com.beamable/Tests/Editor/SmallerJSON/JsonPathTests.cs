using Beamable.Serialization.SmallerJSON;
using NUnit.Framework;

namespace Beamable.Editor.Tests.SmallerJson
{
	public class JsonPathTests
	{
		[Test]
		public void Simple()
		{
			var dict = new ArrayDict
		 {
			{"a", 123}
		 };
			var val = dict.JsonPath("a");
			Assert.AreEqual(123, val);
		}

		[Test]
		public void Nested()
		{
			var dict = new ArrayDict
		 {
			{
			   "a", new ArrayDict
			   {
				  {"b", "1.2.3"}
			   }
			}
		 };
			/*
			 * {
			 *    "a": {
			 *            "b": "1.2.3"
			 *       }
			 * }
			 */
			var val = dict.JsonPath("a.b");
			Assert.AreEqual("1.2.3", val);
		}

		[Test]
		public void Array()
		{
			var json = "{\"Numbers\":[1,2,3,5,8]}";
			var dict = Json.Deserialize(json) as ArrayDict;
			var val = dict.JsonPath("Numbers[3]");
			Assert.AreEqual(5, val);
		}

		[Test]
		public void ArrayThenNestedObject()
		{
			var json = "{\"data\":[ {\"x\":\"tuna\"} ]}";
			var dict = Json.Deserialize(json) as ArrayDict;
			var val = dict.JsonPath("data[0].x");
			Assert.AreEqual("tuna", val);
		}

		[Test]
		public void NestedNested()
		{
			var json = "{\"data\":{\"x\":\"tuna\"}}";
			var dict = Json.Deserialize(json) as ArrayDict;
			var val = dict.JsonPath("data.x");
			Assert.AreEqual("tuna", val);
		}

		[Test]
		public void GetObject()
		{
			var json = "{\"data\":{\"x\":\"tuna\"}}";
			var dict = Json.Deserialize(json) as ArrayDict;
			var val = dict.JsonPath("data");
			Assert.AreEqual(typeof(ArrayDict), val.GetType());
			Assert.AreEqual("tuna", ((ArrayDict)val).JsonPath("x"));
		}

		[Test]
		public void AccessFieldWithDotCharacter()
		{
			var json = "{\"crazy.data\":{\"x\":\"tuna\"}}";
			var dict = Json.Deserialize(json) as ArrayDict;
			var val = dict.JsonPath("crazy.data.x");
			Assert.AreEqual("tuna", val);
		}

		[Test]
		public void AccessFieldWithDotCharacter_AmbigiousPrefersPath()
		{
			var json = "{\"crazy.data\":{\"x\":\"tuna\"}, \"crazy\": {\"data\":{\"x\": \"tuna2\"}}}";
			var dict = Json.Deserialize(json) as ArrayDict;
			var val = dict.JsonPath("crazy.data.x");
			Assert.AreEqual("tuna2", val);
		}

		[Test]
		public void AccessFieldWithDotCharacter_LateChoice()
		{
			var json = "{\"crazy.data\":{\"x\":\"tuna\"}, \"crazy\": {\"data2\":{\"x\": \"tuna2\"}}}";
			var dict = Json.Deserialize(json) as ArrayDict;
			var val = dict.JsonPath("crazy.data.x");
			Assert.AreEqual("tuna", val);
		}

	}
}
