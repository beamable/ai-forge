using Beamable.Common;
using UnityEngine;

namespace Beamable.Api.Auth
{
	public interface IDeviceIdResolver
	{
		/// <summary>
		/// Retrieve an id that is unique to the hardware device executing the game.
		/// In most cases, this will be the <see cref="SystemInfo.deviceUniqueIdentifier"/>
		/// </summary>
		/// <returns>A unique device id</returns>
		Promise<string> GetDeviceId();
	}

	public class DefaultDeviceIdResolver : IDeviceIdResolver
	{
		public Promise<string> GetDeviceId()
		{
			return Promise<string>.Successful(SystemInfo.deviceUniqueIdentifier);
		}
	}

}
