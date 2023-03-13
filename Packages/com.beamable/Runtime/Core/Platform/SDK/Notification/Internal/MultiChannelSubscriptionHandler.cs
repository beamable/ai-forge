using Beamable.Common.Spew;
using PubNubMessaging.Core;
using System;

namespace Beamable.Api.Notification.Internal
{
	public class MultiChannelSubscriptionHandler
	{
		Action<object> mConnectCallback = null;
		Action<object> mDisconnectCallback = null;
		Action<PubnubClientError> mErrorCallback = null;

		int mCount = 0;
		int mExpectedCount = 0;

		public bool ignoreError;

		public void Setup(string channelList, Action<object> connectCallback, Action<object> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
			mExpectedCount = 1;
			for (int i = 0; i < channelList.Length; i++)
			{
				if (channelList[i] == ',')
				{
					mExpectedCount++;
				}
			}

			mCount = 0;

			PubnubSubscriptionLogger.LogFormat("Setting subscription count {0}/{1}", mCount, mExpectedCount);
			mConnectCallback = connectCallback;
			mDisconnectCallback = disconnectCallback;
			mErrorCallback = errorCallback;
		}

		public void OnConnectMessage(object data)
		{
			HandleCallback(mConnectCallback, data);
		}

		public void OnDisconnectMessage(object data)
		{
			HandleCallback(mDisconnectCallback, data);
		}

		public void OnError(PubnubClientError error)
		{
			if (!ignoreError)
			{
				HandleCallback(mErrorCallback, error);
			}
			else
			{
				PubnubSubscriptionLogger.Log("Ignoring pubnub error");
			}
		}

		private void HandleCallback<T>(Action<T> callback, T param)
		{
			mCount += 1;
			PubnubSubscriptionLogger.LogFormat("Subscription Count goes to {0}/{1}", mCount, mExpectedCount);
			if (callback != null)
			{
				callback(param);
			}
		}

		public bool CheckOperationCount()
		{
			return mExpectedCount != 0 && mCount >= mExpectedCount;
		}

		public bool HasOperations
		{
			get { return mExpectedCount > 0; }
		}

		public void Reset()
		{
			mExpectedCount = 0;
			mCount = 0;
			// Dont want to keep things in memory by accident
			mConnectCallback = null;
			mDisconnectCallback = null;
			mErrorCallback = null;
		}

	}
}
