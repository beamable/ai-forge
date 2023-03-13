using Beamable.Common;
using Beamable.Editor.UI.Model;
using Beamable.Server.Editor.CodeGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using static Beamable.Common.Constants.Features.Docker;
using Debug = UnityEngine.Debug;

namespace Beamable.Server.Editor.DockerCommands
{
	public class BuildImageCommand : DockerCommandReturnable<Unit>
	{
		private const string BUILD_PREF = "{0}BuildAtLeastOnce";
		private MicroserviceDescriptor _descriptor;
		private readonly bool _pull;
		private readonly CPUArchitectureContext _cpuContext;
		public bool IncludeDebugTools { get; }
		public string ImageName { get; set; }
		public string BuildPath { get; set; }

		public Promise<Unit> ReadyForExecution { get; private set; }

		private Exception _constructorEx;
		private List<string> _availableArchitectures;

		public static bool WasEverBuildLocally(IDescriptor descriptor)
		{
			return EditorPrefs.GetBool(string.Format(BUILD_PREF, descriptor.Name), false);
		}

		static void SetAsBuild(IDescriptor descriptor, bool build = true)
		{
			EditorPrefs.SetBool(string.Format(BUILD_PREF, descriptor.Name), build);
		}

		public BuildImageCommand(MicroserviceDescriptor descriptor, List<string> availableArchitectures, bool includeDebugTools, bool watch, bool pull = true, CPUArchitectureContext cpuContext = CPUArchitectureContext.LOCAL)
		{
			_descriptor = descriptor;
			_availableArchitectures = availableArchitectures;
			_pull = pull;
			_cpuContext = cpuContext;
			IncludeDebugTools = includeDebugTools;
			ImageName = descriptor.ImageName;
			BuildPath = descriptor.BuildPath;
			UnityLogLabel = "[BUILD]";
			ReadyForExecution = new Promise<Unit>();
			// copy the cs files from the source path to the build path
			// build the Program file, and place it in the temp dir.
			BuildUtils.PrepareBuildContext(descriptor, includeDebugTools, watch);

			MapDotnetCompileErrors();
		}

		protected override void ModifyStartInfo(ProcessStartInfo processStartInfo)
		{
			base.ModifyStartInfo(processStartInfo);
			processStartInfo.EnvironmentVariables["DOCKER_BUILDKIT"] = MicroserviceConfiguration.Instance.DisableDockerBuildkit ? "0" : "1";
			processStartInfo.EnvironmentVariables["DOCKER_SCAN_SUGGEST"] = "false";
		}

		public string GetProcessArchitecture()
		{
			var preferred = MicroserviceConfiguration.Instance.GetCPUArchitecture(_cpuContext);
			if (string.IsNullOrEmpty(preferred)) return preferred;

			// the system needs to be able to build this architecture...
			if (!_availableArchitectures.Contains(preferred))
			{
				// otherwise, get rid of the preference, and use the system's default.
				var warning =
					$@"Your machine cannot build docker images to {preferred}. The available builders are {string.Join(",", CPU_SUPPORTED)}. Defaulting to host architecture...
You can install more builders manually by using `docker buildx create --name beamable-builder --driver docker-container --platform linux/arm64,linux/arm/v8,linux/amd64`,
and then setting the beamable-builder as the default docker builder.";
				MicroserviceLogHelper.HandleLog(_descriptor, LogLevel.WARNING, warning, Color.white, false, null);
				preferred = null;
			}

			return preferred;
		}

		public override string GetCommandString()
		{
			var pullStr = _pull ? "--pull" : "";
#if BEAMABLE_DEVELOPER
			pullStr = ""; // we cannot force the pull against the local image.
#endif
			var platformStr = "";

#if !BEAMABLE_DISABLE_AMD_MICROSERVICE_BUILDS
			var arch = GetProcessArchitecture();
			platformStr = string.IsNullOrEmpty(arch)
				? ""
				: $"--platform {arch}";
#endif

			return $"{DockerCmd} build {pullStr} {platformStr} --label \"beamable-service-name={_descriptor.Name}\" -t {ImageName} \"{BuildPath}\" ";
		}

		protected override void HandleStandardOut(string data)
		{
			if (string.IsNullOrEmpty(data) || !MicroserviceLogHelper.HandleLog(_descriptor, UnityLogLabel, data, logProcessor: _standardOutProcessors))
			{
				base.HandleStandardOut(data);
			}
			OnStandardOut?.Invoke(data);
		}

		protected override void HandleStandardErr(string data)
		{
			if (string.IsNullOrEmpty(data) || !MicroserviceLogHelper.HandleLog(_descriptor, UnityLogLabel, data, logProcessor: _standardErrProcessors))
			{
				base.HandleStandardErr(data);
			}
			OnStandardErr?.Invoke(data);
		}

		protected override void Resolve()
		{
			var success = _exitCode == 0;
			SetAsBuild(_descriptor, success);
			if (success)
			{
				Promise.CompleteSuccess(PromiseBase.Unit);
			}
			else
			{
				Promise.CompleteError(new Exception($"Build failed err=[{StandardErrorBuffer}]"));
			}
		}
	}
}
