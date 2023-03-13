using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Spew;
using Beamable.Coroutines;
using NativeWebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Beamable.Connection
{
	public class WebSocketConnection : IBeamableConnection
	{
		public event Action Open;
		public event Action<string> Message;
		public event Action<string> Error;
		public event Action Close;

		private bool _disconnecting;
		private WebSocket _webSocket;
		private readonly CoroutineService _coroutineService;
		private IEnumerator _dispatchMessagesRoutine;
		private Promise _onConnectPromise;

		public WebSocketConnection(CoroutineService coroutineService)
		{
			_coroutineService = coroutineService;
		}

		public Promise Connect(string address, AccessToken token)
		{
			string uri = $"{address}/connect";
			_webSocket = CreateWebSocket(uri, token);
			SetUpEventCallbacks();
			// This is a bit gross but the underlying library doesn't complete the connect task until the connection
			// is closed.
			DoConnect();

#if !UNITY_WEBGL || UNITY_EDITOR
			_dispatchMessagesRoutine = DispatchMessages();
			_coroutineService.StartCoroutine(_dispatchMessagesRoutine);
#endif
			return _onConnectPromise;
		}

		public Promise Disconnect()
		{
			_disconnecting = true;
#if !UNITY_WEBGL || UNITY_EDITOR
			if (_dispatchMessagesRoutine != null)
			{
				_coroutineService.StopCoroutine(_dispatchMessagesRoutine);
			}
#endif
			var p = new Promise();
			_webSocket
				.Close()
				.ContinueWith(result =>
				{
					if (result.IsFaulted)
					{
						p.CompleteError(result.Exception);
					}
					else
					{
						p.CompleteSuccess();
					}
				});
			return p;
		}

		private Promise DoConnect()
		{
			_onConnectPromise = new Promise();
			Task _ = _webSocket.Connect();
			return _onConnectPromise;
		}

		private static WebSocket CreateWebSocket(string address, IAccessToken token)
		{
			var subprotocols = new List<string>();
			var headers = new Dictionary<string, string>
			{
				{"Authorization", $"Bearer {token.Token}"}
			};
			return new WebSocket(address, subprotocols, headers);
		}

		private void SetUpEventCallbacks()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.playModeStateChanged += state =>
			{
				if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
				{
					Disconnect();
				}
			};
#endif
			_webSocket.OnOpen += () =>
			{
				_disconnecting = false;
				PlatformLogger.Log($"<b>[WebSocketConnection]</b> OnOpen received");
				try
				{
					_onConnectPromise.CompleteSuccess();
					Open?.Invoke();
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			};
			_webSocket.OnMessage += message =>
			{
				string messageString = Encoding.UTF8.GetString(message);
				PlatformLogger.Log($"<b>[WebSocketConnection]</b> OnMessage received: {messageString}");
				try
				{
					Message?.Invoke(messageString);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			};
			_webSocket.OnError += error =>
			{
				PlatformLogger.Log($"<b>[WebSocketConnection]</b> OnError received: {error}");
				try
				{
					_onConnectPromise.CompleteError(new WebSocketConnectionException(error));
					Error?.Invoke(error);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			};
			_webSocket.OnClose += async code =>
			{
				PlatformLogger.Log($"<b>[WebSocketConnection]</b> OnClose received: {code}");
				try
				{
					if (_disconnecting)
					{
						Close?.Invoke();
					}
					else
					{
						PlatformLogger.Log($"<b>[WebSocketConnection]</b> Ungraceful close of websocket. Reconnecting!");
						// eh?
						// TODO: Add retry interval
						await DoConnect();
					}

				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			};
		}

#if !UNITY_WEBGL || UNITY_EDITOR
		private IEnumerator DispatchMessages()
		{
			const float dispatchIntervalMs = 50.0f;
			var wait = new WaitForSeconds(dispatchIntervalMs / 1000.0f);
			while (true)
			{
				_webSocket.DispatchMessageQueue();
				yield return wait;
			}
			// ReSharper disable once IteratorNeverReturns
		}
#endif

		public async Promise OnDispose()
		{
			await Disconnect();
		}
	}
}
