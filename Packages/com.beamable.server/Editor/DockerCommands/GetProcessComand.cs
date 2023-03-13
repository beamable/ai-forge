using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Server.Editor.DockerCommands
{
	public class GetProcessComand : DockerCommandReturnable<string>
	{
		private int processId;
		public string _result;

		public GetProcessComand(int processId)
		{
			this.processId = processId;
		}

		public override string GetCommandString()
		{
#if UNITY_EDITOR && !UNITY_EDITOR_WIN
			return $"ps -p {processId}";
#else
			return $"wmic.exe path Win32_Process where handle='{processId}' get Commandline";
#endif
		}

		protected override void HandleStandardOut(string data)
		{
			if (!string.IsNullOrEmpty(data))
				_result = data.ToLower();
			base.HandleStandardOut(data);
		}

		protected override void Resolve()
		{
			Promise.CompleteSuccess(_result);
		}
	}
}
