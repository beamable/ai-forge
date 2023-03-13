using Beamable.Editor.Reflection;
using System.Linq;
using UnityEditor;

#if !DISABLE_BEAMABLE_TOOLBAR_EXTENDER
using Beamable.Editor.ToolbarExtender;
#endif

namespace Beamable.Editor.Assistant
{
	/// <summary>
	/// An asset post-processor that reloads and re-builds Beamable Assistant-related data defined in relevant scriptable objects.
	/// </summary>
	public class BeamableAssistantAssetPostProcessor : AssetPostprocessor
	{
		public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			if (!BeamEditor.IsInitialized)
				return;

#if !DISABLE_BEAMABLE_TOOLBAR_EXTENDER
			var toolbarExtendedRelatedAssets = importedAssets.Union(movedAssets)
															 .Select(path => (path, type: AssetDatabase.GetMainAssetTypeAtPath(path)))
															 .Where(t => typeof(BeamableAssistantMenuItem).IsAssignableFrom(t.type) || typeof(BeamableToolbarButton).IsAssignableFrom(t.type))
															 .ToList();

			if (toolbarExtendedRelatedAssets.Count > 0 || deletedAssets.Length > 0)
				BeamableToolbarExtender.LoadToolbarExtender();
#endif
			var beamHintDetailsRelatedAssets = importedAssets.Union(movedAssets)
															 .Select(path => (path, type: AssetDatabase.GetMainAssetTypeAtPath(path)))
															 .Where(t => typeof(BeamHintDetailsConfig).IsAssignableFrom(t.type) || typeof(BeamHintTextMap).IsAssignableFrom(t.type))
															 .ToList();

			if (beamHintDetailsRelatedAssets.Count > 0 || deletedAssets.Length > 0)
			{
				BeamEditor.EditorReflectionCache.GetFirstSystemOfType<BeamHintReflectionCache.Registry>()
						  .ReloadHintDetailConfigScriptableObjects(BeamEditor.CoreConfiguration.BeamableAssistantHintDetailConfigPaths);

				BeamEditor.EditorReflectionCache.GetFirstSystemOfType<BeamHintReflectionCache.Registry>()
						  .ReloadHintTextMapScriptableObjects(BeamEditor.CoreConfiguration.BeamableAssistantHintDetailConfigPaths);
			}
		}
	}
}
