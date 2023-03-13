using System;

namespace Beamable.Experimental.Api.Sim
{
	/// <summary>
	/// This type defines the %SimEvent for the %Multiplayer feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Experimental.Api.Sim.SimClient script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class SimEvent
	{
		/// <summary>
		/// The frame number
		/// </summary>
		public long Frame;
		public string Type;
		public string Origin;
		public string Body;

		public SimEvent(string origin, string type, string body)
		{
			this.Origin = origin;
			this.Type = type;
			this.Body = body;
		}

		public override string ToString()
		{
			return Frame + ": [" + Origin + " " + Type + " " + Body + "]";
		}
	}
}
