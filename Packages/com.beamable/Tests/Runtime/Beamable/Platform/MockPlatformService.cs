using Beamable.Api;
using Beamable.Api.Connectivity;
using Beamable.Api.Notification;
using Beamable.Api.Sessions;
using Beamable.Common;
using Beamable.Common.Api.Auth;
using Beamable.Common.Api.Notifications;
using Beamable.Coroutines;
using Beamable.Platform.Tests.Connectivity;
using System;

namespace Beamable.Platform.Tests
{
	public class MockPlatformService : IPlatformService
	{
		public long UserId => User.id;
		public User User { get; set; }
		public Promise<Unit> OnReady { get; }
		INotificationService IPlatformService.Notification => Notification;

		public IPubnubNotificationService PubnubNotificationService { get; }
		public IHeartbeatService Heartbeat { get; }
		public string Cid { get; }
		public string Pid { get; }
		public string TimeOverride { get; set; }
		// These events only exist to satisfy the interface, so we suppress CS0067. ~ACM 2021-03-23
#pragma warning disable 0067
		public event Action OnShutdown;
		public event Action OnReloadUser;
		public event Action TimeOverrideChanged;
#pragma warning restore 0067
		public NotificationService Notification { get; }
		public IConnectivityService ConnectivityService { get; set; }
		public CoroutineService CoroutineService { get; }

		public MockPlatformService()
		{


			Notification = new NotificationService();
			ConnectivityService = new MockConnectivityService();

			OnReady = new Promise<Unit>();
			OnReady.CompleteSuccess(PromiseBase.Unit);
		}
	}
}
