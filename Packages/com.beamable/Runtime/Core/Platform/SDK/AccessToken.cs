using Beamable.Common;
using Beamable.Common.Api;
using System;
using System.Globalization;

namespace Beamable.Api
{
	/// <summary>
	/// This type defines the %Client main entry point for the %AccessToken feature.
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
	public class AccessToken : IAccessToken
	{
		private AccessTokenStorage _storage;
		public string Token { get; private set; }
		public string RefreshToken { get; }
		public DateTime ExpiresAt { get; set; }
		public string Cid { get; }
		public string Pid { get; }

		private bool _neverExpires;

		/// <summary>
		/// Check if the <see cref="Token"/> is expired, or will expire within the play session.
		/// </summary>
		public bool IsExpired =>
			//Consider the token expired if we're within 1 Day of true expiration
			//This is to avoid the token expiring during a play session
			!_neverExpires && DateTime.UtcNow.AddDays(1) > ExpiresAt;

		public AccessToken(AccessTokenStorage storage, string cid, string pid, string token, string refreshToken, long expiresAt)
		{
			_storage = storage;
			AliasHelper.ValidateCid(cid);
			Cid = cid;
			Pid = pid;
			Token = token;
			RefreshToken = refreshToken;
			if (expiresAt >= long.MaxValue - 1)
			{
				_neverExpires = true;
				ExpiresAt = DateTime.MaxValue;
			}
			else
			{
				ExpiresAt = DateTime.UtcNow.AddMilliseconds(expiresAt);
			}
		}

		public AccessToken(AccessTokenStorage storage, string cid, string pid, string token, string refreshToken, string expiresAtISO)
		{
			AliasHelper.ValidateCid(cid);
			_storage = storage;
			Cid = cid;
			Pid = pid;
			Token = token;
			RefreshToken = refreshToken;
			if (long.TryParse(expiresAtISO, out var fileTimeUtc))
			{
				ExpiresAt = DateTime.FromFileTimeUtc(fileTimeUtc);
			}
			else
			{
				ExpiresAt = DateTime.Parse(expiresAtISO, CultureInfo.InvariantCulture);
			}
		}

		/// <summary>
		/// Saving an <see cref="AccessToken"/> commits the full token structure to PlayerPrefs.
		/// Only one token can be saved per player code / cid / pid combo.
		/// </summary>
		/// <returns>A promise indicating when the write operation will complete.</returns>
		public Promise<Unit> Save()
		{
			return _storage.SaveTokenForRealm(
			   Cid,
			   Pid,
			   this
			);
		}

		/// <summary>
		/// Saving an <see cref="AccessToken"/> as a customer scoped token will commit the full token structure
		/// to PlayerPrefs, but do so without scoping it with the token's <see cref="AccessToken.Pid"/>.
		/// Only one token can be saved per player code / cid combo.
		/// </summary>
		/// <returns>A promise indicating when the write operation will complete.</returns>
		public Promise<Unit> SaveAsCustomerScoped()
		{
			return _storage.SaveTokenForCustomer(Cid, this);
		}

		/// <summary>
		/// Deleting an <see cref="AccessToken"/> will remove the full token structure from PlayerPrefs.
		/// </summary>
		/// <returns>A promise indicating when the delete operation will complete.</returns>
		public Promise<Unit> Delete()
		{
			return _storage.DeleteTokenForRealm(Cid, Pid);
		}

		/// <summary>
		/// Deleting an <see cref="AccessToken"/> will remove the full token structure from PlayerPrefs.
		/// </summary>
		/// <returns>A promise indicating when the delete operation will complete.</returns>
		public Promise<Unit> DeleteAsCustomerScoped()
		{
			return _storage.DeleteTokenForCustomer(Cid);
		}

		/// <summary>
		/// <b> DANGEROUS </b>
		/// This method can be used to simulate what happens if the internal <see cref="AccessToken.Token"/> is corrupted.
		/// You may want to do this during testing to check if the game can recover from a corrupted token.
		/// </summary>
		public void CorruptAccessToken()
		{
			// Set as a garbage (but plausible) token
			Token = "ffffffff-ffff-ffff-ffff-ffffffffffff";
			Save();
		}

		/// <summary>
		/// <b> DANGEROUS </b>
		/// This method can be used to simulate what happens if the internal <see cref="AccessToken.Token"/> has expired.
		/// You may want to do this during testing to check if the game can recover from an expired token.
		/// </summary>
		public void ExpireAccessToken()
		{
			ExpiresAt = DateTime.UtcNow.AddDays(-2);
			Save();
		}
	}
}
