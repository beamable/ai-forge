using Beamable.Common.Content;
using Beamable.Editor.Environment;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Content.UI
{
	[CustomEditor(typeof(ApiContent), true)]
	public class ReadonlyUntilMicroserviceEditor : ContentObjectEditor
	{
		public override void OnInspectorGUI()
		{
			var package = BeamablePackages.ServerPackageMeta.GetResult();
			if (package?.IsPackageAvailable ?? false)
			{
				base.OnInspectorGUI();
			}
			else
			{
				EditorGUILayout.LabelField("This feature is meant to be used with the Microservices package. Please download the package from the Toolbox", EditorStyles.wordWrappedLabel);
				var wasEnabled = GUI.enabled;
				GUI.enabled = false;
				base.OnInspectorGUI();
				GUI.enabled = wasEnabled;
			}

		}
	}
}
