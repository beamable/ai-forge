using UnityEngine.Networking;

namespace Beamable.Api
{
	public static class RequesterPolyfill
	{
		public static bool IsHttpError(this UnityWebRequest webRequest)
		{
#if UNITY_2020_1_OR_NEWER
         return webRequest.result == UnityWebRequest.Result.ProtocolError;
#else
			return webRequest.isHttpError;
#endif
		}

		public static bool IsNetworkError(this UnityWebRequest webRequest)
		{
#if UNITY_2020_1_OR_NEWER
         return webRequest.result == UnityWebRequest.Result.ConnectionError;
#else
			return webRequest.isNetworkError;
#endif
		}
	}
}
