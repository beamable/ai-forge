using Beamable.Common.Content.Validation;
using Beamable.Content;
using Beamable.Content.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Common.Content
{
	[Serializable]
	public class OptionalSchedule : Optional<Schedule> { }

	[Serializable]
	public class OptionalListingSchedule : Optional<ListingSchedule> { }

	[Serializable]
	public class OptionalEventSchedule : Optional<EventSchedule> { }

	[Serializable]
	public class EventSchedule : Schedule { }

	[Serializable]
	public class ListingSchedule : Schedule { }

	[Serializable]
	public class Schedule
	{
		public bool IsPeriod => definitions.Any(def =>
													(def.hour.Contains("*") && !def.minute.Contains("*")) ||
													(!def.hour.Contains("*") && def.minute.Contains("*")) ||
													(!def.hour.Contains("*") && !def.minute.Contains("*")));

		public string description;

		[MustBeDateString]
		public string activeFrom = DateTime.UtcNow.ToString(DateUtility.ISO_FORMAT);

		[MustBeDateString]
		public OptionalString activeTo = new OptionalString();

		public List<ScheduleDefinition> definitions = new List<ScheduleDefinition>();


		public void AddDefinition(ScheduleDefinition definition)
		{
			definitions.Add(definition);
		}

		public void AddDefinitions(List<ScheduleDefinition> definitions)
		{
			this.definitions.AddRange(definitions);
		}
	}

	[Serializable]
	public class ScheduleDefinition
	{
		[IgnoreContentField]
		public Action<ScheduleDefinition> OnCronRawSaveButtonPressed;

		[IgnoreContentField]
		public Action<Schedule> OnScheduleModified;

		[HideInInspector]
		[IgnoreContentField]
		public int index = -1;

		[TextArea(2, 5)]
		[IgnoreContentField]
		public string cronRawFormat;

		[TextArea]
		[IgnoreContentField]
		public string cronHumanFormat;

		[HideInInspector]
		public List<string> second;
		[HideInInspector]
		public List<string> minute;
		[HideInInspector]
		public List<string> hour;
		[HideInInspector]
		public List<string> dayOfMonth;
		[HideInInspector]
		public List<string> month;
		[HideInInspector]
		public List<string> year;
		[HideInInspector]
		public List<string> dayOfWeek;

		public ScheduleDefinition() { }

		public ScheduleDefinition(string second, string minute, string hour, List<string> dayOfMonth, string month, string year, List<string> dayOfWeek)
		{
			this.second = new List<string> { second };
			this.minute = new List<string> { minute };
			this.hour = new List<string> { hour };
			this.dayOfMonth = new List<string>(dayOfMonth);
			this.month = new List<string> { month };
			this.year = new List<string> { year };
			this.dayOfWeek = new List<string>(dayOfWeek);
		}
		public ScheduleDefinition(List<string> second, List<string> minute, List<string> hour, List<string> dayOfMonth, List<string> month, List<string> year, List<string> dayOfWeek)
		{
			this.second = new List<string>(second);
			this.minute = new List<string>(minute);
			this.hour = new List<string>(hour);
			this.dayOfMonth = new List<string>(dayOfMonth);
			this.month = new List<string>(month);
			this.year = new List<string>(year);
			this.dayOfWeek = new List<string>(dayOfWeek);
		}
	}


}
