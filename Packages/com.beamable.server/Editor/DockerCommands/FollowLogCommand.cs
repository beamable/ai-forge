using Beamable.Editor;
using Beamable.Editor.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Server.Editor.DockerCommands
{
	public class FollowLogCommand : DockerCommand
	{
		private readonly IDescriptor _descriptor;

		public string ContainerName { get; }

		public FollowLogCommand(IDescriptor descriptor) : this(descriptor, descriptor.ContainerName)
		{

		}

		public FollowLogCommand(IDescriptor descriptor, string containerName)
		{
			_descriptor = descriptor;
			ContainerName = containerName;
			UnityLogLabel = null;
		}

		protected override void HandleStandardOut(string data)
		{

			CheckFallbackTime(ref data, out var fallbackTime);
			if (!MicroserviceLogHelper.HandleLog(_descriptor, UnityLogLabel, data, fallbackTime, _standardOutProcessors))
			{
				base.HandleStandardOut(data);
			}
		}

		protected override void HandleStandardErr(string data)
		{

			// the logs will always start with a timestamp, we just don't know how long it will be.

			CheckFallbackTime(ref data, out var fallbackTime);
			if (!MicroserviceLogHelper.HandleLog(_descriptor, UnityLogLabel, data, fallbackTime, _standardErrProcessors))
			{
				base.HandleStandardErr(data);
			}
		}

		private void CheckFallbackTime(ref string data, out DateTime fallbackTime)
		{
			fallbackTime = default;
			if (data != null)
			{
				var firstSpace = data.IndexOf(' ');
				if (firstSpace > 0)
				{
					var timestampStr = data.Substring(0, firstSpace);
					if (DateTime.TryParse(timestampStr, out fallbackTime))
					{
						data = data.Substring(firstSpace + 1); // consume the space.
					}
				}
			}
		}

		public override string GetCommandString()
		{
			/*
			 * The logs command is meant to retrieve logs from a container that was started during a different compilation pass.
			 * For example, start a microservice, then edit some client code, then go back to unity, then cause some service logs...
			 * You'd expect to see those logs.
			 *
			 * That works!
			 *
			 * But here is an issue, because of hot-reload, if you edit the microservice code itself...
			 * then Unity will do a recompile, and this follow command _won't_ run until Unity has finished compiling.
			 * Unity is slow, and the dotnet watch command is much faster, so the logs that represent the microservice
			 * recompiling will have finished by the time Unity runs this command.
			 *
			 * Which means, if we use a "--since 0m" flag (like we did in 1.2.6 and before), we'll MISS the important
			 * logs saying the service is alive. It is alive, it'll just look sort of STUCK, because there are missing logs :/
			 * Non deterministic missing logs are the worst kind.
			 *
			 *
			 * So to fix that, we can use a "--since Nm" where N is some "reasonable" amount of time that a compile should take.
			 * This will give us duplicate logs, so the logging window needs to be smart enough not to re-render duplicated logs.
			 * We can achieve that by using the timestamp on the log itself. Don't render _old_ logs.
			 */
			return $"{DockerCmd} logs {ContainerName} -f -t --since 2m"; // a compile longer than 2m... sad.
		}
	}
}
