using Beamable.Api.Analytics.Batch;
using Beamable.Common.Spew;
using Beamable.Coroutines;
using Beamable.Serialization;
using System.Collections.Generic;

namespace Beamable.Api.Analytics
{
	/// <summary>
	/// This type defines the %Client main entry point for the %Analytics feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/analytics-feature">Analytics</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public interface IAnalyticsTracker
	{
		/// <summary>
		/// Report some analytics event to the Beamable Cloud.
		/// Note that this analytic event may not be reported immediately unless the <see cref="sendImmediately"/> argument is set.
		/// Otherwise, the event will be recorded during the next analytics batch event.
		/// </summary>
		/// <param name="analyticsEvent">
		/// An instance of a <see cref="IAnalyticsEvent"/>.
		/// You can create your own types of events. For an example, see, <see cref="SampleCustomEvent"/>
		/// </param>
		/// <param name="sendImmediately">If set to <c>true</c> send immediately.</param>
		void TrackEvent(IAnalyticsEvent analyticsEvent, bool sendImmediately);

		/// <summary>
		/// Change the <see cref="BatchSettings"/> that the <see cref="IAnalyticsTracker"/> uses to send messages to Beamable.
		/// You can revert these changes by using the <see cref="RevertBatchSettings"/> method.
		/// </summary>
		/// <param name="batchSettings">A new set of <see cref="BatchSettings"/>. This will override the existing settings.</param>
		/// <param name="flush">Force that any pending analytic events are sent to Beamable at this moment.</param>
		void UpdateBatchSettings(BatchSettings batchSettings, bool flush = true);

		/// <summary>
		/// Revert any <see cref="BatchSettings"/> customizations made through the <see cref="UpdateBatchSettings"/> method.
		/// </summary>
		/// <param name="flush">Force that any pending analytic events are sent to Beamable at this moment.</param>
		void RevertBatchSettings(bool flush = true);
	}

	public struct BatchSettings
	{
		/// <summary>
		/// How many seconds go by before a batch event is forced.
		/// </summary>
		public int TimeoutSeconds;

		/// <summary>
		/// How many analytic events can occur before a batch event is forced.
		/// </summary>
		public int MaxSize;

		/// <summary>
		/// The frequency that the batch events may be processed on.
		/// </summary>
		public float HeartbeatInterval;

		public BatchSettings(int timeout, int maxSize, float heartbeat)
		{
			TimeoutSeconds = timeout;
			MaxSize = maxSize;
			HeartbeatInterval = heartbeat;
		}
	}

	/// <summary>
	/// This type defines the %Client main entry point for the %Analytics feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/analytics-feature">Analytics</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class AnalyticsTracker : IAnalyticsTracker
	{
		/// <summary>
		/// The storage key used to persist batches to disk.
		/// </summary>
		private const string _storageKey = "analytics_event_batch";

		/// <summary>
		/// The default batch capacity threshold for expiration.
		/// </summary>
		private const int _defaultBatchCapacity = 100;

		/// <summary>
		/// The default batch timeout in seconds for expiration.
		/// </summary>
		private const double _defaultBatchTimeout = 60f;

		private List<AnalyticsEventRequest> _earlyRequests = new List<AnalyticsEventRequest>();
		private IPlatformService _platform;
		private AnalyticsService service;

		/// <summary>
		/// The Batch Manager responsible for managing analytics batches.
		/// </summary>
		private PersistentBatchManager<AnalyticsEventRequest> batchManager;

		private BatchSettings _defaultBatchSettings;
		private bool usingDefaultSettings = true;

		/// <summary>
		/// Initialize the specified platformServer, customerId, projectId, batchTimeoutSeconds and batchMaxSize.
		/// Analytics Events can be tracked before initialization, but will not be sent until initialization occurs.
		/// </summary>
		/// <param name="batchTimeoutSeconds">Batch timeout seconds before expiration.</param>
		/// <param name="batchMaxSize">Batch max size before expiration.</param>
		public AnalyticsTracker(IPlatformService platform, PlatformRequester requester, CoroutineService coroutineService, int batchTimeoutSeconds, int batchMaxSize)
		{
			_platform = platform;
			batchManager = new PersistentBatchManager<AnalyticsEventRequest>(coroutineService, _storageKey, _defaultBatchCapacity, _defaultBatchTimeout);

			this.service = new AnalyticsService(platform, requester);

			// Set up default settings
			_defaultBatchSettings.HeartbeatInterval = 1f;
			_defaultBatchSettings.TimeoutSeconds = batchTimeoutSeconds;
			_defaultBatchSettings.MaxSize = batchMaxSize;

			batchManager.OnBatchExpired += OnBatchExpired;
			if (usingDefaultSettings)
				UpdateBatchSettings(_defaultBatchSettings);

			// Add Saved events into batch now that we have a proper gamertag to assign them to.
			AnalyticsLogger.LogFormat("Backfilling early requests: {0} total", _earlyRequests.Count);
			for (var i = 0; i < _earlyRequests.Count; i++)
			{
				var request = _earlyRequests[i];
				batchManager.Add(request);
			}
			_earlyRequests.Clear();


			batchManager.Flush();
			batchManager.Start();
		}

		public void TrackEvent(IAnalyticsEvent analyticsEvent, bool sendImmediately = false)
		{
			var analyticsEventRequest = BuildRequest(analyticsEvent);

			if (sendImmediately)
			{
				AnalyticsLogger.LogFormat("AnalyticsTracker.TrackEvent: Immediate payload={0}", analyticsEventRequest.Payload);
				service.SendAnalyticsEvent(analyticsEventRequest);
			}
			else
			{
				AnalyticsLogger.LogFormat("AnalyticsTracker.TrackEvent: Batching payload={0}", analyticsEventRequest.Payload);
				batchManager.Add(analyticsEventRequest);
			}
		}

		public void UpdateBatchSettings(BatchSettings batchSettings, bool flush = true)
		{
			UpdateBatchSettingsInternal(batchSettings, flush, false);
		}

		private void UpdateBatchSettingsInternal(BatchSettings batchSettings, bool flush, bool isDefault)
		{
			usingDefaultSettings = isDefault;
			batchManager.SetCapacity(batchSettings.MaxSize);
			batchManager.SetTimeoutSeconds(batchSettings.TimeoutSeconds);
			batchManager.SetHeartbeat(batchSettings.HeartbeatInterval);

			if (flush)
			{
				batchManager.Flush();
			}
		}

		public void RevertBatchSettings(bool flush = true)
		{
			UpdateBatchSettingsInternal(_defaultBatchSettings, flush, true);
		}

		private AnalyticsEventRequest BuildRequest(IAnalyticsEvent analyticsEvent)
		{
			using (var jsonSaveStream = JsonSerializable.JsonSaveStream.Spawn())
			{
				// Start as Object and Serialize Directly, so that we can inject shard
				jsonSaveStream.Init(JsonSerializable.JsonSaveStream.JsonType.Object);
				analyticsEvent.Serialize(jsonSaveStream);
				// TODO: Double check that we don't support shards anymore.
				// string shard = _platform.Shard;
				// if(shard != null)
				// 	jsonSaveStream.Serialize("shard", ref shard);
				jsonSaveStream.Conclude();
				return new AnalyticsEventRequest(jsonSaveStream.ToString());
			}
		}

		/// <summary>
		/// Raises the batch expired event.
		/// </summary>
		/// <param name="batchList">Batch list.</param>
		private void OnBatchExpired(List<AnalyticsEventRequest> batchList)
		{
			AnalyticsLogger.LogFormat("AnalyticsTracker.OnBatchExpired: Sending {0} events.", batchList.Count);
			service.SendAnalyticsEventBatch(batchList);
		}
	}
}
