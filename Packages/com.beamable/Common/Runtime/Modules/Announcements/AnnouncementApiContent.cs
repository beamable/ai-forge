using Beamable.Common.Content;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Beamable.Common.Announcements
{
	[ContentType("announcementApi")]
	[Serializable]
	public class AnnouncementApiContent : ApiContent
	{
		protected sealed override ApiVariable[] GetVariables()
		{
			return new[]
			{
			 new ApiVariable {Name = "announcementId", TypeName = ApiVariable.TYPE_STRING}
		 };
		}
	}

	[Serializable]
	public class AnnouncementApiRef : ApiRef<AnnouncementApiContent> { }

	[Serializable]
	public class AnnouncementApiReward : ApiReward<AnnouncementApiContent, AnnouncementApiRef> { }

	[Serializable]
	public class ListOfAnnouncementApi : DisplayableList<AnnouncementApiReward>
	{
		public List<AnnouncementApiReward> listData = new List<AnnouncementApiReward>();

		protected override IList InternalList => listData;
		public override string GetListPropertyPath() => nameof(listData);
	}

	[Serializable]
	public class OptionalListOfAnnouncementRewards : Optional<ListOfAnnouncementApi> { }

}
