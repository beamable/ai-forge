using Beamable.Tests.Content.Serialization.Support;
using NUnit.Framework;

namespace Beamable.Tests.Content.Serialization.ClientContentSerializationTests
{
	public class DeserializeEmptyFields
	{
		[Test]
		public void DeserializingEmptyValueDoesntBreak()
		{
			var json = @"{
               ""id"": ""test.nothing"",
               ""version"": ""123"",
               ""properties"": {
                  ""number"": { ""data"": """" },
               },
            }";

			var s = new TestSerializer();
			var o = s.Deserialize<SimpleContent>(json);

			Assert.AreEqual(0, o.number);
		}

		[Test]
		public void DeserializeEmptyValueIntoSerializedObject_YieldsDefault()
		{
			var json = @"{
               ""id"": ""test.nothing"",
               ""version"": ""123"",
               ""properties"": {
                  ""code"": { ""data"": """" },
               },
            }";

			var s = new TestSerializer();
			var o = s.Deserialize<ContentWithNestedType>(json);

			Assert.AreEqual("fish", o.code.tuna);
			Assert.AreEqual(1, o.code.x);
		}

		[Test]
		public void DeserializeEmptyValueIntoSerializedObject_SetToNull_DoesntCreate()
		{
			var json = @"{
               ""id"": ""test.nothing"",
               ""version"": ""123"",
               ""properties"": {
                  ""code"": { ""data"": """" },
               },
            }";

			var s = new TestSerializer();
			var o = s.Deserialize<ContentWithNestedTypeNull>(json);

			Assert.IsNull(o.code);
		}

		[Test]
		public void DeserializingWrongObjectValueDoesntBreak()
		{
			var json = @"{
               ""id"": ""test.nothing"",
               ""version"": ""123"",
				""properties"": {
				""number"": { ""data"": 4 },
                ""tst1"": {""data"": {""a"" : ""1"", ""b"" : ""2""}},
                ""tst2"": {""data"": ""test"" },
                ""tst3"": {""data"": [1,2,3,4] },
               },

            }";

			var s = new TestSerializer();
			var o = s.Deserialize<ExtendedSimpleContent>(json, true);

			Assert.AreEqual(4, o.number);
			Assert.Pass($"[nothing] file is corrupted. Repair content before publish.");
		}

#pragma warning disable CS0649

		class SimpleContent : TestContentObject
		{
			public int number;
		}

		class ExtendedSimpleContent : SimpleContent
		{
			public int tst1;
			public int tst2;
			public int tst3;
		}

		class ContentWithNestedType : TestContentObject
		{
			public PromoCode code = new PromoCode();
		}

		class ContentWithNestedTypeNull : TestContentObject
		{
			public PromoCode code;
		}


		[System.Serializable]
		class PromoCode
		{
			public int x = 1;
			public string tuna = "fish";
		}
	}
}
