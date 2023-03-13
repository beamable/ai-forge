using System.IO;
using UnityEngine;

namespace Beamable.Common
{
	public static partial class Constants
	{
		public static partial class Features
		{
			public static partial class Content
			{
				public const string PUBLIC = "public";
				public const string PRIVATE = "private";

				public const string MISSING_SUFFIX = " (missing)";

				public static readonly string BEAMABLE_RESOURCES_PATH = Path.Combine(Application.dataPath, "Beamable/Resources");
				public static readonly string BAKED_FILE_RESOURCE_PATH = "bakedContent";
				public static readonly string BAKED_CONTENT_FILE_PATH = Path.Combine(BEAMABLE_RESOURCES_PATH, BAKED_FILE_RESOURCE_PATH);
				public static readonly string BAKED_MANIFEST_RESOURCE_PATH = "bakedManifest";
				public static readonly string BAKED_MANIFEST_FILE_PATH = Path.Combine(BEAMABLE_RESOURCES_PATH, BAKED_MANIFEST_RESOURCE_PATH);
			}
		}
	}
}
