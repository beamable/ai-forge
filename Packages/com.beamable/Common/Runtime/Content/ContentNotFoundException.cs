using System;

namespace Beamable.Common.Content
{
	public class ContentNotFoundException : Exception
	{
		public ContentNotFoundException(string contentId = "unknown") : base($"Content reference not found with ID: '{contentId}' ")
		{

		}

		public ContentNotFoundException(Type type) : base($"No content name found for type=[{type.Name}]")
		{

		}
	}
}
