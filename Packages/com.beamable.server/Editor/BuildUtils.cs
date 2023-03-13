using Beamable.Editor.UI.Model;
using Beamable.Server.Editor.CodeGen;
using Beamable.Server.Editor.DockerCommands;
using System;
using System.IO;
using UnityEngine;

namespace Beamable.Server.Editor
{
	public static class BuildUtils
	{
		public static void PrepareBuildContext(MicroserviceDescriptor descriptor, bool includeDebugTools, bool watch)
		{
			try
			{
				FileUtils.CleanBuildDirectory(descriptor);

				var dependencies = DependencyResolver.GetDependencies(descriptor);
				FileUtils.CopyAssemblies(descriptor, dependencies);
				FileUtils.CopyDlls(descriptor, dependencies);
				FileUtils.CopySingleFiles(descriptor, dependencies);

				var programFilePath = Path.Combine(descriptor.BuildPath, "Program.cs");
				var csProjFilePath = Path.Combine(descriptor.BuildPath, $"{descriptor.ImageName}.csproj");
				var dockerfilePath = Path.Combine(descriptor.BuildPath, "Dockerfile");
				(new ProgramCodeGenerator(descriptor)).GenerateCSharpCode(programFilePath);
				(new DockerfileGenerator(descriptor, includeDebugTools, watch)).Generate(dockerfilePath);
				(new ProjectGenerator(descriptor, dependencies)).Generate(csProjFilePath);
			}
			catch (Exception ex)
			{
				BeamEditorContext.Default.ServiceScope.GetService<MicroservicesDataModel>()
								 .AddLogException(descriptor, ex);
				Debug.LogException(ex);
			}
		}


		public static void UpdateBuildContextWithSource(MicroserviceDescriptor descriptor)
		{
			var dependencies = DependencyResolver.GetDependencies(descriptor);
			FileUtils.CopyAssemblies(descriptor, dependencies);
			FileUtils.CopyDlls(descriptor, dependencies);
			FileUtils.CopySingleFiles(descriptor, dependencies);
		}
	}
}
