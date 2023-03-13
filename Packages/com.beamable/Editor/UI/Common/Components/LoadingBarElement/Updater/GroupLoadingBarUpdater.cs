using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Beamable.Editor.UI.Components
{
	public class VirtualLoadingBar : ILoadingBar
	{
		public float Progress { get; private set; }
		public string Message { get; private set; }
		public bool Failed { get; private set; }
		public void UpdateProgress(float progress, string message = null, bool failed = false, bool hideOnFinish = false)
		{
			Progress = progress;
			Message = message;
			Failed = failed;
			OnUpdated?.Invoke();
		}

		public void SetUpdater(LoadingBarUpdater updater)
		{
		}

		public event Action OnUpdated;
	}

	public class GroupLoadingBarUpdater : LoadingBarUpdater
	{
		private readonly bool _singleSteps;
		private List<LoadingBarUpdater> _children;
		public override string ProcessName { get; }

		public GroupLoadingBarUpdater(string processName, ILoadingBar loadingBar, bool singleSteps, params LoadingBarUpdater[] children) : base(loadingBar)
		{
			_singleSteps = singleSteps;
			_children = children.ToList();
			ProcessName = processName;

			foreach (var child in _children)
			{
				if (child == null)
					continue;

				if (child.LoadingBar != null)
					child.LoadingBar.OnUpdated += HandleUpdates;

				child.OnKilledEvent += HandleUpdates;
			}
		}

		private float GetActualProgress()
		{
			if (_children.Count == 0)
			{
				return 0f;
			}

			var completedSteps = _children.Sum(l => l.Step);
			var totalStepsToDo = _children.Sum(l => l.TotalSteps);
			return completedSteps / (float)totalStepsToDo;
		}

		void HandleUpdates()
		{
			float actualProgress;

			if (_singleSteps)
			{
				Step = _children.Count(lb => lb.Succeeded || lb.Killed);
				TotalSteps = _children.Count;
				actualProgress = Step / (float)TotalSteps;
			}
			else
			{
				actualProgress = GetActualProgress();
				Step = _children.Sum(lb => lb.Killed ? lb.TotalSteps : lb.Step);
				TotalSteps = _children.Sum(lb => lb.TotalSteps);
			}
			Succeeded = _children.All(lb => lb.Succeeded || lb.Killed);
			Succeeded |= Step > TotalSteps;
			var errors = _children.Count(lb => lb.GotError);
			GotError = errors > 0;

			if (Succeeded)
			{
				LoadingBar.UpdateProgress(1f, $"(Success: {ProcessName})");
			}
			else
			{
				string errorMessage = "";
				if (GotError)
				{
					errorMessage = $" Errors: {errors}";
					actualProgress = 0f;
				}
				LoadingBar.UpdateProgress(actualProgress, $"({ProcessName} {StepText}{errorMessage})", GotError);
			}

			EditorApplication.delayCall += () =>
			{
				if (GotError || _children.All(lb => lb.Killed || Succeeded))
				{
					Kill();
				}
			};
		}

		protected override void OnKill()
		{
			foreach (var child in _children)
			{
				if (child.LoadingBar != null)
					child.LoadingBar.OnUpdated -= HandleUpdates;

				child.OnKilledEvent -= HandleUpdates;
			}
		}
	}
}
