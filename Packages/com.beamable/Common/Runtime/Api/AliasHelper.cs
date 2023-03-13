using System;

namespace Beamable.Common.Api
{
	public static class AliasHelper
	{
		/// <summary>
		/// Check if the given cid string is in the valid cid format.
		/// A valid cid must be numeric, and cannot be empty.
		/// </summary>
		/// <param name="cid">Some cid string</param>
		/// <returns>true if the string can be a valid cid; false otherwise.</returns>
		public static bool IsCid(string cid)
		{
			// a cid must start with a number.
			if (string.IsNullOrEmpty(cid)) return false;
			return char.IsDigit(cid[0]);
		}

		/// <summary>
		/// Guarantees the given alias is in the valid alias format.
		/// A valid alias must not be a cid.
		/// This method will allow an empty string to be valid.
		/// </summary>
		/// <param name="alias">Some alias string</param>
		/// <exception cref="ArgumentException">Throws an exception if the given alias is actually a cid.</exception>
		public static void ValidateAlias(string alias)
		{
			if (string.IsNullOrWhiteSpace(alias)) return;
			if (IsCid(alias))
			{
				throw new ArgumentException(nameof(alias) + " is a cid");
			}
			if (alias.Contains(" "))
			{
				throw new ArgumentException(nameof(alias) + " cannot contain whitespaces");
			}
		}

		/// <summary>
		/// Guarantees the given cid is in the valid cid format.
		/// A valid cid must be numeric.
		/// This method will allow an empty string to be valid.
		/// </summary>
		/// <param name="cid">Some cid string</param>
		/// <exception cref="ArgumentException">Throws an exception if the given cid is not a cid.</exception>
		public static void ValidateCid(string cid)
		{
			if (string.IsNullOrWhiteSpace(cid)) return;
			if (!IsCid(cid)) throw new ArgumentException(nameof(cid) + " is not a cid");
		}
	}
}
