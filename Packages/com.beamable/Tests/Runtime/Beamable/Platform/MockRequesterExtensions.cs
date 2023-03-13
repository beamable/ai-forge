using Beamable.Common.Api;
using UnityEngine;

namespace Beamable.Platform.Tests
{
	public static class MockRequesterExtensions
	{
		public static MockPlatformRoute<EmptyResponse> MockPresenceCalls(this MockPlatformAPI mock, long dbid)
		{
			return mock.MockRequest<EmptyResponse>(Method.PUT, $"/players/{dbid}/presence");
		}
	}
}
