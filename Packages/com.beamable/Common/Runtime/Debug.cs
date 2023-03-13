using System;

namespace Beamable.Common
{
	/// <summary>
	/// This type defines the passthrough for a %Beamable %Log %Provider
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public abstract class BeamableLogProvider
	{

#if UNITY_EDITOR || UNITY_ENGINE
      private static readonly BeamableLogProvider DefaultProvider = new BeamableLogUnityProvider();
#else
		private static readonly BeamableLogProvider DefaultProvider = new SilentLogProvider();
#endif // UNITY_EDITOR || UNITY_ENGINE

		public static BeamableLogProvider Provider { get; set; } = DefaultProvider;

		public abstract void Info(string message);
		public abstract void Info(string message, params object[] args);
		public abstract void Warning(string message);
		public abstract void Warning(string message, params object[] args);
		public abstract void Error(Exception ex);
		public abstract void Error(string error);
		public abstract void Error(string error, params object[] args);
	}

	/// <summary>
	/// This type defines the passthrough to %UnityEngine.Debug methods such as Log, LogWarning, and LogException.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// ![img beamable-logo]
	///
	/// </summary>
#if !DB_MICROSERVICE
	public class BeamableLogUnityProvider : BeamableLogProvider
	{
		public override void Info(string message)
		{
			UnityEngine.Debug.Log(message);
		}

		public override void Info(string message, params object[] args)
		{
			UnityEngine.Debug.Log(string.Format(message, args));
		}

		public override void Warning(string message)
		{
			UnityEngine.Debug.LogWarning(message);
		}

		public override void Warning(string message, params object[] args)
		{
			UnityEngine.Debug.LogWarning(string.Format(message, args));
		}

		public override void Error(Exception ex)
		{
			UnityEngine.Debug.LogException(ex);
		}

		public override void Error(string error)
		{
			UnityEngine.Debug.LogError(error);
		}

		public override void Error(string error, params object[] args)
		{
			UnityEngine.Debug.LogError(string.Format(error, args));
		}
	}
#endif

	/// <summary>
	/// This type defines the provider for use on physical devices,
	/// where spamming the device log is undesirable. This log
	/// provider silently swallows all input it receives.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class SilentLogProvider : BeamableLogProvider
	{
		public override void Info(string message) { }
		public override void Info(string message, params object[] args) { }
		public override void Warning(string message) { }
		public override void Warning(string message, params object[] args) { }
		public override void Error(Exception ex) { }
		public override void Error(string error) { }
		public override void Error(string error, params object[] args) { }
	}

	/// <summary>
	/// This type defines a simple mock of the %UnityEngine %Debug class.
	/// The intention is not to replicate the entire set of functionality from Unity's Debug class,
	/// but to provide an easy reflexive log solution for dotnet core code.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public static class BeamableLogger
	{
		public static void Assert(bool assertion)
		{
			if (!assertion)
			{
				LogError(new Exception("Assertion failed")); // TODO throw callstack info?
			}
		}


		public static void Log(string info)
		{
			BeamableLogProvider.Provider.Info(info);
		}

		public static void Log(string info, params object[] args)
		{
			BeamableLogProvider.Provider.Info(info, args);
		}


		public static void LogWarning(string warning)
		{
			BeamableLogProvider.Provider.Warning(warning);

		}


		public static void LogWarning(string warning, params object[] args)
		{
			BeamableLogProvider.Provider.Warning(warning, args);
		}


		public static void LogException(Exception ex)
		{
			BeamableLogProvider.Provider.Error(ex);
		}


		public static void LogError(Exception ex)
		{
			BeamableLogProvider.Provider.Error(ex);
		}


		public static void LogError(string error)
		{
			BeamableLogProvider.Provider.Error(error);
		}


		public static void LogError(string error, params object[] args)
		{
			BeamableLogProvider.Provider.Error(error, args);
		}

		public static void LogErrorFormat(string format, params object[] args) =>
			BeamableLogProvider.Provider.Error(string.Format(format, args));
	}
}
