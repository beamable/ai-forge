using Beamable.Editor.UI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Editor.Toolbox.Models
{
	public interface IWidgetSource
	{
		int Count { get; }
		Widget Get(int index);

		// TODO: Add some sort of update event to allow for network backed systems
	}

	public class EmptyWidgetSource : IWidgetSource
	{
		public int Count => 0;
		public Widget Get(int index)
		{
			throw new NotImplementedException();
		}

	}

	public class WidgetSource : ScriptableObject, IWidgetSource
	{
		[SerializeField]
		private List<Widget> _widgets = new List<Widget>();

		public int Count => _widgets.Count;
		public Widget Get(int index)
		{
			return _widgets[index];
		}
	}

	[System.Serializable]
	public class Widget
	{
		public string Name;
		public string Description;
		[EnumFlags]
		public WidgetOrientationSupport OrientationSupport;
		[EnumFlags]
		public WidgetTags Tags;
		public Texture Icon;
		public GameObject Prefab;
		[EnumFlags]
		public SupportStatus Support;
	}

	[Flags]
	[System.Serializable]
	public enum SupportStatus
	{
		OBSOLETE = 1,
		EXPERIMENTAL = 2,
	}

	[Flags]
	[System.Serializable]
	public enum WidgetOrientationSupport
	{
		PORTRAIT = 1,
		LANDSCAPE = 2
	}

	[Flags]
	[System.Serializable]
	public enum WidgetTags
	{
		FLOW = 1,
		COMPONENT = 1 << 1,
		ACCOUNTS = 1 << 2,
		SHOP = 1 << 3,
		INVENTORY = 1 << 4,
		CURRENCY = 1 << 5,
		LEADERBOARDS = 1 << 6,
		ADMIN = 1 << 7,
		ANNOUNCEMENTS = 1 << 8,
	}

	public static class WidgetOrientationSupportExtensions
	{
		static Dictionary<WidgetOrientationSupport, string> enumToString = new Dictionary<WidgetOrientationSupport, string>
	  {
		 {WidgetOrientationSupport.PORTRAIT, "portrait"},
		 {WidgetOrientationSupport.LANDSCAPE, "landscape"},
	  };
		static Dictionary<string, WidgetOrientationSupport> stringToEnum = new Dictionary<string, WidgetOrientationSupport>();

		static WidgetOrientationSupportExtensions()
		{
			foreach (var kvp in enumToString)
			{
				stringToEnum.Add(kvp.Value, kvp.Key);
			}
		}
		public static string Serialize(this WidgetOrientationSupport self)
		{
			var str = self.ToString();
			foreach (var kvp in stringToEnum)
			{
				str = str.Replace(kvp.Value.ToString(), kvp.Key);
			}
			str = str.Replace(",", "");
			return str;
		}

		public static bool TryParse(string str, out WidgetOrientationSupport status)
		{
			var parts = str.Split(new[] { ' ' }, StringSplitOptions.None);
			status = WidgetOrientationSupport.PORTRAIT;

			var any = false;
			foreach (var part in parts)
			{
				if (stringToEnum.TryGetValue(part, out var subStatus))
				{
					if (!any)
					{
						status = subStatus;
					}
					else
					{
						status |= subStatus;
					}
					any = true;
				}
			}
			return any;
		}
	}

	public static class WidgetTagExtensions
	{
		static Dictionary<WidgetTags, string> enumToString = new Dictionary<WidgetTags, string>
		{
		};
		static Dictionary<string, WidgetTags> stringToEnum = new Dictionary<string, WidgetTags>();

		static WidgetTagExtensions()
		{
			Enum.GetValues(typeof(WidgetTags)).Cast<WidgetTags>().ToList().ForEach(tag =>
			{
				enumToString.Add(tag, tag.ToString().ToLower());
			});
			foreach (var kvp in enumToString)
			{
				stringToEnum.Add(kvp.Value, kvp.Key);
			}
		}
		public static bool TryParse(string raw, out WidgetTags tags)
		{
			return Enum.TryParse(raw, true, out tags);
		}

		public static string Serialize(this WidgetTags widget)
		{
			var str = widget.ToString();
			foreach (var kvp in stringToEnum)
			{
				str = str.Replace(kvp.Value.ToString(), kvp.Key);
			}
			str = str.Replace(",", "");
			return str;
		}
	}

	public static class WidgetStatusExtensions
	{
		static Dictionary<SupportStatus, string> enumToString = new Dictionary<SupportStatus, string>
		{
		};
		static Dictionary<string, SupportStatus> stringToEnum = new Dictionary<string, SupportStatus>();
		static WidgetStatusExtensions()
		{
			Enum.GetValues(typeof(SupportStatus)).Cast<SupportStatus>().ToList().ForEach(tag =>
			{
				enumToString.Add(tag, tag.ToString().ToLower());
			});
			foreach (var kvp in enumToString)
			{
				stringToEnum.Add(kvp.Value, kvp.Key);
			}
		}

		public static bool TryParse(string raw, out SupportStatus tags)
		{
			return Enum.TryParse(raw, true, out tags);
		}

		public static string Serialize(this SupportStatus widget)
		{
			var str = widget.ToString();
			foreach (var kvp in stringToEnum)
			{
				str = str.Replace(kvp.Value.ToString(), kvp.Key);
			}
			str = str.Replace(",", "");
			return str;
		}
	}
}
