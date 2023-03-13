using System;
using System.Text;

namespace PubNubMessaging.Core
{
	class SharedStringBuilder
	{
		[ThreadStatic]
		private static StringBuilder _builder;
		public static StringBuilder Builder
		{
			get
			{
				if (_builder == null)
					_builder = new StringBuilder(128);
				else
					_builder.Length = 0;
				return _builder;

			}
		}
	}
}
