using Beamable.Common;
using Beamable.Editor;
using Beamable.Serialization.SmallerJSON;
using Beamable.Server.Editor.DockerCommands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Beamable.Server.Editor
{

	/// <summary>
	/// Helper class that handles the assembly definitions we generate when creating C#MS and StorageObjects.
	/// Also, contains a bunch of helper functions to manage data inside local StorageObject and some other C#MS stuff.
	/// TODO: Refactor the non-assembly-definition helper functions into more appropriate files.
	/// </summary>
	public static class AssemblyDefinitionHelper
	{
		const string PRECOMPILED = "precompiledReferences";
		const string REFERENCES = "references";
		const string OVERRIDE_REFERENCES = "overrideReferences";
		const string AUTO_REFERENCED = "autoReferenced";
		const string INCLUDE_PLATFORMS = "includePlatforms";
		const string NAME = "name";
		private const string ASSETS_BEAMABLE = "Assets/Beamable/";
		private const string ADD_MONGO = ASSETS_BEAMABLE + "Add Mongo Libraries";
		private const string REMOVE_MONGO = ASSETS_BEAMABLE + "Remove Mongo Libraries";
		// private const string OPEN_MONGO = ASSETS_BEAMABLE + "Open Mongo Data Explorer"; // TODO: Delete this when we have a UI
		// private const string RUN_MONGO = ASSETS_BEAMABLE + "Run Mongo"; // TODO: Delete this when we have a UI
		// private const string KILL_MONGO = ASSETS_BEAMABLE + "Kill Mongo"; // TODO: Delete this when we have a UI
		// private const string CLEAR_MONGO = ASSETS_BEAMABLE + "Clear Mongo Data"; // TODO: Delete this when we have a UI
		// private const string SNAPSHOT_MONGO = ASSETS_BEAMABLE + "Create Mongo Snapshot"; // TODO: Delete this when we have a UI
		// private const string RESTORE_MONGO = ASSETS_BEAMABLE + "Restore Mongo Snapshot"; // TODO: Delete this when we have a UI
		private const int BEAMABLE_PRIORITY = 190;

		public static readonly string[] MongoLibraries = new[]
		{
		 "DnsClient.dll",
		 "MongoDB.Bson.dll",
		 "MongoDB.Driver.Core.dll",
		 "MongoDB.Driver.dll",
		 "MongoDB.Libmongocrypt.dll",
		 "System.Buffers.dll",
		 "System.Runtime.CompilerServices.Unsafe.dll",
		 "SharpCompress.dll"
	  };

		public static void RestoreMongo(StorageObjectDescriptor descriptor)
		{
			var dest = EditorUtility.OpenFolderPanel("Select where to load mongo", "", "default");
			if (string.IsNullOrEmpty(dest)) return;
			Debug.Log("Starting restore...");
			RestoreMongoSnapshot(descriptor, dest).Then(res =>
			{
				if (res)
				{
					Debug.Log($"Finished restoring [{descriptor.Name}]");
				}
				else
				{
					Debug.LogError($"Failed to restore [{descriptor.Name}] database");
				}
			});
		}

		public static void SnapshotMongo(StorageObjectDescriptor descriptor)
		{
			var dest = EditorUtility.OpenFolderPanel("Select where to save mongo", "", "default");
			if (string.IsNullOrEmpty(dest)) return;
			Debug.Log("Starting snapshot...");
			SnapshotMongo(descriptor, dest).Then(res =>
			{
				if (res)
				{
					Debug.Log($"[{descriptor.Name}] snapshot created at {dest}.");
					EditorUtility.OpenWithDefaultApp(dest);
				}
				else
				{
					Debug.Log($"Failed to snapshot [{descriptor.Name}] database.");
				}
			});
		}


		public static void ClearMongo(StorageObjectDescriptor descriptor)
		{
			Debug.Log("Starting clear...");
			ClearMongoData(descriptor).Then(success =>
			{
				if (success)
				{
					Debug.Log($"Cleared [{descriptor.Name}] database.");
				}
				else
				{
					Debug.LogWarning($"Failed to clear [{descriptor.Name}] database.");
				}
			});
		}

		public static void OpenMongoExplorer(StorageObjectDescriptor descriptor)
		{
			Debug.Log("Opening tool...");
			var work = OpenLocalMongoTool(descriptor);
			work.Then(success =>
			{
				if (success)
				{
					Debug.Log("Opened tool.");

				}
				else
				{
					Debug.LogWarning("Failed to open tool.");
				}
			});
		}

		public static void CopyConnectionString(StorageObjectDescriptor descriptor)
		{
			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			var work = serviceRegistry.GetConnectionString(descriptor);
			work.Then(connectionString =>
			{
				if (!string.IsNullOrEmpty(connectionString))
				{
					GUIUtility.systemCopyBuffer = connectionString;
					Debug.Log($"Connection string {connectionString}");

				}
				else
				{
					Debug.LogWarning("Failed to copy connection string.");
				}
			});
		}

		[MenuItem(ADD_MONGO, false, BEAMABLE_PRIORITY)]
		public static void AddMongoLibraries()
		{
			if (Selection.activeObject is AssemblyDefinitionAsset asm)
			{
				asm.AddMongoLibraries();
			}
		}

		[MenuItem(REMOVE_MONGO, false, BEAMABLE_PRIORITY)]
		public static void RemoveMongoLibraries()
		{
			if (Selection.activeObject is AssemblyDefinitionAsset asm)
			{
				asm.RemoveMongoLibraries();
			}
		}

		[MenuItem(ADD_MONGO, true, BEAMABLE_PRIORITY)]
		public static bool ValidateAddMongo()
		{
			return ValidateSelectionIsMicroservice(out var asm) && !asm.HasMongoLibraries();
		}

		[MenuItem(REMOVE_MONGO, true, BEAMABLE_PRIORITY)]
		public static bool ValidateRemoveMongo()
		{
			return ValidateSelectionIsMicroservice(out var asm) && asm.HasMongoLibraries();
		}

		public static bool ValidateSelectionIsMicroservice(out AssemblyDefinitionAsset assembly)
		{
			assembly = null;
			if (!(Selection.activeObject is AssemblyDefinitionAsset asm))
			{
				return false;
			}

			assembly = asm;
			var info = asm.ConvertToInfo();
			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			var descriptor = serviceRegistry.Descriptors.FirstOrDefault(d => d.IsContainedInAssemblyInfo(info));

			var isService = descriptor != null;
			return isService;
		}

		public static IEnumerable<StorageObjectDescriptor> GetStorageReferences(this MicroserviceDescriptor service)
		{
			//TODO: This won't work for nested relationships.

			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			var serviceInfo = service.ConvertToInfo();
			var storages = serviceRegistry.StorageDescriptors.ToDictionary(kvp => kvp.AttributePath);
			var infos = serviceRegistry.StorageDescriptors.Select(s => new Tuple<AssemblyDefinitionInfo, StorageObjectDescriptor>(s.ConvertToInfo(), s)).ToDictionary(kvp => kvp.Item1.Name);
			foreach (var reference in serviceInfo.References)
			{
				if (infos.TryGetValue(reference, out var storageInfo))
				{
					yield return storageInfo.Item2;
				}
			}
		}

		public static bool HasMongoLibraries(this MicroserviceDescriptor service) =>
		   service.ConvertToAsset().HasMongoLibraries();

		public static bool HasMongoLibraries(this AssemblyDefinitionAsset asm)
		{
			var existingRefs = new HashSet<string>(asm.ConvertToInfo().DllReferences);
			foreach (var required in MongoLibraries)
			{
				if (!existingRefs.Contains(required)) return false;
			}
			return true;
		}

		public static void AddMongoLibraries(this MicroserviceDescriptor service) =>
		   service.AddPrecompiledReferences(MongoLibraries);

		public static void RemoveMongoLibraries(this MicroserviceDescriptor service) =>
		   service.RemovePrecompiledReferences(MongoLibraries);

		public static ArrayDict AddMongoLibraries(this AssemblyDefinitionAsset asm) =>
		   asm.AddPrecompiledReferences(MongoLibraries);

		public static ArrayDict AddMongoLibraries(ArrayDict asmJsonData, string asmPath) =>
			AddPrecompiledReferences(asmJsonData, asmPath, MongoLibraries);

		public static void RemoveMongoLibraries(this AssemblyDefinitionAsset asm) =>
		   asm.RemovePrecompiledReferences(MongoLibraries);

		public static bool IsContainedInAssemblyInfo(this IDescriptor service, AssemblyDefinitionInfo asm)
		{
			var assembly = service.Type.Assembly;
			var moduleName = assembly.Modules.FirstOrDefault().Name.Replace(".dll", "");

			return string.Equals(moduleName, asm.Name);
		}

		public static AssemblyDefinitionInfo ConvertToInfo(this AssemblyDefinitionAsset asm)
		{
			var jsonData = Json.Deserialize(asm.text) as ArrayDict;
			var path = AssetDatabase.GetAssetPath(asm);

			var assemblyDefInfo = new AssemblyDefinitionInfo { Location = path };

			if (jsonData.TryGetValue(NAME, out var nameObject) && nameObject is string name)
			{
				assemblyDefInfo.Name = name;
			}

			if (jsonData.TryGetValue(REFERENCES, out var referencesObject) &&
				referencesObject is IEnumerable<object> references)
			{
				assemblyDefInfo.References = references
				   .Cast<string>()
				   .Where(s => !string.IsNullOrEmpty(s))
				   .ToArray();
			}

			if (jsonData.TryGetValue(PRECOMPILED, out var referencesDllObject) &&
				referencesDllObject is IEnumerable<object> dllReferences)
			{
				assemblyDefInfo.DllReferences = dllReferences
				   .Cast<string>()
				   .Where(s => !string.IsNullOrEmpty(s))
				   .ToArray();
			}

			return assemblyDefInfo;
		}

		public static AssemblyDefinitionInfo ConvertToInfo(this IDescriptor service)
		   => service.ConvertToAsset().ConvertToInfo();
		public static AssemblyDefinitionAsset ConvertToAsset(this IDescriptor service)
		   => EnumerateAssemblyDefinitionAssets()
			  .FirstOrDefault(asm => service.IsContainedInAssemblyInfo(asm.ConvertToInfo()));


		private static string[] _assemblyDefGuidsCache;
		public static IEnumerable<AssemblyDefinitionAsset> EnumerateAssemblyDefinitionAssets()
		{
			if (_assemblyDefGuidsCache == null)
				_assemblyDefGuidsCache = AssetDatabase.FindAssets($"t:{nameof(AssemblyDefinitionAsset)}");

			for (int i = 0; i < _assemblyDefGuidsCache.Length; i++)
			{
				var assemblyDefPath = AssetDatabase.GUIDToAssetPath(_assemblyDefGuidsCache[i]);
				var assemblyDef = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(assemblyDefPath);
				if (assemblyDef == null) continue;

				yield return assemblyDef;
			}
		}

		public static IEnumerable<AssemblyDefinitionInfo> EnumerateAssemblyDefinitionInfos()
		{
			foreach (var asm in EnumerateAssemblyDefinitionAssets())
			{
				var assemblyDefInfo = asm.ConvertToInfo();
				if (!string.IsNullOrEmpty(assemblyDefInfo.Name))
				{
					yield return assemblyDefInfo;
				}
			}
		}

		public static void AddPrecompiledReferences(this MicroserviceDescriptor service, params string[] libraryNames)
		   => service.ConvertToAsset().AddPrecompiledReferences(libraryNames);

		public static void AddAndRemoveReferences(this MicroserviceDescriptor service, List<string> toAddReferences, List<string> toRemoveReferences)
			=> service.ConvertToAsset().AddAndRemoveReferences(toAddReferences, toRemoveReferences);

		public static ArrayDict AddPrecompiledReferences(ArrayDict asmJsonData, string asmPath, params string[] libraryNames)
		{
			var dllReferences = GetReferences(PRECOMPILED, asmJsonData);

			foreach (var lib in libraryNames)
			{
				dllReferences.Add(lib);
			}

			asmJsonData[PRECOMPILED] = dllReferences.ToArray();

			if (dllReferences.Count > 0)
			{
				asmJsonData[OVERRIDE_REFERENCES] = true;
			}
			else
			{
				asmJsonData.Remove(OVERRIDE_REFERENCES);
			}

			WriteAssembly(asmPath, asmJsonData);
			return asmJsonData;
		}

		public static ArrayDict AddPrecompiledReferences(this AssemblyDefinitionAsset asm, params string[] libraryNames)
		{
			var jsonData = Json.Deserialize(asm.text) as ArrayDict;
			var path = AssetDatabase.GetAssetPath(asm);
			return AddPrecompiledReferences(jsonData, path, libraryNames);
		}

		public static void CreateAssetDefinitionAssetOnDisk(string filePath, AssemblyDefinitionInfo info)
		{
			var dict = new ArrayDict
			{
				[REFERENCES] = info.References,
				[NAME] = info.Name,
				[AUTO_REFERENCED] = info.AutoReferenced
			};
			if (info.DllReferences.Length > 0) // don't include the field if there are no values.
			{
				dict[PRECOMPILED] = info.DllReferences;
				dict[OVERRIDE_REFERENCES] = true;
			}

			if (info.IncludePlatforms.Length > 0) // don't include the field at all if there are no values
			{
				dict[INCLUDE_PLATFORMS] = info.IncludePlatforms;
			}

			var json = Json.Serialize(dict, new StringBuilder());
			json = Json.FormatJson(json);
			File.WriteAllText(filePath, json);
			AssetDatabase.ImportAsset(filePath);
		}

		public static ArrayDict AddAndRemoveReferences(ArrayDict asmArrayDict,
												  string asmPath,
												  List<string> toAddReferences,
												  List<string> toRemoveReferences)
		{
			var dllReferences = GetReferences(REFERENCES, asmArrayDict);

			if (toAddReferences != null)
			{
				foreach (var toAdd in toAddReferences)
				{
					dllReferences.Add(toAdd);
				}
			}

			if (toRemoveReferences != null)
			{
				foreach (var toRemove in toRemoveReferences)
				{
					dllReferences.Remove(toRemove);
				}
			}

			asmArrayDict[REFERENCES] = dllReferences.ToArray();
			WriteAssembly(asmPath, asmArrayDict);
			return asmArrayDict;
		}

		public static ArrayDict AddAndRemoveReferences(this AssemblyDefinitionAsset asm, List<string> toAddReferences, List<string> toRemoveReferences)
		{
			var jsonData = Json.Deserialize(asm.text) as ArrayDict;
			var path = AssetDatabase.GetAssetPath(asm);
			return AddAndRemoveReferences(jsonData, path, toAddReferences, toRemoveReferences);
		}

		public static void RemovePrecompiledReferences(this MicroserviceDescriptor service, params string[] libraryNames)
		   => service.ConvertToAsset().RemovePrecompiledReferences(libraryNames);

		public static void RemovePrecompiledReferences(this AssemblyDefinitionAsset asm, params string[] libraryNames)
		{
			var jsonData = Json.Deserialize(asm.text) as ArrayDict;
			var dllReferences = GetReferences(PRECOMPILED, jsonData);

			foreach (var lib in libraryNames)
			{
				dllReferences.Remove(lib);
			}

			jsonData[PRECOMPILED] = dllReferences.ToArray();
			if (dllReferences.Count > 0)
			{
				jsonData[OVERRIDE_REFERENCES] = true;
			}
			else
			{
				jsonData.Remove(OVERRIDE_REFERENCES);
			}
			WriteAssembly(asm, jsonData);
		}

		private static HashSet<string> GetReferences(string referenceType, ArrayDict jsonData)
		{
			var dllReferences = new HashSet<string>();
			if (jsonData.TryGetValue(referenceType, out var referencesDllObject) &&
				referencesDllObject is IEnumerable<object> existingReferences)
			{
				dllReferences = new HashSet<string>(existingReferences
					.Cast<string>()
					.Where(s => !string.IsNullOrEmpty(s))
					.ToArray());
			}

			return dllReferences;
		}

		private static void WriteAssembly(string asmPath, ArrayDict jsonData)
		{
			var json = Json.Serialize(jsonData, new StringBuilder());
			json = Json.FormatJson(json);
			File.WriteAllText(asmPath, json);
			AssetDatabase.ImportAsset(asmPath);
		}

		private static void WriteAssembly(AssemblyDefinitionAsset asm, ArrayDict jsonData)
		{
			var path = AssetDatabase.GetAssetPath(asm);
			WriteAssembly(path, jsonData);
		}

		#region Mongo Helpers

		public static async Promise<bool> OpenLocalMongoTool(StorageObjectDescriptor storage)
		{
			var config = MicroserviceConfiguration.Instance.GetStorageEntry(storage.Name);

			var toolCheck = new CheckImageReturnableCommand(storage.LocalToolContainerName);
			var isToolRunning = await toolCheck.StartAsync();

			if (!isToolRunning)
			{
				var run = new RunStorageToolCommand(storage);
				run.Start();
				var success = await run.IsAvailable;
				if (!success)
				{
					return false;
				}
			}

			var path = $"http://localhost:{config.LocalUIPort}";
			Debug.Log($"Opening {path}");
			Application.OpenURL(path);
			return true;
		}

		public static async Promise<bool> SnapshotMongo(StorageObjectDescriptor storage, string destPath)
		{
			var storageCheck = new CheckImageReturnableCommand(storage);
			var isStorageRunning = await storageCheck.StartAsync();
			if (!isStorageRunning) return false;

			var dumpCommand = new MongoDumpCommand(storage);
			var dumpResult = await dumpCommand.StartAsync();
			if (!dumpResult) return false;

			var cpCommand = new DockerCopyCommand(storage, "/beamable/.", destPath);
			return await cpCommand.StartAsync();
		}

		public static async Promise<bool> RestoreMongoSnapshot(StorageObjectDescriptor storage, string srcPath, bool hardReset = true)
		{
			if (hardReset)
			{
				await ClearMongoData(storage);
			}


			var storageCheck = new CheckImageReturnableCommand(storage);
			var isStorageRunning = await storageCheck.StartAsync();
			if (!isStorageRunning)
			{
				var restart = new RunStorageCommand(storage);
				restart.Start();
			}

			srcPath += "/."; // copy _contents_ of folder.
			var cpCommand = new DockerCopyCommand(storage, "/beamable", srcPath, DockerCopyCommand.CopyType.HOST_TO_CONTAINER);
			var cpResult = await cpCommand.StartAsync();
			if (!cpResult) return false;

			var restoreCommand = new MongoRestoreCommand(storage);
			return await restoreCommand.StartAsync();
		}

		public static async Promise<bool> ClearMongoData(StorageObjectDescriptor storage)
		{
			Debug.Log("Clearing mongo");
			var storageCheck = new CheckImageReturnableCommand(storage);
			var isStorageRunning = await storageCheck.StartAsync();
			if (isStorageRunning)
			{
				Debug.Log("Stopping mongo");

				var stopComm = new StopImageCommand(storage);
				await stopComm.StartAsync();
			}

			Debug.Log("Deleting volumes");

			var deleteVolumes = new DeleteVolumeCommand(storage);
			var results = await deleteVolumes.StartAsync();
			var err = results.Any(kvp => !kvp.Value);
			if (err)
			{
				Debug.LogError("Failed to remove all volumes");
				foreach (var kvp in results)
				{
					Debug.LogError($"{kvp.Key} -> {kvp.Value}");
				}
			}

			if (isStorageRunning)
			{
				Debug.Log("Restarting mongo");

				var restart = new RunStorageCommand(storage);
				restart.Start();
			}

			return !err;
		}

		#endregion
	}
}
