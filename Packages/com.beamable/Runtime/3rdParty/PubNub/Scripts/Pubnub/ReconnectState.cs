using Beamable.Common;
using Beamable.Common.Dependencies;
using System;
using System.Collections.Generic;

namespace PubNubMessaging.Core
{

	public class ReconnectState<T>
	{
		public string[] Channels;
		public ResponseType Type;
		public Action<T> Callback;
		public Action<PubnubClientError> ErrorCallback;
		public Action<T> ConnectCallback;
		public object Timetoken;
		public bool Reconnect;

		public ReconnectState()
		{
			Channels = null;
			Callback = null;
			ConnectCallback = null;
			Timetoken = null;
			Reconnect = false;
		}
	}
	#region "States and ResposeTypes"
	public enum ResponseType
	{
		Publish,
		History,
		Time,
		//Subscribe,
		SubscribeV2,
		//Presence,
		PresenceV2,
		HereNow,
		Heartbeat,
		DetailedHistory,
		Leave,
		Unsubscribe,
		PresenceUnsubscribe,
		GrantAccess,
		AuditAccess,
		RevokeAccess,
		PresenceHeartbeat,
		SetUserState,
		GetUserState,
		WhereNow,
		GlobalHereNow,
		PushRegister,
		PushRemove,
		PushGet,
		PushUnregister,
		ChannelGroupAdd,
		ChannelGroupRemove,
		ChannelGroupRemoveAll,
		ChannelGroupGet,
		ChannelGroupGrantAccess,
		ChannelGroupAuditAccess,
		ChannelGroupRevokeAccess
	}

	internal class InternetState<T>
	{
		public Action<bool> Callback;
		public Action<PubnubClientError> ErrorCallback;
		public string[] Channels;

		public InternetState()
		{
			Callback = null;
			ErrorCallback = null;
			Channels = null;
		}
	}

	[BeamContextSystem]
	public class StoredRequestState
	{

		// private static volatile StoredRequestState instance;
		private static readonly object syncRoot = new Object();

		private StoredRequestState()
		{
		}

		[RegisterBeamableDependencies(Constants.SYSTEM_DEPENDENCY_ORDER)]
		public static void RegisterDependencies(IDependencyBuilder builder)
		{
			builder.AddSingleton(() =>
			{
				lock (syncRoot)
				{
					return new StoredRequestState();
				}
			});
		}

		SafeDictionary<int, object> requestStates = new SafeDictionary<int, object>();

		public void SetRequestState(CurrentRequestType key, object requestState)
		{
			int intKey = (int)key;
			object reqState = requestState as object;
			requestStates.AddOrUpdate(intKey, reqState, (oldData, newData) => reqState);
			LoggingMethod.WriteToLog(string.Format("DateTime {0}, SetStoredRequestState {1}",
				DateTime.Now.ToString(), key.ToString()), LoggingMethod.LevelInfo);

		}

		public object GetStoredRequestState(CurrentRequestType aKey)
		{
			int intKey = (int)aKey;
			if (requestStates.ContainsKey(intKey))
			{
				if (requestStates.ContainsKey(intKey))
				{
					LoggingMethod.WriteToLog(string.Format("DateTime {0}, GetStoredRequestState {1}",
						DateTime.Now.ToString(), aKey.ToString()), LoggingMethod.LevelInfo);
					return requestStates[intKey];
				}
				LoggingMethod.WriteToLog(string.Format("DateTime {0}, GetStoredRequestState returning false", DateTime.Now.ToString()), LoggingMethod.LevelInfo);

			}
			else
			{
				LoggingMethod.WriteToLog(string.Format("DateTime {0}, GetStoredRequestState doesnt contain key {1}",
					DateTime.Now.ToString(), aKey.ToString()), LoggingMethod.LevelInfo);
			}
			return null;
		}

	}

	public class RequestState<T>
	{
		public Action<T> SuccessCallback;
		public Action<PubnubClientError> ErrorCallback;
		public PubnubWebRequest Request;
		public PubnubWebResponse Response;
		public ResponseType RespType;
		public List<ChannelEntity> ChannelEntities;
		public bool Timeout;
		public bool Reconnect;
		public long Timetoken;
		public Type TypeParameterType;
		public long ID;
		public string UUID;

		public RequestState()
		{
			SuccessCallback = null;
			Request = null;
			Response = null;
			ChannelEntities = null;
			ID = 0;
		}

		public RequestState(RequestState<T> requestState)
		{
			ErrorCallback = requestState.ErrorCallback;
			ChannelEntities = requestState.ChannelEntities;
			Reconnect = requestState.Reconnect;
			Request = requestState.Request;
			Response = requestState.Response;
			Timeout = requestState.Timeout;
			Timetoken = requestState.Timetoken;
			TypeParameterType = requestState.TypeParameterType;
			SuccessCallback = requestState.SuccessCallback as Action<T>;
			ID = requestState.ID;
			RespType = requestState.RespType;
		}
	}

	#endregion
}

