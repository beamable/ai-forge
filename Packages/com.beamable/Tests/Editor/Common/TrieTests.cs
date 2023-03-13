using Beamable.Common;
using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace Beamable.Editor.Tests.Common
{
	public class TrieTests
	{


		[TestCase(
			new object[] { "a.b", "a.c" },
			new object[] { "a", "a.d" },
			new object[] { "a" },
			TestName = "relevant-test1"
			)]
		[TestCase(
			new object[] { "a.b", "a.c" },
			new object[] { "a", "a.d", "a.b.c", "a.c" },
			new object[] { "a", "a.c" },
			TestName = "relevant-test2"
		)]
		public void Relevant(object[] scopesObj, object[] requestObj, object[] expectedObj)
		{
			var scopes = scopesObj.Cast<string>().ToArray();
			var request = requestObj.Cast<string>().ToArray();
			var expected = expectedObj.Cast<string>().ToArray();
			var t = new IntTrie();
			foreach (var scope in scopes)
			{
				t.Insert(scope, 1);
			}
			var output = t.GetRelevantKeys(request);
			Assert.AreEqual(expected.Length, output.Count);
			foreach (var e in expected)
			{
				Assert.IsTrue(output.Contains(e));
			}
		}

		[TestCase(1, TestName = "simple-1-time")]
		[TestCase(3, TestName = "simple-many-times")]
		public void Simple(int getAllCount)
		{
			var t = new IntTrie();

			t.Insert("a", 1);
			t.Insert("a", 2);
			t.Insert("a", 3);

			for (var i = 0; i < getAllCount; i++) // check the output multiple times, because there is a cache involved.
			{
				var output = t.GetAll("a");
				Assert.AreEqual(3, output.Count);
				Assert.AreEqual(1, output[0]);
				Assert.AreEqual(2, output[1]);
				Assert.AreEqual(3, output[2]);
			}
		}

		[Test]
		public void Simple_MultiPath()
		{
			var t = new IntTrie();

			t.Insert("a", 1);
			t.Insert("a.a", 2);
			t.Insert("a.a.a", 3);

			var output = t.GetAll("a");
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual(1, output[0]);
			Assert.AreEqual(2, output[1]);
			Assert.AreEqual(3, output[2]);

			output = t.GetAll("a.a");
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual(2, output[0]);
			Assert.AreEqual(3, output[1]);

			output = t.GetAll("a.a.a");
			Assert.AreEqual(1, output.Count);
			Assert.AreEqual(3, output[0]);
		}


		[Test]
		public void Simple_GetExactVsAll()
		{
			var t = new IntTrie();

			t.Insert("a", 1);
			t.Insert("a.b", 2);
			t.Insert("a.b.c", 3);


			var output = t.GetAll("a.b");
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual(2, output[0]);
			Assert.AreEqual(3, output[1]);

			output = t.GetExact("a.b");
			Assert.AreEqual(1, output.Count);
			Assert.AreEqual(2, output[0]);

		}


		[Test]
		public void Serialization()
		{
			var t = new IntTrie();

			t.Insert("a", 1);
			t.Insert("a.b", 2);
			t.Insert("a.c", 3);
			t.Insert("a.b.d", 4);
			t.Insert("a", 5);
			t.Insert("a.b", 6);

			var json = JsonUtility.ToJson(t);
			var t2 = JsonUtility.FromJson<IntTrie>(json);
			var output = t2.GetAll("a");
			Assert.AreEqual(6, output.Count);
			Assert.AreEqual(1, output[0]);
			Assert.AreEqual(5, output[1]);
			Assert.AreEqual(2, output[2]);
			Assert.AreEqual(6, output[3]);
			Assert.AreEqual(3, output[4]);
			Assert.AreEqual(4, output[5]);

		}


		[Test]
		public void SerializationOverwrite()
		{
			var t = new IntTrie();

			t.Insert("a", 1);
			t.Insert("a.b", 2);
			t.Insert("a.c", 3);
			t.Insert("a.b.d", 4);
			t.Insert("a", 5);
			t.Insert("a.b", 6);

			var json = JsonUtility.ToJson(t);
			var t2 = JsonUtility.FromJson<IntTrie>(json);

			void Check()
			{
				var output = t2.GetAll("a");
				Assert.AreEqual(6, output.Count);
				Assert.AreEqual(1, output[0]);
				Assert.AreEqual(5, output[1]);
				Assert.AreEqual(2, output[2]);
				Assert.AreEqual(6, output[3]);
				Assert.AreEqual(3, output[4]);
				Assert.AreEqual(4, output[5]);
			}

			Check();

			for (var i = 0; i < 10; i++)
			{
				JsonUtility.FromJsonOverwrite(json, t2);
				Check();
			}


		}


		[TestCase(1, TestName = "simple-change-1-time")]
		[TestCase(3, TestName = "simple-change-many-times")]
		public void SimpleChange(int getAllCount)
		{
			var t = new IntTrie();

			t.InsertRange("a", new int[] { 1, 2, 3 });
			t.InsertRange("a.b", new int[] { 4, 5 });

			for (var i = 0; i < getAllCount; i++) // check the output multiple times, because there is a cache involved.
			{
				var output = t.GetAll("a");
				Assert.AreEqual(5, output.Count);
				Assert.AreEqual(1, output[0]);
				Assert.AreEqual(2, output[1]);
				Assert.AreEqual(3, output[2]);
				Assert.AreEqual(4, output[3]);
				Assert.AreEqual(5, output[4]);
			}

			t.InsertRange("a.b.c", new[] { 6 });

			for (var i = 0; i < getAllCount; i++) // check the output multiple times, because there is a cache involved.
			{
				var output = t.GetAll("a");
				Assert.AreEqual(6, output.Count);
				Assert.AreEqual(1, output[0]);
				Assert.AreEqual(2, output[1]);
				Assert.AreEqual(3, output[2]);
				Assert.AreEqual(4, output[3]);
				Assert.AreEqual(5, output[4]);
				Assert.AreEqual(6, output[5]);
			}

		}

		[TestCase(1, TestName = "simple-nested-1-time")]
		[TestCase(3, TestName = "simple-nested-many-times")]
		public void SimpleNested(int getAllCount)
		{
			var t = new IntTrie();

			t.Insert("a", 1);
			t.Insert("a.b", 2);
			t.Insert("a.b", 3);
			t.Insert("a.b.c", 4);
			t.Insert("a.b", 5);
			t.Insert("a", 6);
			t.Insert("d", 7);

			for (var i = 0; i < getAllCount; i++) // check the output multiple times, because there is a cache involved.
			{
				var a = t.GetAll("a");
				Assert.AreEqual(6, a.Count);
				Assert.AreEqual(1, a[0]);
				Assert.AreEqual(6, a[1]);
				Assert.AreEqual(2, a[2]);
				Assert.AreEqual(3, a[3]);
				Assert.AreEqual(5, a[4]);
				Assert.AreEqual(4, a[5]);

				var b = t.GetAll("a.b");
				Assert.AreEqual(4, b.Count);
				Assert.AreEqual(2, b[0]);
				Assert.AreEqual(3, b[1]);
				Assert.AreEqual(5, b[2]);
				Assert.AreEqual(4, b[3]);

				var c = t.GetAll("a.b.c");
				Assert.AreEqual(1, c.Count);
				Assert.AreEqual(4, c[0]);

				var d = t.GetAll("d");
				Assert.AreEqual(1, d.Count);
				Assert.AreEqual(7, d[0]);
			}
		}
	}
}
