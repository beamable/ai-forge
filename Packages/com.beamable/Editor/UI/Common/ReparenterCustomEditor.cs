using Beamable.UI.Layouts;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor
{
	[CustomEditor(typeof(ReparenterBehaviour), true)]
	public class ReparenterCustomEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var obj = target as ReparenterBehaviour;
			if (obj == null) return;

			if (GUILayout.Button("Recalculate"))
			{
				obj.Source.Calculate();
			}
			EditorGUILayout.LabelField("Current Value", obj.Output ? "True" : "False");
		}
	}
}
