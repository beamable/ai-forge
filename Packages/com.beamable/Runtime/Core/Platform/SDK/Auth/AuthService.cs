using Beamable.Api.Sessions;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Auth;
using System;
using UnityEngine;

namespace Beamable.Api.Auth
{
	/// <summary>
	/// This type defines the %Client main entry point for the %Auth feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/accounts-feature">Accounts</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public interface IAuthService : IAuthApi
	{
		/// <summary>
		/// <inheritdoc cref="IAuthApi.SetLanguage(string)"/>
		/// </summary>
		/// <param name="language">Any <see cref="SystemLanguage"/></param>
		/// <returns>A <see cref="Promise{User}"/> containing the updated user.</returns>
		Promise<User> SetLanguage(SystemLanguage language);

		/// <summary>
		/// Check if the current device id is available to be associated with a <see cref="User"/>.
		/// Each device id is only assignable to one <see cref="User"/>, so if one player registers the device id,
		/// that id cannot be registered by any other players.
		/// Device IDs are resolved by the <see cref="IDeviceIdResolver"/> interface.
		/// </summary>
		/// <returns>A promise that will result in true if the device id is available, false otherwise.</returns>
		Promise<bool> IsThisDeviceIdAvailable();

		/// <summary>
		/// A <see cref="User"/> can associate an device id credential to their account.
		/// Once the player registers the credential, they can use the device id to retrieve
		/// a <see cref="TokenResponse"/> for the account by using the <see cref="LoginDeviceId"/> method.
		/// This method will associate the device id with the <i>current</i> <see cref="User"/>.
		/// The device id that will be registered is resolved by the <see cref="IDeviceIdResolver"/> interface.
		/// The device id can be removed with the <see cref="RemoveDeviceId"/>, <see cref="RemoveAllDeviceIds"/> or <see cref="RemoveDeviceIds"/> method
		/// </summary>
		/// <returns>
		/// A <see cref="Promise{User}"/> that will have the updated <see cref="User"/> data for the current user.
		/// The resulting <see cref="User"/> object will have the <see cref="User.deviceIds"/> field filled out.
		/// </returns>
		Promise<User> RegisterDeviceId();

		/// <summary>
		/// You may want to remove an associated device id from a <see cref="User"/>.
		/// This method will remove the device that was resolved with the <see cref="IDeviceIdResolver"/> interface.
		/// </summary>
		/// <returns>
		/// A <see cref="Promise{User}"/> that will have the updated <see cref="User"/> data for the current user.
		/// The resulting <see cref="User"/> object will have the <see cref="User.deviceIds"/> field filled out.
		/// </returns>
		Promise<User> RemoveDeviceId();

		/// <summary>
		/// It is possible for a <see cref="User"/> to have multiple device IDs, if they have used multiple devices.
		/// This method will remove some particular subset of the device IDs. You can check which device IDs exist
		/// on a user by checking the <see cref="User.deviceIds"/> field.
		/// </summary>
		/// <param name="deviceIds">the set of device IDs that you want to remove from the <see cref="User"/></param>
		/// <returns>
		/// A <see cref="Promise{User}"/> that will have the updated <see cref="User"/> data for the current user.
		/// The resulting <see cref="User"/> object will have the <see cref="User.deviceIds"/> field filled out.
		/// </returns>
		Promise<User> RemoveDeviceIds(string[] deviceIds);

		/// <summary>
		/// It is possible for a <see cref="User"/> to have multiple device IDs, if they have used multiple devices.
		/// This method will remove <i>all</i> device IDs.
		/// </summary>
		/// <returns>
		/// A <see cref="Promise{User}"/> that will have the updated <see cref="User"/> data for the current user.
		/// The resulting <see cref="User"/> object will have the <see cref="User.deviceIds"/> field filled out.
		/// </returns>
		Promise<User> RemoveAllDeviceIds();

		/// <summary>
		/// Use the current device id to retrieve a <see cref="TokenResponse"/>. The resulting token response can
		/// be used to change the current <see cref="User"/>
		/// The current device id will be resolved by the <see cref="IDeviceIdResolver"/> interface.
		/// A login will only work after the device id has been registered by using the <see cref="RegisterDeviceId"/> method.
		/// </summary>
		/// <param name="mergeGamerTagToAccount">
		/// Since this function can only be called from a <see cref="IAuthApi"/> that already exists,
		/// there must already be some associated <see cref="User"/> account. If the <see cref="mergeGamerTagToAccount"/> is enabled,
		/// then the current player will be merged with the player associated with the given device id.
		/// </param>
		/// <returns>A <see cref="Promise{TokenResponse}"/> that results in the <see cref="TokenResponse"/> for the requested <see cref="User"/>'s device id</returns>
		Promise<string> GetDeviceId();

		/// <summary>
		/// Use the device id to retrieve a <see cref="TokenResponse"/>. The resulting token response can
		/// be used to change the current <see cref="User"/>
		/// A login will only work after the device id has have registered by using the <see cref="RegisterDeviceId"/> method.
		/// </summary>
		/// <param name="mergeGamerTagToAccount">
		/// Since this function can only be called from a <see cref="IAuthApi"/> that already exists,
		/// there must already be some associated <see cref="User"/> account. If the <see cref="mergeGamerTagToAccount"/> is enabled,
		/// then the current player will be merged with the player associated with the given device id
		/// </param>
		/// <returns>A <see cref="Promise{TokenResponse}"/> that results in the <see cref="TokenResponse"/> for the requested <see cref="User"/>'s device id</returns>
		Promise<TokenResponse> LoginDeviceId(bool mergeGamerTagToAccount = true);
	}

	/// <summary>
	/// This type defines the %Client main entry point for the %Auth feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/accounts-feature">Accounts</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class AuthService : AuthApi, IAuthService
	{
		private readonly IDeviceIdResolver _deviceIdResolver;
		const string DEVICE_ID_URI = ACCOUNT_URL + "/me";
		const string DEVICE_DELETE_URI = ACCOUNT_URL + "/me/device";

		public AuthService(IBeamableRequester requester, IDeviceIdResolver deviceIdResolver = null, IAuthSettings settings = null) : base(requester, settings)
		{
			_deviceIdResolver = deviceIdResolver ?? new DefaultDeviceIdResolver();
		}

		public async Promise<bool> IsThisDeviceIdAvailable()
		{
			var deviceId = await _deviceIdResolver.GetDeviceId();
			var encodedDeviceId = Requester.EscapeURL(deviceId);
			return await Requester.Request<AvailabilityResponse>(Method.GET,
					$"{ACCOUNT_URL}/available/device-id?deviceId={encodedDeviceId}", null, false)
				.Map(resp => resp.available);
		}

		public Promise<User> SetLanguage(SystemLanguage language) =>
			SetLanguage(SessionServiceHelper.GetISO639CountryCodeFromSystemLanguage(language));

		public async Promise<TokenResponse> LoginDeviceId(bool mergeGamerTagToAccount = true)
		{
			var deviceId = await _deviceIdResolver.GetDeviceId();

			var req = new LoginDeviceIdRequest
			{
				grant_type = "device",
				device_id = deviceId
			};
			return await Requester.Request<TokenResponse>(Method.POST, TOKEN_URL, req, includeAuthHeader: mergeGamerTagToAccount);
		}

		public class LoginDeviceIdRequest
		{
			public string grant_type;
			public string device_id;
		}

		public async Promise<User> RegisterDeviceId()
		{
			var deviceId = await _deviceIdResolver.GetDeviceId();

			return await UpdateDeviceId(new RegisterDeviceIdRequest
			{
				deviceId = deviceId
			});
		}

		public async Promise<string> GetDeviceId()
		{
			return await _deviceIdResolver.GetDeviceId();
		}

		private Promise<User> UpdateDeviceId(RegisterDeviceIdRequest requestBody)
		{
			return Requester.Request<User>(Method.PUT, DEVICE_ID_URI, requestBody);
		}

		public async Promise<User> RemoveDeviceId()
		{
			var deviceId = await _deviceIdResolver.GetDeviceId();

			var ids = new string[] { deviceId };
			return await RemoveDeviceIds(ids);
		}

		public Promise<User> RemoveAllDeviceIds()
		{
			return RemoveDeviceIds(null);
		}

		public Promise<User> RemoveDeviceIds(string[] deviceIds)
		{
			object body = new EmptyResponse();
			if (deviceIds != null)
			{
				body = DeleteDevicesRequest.Create(deviceIds);
			}
			return Requester.Request<User>(Method.DELETE, DEVICE_DELETE_URI, body);
		}

		[Serializable]
		private class RegisterDeviceIdRequest
		{
			public string deviceId;
		}

		[Serializable]
		private class DeleteDevicesRequest
		{
			public string[] deviceIds;

			public static DeleteDevicesRequest Create(string[] ids)
			{
				var req = new DeleteDevicesRequest
				{
					deviceIds = ids
				};

				return req;
			}
		}
	}
}
