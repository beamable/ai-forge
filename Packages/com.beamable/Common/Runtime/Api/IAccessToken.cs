using System;

namespace Beamable.Common.Api
{
	public interface IAccessToken
	{
		/// <summary>
		/// The token is a temporary access token that authenticates a player.
		/// This string should not be stored on its own, because eventually it will expire and cease to function.
		/// When the token expires, a new token will be issued by using the <see cref="RefreshToken"/>
		/// </summary>
		string Token { get; }

		/// <summary>
		/// When the <see cref="Token"/> expires, the refresh token can be used to issue a new token string
		/// by using the <see cref="Auth.IAuthApi.LoginRefreshToken"/> method
		/// </summary>
		string RefreshToken { get; }

		/// <summary>
		/// Indicates when the <see cref="Token"/> will expire.
		/// </summary>
		DateTime ExpiresAt { get; }

		/// <summary>
		/// The customer organization that this token is valid for.
		/// </summary>
		string Cid { get; }

		/// <summary>
		/// The realm id that this token is valid for.
		/// If this token is a customer scoped token, then the value of the <see cref="Pid"/> will be null
		/// </summary>
		string Pid { get; }
	}
}
