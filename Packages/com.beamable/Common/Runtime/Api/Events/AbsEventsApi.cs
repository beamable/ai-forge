using Beamable.Serialization.SmallerJSON;
using System.Collections.Generic;
using System.Text;

namespace Beamable.Common.Api.Events
{
	public abstract class AbsEventsApi : IEventsApi
	{
		public const string SERVICE_NAME = "event-players";

		public IBeamableRequester Requester { get; }
		public IUserContext Ctx { get; }

		protected AbsEventsApi(IBeamableRequester requester, IUserContext ctx)
		{
			Requester = requester;
			Ctx = ctx;
		}

		public virtual Promise<EventClaimResponse> Claim(string eventId)
		{
			return Requester.Request<EventClaimResponse>(
			   Method.POST,
			   $"/object/{SERVICE_NAME}/{Ctx.UserId}/claim?eventId={eventId}"
			);
		}

		public virtual Promise<Unit> SetScore(string eventId, double score, bool incremental = false, IDictionary<string, object> stats = null)
		{
			var payload = new ArrayDict
		 {
			{"eventId", eventId},
			{"score", score},
			{"increment", incremental},
			{"stats", new ArrayDict(stats)}
		 };

			return Requester.Request<Unit>(
			   Method.PUT,
			   $"/object/{SERVICE_NAME}/{Ctx.UserId}/score",
			   Json.Serialize(payload, new StringBuilder())
			);
		}

		public abstract Promise<EventsGetResponse> GetCurrent(string scope = "");
	}
}
