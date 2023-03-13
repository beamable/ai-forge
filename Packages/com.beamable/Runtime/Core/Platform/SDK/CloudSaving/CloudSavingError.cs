using System;

namespace Beamable.Api.CloudSaving
{
	/// <summary>
	/// An exception that comes from the <see cref="CloudSavingService"/>
	/// </summary>
	public class CloudSavingError : Exception
	{
		public CloudSavingError(string message, Exception inner) : base(message, inner)
		{
		}
	}
}
