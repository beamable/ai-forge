using Beamable.Common.Content;
using System;

namespace Beamable.Editor.Schedules
{
	[Serializable]
	public class ScheduleWrapper
	{
		public Schedule schedule;

		public ScheduleWrapper(Schedule schedule)
		{
			this.schedule = schedule;
		}
	}
}
