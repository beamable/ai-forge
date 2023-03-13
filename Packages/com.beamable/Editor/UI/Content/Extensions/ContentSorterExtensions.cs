using Beamable.Editor.Content.Helpers;
using Beamable.Editor.Content.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Editor.Content.Extensions
{
	public static class ContentSorterExtensions
	{
		public static List<ContentItemDescriptor> Sort(this List<ContentItemDescriptor> contentItems,
			ContentSortType contentSortType = ContentSortType.IdAZ)
		{
			if (contentItems == null)
				return null;

			if (contentItems.Count == 0)
				return contentItems;

			switch (contentSortType)
			{
				case ContentSortType.IdAZ:
					contentItems.Sort(new ContentComparer_Alphabetical());
					break;
				case ContentSortType.IdZA:
					contentItems.Sort(new ContentComparer_ReverseAlphabetical());
					break;
				case ContentSortType.TypeAZ:
					contentItems.Sort(new ContentComparer_TypeAlphabetical());
					break;
				case ContentSortType.TypeZA:
					contentItems.Sort(new ContentComparer_ReverseTypeAlphabetical());
					break;
				// case ContentSortType.PublishedDate:
				// 	Debug.LogWarning("NOT IMPLEMENTED");
				// 	break;
				case ContentSortType.RecentlyUpdated:
					contentItems.Sort(new ContentComparer_RecentlyUpdated());
					break;
				case ContentSortType.Status:
					contentItems.Sort(new ContentComparer_Status());
					break;
			}

			return contentItems;
		}

		private class ContentComparer_Alphabetical : IComparer<ContentItemDescriptor>
		{
			public int Compare(ContentItemDescriptor go1, ContentItemDescriptor go2)
			{
				return String.CompareOrdinal(go1.Name, go2.Name);
			}
		}

		private class ContentComparer_ReverseAlphabetical : IComparer<ContentItemDescriptor>
		{
			public int Compare(ContentItemDescriptor go1, ContentItemDescriptor go2)
			{
				return -String.CompareOrdinal(go1.Name, go2.Name);
			}
		}

		private class ContentComparer_TypeAlphabetical : IComparer<ContentItemDescriptor>
		{
			public int Compare(ContentItemDescriptor go1, ContentItemDescriptor go2)
			{
				return String.CompareOrdinal(go1.ContentType.TypeName, go2.ContentType.TypeName);
			}
		}

		private class ContentComparer_ReverseTypeAlphabetical : IComparer<ContentItemDescriptor>
		{
			public int Compare(ContentItemDescriptor go1, ContentItemDescriptor go2)
			{
				return -String.CompareOrdinal(go1.ContentType.TypeName, go2.ContentType.TypeName);
			}
		}

		private class ContentComparer_RecentlyUpdated : IComparer<ContentItemDescriptor>
		{
			public int Compare(ContentItemDescriptor go1, ContentItemDescriptor go2)
			{
				return -go1.LastChanged.CompareTo(go2.LastChanged);
			}
		}

		private class ContentComparer_Status : IComparer<ContentItemDescriptor>
		{
			public int Compare(ContentItemDescriptor go1, ContentItemDescriptor go2)
			{
				return go1.Status.CompareTo(go2.Status);
			}
		}
	}
}
