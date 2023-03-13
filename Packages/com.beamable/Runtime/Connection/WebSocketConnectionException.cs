using System;

namespace Beamable.Connection
{
	public class WebSocketConnectionException : Exception
	{
		public WebSocketConnectionException(string message) : base(message) { }
	}
}
