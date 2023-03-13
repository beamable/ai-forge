using Beamable.Common.Api;
using Beamable.Common.Api.Auth;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

namespace Beamable.Platform.Tests.Auth.AuthServiceTests
{
	public class LoginTests : AuthServiceTestBase
	{

		[UnityTest]
		public IEnumerator MakesWebCall()
		{
			var password = "password";
			var email = "test@test.com";
			var merge = false;
			var result = new TokenResponse();

			var req = _requester.MockRequest<TokenResponse>(Method.POST, $"{TOKEN_URL}")
			   .WithNoAuthHeader(merge)
			   .WithJsonFieldMatch("username", email)
			   .WithJsonFieldMatch("grant_type", "password")
			   .WithJsonFieldMatch("password", password)
			   .WithJsonFieldMatch("customerScoped", false)
			   .WithResponse(result);

			yield return _service.Login(email, password, merge)
			   .Then(response => Assert.AreEqual(result, response))
			   .AsYield();


			Assert.AreEqual(1, req.CallCount);
		}

	}
}
