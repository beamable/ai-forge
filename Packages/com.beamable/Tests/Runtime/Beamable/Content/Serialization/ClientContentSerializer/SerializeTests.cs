using Beamable.Common.Content;
using Beamable.Tests.Content.Serialization.Support;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Beamable.Tests.Content.Serialization.ClientContentSerializationTests
{
	public class SerializeTests
	{
		[Test]
		public void Primitives()
		{
			var c = new PrimitiveContent
			{
				Id = "test.nothing",
				x = 3,
				b = true,
				s = "test",
				f = 3.2f,
				d = 3.4,
				l = 101,
				u = 7,
				c = '#',
				by = 2
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""x"": { ""data"": 3 },
      ""b"": { ""data"": true },
      ""s"": { ""data"": ""test"" },
      ""f"": { ""data"": 3.2 },
      ""d"": { ""data"": 3.4 },
      ""l"": { ""data"": 101 },
      ""u"": { ""data"": 7 },
      ""c"": { ""data"": 35 },
      ""by"": { ""data"": 2 }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void Primitive_SubClassed()
		{
			var c = new PrimitiveSubclass()
			{
				Id = "test.nothing",
				x = 3,
				b = true,
				s = "test",
				f = 3.2f,
				d = 3.4,
				l = 101,
				y = 9,
				bb = true,
				u = 7,
				c = '#',
				by = 2
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""y"": { ""data"": 9 },
      ""bb"": { ""data"": true },
      ""x"": { ""data"": 3 },
      ""b"": { ""data"": true },
      ""s"": { ""data"": ""test"" },
      ""f"": { ""data"": 3.2 },
      ""d"": { ""data"": 3.4 },
      ""l"": { ""data"": 101 },
      ""u"": { ""data"": 7 },
      ""c"": { ""data"": 35 },
      ""by"": { ""data"": 2 }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void SerializeField_Serialize()
		{
			var c = new SerializeFieldContent(3)
			{
				Id = "test.nothing"
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""x"": { ""data"": 3 }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void SerializeFieldSubClass_Serialize()
		{
			var c = new SerializeFieldSubContent(3, 2)
			{
				Id = "test.nothing"
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""y"": { ""data"": 2 },
      ""x"": { ""data"": 3 }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void IdAndVersion()
		{
			var c = new PrimitiveContent
			{
				Id = "test.nothing",
				Version = "123",
				x = 3,
				b = true,
				s = "test",
				f = 3.2f,
				d = 3.4,
				l = 101,
				u = 7,
				c = '#',
				by = 2
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": ""123"",
   ""properties"": {
      ""x"": { ""data"": 3 },
      ""b"": { ""data"": true },
      ""s"": { ""data"": ""test"" },
      ""f"": { ""data"": 3.2 },
      ""d"": { ""data"": 3.4 },
      ""l"": { ""data"": 101 },
      ""u"": { ""data"": 7 },
      ""c"": { ""data"": 35 },
      ""by"": { ""data"": 2 }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void Nested()
		{
			var c = new NestedContent
			{
				Id = "test.nothing",
				sub = new PrimitiveContent
				{
					x = 3,
					b = true,
					s = "test",
					f = 3.2f,
					d = 3.4,
					l = 101,
					u = 7,
					c = '#',
					by = 2
				}
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": { ""data"": {
         ""x"":  3,
         ""b"":  true,
         ""s"":  ""test"",
         ""f"":  3.2,
         ""d"":  3.4,
         ""l"":  101,
         ""u"": 7,
         ""c"": 35,
         ""by"": 2
      } }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void OptionalWithValue()
		{
			var c = new OptionalContent
			{
				Id = "test.nothing",
				maybeNumber = new OptionalInt
				{
					HasValue = true,
					Value = 32
				}
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""maybeNumber"": { ""data"": 32 }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void OptionalWithoutValue()
		{
			var c = new OptionalContent
			{
				Id = "test.nothing",
				maybeNumber = new OptionalInt { HasValue = false, Value = 32 }
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void OptionalNestedWithValue()
		{
			var c = new NestedOptionalContent
			{
				Id = "test.nothing",
				sub = new OptionalContent
				{
					Id = "sub.nothing",
					maybeNumber = new OptionalInt { HasValue = true, Value = 30 }
				}
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {""maybeNumber"": 30}
      }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void OptionalNestedWithoutValue()
		{
			var c = new NestedOptionalContent
			{
				Id = "test.nothing",
				sub = new OptionalContent
				{
					Id = "sub.nothing",
					maybeNumber = new OptionalInt { HasValue = false, Value = 30 }
				}
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {}
      }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void Color()
		{
			var c = new ColorContent
			{
				Id = "test.nothing",
				color = new Color(1f, 0f, 0f)
			};
			var expected = @"{
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
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void PropertyColor()
		{
			var c = new PropertyColorContent()
			{
				Id = "test.nothing",
				Color = new Color(1f, 0f, 0f)
			};
			var expected = @"{
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
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void Ref()
		{
			var c = new RefContent
			{
				Id = "test.nothing",
				reference = new PrimitiveRef { Id = "primitive.foo" }
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""reference"": {
         ""data"": ""primitive.foo""

      }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void RefNested()
		{
			var c = new NestedRefContent
			{
				Id = "test.nothing",
				sub = new RefContent
				{
					reference = new PrimitiveRef { Id = "primitive.foo" }
				}
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {
            ""reference"": ""primitive.foo""
         }
      }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void Link()
		{
			var c = new LinkContent
			{
				Id = "test.nothing",
				link = new PrimitiveLink { Id = "primitive.foo" }
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""link"": {
         ""$link"": ""primitive.foo""
      }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void LinkNested()
		{
			var c = new LinkNestedContent
			{
				Id = "test.nothing",
				sub = new LinkContent { link = new PrimitiveLink { Id = "primitive.foo" } }
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {""link"": ""primitive.foo"" }
      }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void LinkArray()
		{
			var c = new LinkArrayContent
			{
				Id = "test.nothing",
				links = new PrimitiveLink[] {
					new PrimitiveLink {Id = "primitive.foo"},
					new PrimitiveLink {Id = "primitive.foo2"},
				}
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""links"": {
         ""$links"": [""primitive.foo"", ""primitive.foo2""]
      }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void LinkList()
		{
			var c = new LinkListContent
			{
				Id = "test.nothing",
				links = new List<PrimitiveLink> {
					new PrimitiveLink {Id = "primitive.foo"},
					new PrimitiveLink {Id = "primitive.foo2"},
				}
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""links"": {
         ""$links"": [""primitive.foo"", ""primitive.foo2""]
      }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void ListNumbers()
		{
			var c = new NumberListContent
			{
				Id = "test.nothing",
				numbers = new List<int> { 1, 2, 3 }
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""numbers"": {
         ""data"": [1,2,3]
      }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void ArrayNumbers()
		{
			var c = new NumberArrayContent
			{
				Id = "test.nothing",
				numbers = new int[] { 1, 2, 3 }
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""numbers"": {
         ""data"": [1,2,3]
      }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void ArrayNestedNumbers()
		{
			var c = new NestedNumberArrayContent
			{
				Id = "test.nothing",
				sub = new NumberArrayContent
				{
					numbers = new int[] { 1, 2, 3 }
				}
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {""numbers"": [1,2,3]}
      }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void Addressable()
		{
			var fakeGuid = Guid.NewGuid().ToString();
			var c = new SpriteAddressableContent
			{
				Id = "test.nothing",
				sprite = new AssetReferenceSprite(fakeGuid)
			};
			c.sprite.SubObjectName = "tuna";
			var expected = (@"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""sprite"": {
         ""data"": {""referenceKey"": """ + fakeGuid + @""", ""subObjectName"": ""tuna""}
      }
   }
}").Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void Enum()
		{
			var c = new EnumContent
			{
				Id = "test.nothing",
				e = TestEnum.B
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""e"": {
         ""data"": ""B""
      }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void DictStringToString()
		{
			var c = new SerializeDictStringToString
			{
				Id = "test.nothing",
				Dict = new SerializableDictionaryStringToString {
					{"a", "v1"},
					{"b", "v2"},
				}
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""Dict"": {
         ""data"": { ""a"": ""v1"", ""b"": ""v2"" }
      }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void DictStringToInt()
		{
			var c = new SerializeDictStringToInt
			{
				Id = "test.nothing",
				Dict = new SerializableDictionaryStringToInt {
					{"a", 2},
					{"b", 4},
				}
			};
			var expected = @"{
   ""id"": ""test.nothing"",
   ""version"": """",
   ""properties"": {
      ""Dict"": {
         ""data"": { ""a"": 2, ""b"": 4 }
      }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");

			var s = new TestSerializer();
			var json = s.Serialize(c);

			Assert.AreEqual(expected, json);
		}

		[Test]
		public void SerializationWithCallback()
		{
			var s = new TestSerializer();
			var obj = new SerializeWithCallbackContent();
			var expected = @"{
   ""id"":null,
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
			var json = s.Serialize(obj).Replace("\r\n", "").Replace("\n", "").Replace(" ", "");
			Debug.LogWarning(json + "\n" + expected);
			Debug.LogWarning(json.Equals(expected));
			Assert.AreEqual(json, expected);
		}

		[Test]
		public void NullArray_Nested_SerializesAsEmpty()
		{
			var c = new NestedNumberArrayContent()
			{
				Id = "test.tuna",
				sub = new NumberArrayContent
				{
					numbers = null
				}
			};
			var expected = @"{
   ""id"": ""test.tuna"",
   ""version"": """",
   ""properties"": {
      ""sub"": {
         ""data"": {""numbers"":[]}
      }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");
			var s = new TestSerializer();
			var json = s.Serialize(c);
			Assert.AreEqual(expected, json);
		}

		[Test]
		public void NullArray_SerializesAsEmpty()
		{
			var c = new NumberArrayContent
			{
				Id = "test.tuna",
				numbers = null
			};
			var expected = @"{
   ""id"": ""test.tuna"",
   ""version"": """",
   ""properties"": {
      ""numbers"": {
         ""data"": []
      }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");
			var s = new TestSerializer();
			var json = s.Serialize(c);
			Assert.AreEqual(expected, json);
		}

		[Test]
		public void NullList_SerializesAsEmpty()
		{
			var c = new NumberListContent
			{
				Id = "test.tuna",
				numbers = null
			};
			var expected = @"{
   ""id"": ""test.tuna"",
   ""version"": """",
   ""properties"": {
      ""numbers"": {
         ""data"": []
      }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");
			var s = new TestSerializer();
			var json = s.Serialize(c);
			Assert.AreEqual(expected, json);
		}

		[Test]
		public void NullSerializable_SerializesWithDefaultInstance()
		{
			var c = new NestedContent
			{
				Id = "test.tuna",
				sub = null
			};
			var expected = @"{
   ""id"": ""test.tuna"",
   ""version"": """",
   ""properties"": {
      ""sub"": { ""data"": {
         ""x"":  0,
         ""b"":  false,
         ""s"":  null,
         ""f"":  0,
         ""d"":  0,
         ""l"":  0,
         ""u"": 0,
         ""c"": 0,
         ""by"": 0
      } }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");
			var s = new TestSerializer();
			var json = s.Serialize(c);
			Assert.AreEqual(expected, json);
		}

		[Test]
		public void CustomFieldName_SerializeWithCustomName()
		{
			var c = new CustomFieldNameContent
			{
				Id = "test.tuna",
				FooBar = 123
			};
			var expected = @"{
   ""id"": ""test.tuna"",
   ""version"": """",
   ""properties"": {
      ""tunafish"": { ""data"": 123 }
   }
}".Replace("\r\n", "").Replace("\n", "").Replace(" ", "");
			var s = new TestSerializer();
			var json = s.Serialize(c);
			Assert.AreEqual(expected, json);
		}

		[System.Serializable]
		class PrimitiveContent : TestContentObject
		{
			public int x;
			public bool b;
			public string s;
			public float f;
			public double d;
			public long l;
			public uint u;
			public char c;
			public byte by;
		}

		[System.Serializable]
		class PrimitiveSubclass : PrimitiveContent
		{
			public int y;
			public bool bb;
		}

		class NestedContent : TestContentObject
		{
			public PrimitiveContent sub;
		}

		class OptionalContent : TestContentObject
		{
			public OptionalInt maybeNumber;
		}

		class NestedOptionalContent : TestContentObject
		{
			public OptionalContent sub;
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

		class PrimitiveRef : TestContentRef<PrimitiveContent> { }

		class PrimitiveLink : TestContentLink<PrimitiveContent> { }

		class RefContent : TestContentObject
		{
			public PrimitiveRef reference;
		}

		class NestedRefContent : TestContentObject
		{
			public RefContent sub;
		}

		class NumberArrayContent : TestContentObject
		{
			public int[] numbers;
		}

		class NestedNumberArrayContent : TestContentObject
		{
			public NumberArrayContent sub;
		}

		class NumberListContent : TestContentObject
		{
			public List<int> numbers;
		}

		class SpriteAddressableContent : TestContentObject
		{
			public AssetReferenceSprite sprite;
		}

		class LinkContent : TestContentObject
		{
			public PrimitiveLink link;
		}

		class LinkNestedContent : TestContentObject
		{
			public LinkContent sub;
		}

		class LinkArrayContent : TestContentObject
		{
			public PrimitiveLink[] links;
		}

		class LinkListContent : TestContentObject
		{
			public List<PrimitiveLink> links;
		}

		enum TestEnum
		{
			A, B, C
		}

		class EnumContent : TestContentObject
		{
			public TestEnum e;
		}

		class CustomFieldNameContent : TestContentObject
		{
			[ContentField("tunafish")]
			public int FooBar;
		}

		class SerializeFieldContent : TestContentObject
		{
			[SerializeField]
			protected int x;

			public SerializeFieldContent() { }

			public SerializeFieldContent(int x)
			{
				this.x = x;
			}
		}

		class SerializeFieldSubContent : SerializeFieldContent
		{
			[SerializeField]
			private int y;

			public SerializeFieldSubContent(int x, int y)
			{
				this.x = x;
				this.y = y;
			}

			public SerializeFieldSubContent() { }
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
	}
}
