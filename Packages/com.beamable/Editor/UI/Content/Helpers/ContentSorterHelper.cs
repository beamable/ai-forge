using System;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Editor.Content.Helpers
{
	public static class ContentSorterHelper
	{
		private static readonly Dictionary<ContentSortType, string> _sorterOptions = new Dictionary<ContentSortType, string>
		{
			{ ContentSortType.IdAZ, "ID (A-Z)" },
			{ ContentSortType.IdZA, "ID (Z-A)" },
			{ ContentSortType.TypeAZ, "Type (A-Z)" },
			{ ContentSortType.TypeZA, "Type (Z-A)" },
			//{ ContentSortType.PublishedDate, "Published Date" },
			{ ContentSortType.RecentlyUpdated, "Recently updated" },
			{ ContentSortType.Status, "Status" }
		};

		public static List<ContentSortType> GetAllSorterOptions => _sorterOptions.Keys.ToList();

		public static string GetContentSorterTitle(ContentSortType type)
		{
			if (!_sorterOptions.ContainsKey(type))
				throw new ArgumentNullException($"Sorter options dict doesn't contains \"{type.ToString()}\" key");
			return _sorterOptions[type];
		}
	}

	public enum ContentSortType
	{
		IdAZ,
		IdZA,
		TypeAZ,
		TypeZA,
		//PublishedDate,
		Status,
		RecentlyUpdated
	}
}
