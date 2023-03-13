using Beamable.Api.Connectivity;
using Beamable.Common;
using System;

namespace Beamable.Platform.Tests.Connectivity
{
	public class MockConnectivityService : IConnectivityService
	{
		private bool _connectivity = true;
		public bool HasConnectivity => _connectivity;
		public bool ForceDisabled
		{
			get;
			set;
		}

		public bool Disabled
		{
			get;
		}

		public event Action<bool> OnConnectivityChanged;
		public Promise SetHasInternet(bool hasInternet)
		{
			_connectivity = hasInternet;
			OnConnectivityChanged?.Invoke(_connectivity);
			return Promise.Success;
		}

		public Promise ReportInternetLoss()
		{
			return SetHasInternet(false);
		}

		public void OnReconnectOnce(Action onReconnection)
		{

		}

		public void OnReconnectOnce(ConnectionCallback promise, int order = 0)
		{

		}
	}
}
