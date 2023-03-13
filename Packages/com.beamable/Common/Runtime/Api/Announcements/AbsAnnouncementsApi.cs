using System;
using System.Collections.Generic;

namespace Beamable.Common.Api.Announcements
{
	public abstract class AbsAnnouncementsApi : IAnnouncementsApi
	{
		public IBeamableRequester Requester { get; }
		public IUserContext Ctx { get; }

		protected AbsAnnouncementsApi(IBeamableRequester requester, IUserContext ctx)
		{
			Requester = requester;
			Ctx = ctx;
		}


		public Promise<EmptyResponse> MarkRead(string id)
		{
			return MarkRead(new List<string> { id });
		}

		public virtual Promise<EmptyResponse> MarkRead(List<string> ids)
		{
			return Requester.Request<EmptyResponse>(
			   Method.PUT,
			   String.Format("/object/announcements/{0}/read", Ctx.UserId),
			   new AnnouncementRequest(ids)
			);
		}

		public Promise<EmptyResponse> MarkDeleted(string id)
		{
			return MarkDeleted(new List<string> { id });
		}

		public virtual Promise<EmptyResponse> MarkDeleted(List<string> ids)
		{
			return Requester.Request<EmptyResponse>(
			   Method.DELETE,
			   String.Format("/object/announcements/{0}", Ctx.UserId),
			   new AnnouncementRequest(ids)
			);
		}

		public Promise<EmptyResponse> Claim(string id)
		{
			return Claim(new List<string> { id });
		}

		public virtual Promise<EmptyResponse> Claim(List<string> ids)
		{
			return Requester.Request<EmptyResponse>(
			   Method.POST,
			   String.Format("/object/announcements/{0}/claim", Ctx.UserId),
			   new AnnouncementRequest(ids)
			);
		}

		public abstract Promise<AnnouncementQueryResponse> GetCurrent(string scope = "");
	}
}
