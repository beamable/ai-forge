using Beamable.Editor.UI.Components;
using Beamable.Editor.UI.Model;
using System.Linq;
using System.Threading.Tasks;

namespace Beamable.Editor.Microservice.UI.Components
{
	public class StepLogParser : LoadingBarUpdater
	{
		private readonly ServiceModelBase _model;
		private readonly Task _task;

		public override string StepText => $"(Building {base.StepText} MS {_model.Name})";
		public override string ProcessName => $"Building MS {_model?.Descriptor?.Name}";

		public StepLogParser(ILoadingBar loadingBar, ServiceModelBase model, Task task) : base(loadingBar)
		{
			_model = model;
			_task = task;

			LoadingBar.UpdateProgress(0f, $"({ProcessName})");

			_model.Builder.OnBuildingFinished += HandleBuildingFinished;
			_model.Builder.OnBuildingProgress += HandleBuildingProgress;
			task?.ContinueWith(_ => Kill());
		}

		private void HandleBuildingProgress(int currentStep, int totalSteps)
		{
			var message = _model.Logs.Messages.LastOrDefault()?.Message;
			Step = currentStep;
			TotalSteps = totalSteps;
			LoadingBar.UpdateProgress((currentStep - 1f) / totalSteps, StepText);
		}

		private void HandleBuildingFinished(bool success)
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
				Kill();
			}
		}

		protected override void OnKill()
		{
			if (_task?.IsFaulted ?? false)
			{
				GotError = true;
				LoadingBar.UpdateProgress(0f, "(Error)", true);
			}
			_model.Builder.OnBuildingFinished -= HandleBuildingFinished;
			_model.Builder.OnBuildingProgress -= HandleBuildingProgress;
		}
	}
}
