using Beamable.Api.Notification.Internal;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Dependencies;
using Beamable.Common.Spew;
using Beamable.Serialization.SmallerJSON;
using PubNubMessaging.Core;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Beamable.Api.Notification
{
	public interface IPubnubSubscriptionManager
	{
		void Initialize(IPlatformService platform, IDependencyProvider provider);
		void UnsubscribeAll();
		Promise SubscribeToProvider();

		void LoadChannelHistory(string channel,
								int msgLimit,
								Action<List<object>> onHistory,
								Action<PubnubClientError> onHistoryError);

		void EnqueueOperation(PubNubOp operation, bool shouldRunNextOp = false);
	}

	/**
	 * Manage the connection and subscriptions to PubNub
	 */
	public class PubnubSubscriptionManager : MonoBehaviour, IPubnubSubscriptionManager, IBeamableDisposable
	{
		// We are setting the timeout value low due to a current bug in iOS Unity that is causing
		// disconnects and failed reconnects if we keep the timeout longer. The details of this change are summarized
		// by Pubnub tech support in ticket #14805.
		// 10-13-2015: Putting this timeout back to original, since this bug should have been fixed in Unity 5.2
		const int SubscribeTimeout = 310;
		const int NonSubscribeTimeout = 45;
		const int MaxRetries = 5;
		const int RetryInterval = 30;

		bool destroyingObject;
		bool ignoreErrors; // a flag for ignoring certain errors that occur during filter changes;
		private IPlatformService _platform;

		Pubnub pubnub;
		readonly Queue<PubNubOp> pendingOps = new Queue<PubNubOp>();

		readonly HashSet<Channel> activeChannels = new HashSet<Channel>();

		SubscriberDetailsResponse subscriberDetails;

		MultiChannelSubscriptionHandler mMultiChannelSubscriptionHelper = new MultiChannelSubscriptionHandler();

		public delegate void OnPubNubOperationDelegate();

		public Subscription Subscription => _provider.GetService<Subscription>();

		public bool PubnubIsConnected
		{
			get { return pubnub != null; }
		}

		public long NumActiveChannels
		{
			get { return activeChannels.Count; }
		}

		public string FilterExpression
		{
			get
			{
				if (PubnubIsConnected)
				{
					return pubnub.FilterExpression;
				}

				return null;
			}
			set
			{
				if (PubnubIsConnected)
				{
					// TODO: 2016-10-18 Figure out why this is erroring
					ignoreErrors =
					   true; // This shit is loud yo, calls Subscribe and Presence error handlers, which do some things we dont really need to have done.
					mMultiChannelSubscriptionHelper.ignoreError = true;
					pubnub.FilterExpression = value;
					mMultiChannelSubscriptionHelper.ignoreError = false;
					ignoreErrors = false;
				}
			}
		}

		static string deviceId
		{
			get
			{
				if (!PlayerPrefs.HasKey("DeviceID"))
				{
					PlayerPrefs.SetString("DeviceID", Guid.NewGuid().ToString());
				}

				return PlayerPrefs.GetString("DeviceID");
			}
		}

		public void Initialize(IPlatformService platform, IDependencyProvider provider)
		{
			_provider = provider;
			_platform = platform;

			// Pubnub.SetGameObject = this.gameObject;
		}

		void removeActiveChannel(string channel)
		{
			activeChannels.RemoveWhere(c => c.GetType() == typeof(Channel) && c.channel == channel);
		}

		#region Unity Monobehaviour methods

		public void Reset()
		{
			destroyingObject = true;
			UnsubscribeAll();
		}

		public void Update()
		{
			if (mMultiChannelSubscriptionHelper.CheckOperationCount())
			{
				// All operations for this subscribe/unsubscribe are complete, please run the next operation and clean the object to remove any danglers
				mMultiChannelSubscriptionHelper.Reset();
				// create a new instance to prevent callbacks from earlier subscriptions interfering with the next subscription
				mMultiChannelSubscriptionHelper = new MultiChannelSubscriptionHandler();
				RunNextOperation();
			}
		}

		public void OnDisable()
		{
			// This object is only disabled when being destroyed so unsubscribe from everything
			Reset();
		}

		#endregion

		public async Promise SubscribeToProvider()
		{
			// First we need to get the keys from the server
			PubnubSubscriptionLogger.Log("Requesting Subscriber Details");
			await _platform.PubnubNotificationService.GetSubscriberDetails().Then(rsp =>
			{
				subscriberDetails = rsp;

				pendingOps.Enqueue(new PubNubOp(PubNubOp.PNO.OpSubscribe, rsp.playerForRealmChannel));
				pendingOps.Enqueue(new PubNubOp(PubNubOp.PNO.OpSubscribe, rsp.playerChannel));
				pendingOps.Enqueue(new PubNubOp(PubNubOp.PNO.OpSubscribe, rsp.gameNotificationChannel));
				if (rsp.gameGlobalNotificationChannel != null)
				{
					pendingOps.Enqueue(new PubNubOp(PubNubOp.PNO.OpSubscribe, rsp.gameGlobalNotificationChannel));
				}

				PubnubSubscriptionLogger.Log("Subscriber Details Success");
				DoSubscribeToPubnub();
			}).Error(err =>
			{
				if (err is NoConnectivityException) return; // we don't care about a no-connectivity exception.

				Debug.LogError("ERROR - Subscriber Details Failure: " + err.ToString());
			});
		}

		void DoSubscribeToPubnub()
		{
#if UNITY_EDITOR
         if (!Application.isPlaying)
         {
	         Debug.Log("Subscribing to Pubnub done after quiting Play Mode, aborting.");
            return;
         }
#endif
			if (subscriberDetails.subscribeKey == null)
			{
				Debug.LogError("Missing Subscription Key");
				return;
			}

			// Set up the connection
			pubnub = new Pubnub(_provider, "", subscriberDetails.subscribeKey, "", "", true, gameObject);
			pubnub.SubscribeTimeout = SubscribeTimeout;
			pubnub.NonSubscribeTimeout = NonSubscribeTimeout;
			pubnub.NetworkCheckMaxRetries = MaxRetries;
			pubnub.NetworkCheckRetryInterval = RetryInterval;
			pubnub.EnableResumeOnReconnect = true;

			pubnub.AuthenticationKey = subscriberDetails.authenticationKey;

			pubnub.SessionUUID = _platform.User.id + "." + deviceId;

			// Reduce amount of console logging
			pubnub.PubnubLogLevel = LoggingMethod.Level.Error;

			// We have queued up the operations, kick them off
			RunNextOperation();
		}

		private IDependencyProvider _provider;

		public void EnqueueOperation(PubNubOp operation, bool shouldRunNextOp = false)
		{
			PubnubSubscriptionLogger.Log("==> Queuing Operation: " + operation.operation.ToString() + "channel: " +
										 operation.channel + ")");
			pendingOps.Enqueue(operation);
			if (pendingOps.Count >= 1 && shouldRunNextOp && PubnubIsConnected)
			{
				// Start up the queue
				RunNextOperation();
			}
		}

		void RunNextOperation()
		{
			if (mMultiChannelSubscriptionHelper.HasOperations)
			{
				PubnubSubscriptionLogger.Log("Pending pubnub subscription, waiting to run next operation");
				return;
			}

			if (pendingOps.Count > 0)
			{
				PubNubOp nextOp = pendingOps.Dequeue();


				PubnubSubscriptionLogger.Log("==> Running Operation: " + nextOp.operation.ToString() + "channel: " +
											 nextOp.channel + ")");

				switch (nextOp.operation)
				{
					case PubNubOp.PNO.OpSubscribe:
						// Subscribe to the channel
						PubnubSubscriptionLogger.Log("Subscribing to channel: " + nextOp.channel);
						if (string.IsNullOrEmpty(nextOp.channel))
						{
							RunNextOperation();
						}
						else
						{
							mMultiChannelSubscriptionHelper.Setup(nextOp.channel, OnSubscribeMessage, OnUnsubscribeMessage,
							   OnSubscribeError);
							pubnub.Subscribe(
							   nextOp.channel,
							   OnMessageReceived,
							   mMultiChannelSubscriptionHelper.OnConnectMessage,
							   mMultiChannelSubscriptionHelper.OnError
							);
						}

						break;
					case PubNubOp.PNO.OpUnsubscribe:
						PubnubSubscriptionLogger.Log("Unsubscribing from channel: " + nextOp.channel);

						if (string.IsNullOrEmpty(nextOp.channel))
						{
							RunNextOperation();
						}
						else
						{
							// The MultiChannelSubscriptionHelper tracks all of the callbacks so that when all of them are complete, the next operation is called at the next Update Call.
							mMultiChannelSubscriptionHelper.Setup(nextOp.channel, OnSubscribeMessage, OnUnsubscribeMessage,
							   OnUnsubscribeError);
							pubnub.Unsubscribe(
							   nextOp.channel,
							   OnMessageReceived,
							   mMultiChannelSubscriptionHelper.OnConnectMessage,
							   mMultiChannelSubscriptionHelper.OnDisconnectMessage,
							   mMultiChannelSubscriptionHelper.OnError
							);
						}

						break;
				}

				if (nextOp.onProcessCallback != null)
				{
					nextOp.onProcessCallback();
				}

				// Noop Operations Only Exist to send off a callback, so once it is done kickoff, another one.
				if (nextOp.operation == PubNubOp.PNO.OpNone)
				{
					RunNextOperation();
				}
			}
		}

		/** This method is used to unsubscribe from all channels used for notification
		 * and for chat. It is meant for use on exit
		 */
		public void UnsubscribeAll()
		{
			bool operationsNeedToRun = pendingOps.Count == 0;

			if (PubnubIsConnected)
			{
				PubnubSubscriptionLogger.Log("PubnubSubscriptionManager: Unsubscribing from all active channels");

				var channelList = new StringBuilder();
				var presenceList = new StringBuilder();

				foreach (Channel channel in activeChannels)
				{
					if (channel.GetType() == typeof(PresenceChannel))
					{
						if (presenceList.Length > 0)
							presenceList.Append("," + channel.channel);
						else
							presenceList.Append(channel.channel);
					}
					else
					{
						if (channelList.Length > 0)
							channelList.Append("," + channel.channel);
						else
							channelList.Append(channel.channel);
					}
				}

				if (channelList.Length > 0 || presenceList.Length > 0)
				{
					pendingOps.Enqueue(new PubNubOp(PubNubOp.PNO.OpUnsubscribe, channelList.ToString(),
					   presenceList.ToString()));
				}

				if (operationsNeedToRun)
				{
					RunNextOperation();
				}
			}
		}

		/**
		 * History request for the channel.
		 * We pay by the message and history messages count towards that total.
		 * Be judicious on the amount of history retrieved
		 */
		public void LoadChannelHistory(string channel, int msgLimit, Action<List<object>> onHistory, Action<PubnubClientError> onHistoryError)
		{
			if (pubnub == null)
			{
				Debug.LogError("ERROR - LoadChannelHistory: no connection");
				return;
			}

			PubnubSubscriptionLogger.Log("Requesting history for channel: " + channel + "[" + msgLimit + "]");

			Action<List<object>> onHistoryUnparsed = response =>
			{
				var pubnubMessages = new List<object>();
				var history = (List<object>)response[0];

				foreach (var item in history)
				{
					var itemDict = (IDictionary<string, object>)item;

					IDictionary<string, object> dict;
					if (itemDict["message"] is string)
					{
						var jsonMessage = (string)itemDict["message"];

						// The string was encoded to prevent Pubnub from misinterpreting the json so we
						// need to decode it.
						var decodedString = UnityWebRequest.UnEscapeURL(jsonMessage);

						dict = Json.Deserialize(decodedString) as IDictionary<string, object>;
						if (dict == null)
						{
							Debug.LogError("LoadChannelHistory: ERROR deserializing json");
							return;
						}
					}
					else if (itemDict["message"] is IDictionary<string, object>)
					{
						dict = (IDictionary<string, object>)itemDict["message"];
					}
					else
					{
						Debug.LogError("LoadChannelHistory: ERROR unexpected value type");
						return;
					}

					object messageFull;
					dict.TryGetValue("messageFull", out messageFull);
					object parsedPayload = Json.Deserialize(messageFull as string);
					pubnubMessages.Add(parsedPayload);
				}

				onHistory(pubnubMessages);
			};

			pubnub.DetailedHistory(channel, msgLimit, true, onHistoryUnparsed, onHistoryError);
		}

		public void OnMessageReceived(object result)
		{
			// The result will be an object array with 3 elements
			// [0] - the message json
			// [1] - pubnub time
			// [2] - the channel
			List<object> resultArray = (List<object>)result;

			if (resultArray.Count >= 3)
			{
				IDictionary<string, object> dict;
				if (resultArray[0] is string)
				{
					string jsonMessage = (string)resultArray[0];

					// The string was encoded to prevent Pubnub from misinterpreting the json so we
					// need to decode it.
					string decodedString = UnityWebRequest.UnEscapeURL(jsonMessage);

					PubnubSubscriptionLogger.Log("OnMessageReceived: Got Json: " + decodedString);
					PubnubSubscriptionLogger.Log("OnMessageReceived: Filter Was: " + FilterExpression);

					dict = Json.Deserialize(decodedString) as IDictionary<string, object>;
					if (dict == null)
					{
						Debug.LogError("OnMessageReceived: ERROR deserializing json");
						return;
					}
				}
				else if (resultArray[0] is IDictionary<string, object>)
				{
					dict = (IDictionary<string, object>)resultArray[0];
				}
				else
				{
					Debug.LogError("OnMessageReceived: ERROR unexpected value type");
					return;
				}

				object context;
				object messageFull;
				dict.TryGetValue("context", out context);
				dict.TryGetValue("messageFull", out messageFull);

				//            PubnubSubscriptionLogger.Log("OnMessageReceived: context: " + context + " messageFull: " + messageFull);

				// Invoke notification service
				object parsedPayload = Json.Deserialize(messageFull as string);

				if (parsedPayload == null)
				{
					_platform.Notification.Publish(
					   context as string,
					   messageFull
					);
				}
				else
				{
					_platform.Notification.Publish(
					   context as string,
					   parsedPayload
					);
				}
			}
			else
			{
				Debug.LogError("OnMessageReceived: ERROR processing result: " + result);
			}
		}

		public void OnSubscribeMessage(object result)
		{
			List<object> connectMsg = (List<object>)result;
			if (connectMsg.Count >= 3)
			{
				int statusCode = Int32.Parse(connectMsg[0].ToString());
				string statusMessage = (string)connectMsg[1];
				string channel = (string)connectMsg[2];

				if (statusCode == 1 && statusMessage.ToLower() == "connected")
				{
					PubnubSubscriptionLogger.LogFormat("Subscribed to channel: {0}", channel);
					activeChannels.Add(new Channel(channel));
				}
			}
		}

		public void OnUnsubscribeMessage(object result)
		{
			// A separate unsubscribe message will generate for every channel that unsubscribes
			if (PubnubIsConnected)
			{
				List<object> disconnectMsg = (List<object>)result;
				if (disconnectMsg.Count >= 3)
				{
					//int statusCode = Int32.Parse(disconnectMsg[0].ToString());
					string statusMessage = (string)disconnectMsg[1];
					string channel = (string)disconnectMsg[2];

					removeActiveChannel(channel);

					PubnubSubscriptionLogger.Log("OnUnsubscribeMessage: " + statusMessage);

					if (destroyingObject && NumActiveChannels == 0)
					{
						PubnubSubscriptionLogger.Log("OnUnsubscribeMessage: INVOKING EndPendingRequests");
						pubnub.EndPendingRequests();
						pubnub = null;
					}
				}
			}
		}

		public void OnSubscribeError(PubnubClientError pubnubError)
		{
			if (ignoreErrors)
				return;

			bool fullDetails = false;
			bool isAnError = false;

			var errorMsg = new StringBuilder();
			switch (pubnubError.Severity)
			{
				case PubnubErrorSeverity.Critical:
					//This type of error needs to be handled.
					errorMsg.Append("Subscribe Critical ERROR: ");
					fullDetails = true;
					isAnError = true;
					break;
				case PubnubErrorSeverity.Warn:
					//This type of error needs to be handled
					errorMsg.Append("Subscribe WARN: ");
					break;
				case PubnubErrorSeverity.Info:
					//This type of error can be ignored
					errorMsg.Append("Subscribe Info: ");
					break;
			}

			errorMsg.Append(pubnubError.MessageSource + " : "); // Did this originate from Server or Client-side logic

			if (pubnubError.DetailedDotNetException != null)
			{
				errorMsg.Append(pubnubError.DetailedDotNetException.ToString()); // Full Details of .NET exception
			}

			errorMsg.Append(string.Format("ECode: {0}) Desc: {1}, Channel: {2}, Time: {3}", pubnubError.StatusCode,
			   pubnubError.Description, pubnubError.Channel, pubnubError.ErrorDateTimeGMT));
			if (isAnError)
			{
				Debug.LogError(errorMsg.ToString());
				if (fullDetails)
				{
					if (pubnubError.PubnubWebRequest != null)
					{
						//Captured Web Request details
						PubnubSubscriptionLogger.Log("PubNub RequestURI: " + pubnubError.PubnubWebRequest.RequestUri);
						PubnubSubscriptionLogger.Log("PubNub RequestHeaders: " + pubnubError.PubnubWebRequest.Headers);
					}

					if (pubnubError.PubnubWebResponse != null)
					{
						//Captured Web Response details
						PubnubSubscriptionLogger.Log("PubNub Response: " + pubnubError.PubnubWebResponse.Headers);
					}
				}

				if ((PubnubIsConnected) && (pubnubError.IsDotNetException))
				{
					PubnubSubscriptionLogger.Log("OnSubscribeError: INVOKING EndPendingRequests");
					pubnub.EndPendingRequests();

					if ((pubnubError.StatusCode == (int)PubnubErrorCode.UnsubscribedAfterMaxRetries) ||
						(pubnubError.StatusCode == (int)PubnubErrorCode.PresenceUnsubscribedAfterMaxRetries))
					{
						// A network problem has to caused the subscribed channels to be unsubscribed.
					}
				}
			}
			else
			{
				PubnubSubscriptionLogger.Log(errorMsg.ToString());

				// This wasn't a fatal error so go on to the next operation
				RunNextOperation();
			}
		}

		public void OnUnsubscribeError(PubnubClientError pubnubError)
		{
			bool fullDetails = false;
			bool isAnError = false;

			var errorMsg = new StringBuilder();
			switch (pubnubError.Severity)
			{
				case PubnubErrorSeverity.Critical:
					//This type of error needs to be handled.
					errorMsg.Append("Unsubscribe Critical ERROR: ");
					fullDetails = true;
					isAnError = true;
					break;
				case PubnubErrorSeverity.Warn:
					//This type of error needs to be handled
					errorMsg.Append("Unsubscribe WARN: ");
					break;
				case PubnubErrorSeverity.Info:
					//This type of error can be ignored
					errorMsg.Append("Unsubscribe Info: ");
					break;
			}

			errorMsg.Append(pubnubError.MessageSource + " : "); // Did this originate from Server or Client-side logic

			if (pubnubError.DetailedDotNetException != null)
			{
				errorMsg.Append(pubnubError.DetailedDotNetException.ToString()); // Full Details of .NET exception
			}

			errorMsg.Append(string.Format("ECode: {0}) Desc: {1}, Channel: {2}, Time: {3}", pubnubError.StatusCode,
			   pubnubError.Description, pubnubError.Channel, pubnubError.ErrorDateTimeGMT));
			if (isAnError)
			{
				Debug.LogError(errorMsg.ToString());
				if (fullDetails)
				{
					if (pubnubError.PubnubWebRequest != null)
					{
						//Captured Web Request details
						PubnubSubscriptionLogger.Log("PubNub RequestURI: " + pubnubError.PubnubWebRequest.RequestUri);
						PubnubSubscriptionLogger.Log("PubNub RequestHeaders: " + pubnubError.PubnubWebRequest.Headers);
					}

					if (pubnubError.PubnubWebResponse != null)
					{
						//Captured Web Response details
						PubnubSubscriptionLogger.Log("PubNub Response: " + pubnubError.PubnubWebResponse.Headers);
					}
				}

				if (PubnubIsConnected)
				{
					PubnubSubscriptionLogger.Log("OnUnsubscribeError: INVOKING EndPendingRequests");
					pubnub.EndPendingRequests();
				}
			}
			else
			{
				PubnubSubscriptionLogger.Log(errorMsg.ToString());
			}
		}

		public Promise OnDispose()
		{
			UnsubscribeAll();
			pubnub?.Dispose();
			Destroy(this);
			return Promise.Success;
		}
	}
}
