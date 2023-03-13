using System;

namespace Beamable.Common.Content
{
	public class ContentCorruptedException : Exception
	{
		public ContentCorruptedException(string message) : base(message)
		{

		}
	}
}

