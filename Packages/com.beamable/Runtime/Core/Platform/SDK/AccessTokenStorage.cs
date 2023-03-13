using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Auth;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Api
{
	public class AccessTokenStorage
	{
		private string _prefix;

		private const char DeviceTokenDelimiter = ',';
		private const char DeviceTokenSeparator = '|';
		private const string DeviceTokenDelimiterStr = ",";

		private string GetDeviceTokenKey(string cid, string pid)
		{
			AliasHelper.ValidateCid(cid);
			return $"{_prefix}device-tokens{cid}{pid ?? ""}";
		}

		public AccessTokenStorage(string prefix = "")
		{
			_prefix = prefix;
		}

		public Promise<AccessToken> LoadTokenForCustomer(string cid)
		{
			AliasHelper.ValidateCid(cid);
			string accessToken = PlayerPrefs.GetString($"{_prefix}{cid}.access_token");
			string refreshToken = PlayerPrefs.GetString($"{_prefix}{cid}.refresh_token");
			string expires = PlayerPrefs.GetString($"{_prefix}{cid}.expires");

			if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(expires))
				return Promise<AccessToken>.Successful(null);

			return Promise<AccessToken>.Successful(new AccessToken(this, cid, null, accessToken, refreshToken, expires));
		}

		public AccessToken LoadTokenForRealmImmediate(string cid, string pid)
		{
			AliasHelper.ValidateCid(cid);
			string accessToken = PlayerPrefs.GetString($"{_prefix}{cid}.{pid}.access_token");
			string refreshToken = PlayerPrefs.GetString($"{_prefix}{cid}.{pid}.refresh_token");
			string expires = PlayerPrefs.GetString($"{_prefix}{cid}.{pid}.expires");
			if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(expires))
				return null;
			return new AccessToken(this, cid, pid, accessToken, refreshToken, expires);
		}

		public Promise<AccessToken> LoadTokenForRealm(string cid, string pid)
		{
			AliasHelper.ValidateCid(cid);
			return Promise<AccessToken>.Successful(LoadTokenForRealmImmediate(cid, pid));
		}

		public Promise<Unit> SaveTokenForCustomer(string cid, AccessToken token)
		{
			AliasHelper.ValidateCid(cid);
			PlayerPrefs.SetString($"{_prefix}{cid}.access_token", token.Token);
			PlayerPrefs.SetString($"{_prefix}{cid}.refresh_token", token.RefreshToken);
			try
			{
				PlayerPrefs.SetString(
					$"{_prefix}{cid}.expires",
					token.ExpiresAt.ToFileTimeUtc().ToString()
				);
			}
			catch (ArgumentOutOfRangeException)
			{
				Debug.LogWarning($"Wasn't able to set the expiration time of the token in playerprefs. ExpiresAt=[{token.ExpiresAt}]. " +
								 "This is a non-fatal error, because if the token is expired, it will be re-issued after the first auth failure, " +
								 "and the original request will be reattempted.");
			}

			StoreDeviceRefreshToken(cid, null, token);
			PlayerPrefs.Save();
			return Promise<Unit>.Successful(PromiseBase.Unit);
		}

		public Promise<Unit> SaveTokenForRealm(string cid, string pid, AccessToken token)
		{
			AliasHelper.ValidateCid(cid);
			PlayerPrefs.SetString($"{_prefix}{cid}.{pid}.access_token", token.Token);
			PlayerPrefs.SetString($"{_prefix}{cid}.{pid}.refresh_token", token.RefreshToken);
			PlayerPrefs.SetString(
			   $"{_prefix}{cid}.{pid}.expires",
			   token.ExpiresAt.ToFileTimeUtc().ToString()
			);
			StoreDeviceRefreshToken(cid, pid, token);
			PlayerPrefs.Save();
			return Promise<Unit>.Successful(PromiseBase.Unit);
		}

		public Promise<Unit> DeleteTokenForCustomer(string cid)
		{
			AliasHelper.ValidateCid(cid);
			PlayerPrefs.DeleteKey($"{_prefix}{cid}.access_token");
			PlayerPrefs.DeleteKey($"{_prefix}{cid}.refresh_token");
			PlayerPrefs.DeleteKey($"{_prefix}{cid}.expires");
			PlayerPrefs.Save();
			return Promise<Unit>.Successful(PromiseBase.Unit);
		}

		public Promise<Unit> DeleteTokenForRealm(string cid, string pid)
		{
			AliasHelper.ValidateCid(cid);
			PlayerPrefs.DeleteKey($"{_prefix}{cid}.{pid}.access_token");
			PlayerPrefs.DeleteKey($"{_prefix}{cid}.{pid}.refresh_token");
			PlayerPrefs.DeleteKey($"{_prefix}{cid}.{pid}.expires");
			PlayerPrefs.Save();
			return Promise<Unit>.Successful(PromiseBase.Unit);
		}

		public void StoreDeviceRefreshToken(string cid, string pid, IAccessToken token)
		{
			AliasHelper.ValidateCid(cid);
			string key = GetDeviceTokenKey(cid, pid);
			var compressedTokens = PlayerPrefs.GetString(key, "");
			PlayerPrefs.SetString(key, NextCompressedTokens(compressedTokens, token));
		}

		private string NextCompressedTokens(string compressedTokens, IAccessToken token)
		{
			// this should overwrite any existing account that shares the same refresh token, so that the latest access token is kept up to date.
			var codedToken = Convert(token);
			if (string.IsNullOrEmpty(compressedTokens))
			{
				return codedToken;
			}

			var set = compressedTokens.Split(Constants.DelimiterSplit, StringSplitOptions.RemoveEmptyEntries);
			if (set.Length == 0)
			{
				return codedToken;
			}

			for (int i = set.Length - 1; i >= 0; --i)
			{
				if (MatchesRefreshToken(set[i], token.RefreshToken))
				{
					set[i] = codedToken;
					var nextCompressedTokens = string.Join(DeviceTokenDelimiterStr, set);
					return nextCompressedTokens;
				}
			}

			return $"{compressedTokens}{DeviceTokenDelimiter}{codedToken}";
		}

		public void RemoveDeviceRefreshToken(string cid, string pid, TokenResponse token)
		{
			AliasHelper.ValidateCid(cid);
			string key = GetDeviceTokenKey(cid, pid);
			var compressedTokens = PlayerPrefs.GetString(key, "");
			var set = compressedTokens.Split(Constants.DelimiterSplit, StringSplitOptions.RemoveEmptyEntries);
			set = Array.FindAll(set, curr => !MatchesRefreshToken(curr, token.refresh_token));
			var nextCompressedTokens = string.Join(DeviceTokenDelimiterStr, set);

			PlayerPrefs.SetString(key, nextCompressedTokens);
		}

		public void ClearDeviceRefreshTokens(string cid, string pid)
		{
			AliasHelper.ValidateCid(cid);
			PlayerPrefs.DeleteKey(GetDeviceTokenKey(cid, pid));
		}

		public TokenResponse[] RetrieveDeviceRefreshTokens(string cid, string pid)
		{
			AliasHelper.ValidateCid(cid);
			var compressedTokens = PlayerPrefs.GetString(GetDeviceTokenKey(cid, pid), "");
			var refreshTokens = compressedTokens.Split(Constants.DelimiterSplit, StringSplitOptions.RemoveEmptyEntries);
			var converted = Array.ConvertAll(refreshTokens, Convert);

			// return converted;
			var validTokens = new List<TokenResponse>();
			foreach (var convert in converted)
			{
				var isOfflineToken = convert.access_token == Common.Constants.Commons.OFFLINE;
				if (!isOfflineToken)
				{
					validTokens.Add(convert);
				}
			}

			return validTokens.ToArray();
		}

		private string Convert(IAccessToken token)
		{
			return $"{token.Token}{DeviceTokenSeparator}{token.RefreshToken}";
		}

		private static bool MatchesRefreshToken(string encoded, string refreshToken)
		{
			return encoded.EndsWith(refreshToken, StringComparison.Ordinal) &&
				   encoded.Length > refreshToken.Length &&
				   encoded[encoded.Length - refreshToken.Length - 1] == DeviceTokenSeparator;
		}

		private TokenResponse Convert(string token)
		{
			var parts = token.Split(Constants.SeparatorSplit, StringSplitOptions.None);
			return new TokenResponse
			{
				access_token = parts[0],
				refresh_token = parts.Length == 2 ? parts[1] : ""
			};
		}

		private static class Constants
		{
			public static readonly char[] SeparatorSplit = new[] { DeviceTokenSeparator };
			public static readonly char[] DelimiterSplit = new[] { DeviceTokenDelimiter };
		}

	}
}
