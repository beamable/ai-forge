using Beamable.Common.Player;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Player
{
	[CustomPropertyDrawer(typeof(AbsRefreshableObservable), true)]
	public class ObservableReadonlyListPropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_data"), label, true);
		}
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			GUI.enabled = false;
			EditorGUI.PropertyField(position, property.FindPropertyRelative("_data"), label, true);
			GUI.enabled = true;
		}
	}
}
