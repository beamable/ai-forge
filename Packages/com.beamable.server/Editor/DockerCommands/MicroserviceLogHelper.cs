using Beamable.Common;
using Beamable.Editor.UI.Model;
using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static Beamable.Common.Constants.Features.Services;

namespace Beamable.Server.Editor.DockerCommands
{
	public static class MicroserviceLogHelper
	{
		public static int RunLogsSteps => ExpectedRunLogs.Length;

		private static readonly Regex StepRegex = new Regex("Step [0-9]+/[0-9]+");
		private static readonly Regex StepBuildKitRegex = new Regex("#[0-9]+");

		private static readonly Regex NumberRegex = new Regex("[0-9]+");
		private static readonly string[] ErrorElements = {
			"error",
			"Error",
			"Exception",
			"exception"
		};

		private static readonly Dictionary<string, string> ContextForLogs =
			new Dictionary<string, string>
			{
				{"pull access denied for beamservice",
					"No version of beamservice exists on your computer. Please rebuild the image and try again. " +
					"Please ignore Docker’s access denied messaging, that is a red herring"}
			};

		private static readonly HashSet<string> ErrorExclusions = new HashSet<string>
		{
			"\" >> /etc/supervisor/conf.d/supervisord.conf && echo \"loglevel=error"
		};
		private static readonly string[] ExpectedRunLogs = {
			Logs.STARTING_PREFIX,
			Logs.SCANNING_CLIENT_PREFIX,
			Logs.REGISTERING_STANDARD_SERVICES,
			Logs.REGISTERING_CUSTOM_SERVICES,
			Logs.SERVICE_PROVIDER_INITIALIZED,
			Logs.EVENT_PROVIDER_INITIALIZED
		};

		/// <summary>
		/// Given a log message, try and recognize a standard dotnet error code in the form of CS1234
		/// </summary>
		/// <param name="message"></param>
		/// <param name="errCode"></param>
		/// <returns>true if an error code was found, false otherwise</returns>
		public static bool TryGetErrorCode(string message, out int errCode)
		{
			errCode = 0;
			if (string.IsNullOrEmpty(message)) return false;
			var index = message.IndexOf(DOTNET_COMPILE_ERROR_SYMBOL, StringComparison.InvariantCulture);
			if (index <= -1) return false; // only care about errors...

			var numbers = message.Substring(index + DOTNET_COMPILE_ERROR_SYMBOL.Length, 4);
			if (!int.TryParse(numbers, out errCode))
			{
				return false;
			}

			return true;
		}

		public static bool HandleMongoLog(StorageObjectDescriptor storage, string data,
			LogLevel defaultLogLevel = LogLevel.INFO, bool forceDisplay = false)
		{
			LogLevel ParseMongoLevel(string level)
			{
				switch (level)
				{
					case "I": return LogLevel.INFO;
					case "F": return LogLevel.FATAL;
					case "E": return LogLevel.ERROR;
					case "W": return LogLevel.WARNING;
					default: return LogLevel.DEBUG;
				}
			}

			if (!(Json.Deserialize(data) is ArrayDict jsonDict))
			{
				if (!forceDisplay || data == null)
				{
					return false;
				}

				var errorMessage = new LogMessage
				{
					Message = data,
					Timestamp = DateTime.Now.ToString(),
					Level = defaultLogLevel,
					ParameterText = data,
					Parameters = new Dictionary<string, object>()
				};

				BeamEditorContext.Default.Dispatcher.Schedule(() =>
				{
					MicroservicesDataModel.Instance.AddLogMessage(storage, errorMessage);
				});

				return true;
			}

			var attrs = ((ArrayDict)jsonDict["attr"]);
			var time = ((ArrayDict)jsonDict["t"])["$date"] as string;

			if (DateTime.TryParse(time, out var logDate))
			{
				time = LogMessage.GetTimeDisplay(logDate);
			}

			var logMessage = new LogMessage
			{
				Message = $" Ctx=[{jsonDict["ctx"] as string}] {jsonDict["msg"] as string}",
				Timestamp = time,
				Level = ParseMongoLevel(jsonDict["s"] as string),
				ParameterText = attrs == null
					? ""
					: string.Join("\n", attrs.Select(kvp => $"{kvp.Key}={Json.Serialize(kvp.Value, new StringBuilder())}")),
				Parameters = new Dictionary<string, object>()
			};

			BeamEditorContext.Default.Dispatcher.Schedule(() => MicroservicesDataModel.Instance.AddLogMessage(storage, logMessage));
			return true;

		}

		public static bool HandleLog(IDescriptor descriptor, string label, string data, DateTime fallbackTime = default, Func<LogMessage, LogMessage> logProcessor = null)
		{
			if (Json.Deserialize(data) is ArrayDict jsonDict)
			{
				// this is a serilog message!

				var timestamp = string.Empty;
				var logLevel = "Info"; // info by default
				var message = ""; // rendered message
				var objs = new Dictionary<string, object>();
				foreach (var kvp in jsonDict)
				{
					var key = kvp.Key;
					if (key.StartsWith("__"))
					{
						switch (key.Substring("__".Length))
						{
							case "l": // logLevel
								logLevel = kvp.Value.ToString();
								break;
							case "t": // timestamp
								timestamp = kvp.Value.ToString();
								break;
							case "m": // message
								message = kvp.Value.ToString();
								break;
						}
					}
					else
					{
						objs.Add(key, kvp.Value);
					}
				}

				string WithColor(Color logColor, string log)
				{
					if (!MicroserviceConfiguration.Instance.ColorLogs) return log;

					var msg = $"<color=\"#{ColorUtility.ToHtmlStringRGB(logColor)}\">{log}</color>";
					return msg;
				}

				var color = Color.grey;
#pragma warning disable 219
				var logLevelValue = LogLevel.DEBUG;
#pragma warning restore 219
				switch (logLevel)
				{
					case "Debug":
						color = MicroserviceConfiguration.Instance.LogDebugLabelColor;
						logLevelValue = LogLevel.DEBUG;
						break;
					case "Warning":
						color = MicroserviceConfiguration.Instance.LogWarningLabelColor;
						logLevelValue = LogLevel.WARNING;
						break;
					case "Info":
						color = MicroserviceConfiguration.Instance.LogInfoLabelColor;
						logLevelValue = LogLevel.INFO;
						break;
					case "Error":
						color = MicroserviceConfiguration.Instance.LogErrorLabelColor;
						logLevelValue = LogLevel.ERROR;
						break;
					case "Fatal":
						color = MicroserviceConfiguration.Instance.LogFatalLabelColor;
						logLevelValue = LogLevel.FATAL;
						break;
					default:
						color = Color.black;
						break;
				}

				var f = .8f;
				var darkColor = new Color(color.r * f, color.g * f, color.b * f);

				var objsToString = string.Join("\n", objs.Select(kvp => $"{kvp.Key}={Json.Serialize(kvp.Value, new StringBuilder())}"));

				// report the log message to the right bucket.
#if !BEAMABLE_LEGACY_MSW
				if (!DateTime.TryParse(timestamp, out var time))
				{
					time = DateTime.Now;
				}
				var logMessage = new LogMessage
				{
					Message = message,
					Parameters = objs,
					ParameterText = objsToString,
					Level = logLevelValue,
					Timestamp = LogMessage.GetTimeDisplay(time)
				};
				logMessage = logProcessor?.Invoke(logMessage) ?? logMessage;
				BeamEditorContext.Default.Dispatcher.Schedule(() => MicroservicesDataModel.Instance.AddLogMessage(descriptor, logMessage));

				if (MicroserviceConfiguration.Instance.ForwardContainerLogsToUnityConsole)
				{
					Debug.Log($"{WithColor(Color.grey, $"[{label}]")} {WithColor(color, $"[{logLevel}]")} {WithColor(darkColor, $"{message}\n{objsToString}")}");
				}
#else
            Debug.Log($"{WithColor(Color.grey, $"[{label}]")} {WithColor(color, $"[{logLevel}]")} {WithColor(darkColor, $"{message}\n{objsToString}")}");
#endif


				return true;
			}
			else
			{
#if !BEAMABLE_LEGACY_MSW
				if (fallbackTime <= default(DateTime))
				{
					fallbackTime = DateTime.Now;
				}
				var logMessage = new LogMessage
				{
					Message = $"{(string.IsNullOrEmpty(label) ? "" : $"{label}: ")}{data}",
					Parameters = new Dictionary<string, object>(),
					ParameterText = "",
					Level = LogLevel.INFO,
					Timestamp = LogMessage.GetTimeDisplay(fallbackTime)
				};
				logMessage = logProcessor?.Invoke(logMessage) ?? logMessage;
				if (string.IsNullOrEmpty(logMessage.Message))
				{
					return false;
				}

				BeamEditorContext.Default.Dispatcher.Schedule(() => MicroservicesDataModel.Instance.AddLogMessage(descriptor, logMessage));
				return !MicroserviceConfiguration.Instance.ForwardContainerLogsToUnityConsole;
#else
            return false;
#endif
			}
		}


		public static bool HandleLog(IDescriptor descriptor, LogLevel logLevel, string message, Color color, bool isBoldMessage, string postfixIcon)
		{
			var logMessage = new LogMessage
			{
				Message = message,
				Timestamp = LogMessage.GetTimeDisplay(DateTime.Now),
				IsBoldMessage = isBoldMessage,
				PostfixMessageIcon = postfixIcon,
				MessageColor = color,
				Level = logLevel
			};
			BeamEditorContext.Default.Dispatcher.Schedule(() => MicroservicesDataModel.Instance.AddLogMessage(descriptor, logMessage));

			return true;
		}


		public static void HandleBuildCommandOutput(IBeamableBuilder builder, string message)
		{
			const int expectedBuildSteps = 11;

			if (message == null)
				return;

			var stepsRegex = MicroserviceConfiguration.Instance.DisableDockerBuildkit
				? StepRegex
				: StepBuildKitRegex;
			var match = stepsRegex.Match(message);
			if (match.Success)
			{
				var values = NumberRegex.Matches(match.Value);
				var current = int.Parse(values[0].Value);
				var total = values.Count > 1 ? int.Parse(values[1].Value) : expectedBuildSteps;
				builder.OnBuildingProgress?.Invoke(current, total);
			}
			else if (ContextForLogs.Keys.Any(message.Contains))
			{
				var key = ContextForLogs.Keys.First(message.Contains);
				BeamEditorContext.Default.Dispatcher.Schedule(() => Debug.LogError(ContextForLogs[key]));
			}
			else if (message.Contains("Success"))
			{
				builder.OnBuildingFinished?.Invoke(true);
			}
			else if (IsErrorMatch(message))
			{
				builder.OnBuildingFinished?.Invoke(false);
			}
		}

		private static bool IsErrorMatch(string message)
		{
			//" >> /etc/supervisor/conf.d/supervisord.conf && echo "loglevel=error
			var simpleMatch = ErrorElements.Any(message.Contains);
			if (simpleMatch)
			{
				var isExclusion = ErrorExclusions.Contains(message);
				return !isExclusion;
			}
			return false;
		}

		public static void HandleRunCommandOutput(IBeamableBuilder builder, string message)
		{
			if (message == null)
				return;

			for (int i = 0; i < RunLogsSteps; i++)
			{
				if (message.Contains(ExpectedRunLogs[i]))
				{
					builder.OnStartingProgress?.Invoke(i + 1, RunLogsSteps);
				}
			}
			if (message.Contains(Logs.READY_FOR_TRAFFIC_PREFIX) ||
				message.Contains(Logs.STORAGE_READY))
			{
				builder.OnStartingFinished?.Invoke(true);
				builder.IsRunning = true;
			}
			else if (ErrorElements.Any(message.Contains))
			{
				builder.OnStartingFinished?.Invoke(false);
				builder.IsRunning = false;
			}
		}
	}
}
