using Beamable.Common.Assistant;
using Beamable.Editor.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Beamable.Server.Editor.DockerCommands
{
	public class DockerStatus
	{
		public List<string> runningServices = new List<string>();
		public List<uint> usedPorts = new List<uint>();
	}

	public class GetDockerLocalStatus : DockerCommandReturnable<DockerStatus>
	{
		private static string ipPattern =
			@"\b((([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(\.)){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]))\b";

		private static readonly Regex ipRegex = new Regex(ipPattern);
		private readonly List<MicroserviceModel> _services;
		private readonly List<MongoStorageModel> _storages;
		private DockerStatus _status;
		private bool _overlapingPorts;

		public GetDockerLocalStatus()
		{
			_services = MicroservicesDataModel.Instance.Services;
			_storages = MicroservicesDataModel.Instance.Storages;
			_status = new DockerStatus();
			_overlapingPorts = false;
		}

		public override string GetCommandString()
		{
			var command = $"{DockerCmd} ps";
			return command;
		}

		protected override void HandleStandardOut(string data)
		{
			if (string.IsNullOrWhiteSpace(data))
				return;

			base.HandleStandardOut(data);

			bool Match(ServiceModelBase model)
			{
				return data.Contains($" {model.Descriptor.ContainerName}") && data.Contains(" Up ");
			}

			var serviceIndex =
				_services.FindIndex(Match);
			if (serviceIndex >= 0)
			{
				_status.runningServices.Add(_services[serviceIndex].Name);
				return;
			}

			serviceIndex = _storages.FindIndex(Match);
			if (serviceIndex >= 0 && _storages[serviceIndex]?.Config != null)
			{
				_status.runningServices.Add(_storages[serviceIndex].Name);
				_status.usedPorts.Add(_storages[serviceIndex].Config.LocalDataPort);
				_status.usedPorts.Add(_storages[serviceIndex].Config.LocalUIPort);
				return;
			}

			if (ipRegex.IsMatch(data) && data.Contains(" Up "))
			{
				var ipPart = data.Split(' ').FirstOrDefault(ipRegex.IsMatch);

				if (string.IsNullOrEmpty(ipPart))
					return;

				var startIndex = ipRegex.Matches(data)[0].Length + 1;
				var length = ipPart.IndexOf("->", StringComparison.Ordinal) - startIndex;
				var substring = ipPart.Substring(startIndex, length);

				if (uint.TryParse(substring, out var usedPort))
				{
					_status.usedPorts.Add(usedPort);
					var modelWithSamePort =
						_storages.FirstOrDefault(model => model.Config?.LocalDataPort == usedPort ||
														  model.Config?.LocalUIPort == usedPort);
					_overlapingPorts |= modelWithSamePort != null;
				}
			}
		}

		protected override void Resolve()
		{
			var globalHintStorage = BeamEditor.HintGlobalStorage;
			if (!DockerNotInstalled && _overlapingPorts)
				globalHintStorage.AddOrReplaceHint(BeamHintType.Validation,
												   BeamHintDomains.BEAM_CSHARP_MICROSERVICES_DOCKER,
												   BeamHintIds.ID_DOCKER_OVERLAPPING_PORTS);
			else
				globalHintStorage.RemoveHint(new BeamHintHeader(BeamHintType.Validation,
																BeamHintDomains.BEAM_CSHARP_MICROSERVICES_DOCKER,
																BeamHintIds.ID_DOCKER_OVERLAPPING_PORTS));
			Promise.CompleteSuccess(_status);
		}
	}
}
