using Beamable.Common.Api;
using Beamable.Common.Spew;
using Beamable.Pooling;
using Beamable.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beamable.Api.Analytics
{
	/// <summary>
	/// Analytics service.
	/// This service provides an API to communicate with the Platform
	/// </summary>
	public class AnalyticsService
	{
		private PlatformRequester _requester;
		private IPlatformService _platform;

		public AnalyticsService(IPlatformService platform, PlatformRequester requester)
		{
			_platform = platform;
			_requester = requester;
		}

		/// <summary>
		/// Sends a single analytics event.
		/// </summary>
		/// <param name="eventRequest">Event request.</param>
		internal void SendAnalyticsEvent(AnalyticsEventRequest eventRequest)
		{
			var eventBatch = new List<string>();
			eventBatch.Add(eventRequest.Payload);

			AnalyticsEventBatchRequest(eventBatch);
		}

		/// <summary>
		/// Sends the analytics event batch.
		/// This method also groups batches by gamertag, and issues a request for each
		/// </summary>
		/// <param name="eventBatch">Event batch.</param>
		internal void SendAnalyticsEventBatch(List<AnalyticsEventRequest> eventBatch)
		{
			if (eventBatch.Count == 0) return;
			var batch = new List<string>();

			for (int i = 0; i < eventBatch.Count; i++)
			{
				batch.Add(eventBatch[i].Payload);
			}
			AnalyticsEventBatchRequest(batch);
		}

		/// <summary>
		/// Analytics Event Batch Request coroutine.
		/// This constructs the web request and sends it to the Platform
		/// </summary>
		/// <returns>The event batch request.</returns>
		/// <param name="eventBatch">Event batch.</param>
		void AnalyticsEventBatchRequest(List<string> eventBatch)
		{
			long gamerTag = _platform.User.id;
			if (gamerTag == 0)
			{
				gamerTag = 1;
			}
			string uri = String.Format("/report/custom_batch/{0}/{1}/{2}", _platform.Cid, _platform.Pid, gamerTag);

			string batchJson;
			using (var pooledBuilder = StringBuilderPool.StaticPool.Spawn())
			{
				var builder = pooledBuilder.Builder;

				builder.Append('[');
				for (int i = 0; i < eventBatch.Count; i++)
				{
					builder.AppendFormat("{0},", eventBatch[i]);
				}
				// Replace last ',' with ']'
				builder[builder.Length - 1] = ']';

				batchJson = builder.ToString();
			}

			byte[] batchPayload = Encoding.UTF8.GetBytes(batchJson);
			AnalyticsLogger.LogFormat("AnalyticsService.AnalyticsEventBatchRequest: Sending batch of {0} to uri: {1}", eventBatch.Count, uri);

			var request = _requester.BuildWebRequest(Method.POST, uri, "application/json", batchPayload);
			var op = request.SendWebRequest();
			op.completed += _ =>
			{
				request.Dispose();
			};
		}
	}

	/// <summary>
	/// Analytics event request.
	/// This is the request object which is used to send data to the Platform
	/// </summary>
	internal class AnalyticsEventRequest : JsonSerializable.ISerializable
	{
		/// <summary>
		/// Gets the payload.
		/// The payload is a json string
		/// </summary>
		/// <value>The payload.</value>
		public string Payload
		{
			get { return _payload; }
		}

		private string _payload;

		/// <summary>
		/// Initializes a new instance of the <see cref="AnalyticsEventRequest"/> class.
		/// Used mostly for serialization
		/// </summary>
		public AnalyticsEventRequest() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="AnalyticsEventRequest"/> class.
		/// </summary>
		/// <param name="gamerTag">Gamertag.</param>
		/// <param name="payload">Payload (json string)</param>
		public AnalyticsEventRequest(string payload)
		{
			_payload = payload;
		}

		/// <summary>
		/// Serialize the specified object back and forth from json.
		/// </summary>
		/// <param name="s">Serialization stream</param>
		public void Serialize(JsonSerializable.IStreamSerializer s)
		{
			s.Serialize("payload", ref _payload);
		}

	}
}
