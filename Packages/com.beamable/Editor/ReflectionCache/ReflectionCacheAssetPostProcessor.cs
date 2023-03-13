using Beamable.Common.Reflection;
using Beamable.Reflection;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Beamable.Editor.Reflection
{
	/// <summary>
	/// An asset post-processor that reloads and rebuilds all (or the re-imported) <see cref="IReflectionSystem"/> defined via <see cref="ReflectionSystemObject"/> whenever
	/// one gets re-imported, deleted or moved.
	///
	/// This makes it so that a recompile isn't necessary to update the <see cref="ReflectionCache"/> for cases where you might not want that.
	/// </summary>
	public class ReflectionCacheAssetPostProcessor : AssetPostprocessor
	{
		public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			if (!BeamEditor.IsInitialized)
				return;

			// Check to see if any files will trigger a recompile anyway. If they do, we skip rebuilding if we deleted any C# files, since:
			//  - This runs before the domain reload caused by deleting a C# file.
			//  - We don't need to run this if we domain reload, since we'll run it again when the domain reloads.
			var assetExtensions = movedAssets.Union(deletedAssets).Select(s => Path.HasExtension(s) ? Path.GetExtension(s) : string.Empty).ToList();
			if (assetExtensions.Contains(".cs") || assetExtensions.Contains(".asmdef"))
				return;


			var reflectionCacheRelatedAssets = importedAssets.Union(movedAssets)
															 .Select(path => (path, type: AssetDatabase.GetMainAssetTypeAtPath(path)))
															 .Where(t => typeof(ReflectionSystemObject).IsAssignableFrom(t.type))
															 .ToList();

			if (reflectionCacheRelatedAssets.Count > 0)
			{
				var reimportedReflectionSystemObjects = reflectionCacheRelatedAssets
														.Select(tuple => AssetDatabase.LoadAssetAtPath<ReflectionSystemObject>(tuple.path)).ToList();
				var reimportedReflectionTypes = reimportedReflectionSystemObjects.Select(sysObj => sysObj.SystemType).ToList();

				// we may need to add these new types and objects into the system.
				for (int i = 0; i < reimportedReflectionSystemObjects.Count; i++)
				{
					BeamEditor.EditorReflectionCache.TryRegisterTypeProvider(reimportedReflectionSystemObjects[i].TypeProvider);
					BeamEditor.EditorReflectionCache.TryRegisterReflectionSystem(reimportedReflectionSystemObjects[i].System);
				}

				BeamEditor.EditorReflectionCache.RebuildReflectionUserSystems(reimportedReflectionTypes);
				BeamEditor.EditorReflectionCache.SetStorage(BeamEditor.HintGlobalStorage);

				if (reimportedReflectionTypes.Contains(typeof(BeamHintReflectionCache.Registry)))
				{
					// Set up Globally Accessible Hint System Dependencies and then call init
					foreach (var hintSystem in BeamEditor.GetReflectionSystem<BeamHintReflectionCache.Registry>().GloballyAccessibleHintSystems)
					{
						hintSystem.SetStorage(BeamEditor.HintGlobalStorage);
						hintSystem.SetPreferencesManager(BeamEditor.HintPreferencesManager);

						hintSystem.OnInitialized();
					}
				}

				AssetDatabase.Refresh();
			}

			if (deletedAssets.Length > 0)
			{
				//UnityEditor.MonoScript
				BeamEditor.EditorReflectionCache.RebuildReflectionUserSystems();
				BeamEditor.EditorReflectionCache.SetStorage(BeamEditor.HintGlobalStorage);

				// Set up Globally Accessible Hint System Dependencies and then call init
				foreach (var hintSystem in BeamEditor.GetReflectionSystem<BeamHintReflectionCache.Registry>().GloballyAccessibleHintSystems)
				{
					hintSystem.SetStorage(BeamEditor.HintGlobalStorage);
					hintSystem.SetPreferencesManager(BeamEditor.HintPreferencesManager);

					hintSystem.OnInitialized();
				}

				AssetDatabase.Refresh();
			}
		}
	}
}
