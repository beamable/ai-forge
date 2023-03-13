using Beamable.Serialization;
using System.Collections.Generic;

namespace Beamable.Api.Analytics
{
	/// <summary>
	/// Core event class
	///
	/// This class can be used in two ways:
	///   a) Directly create instances in a structured way, such as with factory methods
	///   b) Subclassing
	/// </summary>
	public class CoreEvent : IAnalyticsEvent
	{
		private const string _opCode = "g.core";
		public string OpCode => _opCode;

		public string eventName;
		public string category;
		protected IDictionary<string, object> eventParams;

		/// <param name="category">Low cardinality descriptor of the event</param>
		/// <param name="eventName">High cardinality descriptor of the event</param>
		/// <param name="eventParams">Other parameters: must be flat with no nesting</param>
		public CoreEvent(string category, string eventName, IDictionary<string, object> eventParams)
		{
			this.category = category;
			this.eventName = eventName;
			this.eventParams = eventParams;
		}

		/// <summary>
		/// Constructs a payload which represents this analytics event
		/// </summary>
		public void Serialize(JsonSerializable.IStreamSerializer s)
		{
			var op = _opCode;
			s.Serialize("op", ref op);
			s.Serialize("e", ref eventName);
			s.Serialize("c", ref category);
			s.Serialize("p", ref eventParams);
		}
	}
}
