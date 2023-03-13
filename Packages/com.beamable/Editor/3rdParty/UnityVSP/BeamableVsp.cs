using Beamable;
using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Editor;
using System;

namespace UnityEditor.VspAttribution.Beamable
{
	public class BeamableVsp
	{
		private readonly IBeamableRequester _requester;
		private readonly IHttpRequester _httpRequester;

		public BeamableVsp(IPlatformRequester requester, IHttpRequester httpRequester)
		{
			_requester = requester;
			_httpRequester = httpRequester;
		}

		public void TryToEmitAttribution(string action)
		{
			if (!BeamableEnvironment.IsUnityVsp) return;
			if (string.IsNullOrEmpty(action)) return;

			var cid = _requester?.Cid;
			if (string.IsNullOrEmpty(cid)) return;

			VspAttribution.SendAttributionEvent(
				action,
				"beamable",
				cid);
		}

		public async Promise<VspMetadata> GetLatestVersion()
		{
			var res = await _httpRequester.ManualRequest<VspVersionResponse>(Method.GET, "http://beamable-vsp.beamable.com/vsp-meta.json");
			var metadata = new VspMetadata { storeUrl = res.storeUrl };
			try
			{
				PackageVersion version = res.version;
				metadata.version = version;
			}
			catch
			{
				metadata.version = new PackageVersion(0, 0, 0);
			}

			return metadata;
		}

		[Serializable]
		public class VspVersionResponse
		{
			public string version;
			public string storeUrl;
		}

		public class VspMetadata
		{
			public PackageVersion version;
			public string storeUrl;
		}
	}
}
