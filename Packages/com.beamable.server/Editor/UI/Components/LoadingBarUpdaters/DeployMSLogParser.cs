using Beamable.Editor.UI.Components;
using Beamable.Editor.UI.Model;
using System.Linq;
using UnityEngine;
using static Beamable.Common.Constants.Features.Services;

namespace Beamable.Editor.Microservice.UI.Components
{
	public class DeployMSLogParser : LoadingBarUpdater
	{
		private readonly ServiceModelBase _model;
		public override string ProcessName => "Deploying...";

		private static readonly string[] globalSuccessLogs =
		{
			UPLOAD_CONTAINER_MESSAGE,
			CONTAINER_ALREADY_UPLOADED_MESSAGE
		};

		private static readonly string[] globalFailureLogs = { CANT_UPLOAD_CONTAINER_MESSAGE };

		private readonly string[] successLogs, failureLogs;

		public DeployMSLogParser(ILoadingBar loadingBar, ServiceModelBase model) : base(loadingBar)
		{
			_model = model;
			Step = 0;
			TotalSteps = 1;
			successLogs = globalSuccessLogs.Select(l => string.Format(l, model.Name)).ToArray();
			failureLogs = globalFailureLogs.Select(l => string.Format(l, model.Name)).ToArray();

			OnProgress(0, 0, 1);
			_model.OnDeployProgress += OnProgress;
			Application.logMessageReceived += HandleLog;
		}

		private void HandleLog(string logString, string stackTrace, LogType type)
		{
			if (successLogs.Contains(logString))
			{
				LoadingBar.SetUpdater(null);
				LoadingBar.UpdateProgress(1f);
			}
			else if (failureLogs.Contains(logString))
			{
				LoadingBar.SetUpdater(null);
				LoadingBar.UpdateProgress(0f, failed: true);
			}
		}

		private void OnProgress(float progress, long step, long total)
		{
			TotalSteps = (int)total;
			Step = (int)step;
			LoadingBar.UpdateProgress(progress);
		}

		protected override void OnKill()
		{
			_model.OnDeployProgress -= OnProgress;
			Application.logMessageReceived -= HandleLog;
		}
	}
}
