using Beamable.Common;
using Beamable.Common.Api;

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
	public abstract class AbsCalendarApi : ICalendarApi
	{
		protected const string SERVICE_NAME = "calendars";
		public IBeamableRequester Requester { get; }
		public IUserContext Ctx { get; }

		public AbsCalendarApi(IBeamableRequester requester, IUserContext ctx)
		{
			Requester = requester;
			Ctx = ctx;
		}

		public virtual Promise<EmptyResponse> Claim(string calendarId)
		{
			return Requester.Request<EmptyResponse>(
			   Method.POST,
			   $"/object/calendars/{Ctx.UserId}/claim?id={calendarId}"
			);
		}

		public abstract Promise<CalendarView> GetCurrent(string scope = "");
	}
}
