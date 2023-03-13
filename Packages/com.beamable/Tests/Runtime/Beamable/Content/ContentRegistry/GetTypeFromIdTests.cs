using Beamable.Common.Assistant;
using Beamable.Common.Content;
using Beamable.Common.Reflection;
using Beamable.Tests.Content.Serialization.Support;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Tests.Content.ContentRegistryTests
{
	public class GetTypeFromIdTests
	{
		public ReflectionCache reflectionCache;
		public ContentTypeReflectionCache cache;

		[SetUp]
		public void Setup()
		{
			reflectionCache = new ReflectionCache();
			var hintStorage = new BeamHintGlobalStorage();
			cache = new ContentTypeReflectionCache();
			reflectionCache.RegisterTypeProvider(cache);
			reflectionCache.RegisterReflectionSystem(cache);
			reflectionCache.SetStorage(hintStorage);

			var assembliesToSweep = AppDomain.CurrentDomain.GetAssemblies().Select(asm => asm.GetName().Name).ToList();
			reflectionCache.GenerateReflectionCache(assembliesToSweep);
		}

		[Test]
		public void NonNested_Simple()
		{
			cache.AddContentTypeToDictionaries(typeof(SimpleContent));
			var type = cache.GetTypeFromId("simple.foo");

			Assert.AreEqual(typeof(SimpleContent), type);
		}

		[Test]
		public void Polymorphic_Simple()
		{
			cache.AddContentTypeToDictionaries(typeof(SimpleContent));
			cache.AddContentTypeToDictionaries(typeof(SimpleSubContent));
			var type = cache.GetTypeFromId("simple.sub.foo");

			Assert.AreEqual(typeof(SimpleSubContent), type);
		}

		[Test]
		public void NonNested_MissingType()
		{
			var type = cache.GetTypeFromId("simple.foo");
			Assert.AreEqual(typeof(ContentObject), type);
		}

		[Test]
		public void Polymorphic_MissingAllTypes()
		{
			var type = cache.GetTypeFromId("simple.sub.foo");
			Assert.AreEqual(typeof(ContentObject), type);
		}

		[Test]
		public void Polymorphic_MissingSubType()
		{
			cache.AddContentTypeToDictionaries(typeof(SimpleContent));

			var type = cache.GetTypeFromId("simple.sub.foo");
			Assert.AreEqual(typeof(SimpleContent), type);
		}

		[Test]
		public void FormerlySerializedAs_Simple()
		{
			cache.AddContentTypeToDictionaries(typeof(SimpleFormerlyContent));

			var type = cache.GetTypeFromId("oldschool.foo");
			Assert.AreEqual(typeof(SimpleFormerlyContent), type);
		}

		[Test]
		public void FormerlySerializedAs_Many()
		{
			cache.AddContentTypeToDictionaries(typeof(ManyFormerlyContent));

			var type = cache.GetTypeFromId("cool.foo");
			Assert.AreEqual(typeof(ManyFormerlyContent), type);
		}


		[Test]
		public void FormerlySerializedAs_Polymorphic()
		{
			cache.AddContentTypeToDictionaries(typeof(SimpleFormerlyContent));
			cache.AddContentTypeToDictionaries(typeof(SubFormerlyContent));

			var type1 = cache.GetTypeFromId("oldschool.oldsub.foo");
			var type2 = cache.GetTypeFromId("simple.oldsub.foo");
			var type3 = cache.GetTypeFromId("oldschool.sub.foo");
			var type4 = cache.GetTypeFromId("simple.sub.foo");
			Assert.AreEqual(typeof(SubFormerlyContent), type1);
			Assert.AreEqual(typeof(SubFormerlyContent), type2);
			Assert.AreEqual(typeof(SubFormerlyContent), type3);
			Assert.AreEqual(typeof(SubFormerlyContent), type4);
		}

		[Test]
		public void FormerlySerializedAs_Missing()
		{
			var type = cache.GetTypeFromId("oldschool.foo");
			Assert.AreEqual(typeof(ContentObject), type);
		}

		[Test]
		public void FormerlySerializedAs_Missing_Polymorphic()
		{
			cache.AddContentTypeToDictionaries(typeof(SimpleFormerlyContent));

			var type1 = cache.GetTypeFromId("oldschool.oldsub.foo");
			var type2 = cache.GetTypeFromId("simple.oldsub.foo");
			var type3 = cache.GetTypeFromId("oldschool.sub.foo");
			Assert.AreEqual(typeof(SimpleFormerlyContent), type1);
			Assert.AreEqual(typeof(SimpleFormerlyContent), type2);
			Assert.AreEqual(typeof(SimpleFormerlyContent), type3);
		}

		[Serializable]
		[ContentType("simple")]
		[ContentFormerlySerializedAs("oldschool")]
		class SimpleFormerlyContent : TestContentObject
		{

		}

		[Serializable]
		[ContentType("sub")]
		[ContentFormerlySerializedAs("oldsub")]
		class SubFormerlyContent : SimpleFormerlyContent
		{

		}

		[Serializable]
		[ContentType("simple")]
		[ContentFormerlySerializedAs("oldschool")]
		[ContentFormerlySerializedAs("cool")]
		class ManyFormerlyContent : TestContentObject
		{

		}

		[Serializable]
		[ContentType("simple")]
		class SimpleContent : TestContentObject
		{

		}

		[Serializable]
		[ContentType("sub")]
		class SimpleSubContent : SimpleContent
		{

		}
	}
}
