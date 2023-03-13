using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Server.Editor.DockerCommands
{
	public class GetBuildOutputArchitectureCommand : DockerCommandReturnable<List<string>>
	{
		public List<string> _result;

		public GetBuildOutputArchitectureCommand()
		{
			_result = new List<string>();
		}

		public override string GetCommandString()
		{
			return $"{DockerCmd} buildx ls";
		}

		protected override void HandleStandardOut(string data)
		{
			CheckAvailableArchitectures(data);
			base.HandleStandardOut(data);
		}

		protected override void HandleStandardErr(string data)
		{
			CheckAvailableArchitectures(data);
			base.HandleStandardErr(data);
		}

		void CheckAvailableArchitectures(string data)
		{
			if (data == null || !data.Contains("linux/"))
				return;

			var available = data.Split(' ');

			for (int i = 0; i < available.Length; i++)
			{
				if (!_result.Contains(available[i]) && available[i].Contains("linux/"))
					_result.Add(available[i].Replace(",", string.Empty));
			}
		}

		protected override void Resolve()
		{
			Promise.CompleteSuccess(_result);
		}
	}
}
