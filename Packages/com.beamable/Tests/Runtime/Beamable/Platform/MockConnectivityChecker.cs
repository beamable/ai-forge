using Beamable.Api.Connectivity;
using Beamable.Common;
using Beamable.Common.Api;

namespace Beamable.Platform.Tests
{
	public class MockConnectivityChecker : IConnectivityChecker
	{
		private readonly IConnectivityService _connectivityService;
		public bool ConnectivityCheckingEnabled { get; set; }

		public MockConnectivityChecker(IConnectivityService connectivityService)
		{
			_connectivityService = connectivityService;
			var _ = _connectivityService.SetHasInternet(true);
		}

		public async Promise<bool> ForceCheck()
		{
			await _connectivityService.SetHasInternet(true);
			return true;
		}
	}
}
