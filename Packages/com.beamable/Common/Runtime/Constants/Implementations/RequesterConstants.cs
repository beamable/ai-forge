namespace Beamable.Common
{
	public static partial class Constants
	{
		public static partial class Requester
		{
			public const string ERROR_PREFIX_UNITY_SDK = "HTTP Error";
			public const string ERROR_PREFIX_WEBSOCKET_RES = "WSS Error";
			public const string ERROR_PREFIX_MICROSERVICE = "Service Error";

			public const int DEFAULT_APPLICATION_TIMEOUT_SECONDS = 15;

			/// <summary>
			/// The header describing the customer's cid. This _can_ be alias.
			/// </summary>
			public const string HEADER_CID = "X-KS-CLIENTID";

			/// <summary>
			/// The header describing the customer's pid
			/// </summary>
			public const string HEADER_PID = "X-KS-PROJECTID";

			/// <summary>
			/// An authorization header
			/// </summary>
			public const string HEADER_AUTH = "Authorization";

			/// <summary>
			/// The Beamable shard to use. In all practical use cases, this shouldn't be used. Its mostly a legacy header from a long time ago.
			/// </summary>
			public const string HEADER_SHARD = "X-KS-SHARD";

			/// <summary>
			/// The header that can be used to change what server time will be used while processing this request
			/// </summary>
			public const string HEADER_TIME_OVERRIDE = "X-KS-TIME";

			/// <summary>
			/// The header that contains which language code to use
			/// </summary>
			public const string HEADER_ACCEPT_LANGUAGE = "Accept-Language";

			/// <summary>
			/// The header that contains how long Beamable will let the request live before timing it out. Should be number of milliseconds.
			/// </summary>
			public const string HEADER_TIMEOUT = "X-KS-TIMEOUT";

			/// <summary>
			/// The header that contains the runtime version of the Beamable SDK. Ex: 1.2.5, or 0.18.5
			/// </summary>
			public const string HEADER_BEAMABLE_VERSION = "X-KS-BEAM-SDK-VERSION";

			/// <summary>
			/// The header that contains the runtime version of the engine. Ex: 2019.3.1
			/// This is the version of the calling user agent. For example, if the user agent is Unity, then this
			/// is the unity version. But if the agent is Portal, or Unreal, then this is the application version of those platforms.
			/// </summary>
			public const string HEADER_UNITY_VERSION = "X-KS-USER-AGENT-VERSION";

			/// <summary>
			/// The header that contains the calling user agent. This could be Unity, UnityEditor, Portal, GCD, Unreal, etc....
			/// </summary>
			public const string HEADER_ENGINE_TYPE = "X-KS-USER-AGENT";

			/// <summary>
			/// The header that contains the game version. This is a developer controlled version. Its the Application.version
			/// </summary>
			public const string HEADER_APPLICATION_VERSION = "X-KS-GAME-VERSION";

		}
	}
}
