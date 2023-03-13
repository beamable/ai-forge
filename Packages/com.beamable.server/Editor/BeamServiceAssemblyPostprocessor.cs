using System.IO;
using UnityEditor;
using UnityEngine;

namespace Beamable.Server.Editor
{
	public class BeamServiceAssemblyPostprocessor : AssetPostprocessor
	{
		const string EXTENSION = "asmdef";

#if UNITY_2021_1_OR_NEWER
		static void OnPostprocessAllAssets(string[] importedAssets,
		                                   string[] deletedAssets,
		                                   string[] movedAssets,
		                                   string[] movedFromAssetPaths,
		                                   bool _

		)
		{
			Process(importedAssets);
		}
#else
		static void OnPostprocessAllAssets(string[] importedAssets,
										   string[] deletedAssets,
										   string[] movedAssets,
										   string[] movedFromAssetPaths)
		{
			Process(importedAssets);
		}
#endif

		static void Process(string[] importedAssets)
		{
			if (!BeamEditor.IsInitialized)
			{
				return;
			}

			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			foreach (string importedAsset in importedAssets)
			{
				if (!importedAsset.EndsWith(EXTENSION))
				{
					continue;
				}

				var dirPath = Path.GetDirectoryName(importedAsset);

				MicroserviceDescriptor descriptor = null;
				for (int i = 0; i < serviceRegistry.Descriptors.Count; i++)
				{
					if (serviceRegistry.Descriptors[i].SourcePath.Contains(dirPath) &&
						BeamServicesCodeWatcher.Default.IsServiceDependedOnStorage(serviceRegistry.Descriptors[i]))
					{
						descriptor = serviceRegistry.Descriptors[i];
						break;
					}
				}

				if (descriptor == null)
				{
					return;
				}

				var asset = descriptor.ConvertToAsset();
				if (!asset.HasMongoLibraries())
				{
					Debug.LogError($"<b>{descriptor.Name}</b> is depended on storage, but is missing dependencies, adding it now.");
					asset.AddMongoLibraries();
				}
			}
		}
	}
}
