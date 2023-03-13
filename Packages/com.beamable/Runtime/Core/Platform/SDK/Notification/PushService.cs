using Beamable.Common;
using Beamable.Common.Api;
using System;

namespace Beamable.Api.Notification
{

	/// <summary>
	/// This type defines the %Client main entry point for the %Push %Notifications feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/notifications-feature">Notifications</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class PushService
	{
		private PlatformRequester _requester;
		public PushService(PlatformRequester requester)
		{
			_requester = requester;
		}

		/// <summary>
		/// Register a Push notification provider with Beamable.
		/// A Push provider will enable Beamable to send messages to the player's third party account and have those messages surface
		/// even if the player isn't in the game client.
		/// </summary>
		/// <param name="provider">A supported <see cref="PushProvider"/></param>
		/// <param name="token">The third party provided token for the given <see cref="provider"/> that allows Beamable to send messages
		/// to the player's third party account.</param>
		/// <returns>
		/// A <see cref="Promise{T}"/> representing the network call.
		/// </returns>
		public Promise<EmptyResponse> Register(PushProvider provider, string token)
		{
			return _requester.Request<EmptyResponse>(Method.POST, "/basic/push/register", new PushRegisterRequest(provider.ToRequestString(), token));
		}
	}

	[Serializable]
	public class PushRegisterRequest
	{
		public string provider;
		public string token;

		public PushRegisterRequest(string provider, string token)
		{
			this.provider = provider;
			this.token = token;
		}
	}
}
