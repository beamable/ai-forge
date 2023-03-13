using System;
using System.Collections.Generic;

namespace Beamable.Common.Content
{
	[Serializable]
	public class ContentDataInfo
	{
		public string contentId;
		public string data;
	}

	[Serializable]
	public class ContentDataInfoWrapper
	{
		public List<ContentDataInfo> content = new List<ContentDataInfo>();
	}
}
