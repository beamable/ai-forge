using Beamable.Common;
using Beamable.Common.Api;
using System;
using UnityEngine.Networking;

namespace Beamable.Api
{

	[Serializable]
	public class PlatformError
	{
		public long status;
		public string service;
		public string error;
		public string message;
	}

	public class PlatformRequesterException : RequesterException
	{
		public PlatformError Error { get; }
		public UnityWebRequest Request { get; }
		public PlatformRequesterException(PlatformError error, UnityWebRequest request, string responsePayload)
			: base(Constants.Requester.ERROR_PREFIX_UNITY_SDK, request.method, request.url, request.responseCode, responsePayload)
		{
			Error = error;
			Request = request;
		}
	}


}
