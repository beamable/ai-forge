using System;
using System.Text;
using UnityEngine;

namespace Beamable.Common
{
	[Serializable]
	public class PackageVersion
	{
		private const string PREVIEW_STRING = "PREVIEW";
		private const string PREVIEW_PREFIX_STRING = "PRE";
		private const string EXPERIMENTAL_STRING = "EXPERIMENTAL";
		private const string EXP_PREFIX_STRING = "EXP";
		private const string RC_STRING = "RC";
		private const string NIGHTLY_STRING = "NIGHTLY";
		private const int UNASSIGNED_VALUE = -1;

		private const char VERSION_SEPARATOR = '.';
		private const char PREVIEW_SEPARATOR = '-';

		[SerializeField] private int _major = UNASSIGNED_VALUE;
		[SerializeField] private int _minor = UNASSIGNED_VALUE;
		[SerializeField] private int _patch = UNASSIGNED_VALUE;
		[SerializeField] private int _rc = UNASSIGNED_VALUE;
		[SerializeField] private long _nightlyTime = UNASSIGNED_VALUE;
		[SerializeField] private bool _isPreview, _isExperimental;

		/// <summary>
		/// True if this package version is representing a release candidate version.
		/// </summary>
		public bool IsReleaseCandidate => _rc > UNASSIGNED_VALUE;

		/// <summary>
		/// True if this package version is representing a nightly version
		/// </summary>
		public bool IsNightly => _nightlyTime > UNASSIGNED_VALUE;

		/// <summary>
		/// True if this package version is representing a preview build.
		/// https://docs.unity3d.com/Manual/upm-lifecycle.html
		/// </summary>
		public bool IsPreview => _isPreview;

		/// <summary>
		/// True if this package version represents an experimental build.
		/// https://docs.unity3d.com/Manual/upm-lifecycle.html
		/// </summary>
		public bool IsExperimental => _isExperimental;

		/// <summary>
		/// The major version is the first number in the semantic version string.
		/// In the example, "1.2.3", the major version is 1.
		/// </summary>
		public int Major => _major;

		/// <summary>
		/// The minor version is the second number in the semantic version string.
		/// In the example, "1.2.3", the minor version is 2.
		/// </summary>
		public int Minor => _minor;

		/// <summary>
		/// The patch version is the last number in the semantic version string.
		/// In the example, "1.2.3", the patch version is 3.
		/// </summary>
		public int Patch => _patch;

		/// <summary>
		/// If this is a nightly package, see <see cref="IsNightly"/>, this will represent the date time when the build was created
		/// </summary>
		public long? NightlyTime => IsNightly ? _nightlyTime : default;

		/// <summary>
		/// If this is a release candidate package, see <see cref="IsReleaseCandidate"/>, this will represent the number of the release candidate.
		/// </summary>
		public int? RC => IsReleaseCandidate ? _rc : default;

		public PackageVersion(int major, int minor, int patch, int rc = -1, long nightlyTime = -1, bool isPreview = false, bool isExperimental = false)
		{
			_major = major;
			_minor = minor;
			_patch = patch;
			_rc = rc;
			_nightlyTime = nightlyTime;
			_isPreview = isPreview;
			_isExperimental = isExperimental;
		}

		protected bool Equals(PackageVersion other)
		{
			return _major == other._major && _minor == other._minor && _patch == other._patch && _rc == other._rc && _nightlyTime == other._nightlyTime && _isPreview == other._isPreview;
		}

		/// <summary>
		/// Check if the given object is a semantic match of this package version.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns>True if the given object is the same semantic package version as this instance.</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((PackageVersion)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = _major;
				hashCode = (hashCode * 397) ^ _minor;
				hashCode = (hashCode * 397) ^ _patch;
				hashCode = (hashCode * 397) ^ _rc;
				hashCode = (hashCode * 397) ^ _nightlyTime.GetHashCode();
				hashCode = (hashCode * 397) ^ _isPreview.GetHashCode();
				return hashCode;
			}
		}

		[Obsolete]
		public bool IsMinor(int major, int minor) => IsMajor(major) && Minor == minor;

		[Obsolete]
		public bool IsMajor(int major) => Major == major;

		public static bool operator <(PackageVersion a, PackageVersion b)
		{
			return a.Major < b.Major || (a.Major <= b.Major && a.Minor < b.Minor) || (a.Major <= b.Minor && a.Minor <= b.Minor && (a.Patch < b.Patch));
		}

		public static bool operator >(PackageVersion b, PackageVersion a)
		{
			return a.Major < b.Major || (a.Major <= b.Major && a.Minor < b.Minor) || (a.Major <= b.Minor && a.Minor <= b.Minor && a.Patch < b.Patch);
		}

		public static bool operator ==(PackageVersion a, PackageVersion b)
		{
			return a.Equals(b);
		}
		public static bool operator !=(PackageVersion a, PackageVersion b)
		{
			return !(a == b);
		}

		public static bool operator <=(PackageVersion a, PackageVersion b)
		{
			return a < b || a == b;
		}
		public static bool operator >=(PackageVersion a, PackageVersion b)
		{
			return a > b || a == b;
		}

		public static implicit operator PackageVersion(string versionString) => PackageVersion.FromSemanticVersionString(versionString);


		/// <summary>
		/// Convert the package version into a semantic package string.
		/// </summary>
		/// <returns>A semantic version string</returns>
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append(_major);
			sb.Append(VERSION_SEPARATOR);
			sb.Append(_minor);
			sb.Append(VERSION_SEPARATOR);
			sb.Append(_patch);
			if (_isPreview)
			{
				sb.Append(PREVIEW_SEPARATOR);
				sb.Append(PREVIEW_STRING);
			}
			if (_isExperimental && _major != 0)
			{
				sb.Append(PREVIEW_SEPARATOR);
				sb.Append(EXPERIMENTAL_STRING);
			}

			if (IsNightly)
			{
				sb.Append(VERSION_SEPARATOR);
				sb.Append(NIGHTLY_STRING);
				sb.Append(PREVIEW_SEPARATOR);
				sb.Append(_nightlyTime);
			}

			if (IsReleaseCandidate)
			{
				sb.Append(VERSION_SEPARATOR);
				sb.Append(RC_STRING);
				sb.Append(_rc);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Try to parse a string into a <see cref="PackageVersion"/>
		/// </summary>
		/// <param name="semanticVersion">some semantic string</param>
		/// <param name="version">This value will be assigned to the parsed value of the semantic string, or will be null if the string wasn't a valid semantic version string</param>
		/// <returns>True if the given string was a valid semantic version; false otherwise.</returns>
		public static bool TryFromSemanticVersionString(string semanticVersion, out PackageVersion version)
		{
			try
			{
				version = semanticVersion;
				return true;
			}
			catch
			{
				version = new PackageVersion(0, 0, 0);
				return false;
			}
		}

		/// <summary>
		/// Parse a string into a <see cref="PackageVersion"/>
		/// https://docs.unity3d.com/Manual/upm-lifecycle.html
		/// </summary>
		/// <param name="semanticVersion">some semantic version string</param>
		/// <returns>A <see cref="PackageVersion"/></returns>
		/// <exception cref="ArgumentException">If the string was not a valid semantic version string</exception>
		public static PackageVersion FromSemanticVersionString(string semanticVersion)
		{
			var major = -1;
			var minor = -1;
			var patch = -1;
			var rc = -1;
			var nightlyTime = -1L;
			var isPreview = false;
			var isExp = false;

			var buffer = "";
			for (var i = 0; i < semanticVersion.Length; i++)
			{
				var c = semanticVersion[i];
				if (!isPreview && buffer.ToLowerInvariant().StartsWith(PREVIEW_PREFIX_STRING.ToLowerInvariant()))
				{
					// consume the rest of the PREVIEW_STRING- which is until some delim.
					while (c != PREVIEW_SEPARATOR && c != VERSION_SEPARATOR && i < semanticVersion.Length)
					{
						i++; // outside for loop modification.
						c = semanticVersion[i];
					}

					isPreview = true;
					buffer = "";
				}
				if (!isExp && buffer.ToLowerInvariant().StartsWith(EXP_PREFIX_STRING.ToLowerInvariant()))
				{
					// consume the rest of the PREVIEW_STRING- which is until some delim.
					while (c != PREVIEW_SEPARATOR && c != VERSION_SEPARATOR && i < semanticVersion.Length)
					{
						i++; // outside for loop modification.
						c = semanticVersion[i];
					}

					isExp = true;
					buffer = "";
				}

				if (buffer.Equals(RC_STRING))
				{
					if (!int.TryParse(semanticVersion.Substring(i, semanticVersion.Length - i), out rc))
					{
						throw new ArgumentException("rc version not an int");
					}
					break;
				}

				if (buffer.Equals(NIGHTLY_STRING))
				{
					// add one to ignore the expected - character
					if (!long.TryParse(semanticVersion.Substring(i + 1, semanticVersion.Length - (i + 1)), out nightlyTime))
					{
						throw new ArgumentException("nightly time not a long");
					}

					break;
				}

				switch (c)
				{
					case VERSION_SEPARATOR when major == UNASSIGNED_VALUE:
						if (!int.TryParse(buffer, out major))
						{
							throw new ArgumentException("Major version not an int");
						}

						buffer = "";
						break;
					case VERSION_SEPARATOR when minor == UNASSIGNED_VALUE:
						if (!int.TryParse(buffer, out minor))
						{
							throw new ArgumentException("Minor version not an int");
						}

						buffer = "";
						break;
					case PREVIEW_SEPARATOR when patch == UNASSIGNED_VALUE:
						if (!int.TryParse(buffer, out patch))
						{
							throw new ArgumentException("Patch version not an int");
						}

						buffer = "";
						break;
					case PREVIEW_SEPARATOR:
					case VERSION_SEPARATOR:
						break;
					default:
						buffer += c;
						break;
				}

				var lastChar = i == semanticVersion.Length - 1;
				if (lastChar && patch == UNASSIGNED_VALUE)
				{
					if (!int.TryParse(buffer, out patch))
					{
						throw new ArgumentException("Patch version not an int");
					}
				}

				if (lastChar && buffer.ToLowerInvariant().StartsWith(PREVIEW_PREFIX_STRING.ToLowerInvariant()))
				{
					isPreview = true;
				}

				if (lastChar && buffer.ToLowerInvariant().StartsWith(EXP_PREFIX_STRING.ToLowerInvariant()))
				{
					isExp = true;
				}
			}

			isExp |= major == 0; // if the major version is a 0, then its implied to be a preview package.
			return new PackageVersion(
			   major: major,
			   minor: minor,
			   patch: patch,
			   rc: rc,
			   nightlyTime: nightlyTime,
			   isPreview: isPreview,
			   isExperimental: isExp);
		}
	}
}
