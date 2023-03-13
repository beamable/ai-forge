using Beamable.Common.Content;
using System;

namespace Beamable.Editor.UI.Components
{
	public interface IScheduleWindow<TData>
	{
		event Action OnCancelled;
		event Action<Schedule> OnScheduleUpdated;
		void Set(Schedule schedule, TData data);
		void ApplyDataTransforms(TData data);
	}
}
