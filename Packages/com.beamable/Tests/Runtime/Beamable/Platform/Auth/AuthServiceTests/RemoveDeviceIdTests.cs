using Beamable.Common.Api;
using Beamable.Common.Api.Auth;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TestTools;

namespace Beamable.Platform.Tests.Auth.AuthServiceTests
{
	public class RemoveDeviceIdTests : AuthServiceTestBase
	{
		[UnityTest]
		public IEnumerator PassingNullSendsEmptyJson()
		{
			var mockReq = _requester.MockRequest<User>(Method.DELETE, $"{ROUTE}/me/device")
									.WithoutJsonField("deviceIds")
									.WithResponse(_sampleUser);

			var req = _service.RemoveDeviceIds(null);
			yield return req.AsYield();

			Assert.AreEqual(1, mockReq.CallCount);
		}

		[UnityTest]
		public IEnumerator PassingAnEmptyArraySendsEmptyArray()
		{
			var mockReq = _requester.MockRequest<User>(Method.DELETE, $"{ROUTE}/me/device")
									.WithJsonFieldMatch("deviceIds", x =>
									{
										if (x is List<object> strArr)
										{
											return strArr.Count == 0;
										}


										return false;
									})
									.WithResponse(_sampleUser);

			var req = _service.RemoveDeviceIds(new string[] { });
			yield return req.AsYield();

			Assert.AreEqual(1, mockReq.CallCount);
		}

		[UnityTest]
		public IEnumerator PassAnArrayWithDataSendsTheData()
		{
			var mockReq = _requester.MockRequest<User>(Method.DELETE, $"{ROUTE}/me/device")
									.WithJsonFieldMatch("deviceIds", x =>
									{
										if (x is List<object> strArr)
										{
											return strArr.Count == 2;
										}

										return false;
									})
									.WithResponse(_sampleUser);

			var req = _service.RemoveDeviceIds(new string[] { "a", "b" });
			yield return req.AsYield();

			Assert.AreEqual(1, mockReq.CallCount);
		}

	}
}
