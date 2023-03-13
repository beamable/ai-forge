using System;

namespace Beamable.Common.Content
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	public class ContentFieldAttribute : Attribute
	{
		public string SerializedName { get; set; }
		public string[] FormerlySerializedAs { get; set; }

		public ContentFieldAttribute()
		{

		}

		public ContentFieldAttribute(string name)
		{
			SerializedName = name;
		}

		public ContentFieldAttribute(string name = null, params string[] formerlySerializedAs)
		{
			SerializedName = name;
			FormerlySerializedAs = formerlySerializedAs;
		}

	}
}
