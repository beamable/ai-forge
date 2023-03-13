using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Dependencies;
using Beamable.Config;
using Beamable.Coroutines;
using System.Collections;
using UnityEngine;

namespace Beamable.Api.Connectivity
{
	public class GatewayConnectivityChecker : IConnectivityChecker
	{
		private readonly IConnectivityService _connectivityService;
		private readonly IRequester _requester;
		private readonly IDependencyProviderScope _provider;

		public bool ConnectivityCheckingEnabled { get; set; }

		private const float _secondsBeforeTimeout = 5.0f;
		private const float _secondsBetweenCheck = 3;
		public string ConnectivityRoute { get; private set; }

		private const string HEALTH_ROUTE_CONFIG_KEY = "connectivityRoute";
		private const string DEFAULT_HEALTH_PATH = "/health";

		private float _pingStartDateTime;
		private readonly WaitForSeconds _delay;

		public GatewayConnectivityChecker(
			IConnectivityService connectivityService,
			CoroutineService coroutineService,
			IPlatformRequester requester,
			IDependencyProviderScope provider)
		{
			_connectivityService = connectivityService;
			_requester = requester;
			_provider = provider;

			_delay = new WaitForSeconds(_secondsBetweenCheck);
			if (!ConfigDatabase.TryGetString(HEALTH_ROUTE_CONFIG_KEY, out var route))
			{
				route = DEFAULT_HEALTH_PATH;
			}
			ConnectivityRoute = route;
			coroutineService.StartCoroutine(MonitorConnectivity());
		}

		public async Promise<bool> ForceCheck()
		{
			try
			{
				if (Application.internetReachability == NetworkReachability.NotReachable)
				{
					throw new NoConnectivityException(nameof(Application.internetReachability) + " is not reachable.");
				}
				await _requester.BeamableRequest(new SDKRequesterOptions<EmptyResponse>
				{
					uri = ConnectivityRoute,
					method = Method.GET,
					useCache = false,
					includeAuthHeader = false,
					useConnectivityPreCheck = false
				});
			}
			catch (NoConnectivityException)
			{
				await _connectivityService.SetHasInternet(false);
				return false;
			}

			return true;
		}

		private IEnumerator MonitorConnectivity()
		{
			yield return new ConnectivityService.PromiseYieldInstruction(_connectivityService.SetHasInternet(true));
			while (_provider.IsActive)
			{
				yield return _delay; // don't spam the internet checking...

				if (!ConnectivityCheckingEnabled) continue; // if the checker isn't enabled, then this just sits here doing nothing... // TODO: check this... 

				_pingStartDateTime = Time.time;

				var request = _requester.BeamableRequest(new SDKRequesterOptions<EmptyResponse>
				{
					uri = ConnectivityRoute,
					method = Method.GET,
					useCache = false,
					includeAuthHeader = false,
					useConnectivityPreCheck = false
				}).Error(_ =>
				{
					// the error case is handled below...
				});

				var isTimeout = false;
				while (!request.IsCompleted && !isTimeout)
				{
					isTimeout = Time.time - _pingStartDateTime > _secondsBeforeTimeout;
					yield return null;
				}

				if (isTimeout || request.IsFailed)
				{
					yield return new ConnectivityService.PromiseYieldInstruction(_connectivityService.SetHasInternet(false));
				}
			}
		}

	}
}
