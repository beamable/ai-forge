using Beamable.Common;
using System;

namespace Beamable.Editor.Tests
{
	public class MockLogProvider : BeamableLogProvider
	{
		public Action<string, object[]> onInfo;
		public Action<string> onWarning;
		public Action<Exception> onException;

		public override void Info(string message)
		{
			onInfo?.Invoke(message, new object[] { });
		}

		public override void Info(string message, params object[] args)
		{
			onInfo?.Invoke(message, args);
		}

		public override void Warning(string message)
		{
			onWarning?.Invoke(message);
		}

		public override void Warning(string message, params object[] args)
		{
			throw new NotImplementedException();
		}

		public override void Error(Exception ex)
		{
			onException?.Invoke(ex);
		}

		public override void Error(string error)
		{
			onException?.Invoke(new Exception(error));
		}

		public override void Error(string error, params object[] args)
		{
			onException?.Invoke(new Exception(error));
		}
	}
}
