using Beamable.Editor.UI.Components;
using Beamable.Editor.UI.Model;
using Beamable.Server.Editor.DockerCommands;

namespace Beamable.Editor.Microservice.UI.Components
{
	public class RunImageLogParser : LoadingBarUpdater
	{
		private readonly ServiceModelBase _model;

		public override string StepText => $"(Starting {base.StepText} MS {_model.Name})";
		public override string ProcessName => $"Starting MS {_model?.Descriptor?.Name}";
		protected override void OnKill()
		{
			_model.Builder.OnStartingFinished -= HandleStartingFinished;
			_model.Builder.OnStartingProgress -= HandleStartingProgress;
		}

		public RunImageLogParser(ILoadingBar loadingBar, ServiceModelBase model) : base(loadingBar)
		{
			_model = model;
			TotalSteps = MicroserviceLogHelper.RunLogsSteps;
			LoadingBar.UpdateProgress(0f, $"({ProcessName})");
			_model.Builder.OnStartingFinished += HandleStartingFinished;
			_model.Builder.OnStartingProgress += HandleStartingProgress;
		}

		private void HandleStartingFinished(bool success)
		{
			var value = success ? 1.0f : 0.0f;
			var message = success ? "(Success)" : "(Error)";
			LoadingBar.UpdateProgress(value, message, !success);
			if (success)
			{
				Succeeded = true;
			}
			else
			{
				GotError = true;
			}
			Kill();
		}

		private void HandleStartingProgress(int currentStep, int totalSteps)
		{
			Step = currentStep;
			TotalSteps = totalSteps;
			LoadingBar.UpdateProgress((currentStep - 1f) / totalSteps, StepText);
		}
	}
}
