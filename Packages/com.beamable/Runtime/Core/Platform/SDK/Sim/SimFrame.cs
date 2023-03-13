using System;
using System.Collections.Generic;

namespace Beamable.Experimental.Api.Sim
{

	/// <summary>
	/// This type defines the %SimFrame for the %Multiplayer feature.
	/// 
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	/// 
	/// #### Related Links
	/// - See Beamable.Experimental.Api.Sim.SimClient script reference
	/// 
	/// ![img beamable-logo]
	/// 
	/// </summary>
	public class SimFrame
	{
		[Serializable]
		public class FramePacket
		{
			public long Frame { get; set; }
			public List<SimEvent> Events { get; private set; }

			public FramePacket(long frame)
			{
				this.Frame = frame;
				this.Events = new List<SimEvent>();
			}

			public void AddEvent(SimEvent evt)
			{
				evt.Frame = Frame;
				Events.Add(evt);
			}
		}

		public long Frame { get; set; }
		public List<SimEvent> Events { get; private set; }

		public SimFrame(long frame, List<SimEvent> events)
		{
			this.Frame = frame;
			this.Events = events;
		}

		public void Sort()
		{
			Events.Sort((x, y) =>
			{
				var left = x.Frame + x.Origin + x.Type + x.Body;
				var right = y.Frame + y.Origin + y.Type + y.Body;
				return left.CompareTo(right);
			});
		}

		public bool Apply(FramePacket packet)
		{
			Events.AddRange(packet.Events);

			if (packet.Frame > Frame)
			{
				Frame = packet.Frame;
			}

			// Always re-stamp all events
			foreach (SimEvent evt in Events)
			{
				evt.Frame = Frame;
			}

			return true;
		}
	}
}
