using System.Collections.Generic;

namespace Beamable.Content
{
	public class Manifest
	{
		public string id;
		public long created;
		public List<ManifestReference> references;
	}

	public class ManifestReference
	{
		public string Id;
		public string Version;
		public string Uri;
		public string Checksum;
		public string Type;
		public string Created;
		public string LastChanged;
	}
}
