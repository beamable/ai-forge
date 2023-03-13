using Beamable.Api.Auth;
using Beamable.Common.Api.Auth;
using NUnit.Framework;

namespace Beamable.Platform.Tests.Auth.AuthServiceTests
{
	public class AuthServiceTestBase
	{
		public const string ROUTE = "/basic/accounts";
		public const string TOKEN_URL = "/basic/auth/token";

		protected MockPlatformAPI _requester;
		protected AuthService _service;
		protected User _sampleUser;
		protected IDeviceIdResolver _deviceIdResolver;

		[SetUp]
		public void Init()
		{
			_requester = new MockPlatformAPI();
			_sampleUser = new User();
			_deviceIdResolver = CreateDeviceIdResolver();
			_service = new AuthService(_requester, _deviceIdResolver);
		}

		protected virtual IDeviceIdResolver CreateDeviceIdResolver()
		{
			return new DefaultDeviceIdResolver();
		}

		[TearDown]
		public void Cleanup()
		{

		}

	}

}
