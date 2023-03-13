using System;

namespace Beamable.Editor.UI.Components
{
	public abstract class LoadingBarUpdater
	{
		public ILoadingBar LoadingBar { get; protected set; }
		public int Step { get; protected set; }
		public int TotalSteps { get; protected set; }
		public bool Killed { get; private set; }
		public bool GotError { get; protected set; }
		public bool Succeeded { get; protected set; }

		public virtual string StepText => $"{Step}/{TotalSteps}";
		public abstract string ProcessName { get; }

		public event Action OnKilledEvent;

		public LoadingBarUpdater(ILoadingBar loadingBar)
		{
			LoadingBar = loadingBar;
			LoadingBar.SetUpdater(this);
		}

		public void Kill()
		{
			if (Killed) return;
			Killed = true;
			OnKill();
			OnKilledEvent?.Invoke();
		}

		protected abstract void OnKill();
	}
}
