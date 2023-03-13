using Beamable.Editor.UI.Components;
using Beamable.Editor.UI.Model;
using System;
using System.Linq;

namespace Beamable.Editor.Microservice.UI.Components
{
	public abstract class UniversalLogsParser : LoadingBarUpdater
	{
		protected readonly ServiceModelBase _model;

		public Action OnFailure;

		public UniversalLogsParser(ILoadingBar loadingBar, ServiceModelBase model) : base(loadingBar)
		{
			_model = model;
			_model.Logs.OnMessagesUpdated += OnMessagesUpdated;
		}
		protected override void OnKill()
		{
			_model.Logs.OnMessagesUpdated -= OnMessagesUpdated;
		}

		private void OnMessagesUpdated()
		{
			var message = _model.Logs.Messages.LastOrDefault()?.Message;
			if (string.IsNullOrWhiteSpace(message)) return;

			if (DetectSuccess(message))
			{
				Succeeded = true;
				LoadingBar.UpdateProgress(1f, $"(Success: {ProcessName})", hideOnFinish: true);
				Kill();
			}
			else if (DetectFailure(message))
			{
				OnFailure?.Invoke();
				GotError = true;
				LoadingBar.UpdateProgress(0f, $"(Error: {ProcessName})", true);
				Kill();
			}
			else if (DetectStep(message, out var step))
			{
				Step = step;
				LoadingBar.UpdateProgress((Step - 1f) / TotalSteps, StepText);
			}
		}
		public abstract bool DetectSuccess(string message);
		public abstract bool DetectFailure(string message);
		public abstract bool DetectStep(string message, out int step);
	}
}
