//#define PREPEND_SPEW_METADATA
// With PREPEND_SPEW_METADATA defined, all spew logs will have frame # and time prepended.

using System.Diagnostics;
using static Beamable.Common.Constants.Features.Spew;

#if PREPEND_SPEW_METADATA
using UnityEngine;
#endif

namespace Beamable.Common.Spew
{
	public class Logger
	{
#if UNITY_EDITOR
      static Logger()
      {
         filter = UnityEditor.EditorPrefs.GetString("SpewFilter", "");
      }
#endif

#if PREPEND_SPEW_METADATA
      private static int _lastFrame;
      private static string _lastColor = "orange";

      static string PrependMetadata(string msg)
      {
         if (_lastFrame != Time.frameCount)
         {
            _lastFrame = Time.frameCount;
            _lastColor = _lastColor == "orange" ? "lightblue" : "orange";
         }

         return $"[<color={_lastColor}>{Time.frameCount}</color>:{Time.realtimeSinceStartup}] {msg}";
      }
#endif

		public static string filter = "";

		public static void DoSpew(string msg, params object[] args)
		{
			var s = string.Format(msg, args);
			if (!string.IsNullOrEmpty(filter) && !s.Contains(filter))
				return;

#if PREPEND_SPEW_METADATA
         s = PrependMetadata(s);
#endif

			UnityEngine.Debug.Log(s);
		}

		public static void DoSpew(object msg)
		{
			var s = msg.ToString();
			if (!string.IsNullOrEmpty(filter) && !s.Contains(filter))
				return;

#if PREPEND_SPEW_METADATA
         s = PrependMetadata(s);
#endif

			UnityEngine.Debug.Log(s);
		}

		public static void DoSpew(object msg, UnityEngine.Object context)
		{
			var s = msg.ToString();
			if (!string.IsNullOrEmpty(filter) && !s.Contains(filter))
				return;

#if PREPEND_SPEW_METADATA
         s = PrependMetadata(s);
#endif

			UnityEngine.Debug.Log(s, context);
		}
	}

	[SpewLogger]
	public static class NotificationLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_NOTIFICATION)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional(SPEW_ALL), Conditional(SPEW_NOTIFICATION)]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}

	[SpewLogger]
	public static class ChatLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_CHAT)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional(SPEW_ALL), Conditional(SPEW_CHAT)]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}

	[SpewLogger]
	public static class PubnubSubscriptionLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_PUBNUB_SUBSCRIPTION)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional(SPEW_ALL)]
		[Conditional(SPEW_PUBNUB_SUBSCRIPTION)]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}

	[SpewLogger]
	public static class TutorialLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_TUTORIAL)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional(SPEW_ALL), Conditional(SPEW_TUTORIAL)]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}

	[SpewLogger]
	public static class UIMaterialFillLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_UI_MATERIAL_FILL)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional(SPEW_ALL)]
		[Conditional(SPEW_UI_MATERIAL_FILL)]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}

	[SpewLogger]
	public static class TouchLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_TOUCH)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional(SPEW_ALL)]
		[Conditional(SPEW_TOUCH)]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}

	[SpewLogger]
	public static class GraphicsLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_GRAPHICS)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional(SPEW_ALL), Conditional(SPEW_GRAPHICS)]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}

	[SpewLogger]
	public static class AssetBundleLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_ASSETBUNDLE)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional(SPEW_ALL), Conditional(SPEW_ASSETBUNDLE)]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}

	[SpewLogger]
	public static class BuildLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_BUILD)]
		public static void Log(object msg) { Logger.DoSpew(msg); }
	}

	[SpewLogger]
	public static class ServerStateLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_SERVERSTATE)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional(SPEW_ALL), Conditional(SPEW_SERVERSTATE)]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}

	[SpewLogger]
	public static class InAppPurchaseLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_IAP)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional(SPEW_ALL), Conditional(SPEW_IAP)]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}

	[SpewLogger]
	public static class MailLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_MAIL)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}
	}

	[SpewLogger]
	public static class CloudOnceLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_CLOUDONCE)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional(SPEW_ALL), Conditional(SPEW_CLOUDONCE)]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}

	[SpewLogger]
	public static class ApptentiveLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_APPTENTIVE)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional(SPEW_ALL), Conditional(SPEW_APPTENTIVE)]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}

	[SpewLogger]
	public static class AnalyticsLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_ANALYTICS)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional(SPEW_ALL), Conditional(SPEW_ANALYTICS)]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}

	[SpewLogger]
	public static class NetMsgLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_NETMSG)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional(SPEW_ALL), Conditional(SPEW_NETMSG)]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}

	[SpewLogger]
	public static class PlatformLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_PLATFORM)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional(SPEW_ALL), Conditional(SPEW_PLATFORM)]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}

	[SpewLogger]
	public static class ResourcesLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_RESOURCES)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional(SPEW_ALL), Conditional(SPEW_RESOURCES)]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}

	[SpewLogger]
	public static class AppLifetimeLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_APP_LIFETIME)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional(SPEW_ALL), Conditional(SPEW_APP_LIFETIME)]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}

	[SpewLogger]
	public static class SVGLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_SVG)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional(SPEW_ALL), Conditional(SPEW_SVG)]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}

	[SpewLogger]
	public static class ServicesLogger
	{
		[Conditional(SPEW_ALL), Conditional(SPEW_SERVICES)]
		public static void Log(object msg)
		{
			Logger.DoSpew(msg);
		}

		[Conditional(SPEW_ALL), Conditional(SPEW_SERVICES)]
		public static void LogFormat(string msg, params object[] args)
		{
			Logger.DoSpew(msg, args);
		}
	}
}
