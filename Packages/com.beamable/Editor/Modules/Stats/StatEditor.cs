using Beamable.Stats;
using UnityEditor;

namespace UnityEngine
{
	[CustomEditor(typeof(StatBehaviour), true)]
	public class StatEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			var obj = target as StatBehaviour;
			if (obj == null) return;
			EditorGUILayout.LabelField("Current Dbid", obj.DefaultPlayerDbid.ToString());
			EditorGUILayout.LabelField("Current value", obj.Value);
			base.OnInspectorGUI();
		}
	}
}
