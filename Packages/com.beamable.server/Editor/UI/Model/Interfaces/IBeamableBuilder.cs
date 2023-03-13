using System;
using System.Threading.Tasks;

namespace Beamable.Editor.UI.Model
{
	public interface IBeamableBuilder
	{
		Action<bool> OnIsRunningChanged { get; set; }
		Action<int, int> OnBuildingProgress { get; set; }
		Action<int, int> OnStartingProgress { get; set; }
		Action<bool> OnStartingFinished { get; set; }
		Action<bool> OnBuildingFinished { get; set; }
		bool IsRunning { get; set; }
		Task CheckIfIsRunning();
		Task TryToStart();
		Task TryToStop();
		Task TryToRestart();
	}
}
