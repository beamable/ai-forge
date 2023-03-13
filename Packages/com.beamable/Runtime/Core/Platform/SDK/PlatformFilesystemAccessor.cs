using Beamable.Common.Api;
using UnityEngine;

namespace Core.Platform.SDK
{
	public class PlatformFilesystemAccessor : IBeamableFilesystemAccessor

	{
		public string GetPersistentDataPathWithoutTrailingSlash()
		{
			return Application.persistentDataPath;
		}
	}
}
