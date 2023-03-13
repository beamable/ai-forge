using Beamable.Common;
using Beamable.Common.Api;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Experimental.Common.Api.Calendars
{
	/// <summary>
	/// This type defines the %Client main entry point for the %Calendars feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public interface ICalendarApi : ISupportsGet<CalendarView>
	{
		Promise<EmptyResponse> Claim(string calendarId);
	}

	/// <summary>
	/// This type defines the %CalendarQueryResponse for the %CalendarsService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Experimental.Api.Calendars.CalendarsService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class CalendarQueryResponse
	{
		public List<CalendarView> calendars;

		public void Init()
		{
			// Set the absolute timestamps for when state changes
			foreach (var calendar in calendars)
			{
				calendar.Init();
			}
		}
	}

	/// <summary>
	/// This type defines the %CalendarView for the %CalendarsService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Experimental.Api.Calendars.CalendarsService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class CalendarView
	{
		public string id;
		public List<RewardCalendarDay> days;
		public int nextIndex;
		public long remainingSeconds;
		public long nextClaimSeconds;
		public DateTime nextClaimTime;
		public DateTime endTime;

		public void Init()
		{
			nextClaimTime = DateTime.UtcNow.AddSeconds(nextClaimSeconds);
			endTime = DateTime.UtcNow.AddSeconds(remainingSeconds);
		}
	}

	/// <summary>
	/// This type defines the %RewardCalendarDay for the %CalendarsService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Experimental.Api.Calendars.CalendarsService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class RewardCalendarDay
	{
		[HideInInspector] // this is a legacy entitlements setup we don't support
		[Obsolete]
		public List<RewardCalendarObtain> obtain;
	}

	/// <summary>
	/// This type defines the %RewardCalendarObtain for the %CalendarsService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Experimental.Api.Calendars.CalendarsService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[Serializable]
	public class RewardCalendarObtain
	{
		public string symbol;
		public string specialization;
		public string action;
		public int quantity;
	}
}
