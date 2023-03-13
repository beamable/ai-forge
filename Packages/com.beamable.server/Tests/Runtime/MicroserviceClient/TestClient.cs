using Beamable.Common;
using System;
using UnityEngine;

namespace Beamable.Server.Tests.Runtime
{
	public class TestClient : MicroserviceClient
	{
		private readonly string _serviceName;

		public TestClient(string serviceName)
		{
			_serviceName = serviceName;
			MicroserviceClientHelper.SetPrefix("test");

		}

		public Promise<T> Request<T>(string endpoint, string[] serializedFields)
		{
			return base.Request<T>(_serviceName, endpoint, serializedFields);
		}

		public string GetMockPath(string cid, string pid, string endpoint)
		{
			return CreateUrl(cid, pid, _serviceName, endpoint);
		}
	}

	public class TestJSON
	{
		public int a;
		public int b;
	}

	[Serializable]
	public class TestProperties
	{
		[field: SerializeField] public int A { get; set; }
		[field: SerializeField] public string B { get; set; }
	}
}
