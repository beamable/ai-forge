using Beamable.Api.Connectivity;
using Beamable.Api.Sessions;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Auth;
using Beamable.Common.Api.Notifications;
using Beamable.Coroutines;
using System;

namespace Beamable.Api
{
	public interface IPlatformService : IUserContext
	{
		// XXX: This is a small subset of the PlatformService, only pulled as needed for testing purposes.

		/// <summary>
		/// An event that will trigger when the player instance is shut down
		/// </summary>
		event Action OnShutdown;

		/// <summary>
		/// An event that will trigger when the player instance changes
		/// </summary>
		event Action OnReloadUser;

		/// <summary>
		/// An event that will trigger when a time override has been applied on the <see cref="TimeOverride"/> property.
		/// </summary>
		event Action TimeOverrideChanged;

		/// <summary>
		/// The <see cref="User"/> for this player instance.
		/// </summary>
		User User { get; }

		/// <summary>
		/// A <see cref="Promise"/> that completes once the user has initialized for this player instance.
		/// </summary>
		Promise<Unit> OnReady { get; }

		/// <summary>
		/// Access to the <see cref="INotificationService"/> for this player instance.
		/// </summary>
		INotificationService Notification { get; }

		/// <summary>
		/// Access to the <see cref="IPubnubNotificationService"/> for this player instance.
		/// </summary>
		IPubnubNotificationService PubnubNotificationService { get; }

		/// <summary>
		/// Access to the <see cref="IHeartbeatService"/> for this player instance.
		/// </summary>
		IHeartbeatService Heartbeat { get; }

		/// <summary>
		/// The current Customer ID for this player instance.
		/// </summary>
		string Cid { get; }

		/// <summary>
		/// The current Realm ID for this player instance.
		/// </summary>
		string Pid { get; }

		/// <summary>
		/// A time override that can be used to fake the current Beamable known time. This can be useful to checking Listing or Announcement times.
		/// </summary>
		string TimeOverride { get; set; }

		/// <summary>
		/// Access to the <see cref="IConnectivityService"/> for this player instance.
		/// </summary>
		IConnectivityService ConnectivityService { get; }

		/// <summary>
		/// Access to the <see cref="CoroutineService"/> for this player instance.
		/// </summary>
		CoroutineService CoroutineService { get; }
	}
}
