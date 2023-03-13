using Beamable.Common.Content;
using Beamable.Common.Content.Serialization;
using Beamable.Common.Inventory;
using Beamable.Tests.Content.Serialization.Support;
using NUnit.Framework;
using System;
using System.Reflection;
using UnityEngine;
#pragma warning disable CS0618

namespace Beamable.Tests.Content.Serialization.ClientContentSerializationTests
{
	public class SerializeFieldSubclassSerializeTests
	{
		[Test]
		public void PrivateFieldsInParentClassesAreDeserialized()
		{
			var json = @"{
   ""id"": ""currency.gf.test.tuna"",
   ""version"": """",
   ""properties"": {
      ""test"": { ""data"": ""foo"" },
 ""Icon"": {
         ""data"": null
      },
      ""clientPermission"": {
         ""data"": {
            ""write_self"": false
         }
      },
    ""spriteAssetName"": {
         ""data"": ""spendable_tokens""
      },
     ""bigIcons"": {
         ""data"": [

         ]
      },
      ""currencyTags"": {
         ""data"": [
         ""shop""
            ]
      },

      ""amountFormat"": {
         ""data"": ""tuna""
      }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");
			var realSerializer = new ClientContentSerializer();
			var o = realSerializer.Deserialize<TestCurrency>(json);

			Assert.AreEqual("foo", o.test);
			Assert.AreEqual("spendable_tokens",
							typeof(GFCurrencyContent)
								.GetField("spriteAssetName", BindingFlags.Instance | BindingFlags.NonPublic)
								.GetValue(o));
			Assert.AreEqual(
				"tuna",
				typeof(GFCurrencyContent).GetField("amountFormat", BindingFlags.Instance | BindingFlags.NonPublic)
										 .GetValue(o));
			Assert.IsTrue((typeof(GFCurrencyContent)
						   .GetField("currencyTags", BindingFlags.Instance | BindingFlags.NonPublic)
						   .GetValue(o) as string[]).Length == 1);
		}

		[Test]
		public void PrivateFieldsInParentClassesAreSerialized()
		{
			var c = new TestCurrency();
			c.SetContentName("tuna");
			c.test = "foo";

			typeof(GFCurrencyContent).GetField("spriteAssetName", BindingFlags.Instance | BindingFlags.NonPublic)
									 .SetValue(c, "spendable_tokens");
			typeof(GFCurrencyContent).GetField("amountFormat", BindingFlags.Instance | BindingFlags.NonPublic)
									 .SetValue(c, "tuna");
			typeof(GFCurrencyContent).GetField("currencyTags", BindingFlags.Instance | BindingFlags.NonPublic)
									 .SetValue(c, new[] { "shop" });

			var expected = @"{
   ""id"": ""currency.gf.test.tuna"",
   ""version"": """",
   ""properties"": {
      ""test"": { ""data"": ""foo"" },
 ""icon"": {
         ""data"": null
      },
      ""clientPermission"": {
         ""data"": {
            ""write_self"": false
         }
      },
""startingAmount"": {""data"": 0},
""external"": {
	""data"": {
		""Value"": {
			""service"": null,
			""namespace"": null
		},
		""HasValue"": false
	}
},
    ""spriteAssetName"": {
         ""data"": ""spendable_tokens""
      },
     ""bigIcons"": {
         ""data"": [

         ]
      },
      ""currencyTags"": {
         ""data"": [
         ""shop""
            ]
      },

      ""amountFormat"": {
         ""data"": ""tuna""
      }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "").Replace("\t", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}
	}

	[Agnostic, Serializable, ContentType("gf")]
	public class GFCurrencyContent : CurrencyContent
	{
		// It is okay for these serialized fields to never be assigned, so we suppress CS0649. ~ACM 2021-03-23
#pragma warning disable 0649
		[SerializeField] string spriteAssetName;
		[SerializeField] AmountBasedIcon[] bigIcons;
		[SerializeField] string[] currencyTags;
		[SerializeField, Tooltip("Parameter 0 is icon, parameter 1 is amount, default: '{0} {1}'")]
		string amountFormat;
#pragma warning restore 0649
		public string SpriteAssetName => spriteAssetName;
		public AmountBasedIcon[] BigIcons => bigIcons;
		public string[] CurrencyTags => currencyTags;
		public string AmountFormat => amountFormat;
	}

	[Agnostic, Serializable, ContentType("test")]
	public class TestCurrency : GFCurrencyContent
	{
		public string test;
	}

	[Serializable]
	public class AmountBasedIcon { }
}
