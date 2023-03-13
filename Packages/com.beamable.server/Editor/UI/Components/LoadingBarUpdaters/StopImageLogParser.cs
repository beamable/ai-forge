using Beamable.Editor.UI.Components;
using Beamable.Editor.UI.Model;
using System.Linq;

namespace Beamable.Editor.Microservice.UI.Components
{
	public class StopImageLogParser : UniversalLogsParser
	{
		private static readonly string[] expectedLogs = new[] {
			"Shutdown started...",
			"Handling WS Message."
		};

		private static readonly string[] errorElements = new[] {
			"Error",
			"Exception",
			"exception"
		};

		public override string StepText => $"(Stopping {base.StepText} MS {_model.Name})";
		public override string ProcessName => $"Stopping MS {_model?.Descriptor?.Name}";

		public StopImageLogParser(ILoadingBar loadingBar, ServiceModelBase model) : base(loadingBar, model)
		{
			TotalSteps = expectedLogs.Length;
			LoadingBar.UpdateProgress(0f, $"({ProcessName})");
		}
		public override bool DetectSuccess(string message)
		{
			return message.StartsWith("All pending tasks completed.") || !_model.IsRunning;
		}

		public override bool DetectFailure(string message)
		{
			var hasError = errorElements.Any(message.Contains);
			if (hasError)
			{
				UnityEngine.Debug.LogError("FOUND ERROR: " + message);
			}
			return hasError;
		}

		public override bool DetectStep(string message, out int step)
		{
			step = 0;
			for (int i = 0; i < TotalSteps; i++)
			{
				if (message.StartsWith(expectedLogs[i]))
				{
					step = i + 1;
					return true;
				}
			}

			return false;
		}
	}
}
