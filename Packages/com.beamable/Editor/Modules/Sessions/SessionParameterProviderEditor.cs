using Beamable.Api.Sessions;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Modules.Sessions
{
	[CustomEditor(typeof(SessionParameterProvider), true)]
	public class SessionParameterProviderEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			var provider = target as SessionParameterProvider;
			if (provider == null) return;

			if (GUILayout.Button("Open Script"))
			{
				var script = MonoScript.FromScriptableObject(provider);

				AssetDatabase.OpenAsset(script);
			}

			base.OnInspectorGUI();
		}
	}
}
