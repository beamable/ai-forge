using Beamable.Common.Api;
using Beamable.Common.Content;
using Beamable.Serialization.SmallerJSON;
using Beamable.Tests.Runtime;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace Beamable.Server.Tests.Runtime
{
	public class RequestTests : BeamableTest
	{
		private const string ROUTE = "test";

		[UnityTest]
		public IEnumerator CanDeserializeList_OfInt()
		{
			var client = new TestClient(ROUTE);

			MockRequester.MockRequest<List<int>>(Method.POST,
				  client.GetMockPath(MockApi.Token.Cid, MockApi.Token.Pid, ROUTE))
			   .WithRawResponse("[1,2,3,4,5]");

			var req = client.Request<List<int>>(ROUTE, new string[] { });

			yield return req.ToYielder();
			Assert.AreEqual(new List<int> { 1, 2, 3, 4, 5 }, req.GetResult());
		}

		[UnityTest]
		public IEnumerator CanDeserializeList_OfStrings()
		{
			var client = new TestClient(ROUTE);

			MockRequester.MockRequest<List<string>>(Method.POST,
													client.GetMockPath(MockApi.Token.Cid, MockApi.Token.Pid, ROUTE))
						 .WithRawResponse("[\"a\", \"b\", \"c\"]");

			var req = client.Request<List<string>>(ROUTE, new string[] { });

			yield return req.ToYielder();
			Assert.AreEqual(new List<string> { "a", "b", "c" }, req.GetResult());
		}

		[UnityTest]
		public IEnumerator CanDeserializeDictionary_OfStrings()
		{
			var client = new TestClient(ROUTE);

			MockRequester.MockRequest<Dictionary<string, string>>(Method.POST,
																  client.GetMockPath(MockApi.Token.Cid, MockApi.Token.Pid, ROUTE))
						 .WithRawResponse("{\"one\":\"15\",\"two\":\"151\",\"three\":\"125\"}");

			var req = client.Request<Dictionary<string, string>>(ROUTE, new string[] { });

			yield return req.ToYielder();
			Assert.AreEqual(new Dictionary<string, string> { { "one", "15" }, { "two", "151" }, { "three", "125" } }, req.GetResult());
		}

		[UnityTest]
		public IEnumerator CanDeserializeDictionary_OfInts()
		{
			var client = new TestClient(ROUTE);

			MockRequester.MockRequest<Dictionary<string, int>>(Method.POST,
															   client.GetMockPath(MockApi.Token.Cid, MockApi.Token.Pid, ROUTE))
						 .WithRawResponse("{\"one\":15,\"two\":151,\"three\":125}");

			var req = client.Request<Dictionary<string, int>>(ROUTE, new string[] { });

			yield return req.ToYielder();
			Assert.AreEqual(new Dictionary<string, int> { { "one", 15 }, { "two", 151 }, { "three", 125 } }, req.GetResult());
		}

		[UnityTest]
		public IEnumerator CanDeserializeDictionary_OfLongs()
		{
			var client = new TestClient(ROUTE);

			MockRequester.MockRequest<Dictionary<string, long>>(Method.POST,
																client.GetMockPath(MockApi.Token.Cid, MockApi.Token.Pid, ROUTE))
						 .WithRawResponse("{\"one\":15,\"two\":151,\"three\":125}");

			var req = client.Request<Dictionary<string, long>>(ROUTE, new string[] { });

			yield return req.ToYielder();
			Assert.AreEqual(new Dictionary<string, long> { { "one", 15 }, { "two", 151 }, { "three", 125 } }, req.GetResult());
		}

		[UnityTest]
		public IEnumerator CanDeserializeDictionary_OfFloats()
		{
			var client = new TestClient(ROUTE);

			MockRequester.MockRequest<Dictionary<string, float>>(Method.POST,
																 client.GetMockPath(MockApi.Token.Cid, MockApi.Token.Pid, ROUTE))
						 .WithRawResponse("{\"one\":1.5,\"two\":1.51,\"three\":1.25}");

			var req = client.Request<Dictionary<string, float>>(ROUTE, new string[] { });

			yield return req.ToYielder();
			Assert.AreEqual(new Dictionary<string, float> { { "one", 1.5f }, { "two", 1.51f }, { "three", 1.25f } }, req.GetResult());
		}

		[UnityTest]
		public IEnumerator CanDeserializeDictionary_OfDoubles()
		{
			var client = new TestClient(ROUTE);

			MockRequester.MockRequest<Dictionary<string, double>>(Method.POST,
																 client.GetMockPath(MockApi.Token.Cid, MockApi.Token.Pid, ROUTE))
						 .WithRawResponse("{\"one\":1.5,\"two\":1.51,\"three\":1.25}");

			var req = client.Request<Dictionary<string, double>>(ROUTE, new string[] { });

			yield return req.ToYielder();
			Assert.AreEqual(new Dictionary<string, double> { { "one", 1.5 }, { "two", 1.51 }, { "three", 1.25 } }, req.GetResult());
		}

		[UnityTest]
		public IEnumerator CanDeserializeDictionary_OfBools()
		{
			var client = new TestClient(ROUTE);

			MockRequester.MockRequest<Dictionary<string, bool>>(Method.POST,
																client.GetMockPath(MockApi.Token.Cid, MockApi.Token.Pid, ROUTE))
						 .WithRawResponse("{\"one\":true,\"two\":false,\"three\":true}");

			var req = client.Request<Dictionary<string, bool>>(ROUTE, new string[] { });

			yield return req.ToYielder();
			Assert.AreEqual(new Dictionary<string, bool> { { "one", true }, { "two", false }, { "three", true } }, req.GetResult());
		}

		[UnityTest]
		public IEnumerator CanDeserializeList_OfTypedObjects()
		{
			var client = new TestClient(ROUTE);

			MockRequester.MockRequest<List<SimplePoco>>(Method.POST,
				  client.GetMockPath(MockApi.Token.Cid, MockApi.Token.Pid, ROUTE))
			   .WithRawResponse("[{\"A\": 1}, {\"A\": 2}]");

			var req = client.Request<List<SimplePoco>>(ROUTE, new string[] { });

			yield return req.ToYielder();
			Assert.AreEqual(new List<SimplePoco> { new SimplePoco { A = 1 }, new SimplePoco { A = 2 } }, req.GetResult());
		}

		[UnityTest]
		public IEnumerator CanNotDeserializePolymorphicList()
		{
			var client = new TestClient(ROUTE);

			MockRequester.MockRequest<List<object>>(Method.POST,
				  client.GetMockPath(MockApi.Token.Cid, MockApi.Token.Pid, ROUTE))
			   .WithRawResponse("[3, {\"A\": 2}, \"b\", true]");

			var req = client.Request<List<object>>(ROUTE, new string[] { });

			yield return req.ToYielder();
			Assert.IsNull(req.GetResult());
		}

		[System.Serializable]
		public class SimplePoco
		{
			public int A;

			public override bool Equals(object obj)
			{
				return obj != null && obj is SimplePoco casted && casted.A == A;
			}

			public override int GetHashCode()
			{
				return A;
			}

			public override string ToString() => $"A=[{A}]";
		}

		[System.Serializable]
		public class LocalizeContentObject : ContentObject
		{
			[SerializeField]
			public string Title = "";

			[SerializeField]
			public int RandomSeed = 3;

			public override bool Equals(object obj)
			{
				return obj != null && obj is LocalizeContentObject casted && casted.Title == Title && casted.RandomSeed == RandomSeed;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode(); // need to override because of Equals override
			}
		}

		[UnityTest]
		public IEnumerator CanDeserializeBoolean()
		{
			var client = new TestClient(ROUTE);

			MockRequester.MockRequest<bool>(Method.POST,
				  client.GetMockPath(MockApi.Token.Cid, MockApi.Token.Pid, ROUTE))
			   .WithRawResponse("true");

			var req = client.Request<bool>(ROUTE, new string[] { });

			yield return req.ToYielder();
			Assert.AreEqual(true, req.GetResult());
		}

		[UnityTest]
		public IEnumerator CanDeserializeInt()
		{
			var client = new TestClient(ROUTE);

			MockRequester.MockRequest<int>(Method.POST,
				  client.GetMockPath(MockApi.Token.Cid, MockApi.Token.Pid, ROUTE))
			   .WithRawResponse("31");

			var req = client.Request<int>(ROUTE, new string[] { });

			yield return req.ToYielder();
			Assert.AreEqual(31, req.GetResult());
		}

		[UnityTest]
		public IEnumerator CanDeserializeString()
		{
			var client = new TestClient(ROUTE);

			MockRequester.MockRequest<string>(Method.POST,
				  client.GetMockPath(MockApi.Token.Cid, MockApi.Token.Pid, ROUTE))
			   .WithRawResponse("hello world");

			var req = client.Request<string>(ROUTE, new string[] { });

			yield return req.ToYielder();
			Assert.AreEqual("hello world", req.GetResult());
		}

		[UnityTest]
		public IEnumerator CanDeserializeJSONString()
		{
			TestJSON jsonObj = new TestJSON
			{
				a = 10,
				b = 20
			};

			string serialized = JsonUtility.ToJson(jsonObj);

			var client = new TestClient(ROUTE);

			MockRequester.MockRequest<string>(Method.POST,
											  client.GetMockPath(MockApi.Token.Cid, MockApi.Token.Pid, ROUTE))
						 .WithRawResponse(serialized);

			var req = client.Request<string>(ROUTE, new string[] { });

			yield return req.ToYielder();
			Assert.AreEqual(serialized, req.GetResult());
		}

		[UnityTest]
		public IEnumerator CanDeserializeObject()
		{
			var client = new TestClient(ROUTE);

			MockRequester.MockRequest<Vector2>(Method.POST,
				  client.GetMockPath(MockApi.Token.Cid, MockApi.Token.Pid, ROUTE))
			   .WithRawResponse("{\"x\": 1, \"y\": 3}");

			var req = client.Request<Vector2>(ROUTE, new string[] { });

			yield return req.ToYielder();
			Assert.AreEqual(new Vector2(1, 3), req.GetResult());
		}

		[UnityTest]
		public IEnumerator CanDeserializeProperties()
		{
			var client = new TestClient(ROUTE);

			MockRequester.MockRequest<TestProperties>(Method.POST,
													  client.GetMockPath(MockApi.Token.Cid, MockApi.Token.Pid, ROUTE))
						 .WithRawResponse("{\"<A>k__BackingField\": 7, \"<B>k__BackingField\": \"test string\"}");

			var req = client.Request<TestProperties>(ROUTE, new string[] { });

			yield return req.ToYielder();
			var obj = req.GetResult();
			Assert.AreEqual(obj.A, 7);
			Assert.AreEqual(obj.B, "test string");
		}

		[UnityTest]
		public IEnumerator CanDeserializeContentObject()
		{
			var client = new TestClient(ROUTE);

			MockRequester.MockRequest<LocalizeContentObject>(Method.POST,
											   client.GetMockPath(MockApi.Token.Cid, MockApi.Token.Pid, ROUTE))
						 .WithRawResponse("{\"Title\": \"Tst\", \"RandomSeed\": 3}");

			var req = client.Request<LocalizeContentObject>(ROUTE, new string[] { });

			yield return req.ToYielder();

			var tmp = ScriptableObject.CreateInstance<LocalizeContentObject>();
			tmp.Title = "Tst";
			tmp.RandomSeed = 3;

			Assert.AreEqual(tmp, req.GetResult());
		}

		[UnityTest]
		public IEnumerator CanDeserializeListOf_ContentObject()
		{
			var client = new TestClient(ROUTE);

			MockRequester.MockRequest<List<LocalizeContentObject>>(Method.POST,
																   client.GetMockPath(MockApi.Token.Cid, MockApi.Token.Pid, ROUTE))
						 .WithRawResponse("[{\"Title\": \"Tst1\", \"RandomSeed\": 1},{\"Title\": \"Tst2\", \"RandomSeed\": 2} ]");

			var req = client.Request<List<LocalizeContentObject>>(ROUTE, new string[] { });

			yield return req.ToYielder();

			var tmpList = new List<LocalizeContentObject>();

			for (int i = 1; i <= 2; i++)
			{
				var tmp = ScriptableObject.CreateInstance<LocalizeContentObject>();
				tmp.Title = "Tst" + i;
				tmp.RandomSeed = i;
				tmpList.Add(tmp);
			}

			Assert.AreEqual(tmpList, req.GetResult());
		}
	}
}
