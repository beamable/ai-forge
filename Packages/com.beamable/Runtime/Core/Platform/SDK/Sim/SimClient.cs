using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beamable.Experimental.Api.Sim
{
	// **Top level driver for a deterministic simulation**

	/// <summary>
	/// This type defines the %Client main entry point for the %Multiplayer feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/multiplayer-feature">Multiplayer</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class SimClient
	{
#pragma warning disable CS0067
		/// <inheritdoc cref="SimNetworkInterface.OnErrorStarted"/>
		public event Action<SimFaultResult> OnErrorStarted;

		/// <inheritdoc cref="SimNetworkInterface.OnErrorRecovered"/>
		public event Action<SimErrorReport> OnErrorRecovered;

		/// <inheritdoc cref="SimNetworkInterface.OnErrorFailed"/>
		public event Action<SimFaultResult> OnErrorFailed;
#pragma warning restore CS0067

		public int LogHash { get; private set; }
		public int StateHash { get; private set; }
		public long Ping { get; private set; }
		// The replicated authoritative log of all events and the current most recent frame
		private SimLog log = new SimLog();
		// Handler for all network activity
		public SimNetworkInterface Network;
		// The last frame that was sent to the Playback. This is always equal to or less than the log's latest frame
		private long _playbackFramePointer = 0;
		private long _virtualFramePointer = 0;
		private long _maxFrame = 0;
		// Last time the local simulation advanced
		private long _lastLocalSimulationTimestamp = 0;
		private long _lastVirtualSimulationTimestamp = 0;
		private long _lastNetworkTimestamp = 0;
		// How many ms before each time we try to advance the simulation locally
		private long _frameTickMs;
		// Number of frames desired for the network to be ahead of the live simulation
		private long _targetNetworkLead;
		private bool _first = true;
		// Seeded random from the initial event injected by the server
		private System.Random _random;


		// Sim behavior objects that need to be spawned
		private List<SimBehavior> _sbSpawns = new List<SimBehavior>();
		private List<SimBehavior> _sbSpawnsBuffer = new List<SimBehavior>();
		// Sim behavior objects that need to be removed
		private HashSet<SimBehavior> _sbRemoves = new HashSet<SimBehavior>();
		private List<SimBehavior> _sbRemovesBuffer = new List<SimBehavior>();
		// Live sim behavior objects that are part of the world
		private List<SimBehavior> _sbLive = new List<SimBehavior>();
		// Callback subscription management for simulation events
		public delegate void EventCallback<T>(T body);
		private Dictionary<string, List<EventCallback<string>>> _eventCallbacks = new Dictionary<string, List<EventCallback<string>>>();
		private List<EventCallback<string>> _curCallbacks = new List<EventCallback<string>>();

		public string ClientId
		{
			get { return Network.ClientId; }
		}

		/// <summary>
		/// Create a new relay client that can be used to communicate with other players.
		/// </summary>
		/// <param name="network">A <see cref="SimNetworkInterface"/> that controls how the events are passed between players.</param>
		/// <param name="framesPerSecond">A target network frame rate. </param>
		/// <param name="targetNetworkLead">Number of frames desired for the network to be ahead of the live simulation</param>
		public SimClient(SimNetworkInterface network, long framesPerSecond, long targetNetworkLead)
		{
			this.Network = network;
			this._frameTickMs = 1000 / framesPerSecond;
			this._targetNetworkLead = targetNetworkLead;
			_virtualFramePointer = targetNetworkLead;

			if (network != null)
			{
				network.OnErrorFailed += r => OnErrorFailed?.Invoke(r);
				network.OnErrorStarted += r => OnErrorStarted?.Invoke(r);
				network.OnErrorRecovered += r => OnErrorRecovered?.Invoke(r);
			}

			this.OnInit((seed) =>
			{
				_random = new System.Random(seed.GetHashCode());
			});
		}

		/// <summary>
		/// Create a snapshot at the current frame.
		/// </summary>
		/// <returns>A <see cref="SimLog.Snapshot"/> contains all of the <see cref="SimEvent"/> for all frames</returns>
		public SimLog.Snapshot TakeSnapshot()
		{
			return log.ToSnapshot();
		}

		/// <summary>
		/// Given a <see cref="SimLog.Snapshot"/>, restore the internal simulation to that point
		/// </summary>
		/// <param name="snapshot">A <see cref="SimLog.Snapshot"/> generated with the <see cref="TakeSnapshot"/> method</param>
		public void RestoreSnapshot(SimLog.Snapshot snapshot)
		{
			_maxFrame = snapshot.frame;
			log.FromSnapshot(snapshot);
		}

		/// <summary>
		/// Add an event to the log eventually. It won't be 'real' until it's in the log. The network decides when/how that happens
		/// This will run the <see cref="SendEvent(string, object)"/> method by using the <see cref="evt"/>'s class name as the name, and the evt itself as the body
		/// </summary>
		/// <param name="evt">Any json serializable object</param>
		public void SendEvent(object evt)
		{
			SendEvent(evt.GetType().ToString(), evt);
		}

		/// <summary>
		/// Send an event to the relay log. The event won't be validated until it round-trips through the other clients.
		/// Use the <see cref="On{T}"/> event to register a callback for when the event is put onto the log.
		/// </summary>
		/// <param name="name">A name for the event. This can be any string, but should be a consistent channel name. </param>
		/// <param name="evt">Any json serializable object. The object will be sent to JSON and sent to the relay server.</param>
		public void SendEvent(string name, object evt)
		{
			// TODO: Eliminate these allocations
			string raw = JsonUtility.ToJson(evt);
			Network.SendEvent(new SimEvent(Network.ClientId, name, raw));
		}

		/// <summary>
		/// Call this method on the Unity update loop.
		/// It will make sure to sync the relay state with the configured network frames per second.
		/// </summary>
		public void Update()
		{
			if (_maxFrame > 0 && _playbackFramePointer == _maxFrame)
			{
				return;
			}

			if (!Network.Ready)
			{
				return;
			}
			if (_first)
			{
				_first = false;
				this._lastLocalSimulationTimestamp = GetTimeMs();
				this._lastVirtualSimulationTimestamp = GetTimeMs();
			}

			long logLead = log.Frame - _playbackFramePointer;
			long curTime = GetTimeMs();
			long deltaT = curTime - _lastLocalSimulationTimestamp;

			long virtualDeltaT = curTime - _lastVirtualSimulationTimestamp;
			while (virtualDeltaT >= _frameTickMs)
			{
				_virtualFramePointer += 1;
				if (virtualDeltaT >= _frameTickMs)
				{
					virtualDeltaT -= _frameTickMs;
				}
				_lastVirtualSimulationTimestamp = GetTimeMs();
			}

			// What frame rate should we use? If we're too far behind, let's speed it up
			// Shave 10% off for every frame we're behind
			long frameTickMs = _frameTickMs;
			long catchupThreshold = _targetNetworkLead * 2;
			if (logLead > catchupThreshold)
			{
				long severity = logLead - catchupThreshold;
				frameTickMs = (long)(frameTickMs - (frameTickMs * (severity * 0.1f)));
			}
			if (deltaT >= frameTickMs)
			{
				SimUpdate();
				_lastLocalSimulationTimestamp = curTime;
				frameTickMs += _frameTickMs;
			}
			// If we're really far behind, catch up *a lot*
			while (frameTickMs < 0)
			{
				SimUpdate();
				_lastLocalSimulationTimestamp = curTime;
				frameTickMs += _frameTickMs;
			}

			var tickFrames = Network.Tick(_playbackFramePointer, log.Frame, _virtualFramePointer);
			for (int i = 0; i < tickFrames.Count; i++)
			{
				SimFrame frame = tickFrames[i];
				log.ApplyFrame(frame);
				Ping = curTime - _lastNetworkTimestamp;
				_lastNetworkTimestamp = curTime;
			}
		}

		private long GetTimeMs()
		{
			return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		}

		private void SimUpdate()
		{
			// Stop advancing if there's no where to go
			if (_playbackFramePointer >= log.Frame)
			{
				return;
			}

			// Trigger ticks and events for this time point
			TriggerEventCallbacks("tick", "$system", "" + _playbackFramePointer);
			List<SimEvent> events = log.GetEventsAtFrame(_playbackFramePointer);
			for (int i = 0; i < events.Count; i++)
			{
				SimEvent evt = events[i];
				TriggerEventCallbacks(evt.Type, evt.Origin, evt.Body);
			}
			_playbackFramePointer += 1;

			// Flush any queued spawns / destroys / etc.
			FlushSimBehaviors();

			/*if (_playbackFramePointer % 200 == 0) {
			   DumpLog();
			}*/
		}

		/// <summary>
		/// Create a new GameObject from the given <see cref="SimBehavior"/> prefab
		/// </summary>
		/// <param name="original"></param>
		/// <param name="id"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Spawn<T>(SimBehavior original, string id = "")
		{
			SimBehavior result = Object.Instantiate(original) as SimBehavior;
			this._sbSpawns.Add(result);
			result.SimInit(this, id);
			return result.GetComponent<T>();
		}

		/// <summary>
		/// Remove a <see cref="SimBehavior"/>
		/// </summary>
		/// <param name="simObj"></param>
		public void RemoveSimBehavior(SimBehavior simObj)
		{
			this._sbRemoves.Add(simObj);
		}

		/// <summary>
		/// Run a callback anytime a certain relay event is received onto the simulation log.
		/// </summary>
		/// <param name="evt">The type of event</param>
		/// <param name="origin">the origin string for who sent the event</param>
		/// <param name="callback">A callback to run when the event is recieved</param>
		/// <typeparam name="T">The type to deserialize the event json into</typeparam>
		/// <returns>An object that can be given to <see cref="Remove"/> to remove the handler</returns>
		public EventCallback<string> On<T>(string evt, string origin, EventCallback<T> callback)
		{
			return OnInternal(evt, origin, (raw) =>
			{
				callback(JsonUtility.FromJson<T>(raw));
			});
		}
		private EventCallback<string> OnInternal(string evt, string origin, EventCallback<string> callback)
		{
			List<EventCallback<string>> callbacks;
			if (!_eventCallbacks.TryGetValue(evt + ":" + origin, out callbacks))
			{
				callbacks = new List<EventCallback<string>>();
				_eventCallbacks.Add(evt + ":" + origin, callbacks);
			}
			callbacks.Add(callback);
			return callback;
		}

		/// <summary>
		/// Remove a handler registered with the <see cref="On{T}"/> method
		/// </summary>
		/// <param name="callback">The instance returned from the <see cref="On{T}"/> method</param>
		public void Remove(EventCallback<string> callback)
		{
			foreach (KeyValuePair<string, List<EventCallback<string>>> entry in _eventCallbacks)
			{
				entry.Value.RemoveAll((item) => item == callback);
			}
		}

		/// <summary>
		/// Add a callback that will trigger after the relay has been initialized.
		/// </summary>
		/// <param name="callback">A callback where the only argument is the relay room id</param>
		/// <returns>An instance that can be sent to the <see cref="Remove"/> method to remove the handler.</returns>
		public EventCallback<string> OnInit(EventCallback<string> callback) { return OnInternal("init", "$system", callback); }

		/// <summary>
		/// Add a callback that will trigger after each player joins the game relay.
		/// </summary>
		/// <param name="callback">a callback where the only argument is the gamertag of the player that joined.</param>
		/// <returns>An instance that can be sent to the <see cref="Remove"/> method to remove the handler.</returns>
		public EventCallback<string> OnConnect(EventCallback<string> callback) { return OnInternal("connect", "$system", callback); }

		/// <summary>
		/// Add a callback that will trigger after a player disconnects from the game realy.
		/// </summary>
		/// <param name="callback">a callback where the only argument is the gamertag of the player that disconnected.</param>
		/// <returns>An instance that can be sent to the <see cref="Remove"/> method to remove the handler.</returns>
		public EventCallback<string> OnDisconnect(EventCallback<string> callback) { return OnInternal("disconnect", "$system", callback); }

		/// <summary>
		/// Add a callback that will trigger on every network tick
		/// </summary>
		/// <param name="callback">A callback where the only argument is the current tick number of the simulation</param>
		/// <returns>An instance that can be sent to the <see cref="Remove"/> method to remove the handler.</returns>
		public EventCallback<string> OnTick(EventCallback<long> callback)
		{
			return OnInternal("tick", "$system", (raw) =>
			{
				callback(long.Parse(raw));
			});
		}

		private void FlushSimBehaviors()
		{
			// Copy lists since processing may spawn more (which will defer until next tick)
			_sbSpawnsBuffer.Clear();
			_sbSpawnsBuffer.AddRange(this._sbSpawns);
			this._sbSpawns.Clear();

			_sbRemovesBuffer.Clear();
			_sbRemovesBuffer.AddRange(this._sbRemoves);
			this._sbRemoves.Clear();

			for (int i = 0; i < _sbSpawnsBuffer.Count; i++)
			{
				SimBehavior next = _sbSpawnsBuffer[i];
				this._sbLive.Add(next);
				next.SimEnter();
			}

			for (int i = 0; i < _sbRemovesBuffer.Count; i++)
			{
				SimBehavior next = _sbRemovesBuffer[i];
				// Remove from ticks, unregister callback, destroy the game object
				this._sbLive.Remove(next);
				foreach (KeyValuePair<string, List<EventCallback<string>>> entry in _eventCallbacks)
				{
					entry.Value.RemoveAll((item) => next.EventCallbacks.Contains(item));
				}
				next.SimExit();
			}
		}

		private void TriggerEventCallbacks(string evt, string origin, string body)
		{
			if (evt.StartsWith("$"))
			{
				evt = evt.Substring(1);
				origin = "$system";
			}

			_curCallbacks.Clear();
			List<EventCallback<string>> callbacks;
			if (!_eventCallbacks.TryGetValue(evt + ":" + origin, out callbacks))
			{
				return;
			}

			// Invoke callbacks and guard against de-registration during callbacks
			_curCallbacks.AddRange(callbacks);
			for (int i = 0; i < _curCallbacks.Count; i++)
			{
				EventCallback<string> next = _curCallbacks[i];
				if (callbacks.Contains(next))
				{
					next(body);
				}
			}
		}

		private void DumpLog()
		{
			string strLog = log.ToString(_playbackFramePointer);
			Debug.Log(strLog);
			LogHash = strLog.GetHashCode();
			string stateHash = "";
			for (int i = 0; i < _sbLive.Count; i++)
			{
				stateHash = stateHash + _sbLive[i].StateHash();
			}
			StateHash = stateHash.GetHashCode();
			Debug.Log("HASH: " + LogHash + " " + StateHash);
			log.Prune(_playbackFramePointer - 1);
		}

		/// <summary>
		/// Get a deterministically random number.
		/// The seed for the random values is shared among all players in the relay.
		/// If all clients use this method to get random values, then all clients will get the same random values.
		/// </summary>
		/// <returns>A number</returns>
		public int RandomInt()
		{
			return _random.Next();
		}
	}
}
