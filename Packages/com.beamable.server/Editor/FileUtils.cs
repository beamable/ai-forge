using Beamable.Server.Editor.DockerCommands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Beamable.Server.Editor
{
	public static class FileUtils
	{
		public static void CleanBuildDirectory(MicroserviceDescriptor descriptor)
		{
			var dirExists = Directory.Exists(descriptor.BuildPath);
#if UNITY_EDITOR_WIN
			var longestPathLength = dirExists ? Directory
			                                    .GetFiles(descriptor.BuildPath, "*", SearchOption.AllDirectories)
			                                    .OrderByDescending(p => p.Length)
			                                    .FirstOrDefault()?.Length : descriptor.BuildPath.Length;
			UnityEngine.Assertions.Assert.IsFalse(longestPathLength + Directory.GetCurrentDirectory().Length >= 260,
			                                     "Project path is too long and can cause issues during building on Windows machine. " +
			                                     "Consider moving project to other folder so the project path would be shorter.");
#endif
			// remove everything in the hidden folder...
			if (dirExists)
			{
				OverrideDirectoryAttributes(new DirectoryInfo(descriptor.BuildPath), FileAttributes.Normal);
				Directory.Delete(descriptor.BuildPath, true);
			}
			Directory.CreateDirectory(descriptor.BuildPath);
		}


		public static void CopyDlls(MicroserviceDescriptor descriptor, MicroserviceDependencies dependencies)
		{
			string rootPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);

			var destinationDirectory = Path.Combine(descriptor.BuildPath, "libdll");
			Directory.CreateDirectory(destinationDirectory);
			foreach (var dll in dependencies.DllsToCopy)
			{
				var fullSource = Path.Combine(rootPath, dll.assetPath);
				var fullDest = Path.Combine(destinationDirectory, Path.GetFileName(dll.assetPath));
				MicroserviceLogHelper.HandleLog(descriptor, "Build", "Copying dll from " + fullSource);

				File.Copy(fullSource, fullDest, true);
			}
		}


		public static string GetFullSourcePath(AssemblyDefinitionInfo assemblyDependency)
		{
			string rootPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
			var sourceDirectory = Path.GetDirectoryName(assemblyDependency.Location);
			var fullSource = Path.Combine(rootPath, sourceDirectory);

			return fullSource;
		}

		public static string GetBuildContextPath(AssemblyDefinitionInfo assemblyDependency)
		{
			return assemblyDependency.Name;
		}

		public static void CopyAssemblies(MicroserviceDescriptor descriptor, MicroserviceDependencies dependencies)
		{
			// copy over the assembly definition folders...
			if (dependencies.Assemblies.Invalid.Any())
			{
				throw new Exception($"Invalid dependencies discovered for microservice. {string.Join(",", dependencies.Assemblies.Invalid.Select(x => x.Name))}");
			}

			foreach (var assemblyDependency in dependencies.Assemblies.ToCopy)
			{
				var fullSource = GetFullSourcePath(assemblyDependency);
				MicroserviceLogHelper.HandleLog(descriptor, "Build", "Copying assembly from " + fullSource);

				// TODO: better folder namespacing?
				CopyFolderToBuildDirectory(fullSource, GetBuildContextPath(assemblyDependency), descriptor);
			}
		}

		public static void CopySingleFiles(MicroserviceDescriptor descriptor, MicroserviceDependencies dependencies)
		{
			// copy over the single files...
			foreach (var dep in dependencies.FilesToCopy)
			{
				var targetRelative = dep.Agnostic.SourcePath.Substring(Application.dataPath.Length - "Assets/".Length);
				var targetFull = descriptor.BuildPath + targetRelative;

				MicroserviceLogHelper.HandleLog(descriptor, "Build", "Copying source code to " + targetFull);

				var targetDir = Path.GetDirectoryName(targetFull);
				Directory.CreateDirectory(targetDir);

				// to avoid any file issues, we load the file into memory
				var src = File.ReadAllText(dep.Agnostic.SourcePath);
				File.WriteAllText(targetFull, src);
			}

		}

		public static void CopyFolderToBuildDirectory(string sourceFolderPath, string subFolder, MicroserviceDescriptor descriptor)
		{
			var directoryQueue = new Queue<string>();
			directoryQueue.Enqueue(sourceFolderPath);

			while (directoryQueue.Count > 0)
			{
				var path = directoryQueue.Dequeue();

				var files = Directory
				   .GetFiles(path);
				foreach (var file in files)
				{
					var subPath = file.Substring(sourceFolderPath.Length + 1);
					var destinationFile = Path.Combine(descriptor.BuildPath, subFolder, subPath);

#if UNITY_EDITOR_WIN
               var fullPath = Path.GetFullPath(destinationFile);
               if (fullPath.Length >= 255)
               {
                  Debug.LogError($"There could be problems during building {descriptor.Name}- path is too long. " +
                                      "Consider moving project to another folder so path would be shorter.");
               }
#endif

					Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));
					File.Copy(file, destinationFile, true);
				}

				var subDirs = Directory.GetDirectories(path);
				foreach (var subDir in subDirs)
				{
					var dirName = Path.GetFileName(subDir);
					if (new[] { "~", "obj", "bin" }.Contains(dirName) || dirName.StartsWith("."))
						continue; // skip hidden or dumb folders...

					directoryQueue.Enqueue(subDir);
				}
			}

		}

		public static void OverrideDirectoryAttributes(DirectoryInfo dir, FileAttributes fileAttributes)
		{
			foreach (var subDir in dir.GetDirectories())
			{
				OverrideDirectoryAttributes(subDir, fileAttributes);
				subDir.Attributes = fileAttributes;
			}

			foreach (var file in dir.GetFiles())
			{
				file.Attributes = fileAttributes;
			}
		}
	}
}
