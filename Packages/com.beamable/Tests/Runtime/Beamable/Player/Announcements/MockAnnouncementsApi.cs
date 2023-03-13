using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Announcements;
using System.Collections.Generic;

namespace Beamable.Tests.Runtime.Player.Announcements
{
	public class MockAnnouncementsApi : IAnnouncementsApi
	{
		public Promise<AnnouncementQueryResponse> GetCurrent(string scope = "")
		{
			throw new System.NotImplementedException();
		}

		public Promise<EmptyResponse> MarkRead(string id)
		{
			throw new System.NotImplementedException();
		}

		public Promise<EmptyResponse> MarkRead(List<string> ids)
		{
			throw new System.NotImplementedException();
		}

		public Promise<EmptyResponse> MarkDeleted(string id)
		{
			throw new System.NotImplementedException();
		}

		public Promise<EmptyResponse> MarkDeleted(List<string> ids)
		{
			throw new System.NotImplementedException();
		}

		public Promise<EmptyResponse> Claim(string id)
		{
			throw new System.NotImplementedException();
		}

		public Promise<EmptyResponse> Claim(List<string> ids)
		{
			throw new System.NotImplementedException();
		}
	}
}
