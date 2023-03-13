using Beamable.Editor.UI.Components;
using Beamable.Server.Editor;
using Beamable.Server.Editor.UI.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Beamable.Common.Constants.Features.Services;

namespace Beamable.Editor.Microservice.UI.Components
{
	public class DeployLogParser : LoadingBarUpdater
	{
		public override string ProcessName => "Deploying";
		private readonly MicroserviceRecord[] _records;

		public DeployLogParser(ILoadingBar loadingBar, ManifestModel model, int totalSteps) : base(loadingBar)
		{
			_records = model.Services.Values
				.Select(m => new MicroserviceRecord(m.Name))
				.ToArray();
			TotalSteps = totalSteps;

			Application.logMessageReceived += HandleLog;
			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			serviceRegistry.OnDeployFailed += HandleDeployFailed;
			Update();
		}

		private void HandleDeployFailed(ManifestModel _model, string reason) => EndWithError(reason);

		void HandleLog(string logString, string stackTrace, LogType type)
		{
			foreach (var record in _records)
			{

				if (record.stateDict.ContainsKey(logString))
				{
					record.state = record.stateDict[logString];
					Update();
					return;
				}
			}
		}

		private void Update()
		{
			Step = 0;

			for (int i = 0; i < _records.Length; i++)
			{
				switch (_records[i].state)
				{
					case MicroserviceRecord.MicroserviceRecordState.Failure:
						EndWithError(_records[i].name);
						return;
					case MicroserviceRecord.MicroserviceRecordState.Deployed:
						Step++;
						break;
					default:
						break;
				}
			}

			if (TotalSteps == Step)
			{
				Succeeded = true;
				LoadingBar.UpdateProgress(1f, $"(Deployed)");
				Kill();
			}
			else if (TotalSteps > 0)
			{
				LoadingBar.UpdateProgress(Step / (float)TotalSteps, $"({ProcessName} {StepText})");
			}
		}

		private void EndWithError(string error)
		{
			Succeeded = false;
			LoadingBar.UpdateProgress(0f, $"(Deploy Error: {error})", true);
			Kill();
		}

		protected override void OnKill()
		{
			Application.logMessageReceived -= HandleLog;
		}

		private class MicroserviceRecord
		{

			public readonly string name;
			public MicroserviceRecordState state = MicroserviceRecordState.Pending;
			public Dictionary<string, MicroserviceRecord.MicroserviceRecordState> stateDict;

			public MicroserviceRecord(string name)
			{
				this.name = name;

				this.stateDict = new Dictionary<string, MicroserviceRecord.MicroserviceRecordState>()
				{
					{ string.Format(UPLOAD_CONTAINER_MESSAGE, name), MicroserviceRecord.MicroserviceRecordState.Deployed },
					{ string.Format(CONTAINER_ALREADY_UPLOADED_MESSAGE, name), MicroserviceRecord.MicroserviceRecordState.Deployed },
					{ string.Format(CANT_UPLOAD_CONTAINER_MESSAGE, name), MicroserviceRecord.MicroserviceRecordState.Failure },
				};
			}

			public enum MicroserviceRecordState
			{
				Pending = 0,
				Deployed = 1,
				Failure = 2
			}
		}
	}
}
