using Beamable.Common;
using Beamable.Common.Assistant;
using Beamable.Editor.Microservice.UI;
using Beamable.Editor.UI;
using Beamable.Editor.UI.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Beamable.Server.Editor.DockerCommands
{
	public class DockerNotInstalledException : Exception { }

	public abstract class DockerCommand
	{
		const int PROCESS_NOT_FOUND_EXIT_CODE = 127; // TODO: Check this for windows?

		protected virtual bool CaptureStandardBuffers => true;
		public static bool DockerNotInstalled
		{
			get => EditorPrefs.GetBool("DockerNotInstalled", true);
			protected set
			{
				var globalHintStorage = BeamEditor.HintGlobalStorage;
				if (value)
					globalHintStorage.AddOrReplaceHint(BeamHintType.Validation, BeamHintDomains.BEAM_CSHARP_MICROSERVICES_DOCKER, BeamHintIds.ID_INSTALL_DOCKER_PROCESS);
				else
					globalHintStorage.RemoveHint(new BeamHintHeader(BeamHintType.Validation, BeamHintDomains.BEAM_CSHARP_MICROSERVICES_DOCKER, BeamHintIds.ID_INSTALL_DOCKER_PROCESS));

				EditorPrefs.SetBool("DockerNotInstalled", value);
			}
		}

		public static bool DockerNotRunning
		{
			get => SessionState.GetBool("DockerNotRunning", true);
			set
			{
				var globalHintStorage = BeamEditor.HintGlobalStorage;
				if (!DockerNotInstalled && value)
					globalHintStorage.AddOrReplaceHint(BeamHintType.Validation, BeamHintDomains.BEAM_CSHARP_MICROSERVICES_DOCKER, BeamHintIds.ID_DOCKER_PROCESS_NOT_RUNNING);
				else
					globalHintStorage.RemoveHint(new BeamHintHeader(BeamHintType.Validation, BeamHintDomains.BEAM_CSHARP_MICROSERVICES_DOCKER, BeamHintIds.ID_DOCKER_PROCESS_NOT_RUNNING));

				SessionState.SetBool("DockerNotRunning", value);
			}
		}

		public virtual bool DockerRequired => true;

		protected bool _skipDockerCheck = false;

		private Process _process;
		private TaskCompletionSource<int> _status, _standardOutComplete;

		private bool _started, _hasExited;
		protected int _exitCode = -1;
		protected string DockerCmd => MicroserviceConfiguration.Instance.ValidatedDockerCommand;
		public Action<int> OnExit;

		public bool WriteLogToUnity { get; set; }
		public bool WriteCommandToUnity { get; set; }

		public string UnityLogLabel = "Docker";

		protected string StandardOutBuffer { get; private set; }

		protected string StandardErrorBuffer { get; private set; }

		public Action<string> OnStandardOut;
		public Action<string> OnStandardErr;

		protected List<Func<string, bool>> _standardOutFilters = new List<Func<string, bool>>();
		protected List<Func<string, bool>> _standardErrFilters = new List<Func<string, bool>>();
		protected Func<LogMessage, LogMessage> _standardOutProcessors = m => m;
		protected Func<LogMessage, LogMessage> _standardErrProcessors = m => m;


		public abstract string GetCommandString();

		protected virtual void HandleOnExit() { }

		private void ProcessStandardOut(string data)
		{
			if (_standardOutFilters.Any(pred => !(pred?.Invoke(data) ?? false)))
			{
				return; // ignore the standard out
			}
			if (!string.IsNullOrEmpty(data))
			{
				StandardOutBuffer += data;
			}
			HandleStandardOut(data);
			if (data != null)
			{
				OnStandardOut?.Invoke(data);
			}
		}

		private void ProcessStandardErr(string data)
		{
			if (_standardErrFilters.Any(pred => !(pred?.Invoke(data) ?? false)))
			{
				return; // ignore the standard out
			}
			if (!string.IsNullOrEmpty(data))
			{
				StandardErrorBuffer += data;
			}

			HandleStandardErr(data);
			if (data != null)
			{
				OnStandardErr?.Invoke(data);
			}
		}

		protected virtual void HandleStandardOut(string data)
		{
			if (_hasExited && data == null)
			{
				_standardOutComplete.TrySetResult(0);
			}

			if (WriteLogToUnity && data != null)
			{
				LogInfo(data);
			}
		}

		protected virtual void HandleStandardErr(string data)
		{
			if (WriteLogToUnity && data != null)
			{
				LogError(data);
			}
		}

		public virtual void Start()
		{
			if (!_skipDockerCheck && DockerRequired && DockerNotInstalled)
			{
				throw new DockerNotInstalledException();
			}

			if (_process != null)
			{
				throw new Exception("Process already started.");
			}

			var command = GetCommandString();
			/*do not await. It will keep it on a separate thread, which is very important. */

			Run(command);
		}

		public void Join()
		{
			_status.Task.Wait();
		}

		public void Kill()
		{
			if (_process == null || !_started || _hasExited) return;

			_process.Kill();
			try { }
			catch (InvalidOperationException ex)
			{
				Debug.LogWarning("Unable to stop process, but likely was already stopped. " + ex.Message);
			}
		}

		private string ColorizeMessage(string message, Color labelColor, Color messageColor)
		{
			if (!MicroserviceConfiguration.Instance.ColorLogs)
			{
				return $"[{UnityLogLabel}] {message}";
			}

			var labelColorHex = ColorUtility.ToHtmlStringRGB(labelColor);
			var outColorHex = ColorUtility.ToHtmlStringRGB(messageColor);

			return $"<color=#{labelColorHex}>[{UnityLogLabel}]:</color> <color=#{outColorHex}>{message}</color>";
		}

		protected void LogInfo(string data)
		{
			Debug.Log(ColorizeMessage(
						  data,
						  MicroserviceConfiguration.Instance.LogProcessLabelColor,
						  MicroserviceConfiguration.Instance.LogStandardOutColor));
		}

		protected void LogError(string data)
		{
			Debug.Log(ColorizeMessage(
						  data,
						  MicroserviceConfiguration.Instance.LogProcessLabelColor,
						  MicroserviceConfiguration.Instance.LogStandardErrColor));
		}

		protected virtual void ModifyStartInfo(ProcessStartInfo processStartInfo) { }

		async void Run(string command)
		{
			try
			{
				var _ = MicroserviceConfiguration.Instance; // preload configuration...
				if (WriteCommandToUnity)
				{
					Debug.Log("============== Start Executing [" + command + "] ===============");
				}

				using (_process = new System.Diagnostics.Process())
				{
#if UNITY_EDITOR && !UNITY_EDITOR_WIN
               _process.StartInfo.FileName = "sh";
               _process.StartInfo.Arguments = $"-c '{command}'";
#else
					_process.StartInfo.FileName = "cmd.exe";
					_process.StartInfo.Arguments = $"/C {command}"; //  "/C " + command + " > " + commandoutputfile + "'"; // TODO: I haven't tested this since refactor.
#endif
					// Configure the process using the StartInfo properties.
					_process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
					_process.EnableRaisingEvents = true;
					_process.StartInfo.RedirectStandardInput = true;
					_process.StartInfo.RedirectStandardOutput = CaptureStandardBuffers;
					_process.StartInfo.RedirectStandardError = CaptureStandardBuffers;
					_process.StartInfo.CreateNoWindow = true;
					_process.StartInfo.UseShellExecute = false;
					ModifyStartInfo(_process.StartInfo);

					_status = new TaskCompletionSource<int>();
					_standardOutComplete = new TaskCompletionSource<int>();
					EventHandler eh = (s, e) =>
					{
						Task.Run(async () =>
						{
							await Task.Delay(1); // give 1 ms for log messages to eep out
							BeamEditorContext.Default.Dispatcher.Schedule(() =>
							{
								// there still may pending log lines, so we need to make sure they get processed before claiming the process is complete
								_hasExited = true;
								_exitCode = _process.ExitCode;

								OnExit?.Invoke(_process.ExitCode);
								HandleOnExit();

								_status.TrySetResult(0);
							});
						});
					};

					_process.Exited += eh;

					try
					{
						_process.EnableRaisingEvents = true;

						_process.OutputDataReceived += (sender, args) =>
						{
							BeamEditorContext.Default.Dispatcher.Schedule(() =>
							{
								try
								{
									ProcessStandardOut(args.Data);
								}
								catch (Exception ex)
								{
									Debug.LogException(ex);
								}
							});
						};
						_process.ErrorDataReceived += (sender, args) =>
						{
							BeamEditorContext.Default.Dispatcher.Schedule(() =>
							{
								try
								{
									ProcessStandardErr(args.Data);
								}
								catch (Exception ex)
								{
									Debug.LogException(ex);
								}
							});
						};

						// before starting anything, make sure the beam context has initialized, so that the dispatcher can be accessed later.
						await BeamEditorContext.Default.InitializePromise;
						await MicroserviceEditor.WaitForInit();

						_process.Start();
						_started = true;
						_process.BeginOutputReadLine();
						_process.BeginErrorReadLine();

						await _status.Task;
					}
					finally
					{
						_process.Exited -= eh;
					}

					if (WriteCommandToUnity)
					{
						Debug.Log("============== End ===============");
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		public static void ClearDockerInstallFlag()
		{
			DockerNotInstalled = false;
		}

		public DockerCommand AddStandardOutFilter(Func<string, bool> predicate)
		{
			_standardOutFilters.Add(predicate);
			return this;
		}
		public DockerCommand AddStandardErrFilter(Func<string, bool> predicate)
		{
			_standardErrFilters.Add(predicate);
			return this;
		}

		public DockerCommand MapStandardOut(Func<LogMessage, LogMessage> processor)
		{
			var old = _standardOutProcessors;
			_standardOutProcessors = m => processor(old(m));
			return this;
		}

		public DockerCommand MapStandardErr(Func<LogMessage, LogMessage> processor)
		{
			var old = _standardErrProcessors;

			_standardErrProcessors = m => processor(old(m));
			return this;
		}

		public DockerCommand AddGlobalFilter(Func<string, bool> predicate)
		{
			AddStandardErrFilter(predicate);
			AddStandardOutFilter(predicate);
			return this;
		}

		public DockerCommand MapGlobal(Func<LogMessage, LogMessage> processor)
		{
			MapStandardErr(processor);
			MapStandardOut(processor);
			return this;
		}

		public DockerCommand MapDotnetCompileErrors()
		{
			return MapGlobal((logMessage) =>
			{
				if (MicroserviceLogHelper.TryGetErrorCode(logMessage.Message, out var errCode))
				{
					logMessage.Level = LogLevel.ERROR;
					logMessage.Parameters.Add("errorCode", errCode);
				}
				return logMessage;
			});
		}

		private static Promise<bool> DockerCheckTask;
		public static Promise<bool> CheckDockerAppRunning()
		{
			if (DockerCheckTask != null && !DockerCheckTask.IsCompleted)
			{
				return DockerCheckTask;
			}

			bool dockerNotRunning = DockerNotRunning;
			var task = new Task<bool>(() =>
			{
				var procList = Process.GetProcesses();
				for (int i = 0; i < procList.Length; i++)
				{
					try
					{
#if UNITY_EDITOR_WIN
						const string procName = "docker desktop";
#else
						const string procName = "docker";
#endif
						if (procList[i].ProcessName.ToLower().Contains(procName))
						{
							dockerNotRunning = false;
							return false;
						}
					}
					catch
					{
					}
				}

				dockerNotRunning = true;
				return true;
			});
			task.Start();
			DockerCheckTask = task.ToPromise();
			DockerCheckTask.Then(_ =>
			{
				DockerNotRunning = dockerNotRunning;
			});

			return DockerCheckTask;
		}

		public static bool RunDockerProcess()
		{
			if (DockerNotInstalled || !DockerNotRunning) return false;

			var dockerDesktopPath = MicroserviceConfiguration.Instance.DockerDesktopPath;



			if (!File.Exists(dockerDesktopPath))
			{
				Debug.LogError("Failed to run Docker Desktop as it is not installed. We highly recommend the use of Docker Desktop.");
				return false;
			}

			var dockerProcess = Process.Start(new ProcessStartInfo(dockerDesktopPath));
			dockerProcess.EnableRaisingEvents = true;
			dockerProcess.Exited += async (sender, args) =>
			{
				await DockerCheckTask;

				BeamEditorContext.Default.Dispatcher.Schedule(async () =>
				{
					Debug.Log("Docker Desktop was closed!");
					DockerNotRunning = true;

					var tempQualifier = await BeamEditorWindow<MicroserviceWindow>.GetFullyInitializedWindow();
					tempQualifier.RefreshWindowContent();
				});
			};

			return true;
		}
	}
}
