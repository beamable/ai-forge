using Beamable.Api;
using Beamable.Api.Connectivity;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Notifications;
using Beamable.Common.Dependencies;
using Beamable.Common.Spew;
using Beamable.Coroutines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Api
{
	// put constants in a separate class so that they are shared across generic params

	/// <summary>
	/// This type defines the constants of %Subscribables.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	internal static class SubscribableConsts
	{
		internal static readonly int[] RETRY_DELAYS = new int[] { 1, 2, 5, 10, 20 };
	}

	/// <summary>
	/// This type defines the subscribability of %services.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	/// <typeparam name="TPlatformSubscriber"></typeparam>
	/// <typeparam name="ScopedRsp"></typeparam>
	/// <typeparam name="Data"></typeparam>
	public interface IHasPlatformSubscriber<TPlatformSubscriber, ScopedRsp, Data>
	   where TPlatformSubscriber : PlatformSubscribable<ScopedRsp, Data>
	{
		/// <summary>
		/// Allows scopes to consume fresh data when available.
		/// </summary>
		TPlatformSubscriber Subscribable { get; }
	}

	public interface IHasPlatformSubscribers<TPlatformSubscriber, ScopedRsp, Data>
	   where TPlatformSubscriber : PlatformSubscribable<ScopedRsp, Data>
	{
		/// <summary>
		/// Allows scopes to consume fresh data when available.
		/// </summary>
		Dictionary<string, TPlatformSubscriber> Subscribables { get; }
	}

	/// <summary>
	/// This type defines the subscribability of %services.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public abstract class PlatformSubscribable<ScopedRsp, Data> : ISupportsGet<Data>, ISupportGetLatest<Data>
	{
		// protected IPlatformService platform;
		protected IBeamableRequester requester;
		protected IConnectivityService connectivityService;
		protected INotificationService notificationService;
		protected CoroutineService coroutineService;
		protected IUserContext userContext;

		protected BeamableGetApiResource<ScopedRsp> getter;

		protected readonly string service;
		private Dictionary<string, Data> scopedData = new Dictionary<string, Data>();

		private Dictionary<string, List<PlatformSubscription<Data>>> scopedSubscriptions =
		   new Dictionary<string, List<PlatformSubscription<Data>>>();

		private Dictionary<string, ScheduledRefresh> scheduledRefreshes = new Dictionary<string, ScheduledRefresh>();
		private List<string> nextRefreshScopes = new List<string>();
		private Promise<Unit> nextRefreshPromise = null;

		private int retry = 0;

		public bool UsesHierarchyScopes { get; protected set; }

		protected PlatformSubscribable(IDependencyProvider provider,
									   string service,
									   BeamableGetApiResource<ScopedRsp> getter = null)
		{
			this.connectivityService = provider.GetService<IConnectivityService>();
			this.notificationService = provider.GetService<INotificationService>();
			this.userContext = provider.GetService<IUserContext>();
			this.coroutineService = provider.GetService<CoroutineService>();
			this.requester = provider.GetService<IBeamableRequester>();

			if (getter == null)
			{
				getter = new BeamableGetApiResource<ScopedRsp>();
			}

			this.getter = getter;
			this.service = service;
			notificationService.Subscribe(String.Format("{0}.refresh", service), OnRefreshNtf);

			var platform = provider.GetService<IPlatformService>();
			platform.OnReady.Then(_ => { platform.TimeOverrideChanged += OnTimeOverride; });

			platform.OnShutdown += () => { platform.TimeOverrideChanged -= OnTimeOverride; };

			platform.OnReloadUser += () =>
			{
				this.connectivityService = provider.GetService<IConnectivityService>();
				this.notificationService = provider.GetService<INotificationService>();
				this.userContext = provider.GetService<IUserContext>();
				this.coroutineService = provider.GetService<CoroutineService>();
				this.requester = provider.GetService<IBeamableRequester>();

				Reset();
				Refresh();
			};
		}

		public void UnsubscribeAllNotifications()
		{
			notificationService?.UnsubscribeAll($"{service}.refresh");
		}

		public void PauseAllNotifications()
		{
			notificationService?.Pause($"{service}.refresh");
		}

		public void ResumeAllNotifications()
		{
			notificationService?.Resume($"{service}.refresh");
		}

		private void OnTimeOverride()
		{
			Refresh();
		}

		protected virtual void Reset()
		{
			// implementation specific clean up code...
		}

		/// <summary>
		/// Subscribe to the callback to receive fresh data when available.
		/// </summary>
		/// <param name="callback"></param>
		/// <returns></returns>
		public PlatformSubscription<Data> Subscribe(Action<Data> callback)
		{
			return Subscribe("", callback);
		}

		/// <summary>
		/// Subscribe to the callback to receive fresh data when available.
		/// </summary>
		/// <param name="scope"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public virtual PlatformSubscription<Data> Subscribe(string scope, Action<Data> callback)
		{
			scope = scope ?? String.Empty;

			List<PlatformSubscription<Data>> subscriptions;
			if (!scopedSubscriptions.TryGetValue(scope, out subscriptions))
			{
				subscriptions = new List<PlatformSubscription<Data>>();
				scopedSubscriptions.Add(scope, subscriptions);
			}

			var subscription = new PlatformSubscription<Data>(scope, callback, Unsubscribe);
			subscriptions.Add(subscription);

			Data data;
			if (scopedData.TryGetValue(scope, out data))
			{
				callback.Invoke(data);
			}
			else
			{
				// Refresh if this is the first subscription ever
				if (subscriptions.Count == 1)
					Refresh(scope);
			}

			return subscription;
		}

		/// <summary>
		/// Manually refresh the available data.
		/// </summary>
		/// <returns></returns>
		public Promise<Unit> Refresh()
		{
			return Refresh("");
		}

		private bool ShouldRejectScopeFromRefresh(string scope)
		{
			if (!UsesHierarchyScopes)
			{
				return (!scopedSubscriptions.ContainsKey(scope));
			}

			return !scopedSubscriptions.Any(kvp => scope.StartsWith(kvp.Key));
		}

		protected virtual Promise OnRefreshScope(string scope)
		{
			// do nothing.
			return Promise.Success;
		}

		/// <summary>
		/// Manually refresh the available data.
		/// </summary>
		/// <param name="scope"></param>
		/// <returns></returns>
		protected Promise<Unit> Refresh(string scope)
		{
			if (scope == "")
			{
				nextRefreshScopes.Clear();
				nextRefreshScopes.AddRange(scopedSubscriptions.Keys);
			}
			else
			{
				if (ShouldRejectScopeFromRefresh(scope))
					return Promise<Unit>.Successful(PromiseBase.Unit);

				if (nextRefreshScopes.Contains(scope) && nextRefreshPromise != null)
					return nextRefreshPromise;


				nextRefreshScopes.Add(scope);
			}

			if (nextRefreshScopes.Count == 0)
				return Promise<Unit>.Successful(PromiseBase.Unit);


			if (nextRefreshPromise == null)
			{
				nextRefreshPromise = new Promise<Unit>();

				// Need this null-check to cover errors that happen when leaving play-mode (where this method can run after Unity has already destroyed the CoroutineService's GameObject).
#if UNITY_EDITOR
				if(coroutineService != null)
#endif
				coroutineService.StartCoroutine(ExecuteRefresh());
			}


			return OnRefreshScope(scope).FlatMap(_ => nextRefreshPromise);
		}

		private IEnumerator ExecuteRefresh()
		{
			yield return Yielders.EndOfFrame;
			var promise = nextRefreshPromise;
			nextRefreshPromise = null;
			var sentScopes = nextRefreshScopes.ToArray();
			var scope = string.Join(",", nextRefreshScopes);
			nextRefreshScopes.Clear();

			ExecuteRequest(requester, CreateRefreshUrl(scope)).Error(err =>
			{
				var delay = SubscribableConsts.RETRY_DELAYS[Math.Min(retry, SubscribableConsts.RETRY_DELAYS.Length - 1)];
				PlatformLogger.Log($"PLATFORM SUBSCRIBABLE: Error {service}:{scope}:{err}; Retry {retry + 1} in {delay}");
				promise.CompleteError(err);
				var scopes = scope.Split(',');
				if (scopes.Length > 0)
				{
					// Collapse all outstanding scopes into the next refresh
					for (int i = 0; i < scopes.Length; i++)
					{
						if (!nextRefreshScopes.Contains(scopes[i]))
							nextRefreshScopes.Add(scopes[i]);
					}

					// Schedule a refresh delay to capture all outstanding scopes
					ScheduleRefresh(delay, scopes[0]);
				}
				else
				{
					ScheduleRefresh(delay, "");
				}

				// Avoid incrementing the backoff if the device is definitely not connected to the network at all.
				// This is narrow, and would still increment if the device is connected, but the internet has other problems
				if (connectivityService.HasConnectivity)
				{
					retry += 1;
				}
			}).FlatMap(x => OnRefresh(x, sentScopes)).Then(_ =>
			{
				retry = 0;
				promise.CompleteSuccess(PromiseBase.Unit);
			});
		}

		protected virtual Promise<ScopedRsp> ExecuteRequest(IBeamableRequester requester, string url)
		{
			return getter.RequestData(requester, url);
		}

		protected virtual string CreateRefreshUrl(string scope)
		{
			return getter.CreateRefreshUrl(userContext, service, scope);
		}

		/// <summary>
		/// <inheritdoc cref="GetLatest(string)"/>
		/// </summary>
		/// <returns><inheritdoc cref="GetLatest(string)"/></returns>
		public Data GetLatest()
		{
			return GetLatest("");
		}

		/// <inheritdoc cref="ISupportGetLatest{TData}.GetLatest(string)"/>
		public Data GetLatest(string scope)
		{
			Data data;
			scopedData.TryGetValue(scope, out data);
			return data;
		}

		/// <summary>
		/// Send a request and get the latest state of the subscription.
		/// This method will not trigger existing subscriptions
		/// </summary>
		/// <param name="scope"></param>
		/// <returns></returns>
		public async Promise<ScopedRsp> Fetch(string scope = "")
		{
			var scopedRsp = await ExecuteRequest(requester, CreateRefreshUrl(scope));
			return scopedRsp;
		}

		/// <summary>
		/// Manually fetch the available data. If the server hasn't delivered a new update, this method will not return the absolute latest data unless you pass forceRefresh as true.
		/// </summary>
		/// <param name="scope"></param>
		/// <returns></returns>
		public Promise<Data> GetCurrent(string scope = "")
		{
			if (scopedData.TryGetValue(scope, out var data))
			{
				return Promise<Data>.Successful(data);
			}

			var promise = new Promise<Data>();
			var subscription = Subscribe(scope, nextData => promise.CompleteSuccess(nextData));

			return promise.Then(_ => subscription.Unsubscribe());
		}

		/// <summary>
		/// Manually notify observing scopes regarding the available data.
		/// </summary>
		/// <param name="data"></param>
		public void Notify(Data data)
		{
			Notify("", data);
		}

		/// <summary>
		/// Manually notify observing scopes regarding the available data.
		/// </summary>
		/// <param name="scope"></param>
		/// <param name="data"></param>
		public void Notify(string scope, Data data)
		{
			List<PlatformSubscription<Data>> subscriptions;
			if (scopedSubscriptions.TryGetValue(scope, out subscriptions))
			{
				scopedData[scope] = data;

				for (var i = subscriptions.Count; i > 0; i--)
				{
					subscriptions[i - 1].Invoke(data);
				}
			}
		}

		protected virtual void OnRefresh(ScopedRsp data) { }

		protected virtual Promise OnRefresh(ScopedRsp data, string[] scopes)
		{
			OnRefresh(data);
			return Promise.Success;
		}

		private void OnRefreshNtf(object payloadRaw)
		{
			var payload = payloadRaw as IDictionary<string, object>;

			List<string> scopes = new List<string>();
			object delayRaw = null;
			object scopesRaw = null;
			int delay = 0;
			if (payload != null)
			{
				if (payload.TryGetValue("scopes", out scopesRaw))
				{
					List<object> scopesListRaw = (List<object>)scopesRaw;
					foreach (var next in scopesListRaw)
					{
						scopes.Add(next.ToString());
					}
				}

				if (payload.TryGetValue("delay", out delayRaw))
				{
					delay = int.Parse(delayRaw.ToString());
				}
				else
				{
					delay = 0;
				}
			}

			if (scopes.Count == 0)
			{
				foreach (var scope in scopedSubscriptions.Keys)
				{
					scopes.Add(scope);
				}
			}


			if (delay == 0)
			{
				foreach (var scope in scopes)
				{
					Refresh(scope);
				}
			}
			else
			{
				foreach (var scope in scopes)
				{
					int jitterDelay = UnityEngine.Random.Range(0, delay);
					ScheduleRefresh(jitterDelay, scope);
				}
			}
		}

		protected void ScheduleRefresh(long seconds, string scope)
		{
			DateTime refreshTime = DateTime.UtcNow.AddSeconds(seconds);
			ScheduledRefresh current;
			if (scheduledRefreshes.TryGetValue(scope, out current))
			{
				// If the existing refresh time is sooner, ignore this scheduled refresh
				if (current.refreshTime.CompareTo(refreshTime) <= 0)
				{
					PlatformLogger.Log(
					   $"PLATFORM SUBSCRIBABLE: Ignoring refresh for {service}:{scope}; there is a sooner refresh");
					return;
				}

				coroutineService.StopCoroutine(current.coroutine);
				scheduledRefreshes.Remove(scope);
			}

			var coroutine = coroutineService.StartCoroutine(RefreshIn(seconds, scope));
			scheduledRefreshes.Add(scope, new ScheduledRefresh(coroutine, refreshTime));
		}

		private IEnumerator RefreshIn(long seconds, string scope)
		{
			PlatformLogger.Log($"PLATFORM SUBSCRIBABLE: Schedule {service}:{scope} in {seconds}");
			yield return new WaitForSecondsRealtime(seconds);
			Refresh(scope);
			scheduledRefreshes.Remove(scope);
		}

		protected void Unsubscribe(string scope, PlatformSubscription<Data> subscription)
		{
			List<PlatformSubscription<Data>> subscriptions;
			if (scopedSubscriptions.TryGetValue(scope, out subscriptions))
			{
				subscriptions.Remove(subscription);
				if (subscriptions.Count == 0)
				{
					// FIXME(?): should this also cancel any scheduled refreshes for this scope?
					scopedSubscriptions.Remove(scope);
					scopedData.Remove(scope);
				}
			}
		}
	}

	// A class instead of a struct to reduce code-size bloat from the generic dictionary instantiation
	class ScheduledRefresh
	{
		public Coroutine coroutine;
		public DateTime refreshTime;

		public ScheduledRefresh(Coroutine coroutine, DateTime refreshTime)
		{
			this.coroutine = coroutine;
			this.refreshTime = refreshTime;
		}
	}

	public class PlatformSubscription<T>
	{
		private Action<T> callback;
		private string scope;
		private Action<string, PlatformSubscription<T>> onUnsubscribe;

		public string Scope => scope;

		public PlatformSubscription(string scope, Action<T> callback,
									Action<string, PlatformSubscription<T>> onUnsubscribe)
		{
			this.scope = scope;
			this.callback = callback;
			this.onUnsubscribe = onUnsubscribe;
		}

		internal void Invoke(T data)
		{
			callback.Invoke(data);
		}

		public void Unsubscribe()
		{
			onUnsubscribe.Invoke(scope, this);
		}
	}
}

namespace Beamable
{
	public static class PlatformSubscribableExtensions
	{
		/// <inheritdoc cref="PlatformSubscribable{TScopedRsp, TData}.Subscribe(Action{TData})"/>
		public static PlatformSubscription<TData> Subscribe<TPlatformSubscribable, TScopedRsp, TData>(
		   this IHasPlatformSubscriber<TPlatformSubscribable, TScopedRsp, TData> subscribable,
		   Action<TData> callback)

		   where TPlatformSubscribable : PlatformSubscribable<TScopedRsp, TData>
		{
			return subscribable.Subscribable.Subscribe(callback);
		}

		/// <summary>
		/// Send a request and get the latest state of the subscription.
		/// This method will not trigger existing subscriptions
		/// </summary>
		/// <param name="scope"></param>
		/// <returns></returns>
		public static Promise<TScopedRsp> Fetch<TPlatformSubscribable, TScopedRsp, TData>(
		   this IHasPlatformSubscriber<TPlatformSubscribable, TScopedRsp, TData> subscribable,
		   string scopes = "")
		   where TPlatformSubscribable : PlatformSubscribable<TScopedRsp, TData>
		{
			return subscribable.Subscribable.Fetch(scopes);
		}

		/// <summary>
		/// Manually fetch the available data. If the server hasn't delivered a new update, this method will not return the absolute latest data unless you pass forceRefresh as true.
		/// </summary>
		/// <param name="scope"></param>
		/// <param name="forceRefresh">If true, forces the call to trigger a refresh first. This will trigger all existing subscriptions </param>
		/// <returns></returns>
		public static Promise<TData> GetCurrent<TPlatformSubscribable, TScopedRsp, TData>(
		   this IHasPlatformSubscriber<TPlatformSubscribable, TScopedRsp, TData> subscribable,
		   string scopes = "", bool forceRefresh = false)
		   where TPlatformSubscribable : PlatformSubscribable<TScopedRsp, TData>
		{
			return forceRefresh
			   ? subscribable.Subscribable.Refresh().FlatMap(_ => subscribable.Subscribable.GetCurrent(scopes))
			   : subscribable.Subscribable.GetCurrent(scopes);
		}

		/// <inheritdoc cref="PlatformSubscribable{TScopedRsp, TData}.Subscribe(string, Action{TData})"/>
		public static PlatformSubscription<TData> Subscribe<TPlatformSubscribable, TScopedRsp, TData>(
		   this IHasPlatformSubscriber<TPlatformSubscribable, TScopedRsp, TData> subscribable,
		   string scopes,
		   Action<TData> callback)

		   where TPlatformSubscribable : PlatformSubscribable<TScopedRsp, TData>
		{
			return subscribable.Subscribable.Subscribe(scopes, callback);
		}

		/// <inheritdoc cref="PlatformSubscribable{TScopedRsp, TData}.GetLatest(string)"/>
		public static TData GetLatest<TPlatformSubscribable, TScopedRsp, TData>(
		   this IHasPlatformSubscriber<TPlatformSubscribable, TScopedRsp, TData> subscribable,
		   string scopes = "") where TPlatformSubscribable : PlatformSubscribable<TScopedRsp, TData>
		{
			return subscribable.Subscribable.GetLatest(scopes);
		}
	}
}
