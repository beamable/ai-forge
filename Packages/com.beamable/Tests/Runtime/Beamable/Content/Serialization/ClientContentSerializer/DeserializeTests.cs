using Beamable.Common.Announcements;
using Beamable.Common.Assistant;
using Beamable.Common.Content;
using Beamable.Common.Content.Serialization;
using Beamable.Common.Reflection;
using Beamable.Common.Tournaments;
using Beamable.Tests.Content.Serialization.Support;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.TestTools;

namespace Beamable.Tests.Content.Serialization.ClientContentSerializationTests
{
	public class DeserializeTests
	{
		public ContentTypeReflectionCache cache;

		[SetUp]
		public void Setup()
		{
			var reflectionCache = new ReflectionCache();
			var hintStorage = new BeamHintGlobalStorage();
			cache = new ContentTypeReflectionCache();
			reflectionCache.RegisterTypeProvider(cache);
			reflectionCache.RegisterReflectionSystem(cache);
			reflectionCache.SetStorage(hintStorage);

			var assembliesToSweep = AppDomain.CurrentDomain.GetAssemblies().Select(asm => asm.GetName().Name).ToList();
			reflectionCache.GenerateReflectionCache(assembliesToSweep);
		}

		[TearDown]
		public void Teardown()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture =
				System.Globalization.CultureInfo.GetCultureInfo("en-US");
		}

		[UnityTest]
		public IEnumerator MultithreadedDeserialize()
		{
			// this test simulates what might happen if the content deserializer is hit by many threads (C#MS) at once.

			const int cycleCount = 2500;
			const int threadCount = 50;

			const string json = @"{
               ""id"": ""test.nothing"",
               ""version"": ""123"",
               ""properties"": {
                  ""text"": { ""data"": ""testtext"" },
                  ""number"": { ""data"": 3.21 },
                  ""longNumber"": { ""data"": 123 },
                  ""bigNumber"": { ""data"": 15000000000000000000 },
                  ""flag"": { ""data"": true },
                  ""positiveInteger"": { ""data"": 7 },
                  ""character"": { ""data"": 35 },
                  ""singleByte"": { ""data"": 2 }
               },
            }";

			var s = new TestSerializer();

			void LaunchThread()
			{
				var thread = new Thread(() =>
				{
					try
					{
						var cycle = cycleCount;
						while (cycle-- > 0)
						{
							var o = s.Deserialize<TestContent>(json);
							Assert.AreEqual(true, o.flag);
							Assert.AreEqual(123, o.longNumber);
							Assert.AreEqual(true, Mathf.Abs(o.number - 3.21f) < .001f);
							Assert.AreEqual("testtext", o.text);
							Assert.AreEqual(15000000000000000000.0, o.bigNumber);
							Assert.AreEqual(7, o.positiveInteger);
							Assert.AreEqual('#', o.character);
							Assert.AreEqual(2, o.singleByte);

							Thread.Sleep(1);
						}

						Debug.Log($"Thread finished");
					}
					catch (Exception ex)
					{
						Assert.Fail("Thread failed to deserialize. " + ex.Message + " " + ex.StackTrace);
					}
				});
				thread.Start();
			}

			for (var i = 0; i < threadCount; i++)
			{
				LaunchThread();
			}

			yield return new WaitForSecondsRealtime(7);
		}

		[Test]
		public void Primitives()
		{
			var json = @"{
               ""id"": ""test.nothing"",
               ""version"": ""123"",
               ""properties"": {
                  ""text"": { ""data"": ""testtext"" },
                  ""number"": { ""data"": 3.21 },
                  ""longNumber"": { ""data"": 123 },
                  ""bigNumber"": { ""data"": 15000000000000000000 },
                  ""flag"": { ""data"": true },
                  ""positiveInteger"": { ""data"": 7 },
                  ""character"": { ""data"": 35 },
                  ""singleByte"": { ""data"": 2 }
               },
            }";

			var s = new TestSerializer();
			var o = s.Deserialize<TestContent>(json);

			Assert.AreEqual(true, o.flag);
			Assert.AreEqual(123, o.longNumber);
			Assert.AreEqual(true, Mathf.Abs(o.number - 3.21f) < .001f);
			Assert.AreEqual("testtext", o.text);
			Assert.AreEqual(15000000000000000000.0, o.bigNumber);
			Assert.AreEqual(7, o.positiveInteger);
			Assert.AreEqual('#', o.character);
			Assert.AreEqual(2, o.singleByte);
		}

		[Test]
		public void Primitives_WithEmptyString()
		{
			var json = @"{
               ""id"": ""test.nothing"",
               ""version"": ""123"",
               ""properties"": {
                  ""sub"": { ""data"": {
                        ""text"": """",
                        ""number"": 3.21,
                        ""longNumber"": 123,
                        ""bigNumber"": 15000000000000000000,
                        ""flag"": true
                     }
                  },
               },
            }";

			var s = new TestSerializer();
			var o = s.Deserialize<TestContentComplex>(json);

			Assert.AreEqual(true, o.sub.flag);
			Assert.AreEqual(123, o.sub.longNumber);
			Assert.AreEqual(true, Mathf.Abs(o.sub.number - 3.21f) < .001f);
			Assert.AreEqual("", o.sub.text);
			Assert.AreEqual(15000000000000000000.0, o.sub.bigNumber);
		}

		[Test]
		public void Primitives_WithNullString()
		{
			var json = @"{
               ""id"": ""test.nothing"",
               ""version"": ""123"",
               ""properties"": {
                  ""sub"": { ""data"": {
                        ""text"":null,
                        ""number"": 3.21,
                        ""longNumber"": 123,
                        ""bigNumber"": 15000000000000000000,
                        ""flag"": true
                     }
                  },
               },
            }";

			var s = new TestSerializer();
			var o = s.Deserialize<TestContentComplex>(json);

			Assert.AreEqual(true, o.sub.flag);
			Assert.AreEqual(123, o.sub.longNumber);
			Assert.AreEqual(true, Mathf.Abs(o.sub.number - 3.21f) < .001f);
			Assert.AreEqual(null, o.sub.text);
			Assert.AreEqual(15000000000000000000.0, o.sub.bigNumber);
		}

		[Test]
		public void Primitives_WithNon_ENUS_Doubles()
		{
			// pretend you are not in EN_US
			System.Threading.Thread.CurrentThread.CurrentCulture =
				System.Globalization.CultureInfo.GetCultureInfo("gsw-FR");

			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": ""123"",
   ""properties"": {
      ""text"": { ""data"": ""testtext"" },
      ""number"": { ""data"": 3.21 },
      ""longNumber"": { ""data"": 123 },
      ""bigNumber"": { ""data"": 15.22 },
      ""flag"": { ""data"": true }
   },
}";

			var s = new TestSerializer();
			var o = s.Deserialize<TestContent>(json);

			Assert.AreEqual(true, o.flag);
			Assert.AreEqual(123, o.longNumber);
			Assert.AreEqual(true, Mathf.Abs(o.number - 3.21f) < .001f);
			Assert.AreEqual("testtext", o.text);
			Assert.AreEqual(true, Mathf.Abs((float)(o.bigNumber - 15.22)) < .001f);
		}

		[Test]
		public void NegativeNumbers()
		{
			var json = @"{
               ""id"": ""test.nothing"",
               ""version"": ""123"",
               ""properties"": {
                  ""text"": { ""data"": ""testtext"" },
                  ""number"": { ""data"": 3.21 },
                  ""longNumber"": { ""data"": -123 },
                  ""bigNumber"": { ""data"": -15000000000000000000 },
                  ""flag"": { ""data"": true }
               },
            }";
			var s = new TestSerializer();
			var o = s.Deserialize<TestContent>(json);

			Assert.AreEqual(true, o.flag);
			Assert.AreEqual(-123, o.longNumber);
			Assert.AreEqual(true, Mathf.Abs(o.number - 3.21f) < .001f);
			Assert.AreEqual("testtext", o.text);
			Assert.AreEqual(-15000000000000000000.0, o.bigNumber);
		}

		[Test]
		public void IdAndVerion()
		{
			var json = @"{
               ""id"": ""test.nothing"",
               ""version"": ""123"",
               ""properties"": {
                  ""text"": { ""data"": ""testtext"" },
                  ""number"": { ""data"": 3.21 },
                  ""longNumber"": { ""data"": 123 },
                  ""flag"": { ""data"": true },
               },
            }";

			var s = new TestSerializer();
			var o = s.Deserialize<TestContent>(json);

			Assert.AreEqual("test.nothing", o.Id);
			Assert.AreEqual("123", o.Version);
		}

		[Test]
		public void Nested()
		{
			var json = @"{
               ""id"": ""test.nothing"",
               ""version"": ""123"",
               ""properties"": {
                  ""sub"": { ""data"": {
                        ""text"": ""testtext"",
                        ""number"": 3.21,
                        ""longNumber"": 123,
                        ""bigNumber"": 15000000000000000000,
                        ""flag"": true
                     }
                  },
               },
            }";

			var s = new TestSerializer();
			var o = s.Deserialize<TestContentComplex>(json);

			Assert.AreEqual(true, o.sub.flag);
			Assert.AreEqual(123, o.sub.longNumber);
			Assert.AreEqual(true, Mathf.Abs(o.sub.number - 3.21f) < .001f);
			Assert.AreEqual("testtext", o.sub.text);
			Assert.AreEqual(15000000000000000000.0, o.sub.bigNumber);
		}

		[Test]
		public void OptionalWithValue()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": ""123"",
   ""properties"": {
      ""maybeNumber"": { ""data"": 5 }
   },
}";

			var s = new TestSerializer();
			var o = s.Deserialize<TestOptional>(json);

			Assert.AreEqual(true, o.maybeNumber.HasValue);
			Assert.AreEqual(5, o.maybeNumber.Value);
		}

		[Test]
		public void OptionalNestedWithValue()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": ""123"",
   ""properties"": {
      ""sub"": { ""data"": { ""maybeNumber"": 5} }
   },
}";

			var s = new TestSerializer();
			var o = s.Deserialize<NestedOptional>(json);

			Assert.AreEqual(true, o.sub.maybeNumber.HasValue);
			Assert.AreEqual(5, o.sub.maybeNumber.Value);
		}

		[Test]
		public void OptionalNestedWithoutValue()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": ""123"",
   ""properties"": {
      ""sub"": { ""data"": { } }
   },
}";

			var s = new TestSerializer();
			var o = s.Deserialize<NestedOptional>(json);

			Assert.AreEqual(false, o.sub.maybeNumber.HasValue);
		}

		[Test]
		public void OptionalWithoutValue()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": ""123"",
   ""properties"": {

   },
}";

			var s = new TestSerializer();
			var o = s.Deserialize<TestOptional>(json);

			Assert.AreEqual(false, o.maybeNumber.HasValue);
		}

		[Test]
		public void OptionalStringWithoutValue()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": ""123"",
   ""properties"": {

   },
}";
			var s = new TestSerializer();
			var o = s.Deserialize<TestOptionalString>(json);

			Assert.AreEqual(false, o.maybeString.HasValue);
		}

		[Test]
		public void OptionalStringWithValue()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": ""123"",
   ""properties"": {
   ""maybeString"": { ""data"": ""abc"" }
   },
}";
			var s = new TestSerializer();
			var o = s.Deserialize<TestOptionalString>(json);

			Assert.AreEqual(true, o.maybeString.HasValue);
			Assert.AreEqual("abc", o.maybeString.Value);
		}

		[Test]
		public void OptionalStringWithEmptyValue()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": ""123"",
   ""properties"": {
   ""maybeString"": { ""data"": """" }
   },
}";
			var s = new TestSerializer();
			var o = s.Deserialize<TestOptionalString>(json);

			Assert.AreEqual(true, o.maybeString.HasValue);
			Assert.AreEqual("", o.maybeString.Value);
		}

		[Test]
		public void Color()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""color"": {
         ""data"": {
            ""r"":1,
            ""g"":0,
            ""b"":0,
            ""a"":1
         }
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<ColorContent>(json);

			Assert.AreEqual(1, o.color.r);
			Assert.AreEqual(0, o.color.g);
			Assert.AreEqual(0, o.color.b);
			Assert.AreEqual(1, o.color.a);
		}

		[Test]
		public void PropertyColor()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""Color"": {
         ""data"": {
            ""r"":1,
            ""g"":0,
            ""b"":0,
            ""a"":1
         }
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<PropertyColorContent>(json);

			Assert.AreEqual(1, o.Color.r);
			Assert.AreEqual(0, o.Color.g);
			Assert.AreEqual(0, o.Color.b);
			Assert.AreEqual(1, o.Color.a);
		}


		[Test]
		public void PropertyColor_Legacy_1_2_10()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""<Color>k__BackingField"": {
         ""data"": {
            ""r"":1,
            ""g"":0,
            ""b"":0,
            ""a"":1
         }
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<PropertyColorContent>(json);

			Assert.AreEqual(1, o.Color.r);
			Assert.AreEqual(0, o.Color.g);
			Assert.AreEqual(0, o.Color.b);
			Assert.AreEqual(1, o.Color.a);
		}

		[Test]
		public void Ref_Legacy()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""reference"": {
         ""data"": {
            ""id"":""primitive.foo"",
         }
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<RefContent>(json);

			Assert.AreEqual("primitive.foo", o.reference.GetId());
		}

		[Test]
		public void Ref_AsString()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""reference"": {
         ""data"": ""primitive.foo""
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<RefContent>(json);

			Assert.AreEqual("primitive.foo", o.reference.GetId());
		}

		[Test]
		public void RefNested_Legacy()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {
            ""reference"": {""id"": ""primitive.foo""},
         }
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<NestedRefContent>(json);

			Assert.AreEqual("primitive.foo", o.sub.reference.GetId());
		}

		[Test]
		public void RefNested_AsString()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {
            ""reference"": ""primitive.foo"",
         }
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<NestedRefContent>(json);

			Assert.AreEqual("primitive.foo", o.sub.reference.GetId());
		}

		[Test]
		public void Link()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""link"": {
         ""$link"": ""primitive.foo""
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<LinkContent>(json);

			Assert.AreEqual("primitive.foo", o.link.GetId());
			Assert.AreEqual(true, o.link.WasCreated);
		}

		[Test]
		public void RefFromLink()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""link"": {
         ""$link"": ""primitive.foo""
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<LinkRefContent>(json);

			Assert.AreEqual("primitive.foo", o.link.GetId());
			Assert.AreEqual(false, string.IsNullOrEmpty(o.link.Id));
		}

		[Test]
		public void LinkNested_Legacy()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {""link"": {""id"": ""primitive.foo""}}
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<LinkNestedContent>(json);

			Assert.AreEqual("primitive.foo", o.sub.link.GetId());
			Assert.AreEqual(true, o.sub.link.WasCreated);
		}

		[Test]
		public void LinkNested_AsString()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {""link"": ""primitive.foo""}
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<LinkNestedContent>(json);

			Assert.AreEqual("primitive.foo", o.sub.link.GetId());
			Assert.AreEqual(true, o.sub.link.WasCreated);
		}

		[Test]
		public void LinkNestedArray_Legacy()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {""links"": [{""id"": ""primitive.foo""}, {""id"": ""primitive.foo2""}]}
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<LinkArrayNestedContent>(json);

			Assert.AreEqual(2, o.sub.links.Length);
			Assert.AreEqual("primitive.foo", o.sub.links[0].GetId());
			Assert.AreEqual("primitive.foo2", o.sub.links[1].GetId());
			Assert.AreEqual(true, o.sub.links[0].WasCreated);
			Assert.AreEqual(true, o.sub.links[1].WasCreated);
		}

		[Test]
		public void LinkNestedArray_AsString()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {""links"": [ ""primitive.foo"", ""primitive.foo2""]}
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<LinkArrayNestedContent>(json);

			Assert.AreEqual(2, o.sub.links.Length);
			Assert.AreEqual("primitive.foo", o.sub.links[0].GetId());
			Assert.AreEqual("primitive.foo2", o.sub.links[1].GetId());
			Assert.AreEqual(true, o.sub.links[0].WasCreated);
			Assert.AreEqual(true, o.sub.links[1].WasCreated);
		}

		[Test]
		public void LinkArray()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""links"": {
         ""$links"": [""primitive.foo"", ""primitive.foo2""]
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<LinkArrayContent>(json);

			Assert.AreEqual(2, o.links.Length);
			Assert.AreEqual("primitive.foo", o.links[0].GetId());
			Assert.AreEqual(true, o.links[0].WasCreated);
			Assert.AreEqual("primitive.foo2", o.links[1].GetId());
			Assert.AreEqual(true, o.links[1].WasCreated);
		}

		[Test]
		public void LinkList()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""links"": {
         ""$links"": [""primitive.foo"", ""primitive.foo2""]
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<LinkListContent>(json);

			Assert.AreEqual(2, o.links.Count);
			Assert.AreEqual("primitive.foo", o.links[0].GetId());
			Assert.AreEqual(true, o.links[0].WasCreated);
			Assert.AreEqual("primitive.foo2", o.links[1].GetId());
			Assert.AreEqual(true, o.links[1].WasCreated);
		}

		[Test]
		public void ArrayNumber()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""numbers"": {
         ""data"": [1,2,3]
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<ArrayNumberContent>(json);

			Assert.AreEqual(3, o.numbers.Length);
			Assert.AreEqual(1, o.numbers[0]);
			Assert.AreEqual(2, o.numbers[1]);
			Assert.AreEqual(3, o.numbers[2]);
		}

		[Test]
		public void ArrayNestedNumber()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {""numbers"":[1,2,3]}
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<NestedArrayNumberContent>(json);

			Assert.AreEqual(3, o.sub.numbers.Length);
			Assert.AreEqual(1, o.sub.numbers[0]);
			Assert.AreEqual(2, o.sub.numbers[1]);
			Assert.AreEqual(3, o.sub.numbers[2]);
		}

		[Test]
		public void ListNumber()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""numbers"": {
         ""data"": [1,2,3]
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<ListNumberContent>(json);

			Assert.AreEqual(3, o.numbers.Count);
			Assert.AreEqual(1, o.numbers[0]);
			Assert.AreEqual(2, o.numbers[1]);
			Assert.AreEqual(3, o.numbers[2]);
		}

		[Test]
		public void ListString()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""strings"": {
         ""data"": [""a"", ""b"",""c""]
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<ListStringContent>(json);

			Assert.AreEqual(3, o.strings.Count);
			Assert.AreEqual("a", o.strings[0]);
			Assert.AreEqual("b", o.strings[1]);
			Assert.AreEqual("c", o.strings[2]);
		}

		[Test]
		public void ListStringEmpty()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""strings"": {
         ""data"": []
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<ListStringContent>(json);

			Assert.AreEqual(0, o.strings.Count);
		}

		[Test]
		public void ListStringWithNull()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""strings"": {
         ""data"": [null, null]
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<ListStringContent>(json);

			Assert.AreEqual(2, o.strings.Count);

			Assert.AreEqual(null, o.strings[0]);
			Assert.AreEqual(null, o.strings[1]);
		}

		[Test]
		public void InvalidJson_ThrowsException()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""strings"": {
         ""data"":
      }
   }
}";

			var s = new TestSerializer();
			Assert.Throws<ContentDeserializationException>(() => s.Deserialize<ListStringContent>(json));
		}

		[Test]
		public void Addressable()
		{
			var fakeGuid = Guid.NewGuid().ToString();
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sprite"": {
         ""data"": {
            ""referenceKey"": """ + fakeGuid + @""",
            ""subObjectName"":""tuna""}
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<SpriteAddressableContent>(json);

			Assert.AreEqual(fakeGuid, o.sprite.AssetGUID);
			Assert.AreEqual("tuna", o.sprite.SubObjectName);
		}

		[Test]
		public void Enum()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""e"": {
         ""data"": ""B""
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<EnumContent>(json);

			Assert.AreEqual(TestEnum.B, o.e);
		}

		[Test]
		public void CustomContentField_Works()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""tunafish"": {
         ""data"": 123
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<CustomContentField>(json);

			Assert.AreEqual(123, o.FooBar);
		}

		[Test]
		public void DictStringToString()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""Dict"": { ""data"": {
         ""a"": ""v1"",
         ""b"": ""v2"",
      } }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<SerializeDictStringToString>(json);

			for (int i = 0; i < 66; i++)
			{
				s.Deserialize<SerializeDictStringToString>(json);
			}

			Assert.AreEqual(true, o.Dict.TryGetValue("a", out var aValue));
			Assert.AreEqual("v1", aValue);

			Assert.AreEqual(true, o.Dict.TryGetValue("b", out var bValue));
			Assert.AreEqual("v2", bValue);
		}

		[Test]
		public void DictStringToInt()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""Dict"": { ""data"": {
         ""a"": 2,
         ""b"": 4,
      } }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<SerializeDictStringToInt>(json);

			for (int i = 0; i < 66; i++)
			{
				s.Deserialize<SerializeDictStringToInt>(json);
			}

			Assert.AreEqual(true, o.Dict.TryGetValue("a", out var aValue));
			Assert.AreEqual(2, aValue);

			Assert.AreEqual(true, o.Dict.TryGetValue("b", out var bValue));
			Assert.AreEqual(4, bValue);
		}

		[Test]
		public void DeserializationWithCallback()
		{
			var s = new TestSerializer();
			var json = @"{
   ""id"":""test.test"",
   ""version"":"""",
   ""properties"":{
   ""value"":{
      ""data"":1
      },
         ""nested"":{
            ""data"":{
               ""value"":1
            }
         }
      }
   }".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");
			var obj = s.Deserialize<SerializeWithCallbackContent>(json);
			Assert.NotNull(obj);
			Assert.Zero(obj.value);
			Assert.NotNull(obj.nested);
			Assert.Zero(obj.nested.value);
		}

		[Test]
		public void FormerlyContentField_Works()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""greatscott"": {
         ""data"": 123
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<FormerlyContentField>(json);

			Assert.AreEqual(123, o.FooBar);
		}

		[Test]
		public void NestedFormerlyContentField_Works()
		{
			var json = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""Nested"": {
         ""data"": { ""greatscott"":123 }
      }
   }
}";

			var s = new TestSerializer();
			var o = s.Deserialize<FormerlyContentNestedContent>(json);

			Assert.AreEqual(123, o.Nested.FooBar);
		}

		[Test]
		public void FormerlySerializedTypeName_Works()
		{
			var json = @"{
   ""id"": ""bogus.nothing"",
   ""version"": """",
   ""properties"": {
      ""x"": { ""data"": 5 }
   }
}";

			cache.AddContentTypeToDictionaries(typeof(FormerlyContentName));
			var s = new TestSerializer();
			var o = s.Deserialize<FormerlyContentName>(json);

			Assert.AreEqual(5, o.x);
			Assert.AreEqual("test.nothing", o.Id);
		}

		[Test]
		public void FormerlySerializedTypeName_WhenMissing_UsesDataId()
		{
			var json = @"{
   ""id"": ""bogus.nothing"",
   ""version"": """",
   ""properties"": {
      ""x"": { ""data"": 5 }
   }
}";
			var s = new TestSerializer();
			var o = s.Deserialize<FormerlyContentName>(json);

			Assert.AreEqual(5, o.x);
			Assert.AreEqual("bogus.nothing", o.Id);
		}

		[Test]
		public void TestBeam3283_CorruptAnnouncement()
		{
			var json = "{\r\n  \"id\": \"announcements.welcome\",\r\n  \"version\": \"a3cdf85da9ea673d8be5825f1b5e4cd4420dda02\",\r\n  \"properties\": {\r\n    \"body\": {\r\n      \"data\": \"Welcome to Beamable. It is awesome. You are awesome. We're both awesome. High five!!\"\r\n    },\r\n    \"channel\": {\r\n      \"data\": \"main\"\r\n    },\r\n    \"gift\": {\r\n      \"data\": {\r\n        \"callWebhooks\": {\r\n          \"Value\": [],\r\n          \"HasValue\": false\r\n        },\r\n        \"description\": {\r\n          \"Value\": null,\r\n          \"HasValue\": false\r\n        },\r\n        \"changeCurrencies\": {\r\n          \"Value\": [],\r\n          \"HasValue\": false\r\n        },\r\n        \"addItems\": {\r\n          \"Value\": [],\r\n          \"HasValue\": false\r\n        },\r\n        \"applyVipBonus\": {\r\n          \"Value\": false,\r\n          \"HasValue\": false\r\n        }\r\n      }\r\n    },\r\n    \"title\": {\r\n      \"data\": \"Welcome\"\r\n    },\r\n    \"attachments\": {\r\n      \"data\": [\r\n        {\r\n          \"symbol\": \"items.hat\",\r\n          \"count\": 1,\r\n          \"type\": \"items\"\r\n        }\r\n      ]\r\n    },\r\n    \"summary\": {\r\n      \"data\": \"Thanks for using DE!\"\r\n    }\r\n  }\r\n}";
			var s = new ClientContentSerializer();
			var o = s.Deserialize<AnnouncementContent>(json);

		}

		[Test]
		public void TestBeam3332_CorruptTournament()
		{
			var json = "{\r\n  \"id\": \"tournaments.default\",\r\n  \"version\": \"179eb82587e0ac4d26122e41e0fa05aed70cf6f3\",\r\n  \"properties\": {\r\n    \"ChampionColor\": {\r\n      \"data\": {\r\n        \"r\": 0.968627453,\r\n        \"g\": 0.709803939,\r\n        \"b\": 0,\r\n        \"a\": 1\r\n      }\r\n    },\r\n    \"name\": {\r\n      \"data\": \"Demo Tournament\"\r\n    },\r\n    \"stageChanges\": {\r\n      \"data\": [\r\n        {\r\n          \"minRank\": 1,\r\n          \"maxRank\": 1,\r\n          \"delta\": 2,\r\n          \"color\": {\r\n            \"r\": 0.968627453,\r\n            \"g\": 0.709803939,\r\n            \"b\": 0,\r\n            \"a\": 1\r\n          }\r\n        },\r\n        {\r\n          \"minRank\": 2,\r\n          \"maxRank\": 2,\r\n          \"delta\": 2,\r\n          \"color\": {\r\n            \"r\": 0.7921569,\r\n            \"g\": 0.7921569,\r\n            \"b\": 0.7921569,\r\n            \"a\": 1\r\n          }\r\n        },\r\n        {\r\n          \"minRank\": 3,\r\n          \"maxRank\": 3,\r\n          \"delta\": 2,\r\n          \"color\": {\r\n            \"r\": 0.721568644,\r\n            \"g\": 0.333333343,\r\n            \"b\": 0.160784319,\r\n            \"a\": 1\r\n          }\r\n        },\r\n        {\r\n          \"minRank\": 4,\r\n          \"maxRank\": 10,\r\n          \"delta\": 1,\r\n          \"color\": {\r\n            \"r\": 0,\r\n            \"g\": 0,\r\n            \"b\": 0,\r\n            \"a\": 0\r\n          }\r\n        },\r\n        {\r\n          \"minRank\": 11,\r\n          \"maxRank\": 20,\r\n          \"delta\": 0,\r\n          \"color\": {\r\n            \"r\": 0.6901961,\r\n            \"g\": 0.6901961,\r\n            \"b\": 0.6901961,\r\n            \"a\": 1\r\n          }\r\n        },\r\n        {\r\n          \"minRank\": 41,\r\n          \"maxRank\": 50,\r\n          \"delta\": -1,\r\n          \"color\": {\r\n            \"r\": 0.623529434,\r\n            \"g\": 0,\r\n            \"b\": 0,\r\n            \"a\": 1\r\n          }\r\n        }\r\n      ]\r\n    },\r\n    \"playerLimit\": {\r\n      \"data\": 25\r\n    },\r\n    \"DefaultEntryColor\": {\r\n      \"data\": {\r\n        \"r\": 0.623529434,\r\n        \"g\": 0,\r\n        \"b\": 0,\r\n        \"a\": 1\r\n      }\r\n    },\r\n    \"tiers\": {\r\n      \"data\": [\r\n        {\r\n          \"name\": \"Bronze\",\r\n          \"color\": {\r\n            \"r\": 0.721568644,\r\n            \"g\": 0.333333343,\r\n            \"b\": 0.160784319,\r\n            \"a\": 1\r\n          }\r\n        },\r\n        {\r\n          \"name\": \"Silver\",\r\n          \"color\": {\r\n            \"r\": 0.7921569,\r\n            \"g\": 0.7921569,\r\n            \"b\": 0.7921569,\r\n            \"a\": 1\r\n          }\r\n        },\r\n        {\r\n          \"name\": \"Gold\",\r\n          \"color\": {\r\n            \"r\": 0.968627453,\r\n            \"g\": 0.709803939,\r\n            \"b\": 0,\r\n            \"a\": 1\r\n          }\r\n        }\r\n      ]\r\n    },\r\n    \"anchorTimeUTC\": {\r\n      \"data\": \"2020-01-01T12:00:00Z\"\r\n    },\r\n    \"stagesPerTier\": {\r\n      \"data\": 3\r\n    },\r\n    \"rankRewards\": {\r\n      \"data\": [\r\n        {\r\n          \"name\": \"Winner\",\r\n          \"tier\": 0,\r\n          \"stageMin\": 1,\r\n          \"stageMax\": 1,\r\n          \"minRank\": 1,\r\n          \"maxRank\": 1,\r\n          \"currencyRewards\": [\r\n            {\r\n              \"amount\": 1000,\r\n              \"symbol\": \"currency.gems\"\r\n            }\r\n          ]\r\n        },\r\n        {\r\n          \"name\": \"Bronze Participation\",\r\n          \"tier\": 0,\r\n          \"stageMin\": \"\",\r\n          \"stageMax\": \"\",\r\n          \"minRank\": \"\",\r\n          \"maxRank\": \"\",\r\n          \"currencyRewards\": [\r\n            {\r\n              \"amount\": 20,\r\n              \"symbol\": \"currency.gems\"\r\n            }\r\n          ]\r\n        },\r\n        {\r\n          \"name\": \"Silver Participation\",\r\n          \"tier\": 1,\r\n          \"stageMin\": \"\",\r\n          \"stageMax\": \"\",\r\n          \"minRank\": \"\",\r\n          \"maxRank\": \"\",\r\n          \"currencyRewards\": [\r\n            {\r\n              \"amount\": 50,\r\n              \"symbol\": \"currency.gems\"\r\n            }\r\n          ]\r\n        },\r\n        {\r\n          \"name\": \"Gold Participation\",\r\n          \"tier\": 2,\r\n          \"stageMin\": \"\",\r\n          \"stageMax\": \"\",\r\n          \"minRank\": \"\",\r\n          \"maxRank\": \"\",\r\n          \"currencyRewards\": [\r\n            {\r\n              \"amount\": 80,\r\n              \"symbol\": \"currency.gems\"\r\n            }\r\n          ]\r\n        }\r\n      ]\r\n    },\r\n    \"groupRewards\": {\r\n      \"data\": {\r\n        \"rankRewards\": []\r\n      }\r\n    },\r\n    \"cycleDuration\": {\r\n      \"data\": \"P1D\"\r\n    },\r\n    \"scoreRewards\": {\r\n      \"data\": [\r\n        {\r\n          \"name\": \"Winner Score\",\r\n          \"tier\": 0,\r\n          \"minScore\": 1,\r\n          \"maxScore\": 1,\r\n          \"stageMin\": 1,\r\n          \"stageMax\": 1,\r\n          \"currencyRewards\": []\r\n        }\r\n      ]\r\n    }\r\n  }\r\n}";
			var s = new ClientContentSerializer();
			var o = s.Deserialize<TournamentContent>(json);

		}

#pragma warning disable CS0649

		class TestContent : TestContentObject
		{
			public string text;
			public float number;
			public long longNumber;
			public bool flag;
			public double bigNumber;
			public uint positiveInteger;
			public char character;
			public byte singleByte;
		}

		class TestContentComplex : TestContentObject
		{
			public TestContent sub;
		}

		class TestOptional : TestContentObject
		{
			public OptionalInt maybeNumber;
		}

		class TestOptionalString : TestContentObject
		{
			public OptionalString maybeString;
		}

		class NestedOptional : TestContentObject
		{
			public TestOptional sub;
		}

		class ColorContent : TestContentObject
		{
			public Color color;
		}

		class PropertyColorContent : TestContentObject
		{
			[field: SerializeField]
			public Color Color { get; set; }
		}

		class PrimitiveRef : TestContentRef<TestContent> { }

		class PrimitiveLink : TestContentLink<TestContent> { }

		class RefContent : TestContentObject
		{
			public PrimitiveRef reference;
		}

		class NestedRefContent : TestContentObject
		{
			public RefContent sub;
		}

		class ListNumberContent : TestContentObject
		{
			public List<int> numbers;
		}

		class ListStringContent : TestContentObject
		{
			public List<string> strings;
		}

		class ArrayNumberContent : TestContentObject
		{
			public int[] numbers;
		}

		class NestedArrayNumberContent : TestContentObject
		{
			public ArrayNumberContent sub;
		}

		class SpriteAddressableContent : TestContentObject
		{
			public AssetReferenceSprite sprite;
		}

		class LinkContent : TestContentObject
		{
			public PrimitiveLink link;
		}

		class LinkRefContent : TestContentObject
		{
			public PrimitiveRef link;
		}

		class LinkArrayContent : TestContentObject
		{
			public PrimitiveLink[] links;
		}

		class LinkListContent : TestContentObject
		{
			public List<PrimitiveLink> links;
		}

		class LinkNestedContent : TestContentObject
		{
			public LinkContent sub;
		}

		class LinkArrayNestedContent : TestContentObject
		{
			public LinkArrayContent sub;
		}

		enum TestEnum { A, B, C }

		class EnumContent : TestContentObject
		{
			public TestEnum e;
		}

		class CustomContentField : TestContentObject
		{
			[ContentField("tunafish")]
			public int FooBar;
		}

		class FormerlyContentField : TestContentObject
		{
			//[ContentField(FormerlySerializedAs = new []{"greatscott"})]
			[ContentField(formerlySerializedAs: "greatscott")]
			public int FooBar;
		}

		class FormerlyContentNestedContent : TestContentObject
		{
			public FormerlyContentField Nested;
		}

		[ContentType("test")]
		[ContentFormerlySerializedAs("bogus")]
		class FormerlyContentName : TestContentObject
		{
			public int x;
		}

		class SerializeDictStringToString : TestContentObject
		{
			public SerializableDictionaryStringToString Dict;
		}

		class SerializeDictStringToInt : TestContentObject
		{
			public SerializableDictionaryStringToInt Dict;
		}

		class SerializeWithCallbackContent : TestContentObject, ISerializationCallbackReceiver
		{
			public int value = 0;
			public SerializeWithCallbackObject nested = new SerializeWithCallbackObject();

			public void OnBeforeSerialize()
			{
				value += 1;
			}

			public void OnAfterDeserialize()
			{
				value -= 1;
			}
		}

		[Serializable]
		class SerializeWithCallbackObject : ISerializationCallbackReceiver
		{
			public int value = 0;

			public void OnBeforeSerialize()
			{
				value += 1;
			}

			public void OnAfterDeserialize()
			{
				value -= 1;
			}
		}

#pragma warning restore CS0649
	}
}
