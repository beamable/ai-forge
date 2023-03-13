using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Dependencies;
using Beamable.Experimental.Common.Api.Calendars;

namespace Beamable.Experimental.Api.Calendars
{
	/// <summary>
	/// This type defines the %CalendarsSubscription for the %CalendarsService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Experimental.Api.Calendars.CalendarsService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class CalendarsSubscription : PlatformSubscribable<CalendarQueryResponse, CalendarView>
	{
		public CalendarsSubscription(IDependencyProvider provider, string service) : base(provider, service)
		{
		}

		public void ForceRefresh(string scope)
		{
			Refresh(scope);
		}

		protected override void OnRefresh(CalendarQueryResponse data)
		{
			data.Init();

			foreach (var calendar in data.calendars)
			{
				// Schedule the next callback
				var seconds = long.MaxValue;
				if (calendar.nextClaimSeconds != 0 && calendar.nextClaimSeconds < seconds)
				{
					seconds = calendar.nextClaimSeconds;
				}

				if (calendar.remainingSeconds != 0 && calendar.remainingSeconds < seconds)
				{
					seconds = calendar.remainingSeconds;
				}

				if (seconds > 0)
				{
					ScheduleRefresh(seconds, calendar.id);
				}

				Notify(calendar.id, calendar);
			}
		}
	}

	/// <summary>
	/// This type defines the %Client main entry point for the %Calendars feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/calendars-feature">Calendars</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class CalendarsService : AbsCalendarApi, IHasPlatformSubscriber<CalendarsSubscription, CalendarQueryResponse, CalendarView>
	{

		public CalendarsSubscription Subscribable { get; }

		public CalendarsService(IPlatformService platform, IBeamableRequester requester, IDependencyProvider provider) : base(requester, platform)
		{
			Subscribable = new CalendarsSubscription(provider, SERVICE_NAME);
		}

		public override Promise<EmptyResponse> Claim(string calendarId)
		{
			return base.Claim(calendarId).Then(claimRsp => { Subscribable.ForceRefresh(calendarId); });
		}

		public override Promise<CalendarView> GetCurrent(string scope = "") => Subscribable.GetCurrent(scope);

		//      public Promise<EmptyResponse> Claim(string calendarId)
		//      {
		//         return requester.Request<EmptyResponse>(
		//            Method.POST,
		//            $"/object/calendars/{platform.User.id}/claim?id={calendarId}"
		//         ).Then(claimRsp => { Refresh(calendarId); });
		//      }

		//      protected override void OnRefresh(CalendarQueryResponse data)
		//      {
		//         data.Init();
		//
		//         foreach (var calendar in data.calendars)
		//         {
		//            // Schedule the next callback
		//            var seconds = long.MaxValue;
		//            if (calendar.nextClaimSeconds != 0 && calendar.nextClaimSeconds < seconds)
		//            {
		//               seconds = calendar.nextClaimSeconds;
		//            }
		//
		//            if (calendar.remainingSeconds != 0 && calendar.remainingSeconds < seconds)
		//            {
		//               seconds = calendar.remainingSeconds;
		//            }
		//
		//            if (seconds > 0)
		//            {
		//               ScheduleRefresh(seconds, calendar.id);
		//            }
		//
		//            Notify(calendar.id, calendar);
		//         }
		//      }
	}

}
