using System;
using System.Collections.Generic;

namespace Beamable.Experimental.Api.Sim
{
	public class SimNetworkLocal : SimNetworkInterface
	{
#pragma warning disable CS0067
		public event Action<SimFaultResult> OnErrorStarted;
		public event Action<SimErrorReport> OnErrorRecovered;
		public event Action<SimFaultResult> OnErrorFailed;
		public bool IsFaulted { get; }
#pragma warning restore CS0067
		public string ClientId { get; private set; }
		public bool Ready { get; private set; }
		private List<SimEvent> _eventQueue = new List<SimEvent>();
		private List<SimFrame> _syncFrames = new List<SimFrame>();
		private List<SimFrame> _emptyFrames = new List<SimFrame>();
		private SimFrame _nextFrame = new SimFrame(0, new List<SimEvent>());
		private bool _replay;

		public SimNetworkLocal(bool replay = false)
		{
			Ready = true;
			ClientId = System.Guid.NewGuid().ToString();
			_syncFrames.Add(_nextFrame);
			_replay = replay;
		}

		public List<SimFrame> Tick(long curFrame, long maxFrame, long expectedMaxFrame)
		{
			if (!_replay && curFrame == 1)
			{
				_nextFrame.Events.Add(new SimEvent(ClientId, "$connect", ClientId));
				return _syncFrames;
			}

			if (maxFrame >= expectedMaxFrame)
			{
				return _emptyFrames;
			}

			// Populate the latest frame
			_nextFrame.Frame += expectedMaxFrame - maxFrame;
			_nextFrame.Events.Clear();
			if (_nextFrame.Frame == 1)
			{
				_nextFrame.Events.Add(new SimEvent(ClientId, "$connect", ClientId));
			}
			_nextFrame.Events.AddRange(_eventQueue);
			_eventQueue.Clear();
			return _syncFrames;
		}

		public void SendEvent(SimEvent evt)
		{
			// Set all sent events to an unsynced frame
			evt.Frame = _nextFrame.Frame + 1;
			_eventQueue.Add(evt);
		}
	}
}
