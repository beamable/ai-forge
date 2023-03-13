using System;

namespace Beamable.Editor.UI.Components
{
	public interface ILoadingBar
	{
		float Progress { get; }
		string Message { get; }
		bool Failed { get; }
		void UpdateProgress(float progress, string message = null, bool failed = false, bool hideOnFinish = false);
		void SetUpdater(LoadingBarUpdater updater);

		event Action OnUpdated;
	}
}
