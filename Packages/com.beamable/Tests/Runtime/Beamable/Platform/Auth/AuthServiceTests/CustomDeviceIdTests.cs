using Beamable.Api.Auth;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Auth;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TestTools;

namespace Beamable.Platform.Tests.Auth.AuthServiceTests
{
	public class CustomDeviceIdTests : AuthServiceTestBase
	{
		private TestDeviceIdResolver _resolver;

		protected override IDeviceIdResolver CreateDeviceIdResolver()
		{
			_resolver = new TestDeviceIdResolver();
			return _resolver;
		}

		[UnityTest]
		public IEnumerator CustomProvider()
		{
			var deviceId = "tunafish";
			_resolver.DeviceId = deviceId;

			var getDeviceId = _deviceIdResolver.GetDeviceId();
			yield return getDeviceId.AsYield();
			Assert.AreEqual(deviceId, getDeviceId.GetResult());

			var mockReq = _requester.MockRequest<TokenResponse>(Method.POST, $"{TOKEN_URL}")
									.WithJsonFieldMatch("grant_type", "device")
									.WithJsonFieldMatch("device_id", deviceId);

			var req = _service.LoginDeviceId();
			yield return req.AsYield();

			Assert.AreEqual(1, mockReq.CallCount);
		}

		public class TestDeviceIdResolver : IDeviceIdResolver
		{
			public string DeviceId { get; set; } = "unset";
			public Promise<string> GetDeviceId()
			{
				return Promise<string>.Successful(DeviceId);
			}
		}
	}
}
